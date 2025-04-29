
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using LoggerManager;
using MqttHub.Applications.Interfaces;
using MqttHub.Entities.Models;
using MQTTnet;
using MQTTnet.Adapter;
using MQTTnet.Exceptions;
using MQTTnet.Formatter; 
using MQTTnet.Samples.Helpers;
using MQTTnet.Server;
using Polly;
using Polly.Retry;

namespace MqttHub.Applications.Services;

public class MqttHubService : Logable, IMqttHubService
{
    private MqttClientOptionsBuilder? _clientOptionsBuilder;
    private readonly MqttClientFactory _mqttFactory;
    private IMqttClient _mqttClient; 
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private MqttClientOptions? _mqttClientOptions;
    private readonly ICollection<MqttClientSubscribeOptions> _mqttClientSubscribesOptions = []; 
    public event Func<string, string, Task>? OnMessageReceived; 

    private AsyncRetryPolicy<MqttClientPublishResult> CreatePublishRetryPolicy()
    {
        return Policy
            .Handle<MqttCommunicationException>()
            .Or<TimeoutException>()
            .Or<SocketException>()
            .Or<MqttClientDisconnectedException>()
            .Or<MqttClientNotConnectedException>()
            .Or<MqttProtocolViolationException>()
            .Or<MqttConnectingFailedException>() 
            .OrResult<MqttClientPublishResult>(r => !r.IsSuccess) // Retry if publish failed
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
                (result, timeSpan, retryCount, context) =>
                {
                    if (result.Exception != null)
                    {
                        LCall($"Publish failed due to {result.Exception.Message}, retrying {retryCount}").Wait();
                    }
                    else
                    {
                        LCall($"Publish attempt {retryCount} failed, retrying...").Wait();
                    }
                });
    }

    private AsyncRetryPolicy CreateSubscribeRetryPolicy()
    {
        return Policy
            .Handle<MqttCommunicationException>()
            .Or<TimeoutException>()
            .Or<SocketException>()
            .Or<MqttClientDisconnectedException>()
            .Or<MqttClientNotConnectedException>()
            .Or<MqttProtocolViolationException>()
            .Or<MqttConnectingFailedException>()
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
                (result, timeSpan, retryCount, context) =>
                {
                    if (result != null)
                    {
                        LCall($"Subscribe failed due to {result.Message}, retrying {retryCount}").Wait();
                    }
                    else
                    {
                        LCall($"Subscribe attempt {retryCount} failed, retrying...").Wait();
                    }
                });
    }

    public MqttHubService()
    {
        _clientOptionsBuilder = null;
        _mqttFactory = new();
        _mqttClient = _mqttFactory.CreateMqttClient();

        _mqttClient.ConnectedAsync += HandleConnectedAsync;
        _mqttClient.DisconnectedAsync += HandleDisconnectedAsync;
        _mqttClient.ApplicationMessageReceivedAsync += HandleMessageReceivedAsync; 
    }

    // from https://test.mosquitto.org/ssl/mosquitto.org.crt
    const string mosquitto_org = @"
    -----BEGIN CERTIFICATE-----
    MIIEAzCCAuugAwIBAgIUBY1hlCGvdj4NhBXkZ/uLUZNILAwwDQYJKoZIhvcNAQEL
    BQAwgZAxCzAJBgNVBAYTAkdCMRcwFQYDVQQIDA5Vbml0ZWQgS2luZ2RvbTEOMAwG
    A1UEBwwFRGVyYnkxEjAQBgNVBAoMCU1vc3F1aXR0bzELMAkGA1UECwwCQ0ExFjAU
    BgNVBAMMDW1vc3F1aXR0by5vcmcxHzAdBgkqhkiG9w0BCQEWEHJvZ2VyQGF0Y2hv
    by5vcmcwHhcNMjAwNjA5MTEwNjM5WhcNMzAwNjA3MTEwNjM5WjCBkDELMAkGA1UE
    BhMCR0IxFzAVBgNVBAgMDlVuaXRlZCBLaW5nZG9tMQ4wDAYDVQQHDAVEZXJieTES
    MBAGA1UECgwJTW9zcXVpdHRvMQswCQYDVQQLDAJDQTEWMBQGA1UEAwwNbW9zcXVp
    dHRvLm9yZzEfMB0GCSqGSIb3DQEJARYQcm9nZXJAYXRjaG9vLm9yZzCCASIwDQYJ
    KoZIhvcNAQEBBQADggEPADCCAQoCggEBAME0HKmIzfTOwkKLT3THHe+ObdizamPg
    UZmD64Tf3zJdNeYGYn4CEXbyP6fy3tWc8S2boW6dzrH8SdFf9uo320GJA9B7U1FW
    Te3xda/Lm3JFfaHjkWw7jBwcauQZjpGINHapHRlpiCZsquAthOgxW9SgDgYlGzEA
    s06pkEFiMw+qDfLo/sxFKB6vQlFekMeCymjLCbNwPJyqyhFmPWwio/PDMruBTzPH
    3cioBnrJWKXc3OjXdLGFJOfj7pP0j/dr2LH72eSvv3PQQFl90CZPFhrCUcRHSSxo
    E6yjGOdnz7f6PveLIB574kQORwt8ePn0yidrTC1ictikED3nHYhMUOUCAwEAAaNT
    MFEwHQYDVR0OBBYEFPVV6xBUFPiGKDyo5V3+Hbh4N9YSMB8GA1UdIwQYMBaAFPVV
    6xBUFPiGKDyo5V3+Hbh4N9YSMA8GA1UdEwEB/wQFMAMBAf8wDQYJKoZIhvcNAQEL
    BQADggEBAGa9kS21N70ThM6/Hj9D7mbVxKLBjVWe2TPsGfbl3rEDfZ+OKRZ2j6AC
    6r7jb4TZO3dzF2p6dgbrlU71Y/4K0TdzIjRj3cQ3KSm41JvUQ0hZ/c04iGDg/xWf
    +pp58nfPAYwuerruPNWmlStWAXf0UTqRtg4hQDWBuUFDJTuWuuBvEXudz74eh/wK
    sMwfu1HFvjy5Z0iMDU8PUDepjVolOCue9ashlS4EB5IECdSR2TItnAIiIwimx839
    LdUdRudafMu5T5Xma182OC0/u/xRlEm+tvKGGmfFcN0piqVl8OrSPBgIlb+1IKJE
    m/XriWr/Cq4h/JfB7NTsezVslgkBaoU=
    -----END CERTIFICATE-----";

    public void Dispose()
    {
        /*
         * This sample disconnects from the server without sending a DISCONNECT packet.
         * This way of disconnecting is treated as a non-clean disconnect which will
         * trigger sending the last will etc.
         */

        // Calling _Dispose_ or use of a _using_ statement will close the transport connection
        // without sending a DISCONNECT packet to the server.

        if (_mqttClient == null)
        {
            LCall($"Client is null").Wait();
            return;
        } 

        _mqttClient.Dispose();
        ResetOptionBuilder();
        ResetClient();
        ResetClientOptions();
        ResetSubsribedTopics();
    }

    public MqttHubService UseTcpServer(Server server)
    {
        if (_mqttClient.IsConnected)
        {
            throw new Exception("Already connected to the MQTT broker");
        }

        _clientOptionsBuilder ??= new();

        _clientOptionsBuilder = _clientOptionsBuilder
            .WithTcpServer(server.Address, server.Port)
            .WithCleanSession(false)
            .WithClientId(server.ClientId);

        return this;
    }

    public MqttHubService UseWebSocket(Server server)
    {
        /*
         * This sample creates a simple MQTT client and connects to a public broker using a WebSocket connection.
         *
         * This is a modified version of the sample _Connect_Client_! See other sample for more details.
         */

        if (_mqttClient.IsConnected)
        {
            throw new Exception("Already connected to the MQTT broker");
        }

        _clientOptionsBuilder ??= new();

        _clientOptionsBuilder = _clientOptionsBuilder
            .WithWebSocketServer(o => o.WithUri($"{server.Address}:{server.Port}/mqtt"))
            .WithCleanSession(false).WithClientId(server.ClientId);

        return this;
    }

    public async Task ConnectAsync()
    {
        await _semaphore.WaitAsync(); 

        try
        {
            if (_mqttClient.IsConnected)
            {
                await LCall("Already connected to MQTT broker.");
                return;
            }

            if (_mqttClientOptions == null)
            {
                await LCall("Client options has not been initialized.");
                return;
            }

            using var timeout = new CancellationTokenSource(5000);

            // This will throw an exception if the server is not available.
            // The result from this message returns additional data which was sent
            // from the server. Please refer to the MQTT protocol specification for details.   
            var response = await _mqttClient.ConnectAsync(_mqttClientOptions, timeout.Token);

            await LCall("The MQTT client is connected.");

            response.DumpToConsole();
        }  
        finally
        {
            _semaphore.Release();
        }
    } 

    public MqttHubService UseMQTTv5()
    {
        if (_mqttClient.IsConnected)
        {
            throw new Exception("The MQTT client is connected"); 
        }

        if (_clientOptionsBuilder is null)
        {
            throw new Exception("TCP server or Socket server has not been initialized");
        }

        _clientOptionsBuilder = _clientOptionsBuilder
            .WithProtocolVersion(MqttProtocolVersion.V500);

        return this;
    }

    public MqttHubService UseTLS(ICollection<Certificate> certificates, SslProtocols protocol = SslProtocols.Tls12)
    {
        /*
         * This sample creates a simple MQTT client and connects to a public broker using TLS 1.2 encryption.
         *
         * This is a modified version of the sample _Connect_Client_! See other sample for more details.
         */
        if (_mqttClient.IsConnected)
        {
            throw new Exception("Already connected to the MQTT broker");
        }

        if (_clientOptionsBuilder is null)
        {
            throw new Exception("TCP server or Socket server has not been initialized");
        }
         
        X509Certificate2Collection x509Certificate2s = LoadCertificates(certificates); 

        _clientOptionsBuilder = _clientOptionsBuilder 
            .WithTlsOptions(
                o =>
                {
                    // Is Using TLS
                    o.UseTls(true);
                    // certificates is used
                    o.WithClientCertificates(x509Certificate2s);
                    // The used public broker sometimes has invalid certificates. This sample accepts all
                    // certificates. This should not be used in live environments.
                    o.WithCertificateValidationHandler(validationEvent => ValidateServerCertificate(validationEvent)); 
                    // The default value is determined by the OS. Set manually to force version.
                    o.WithSslProtocols(protocol);
                });

        return this;
    }

  

    public MqttHubService UseServerCA(string certificate)
    {
        if (_mqttClient.IsConnected)
        {
            throw new Exception("Already connected to the MQTT broker");
        }

        if (_clientOptionsBuilder is null)
        {
            throw new Exception("TCP server or Socket server has not been initialized");
        }

        var caChain = new X509Certificate2Collection();

        caChain.ImportFromPem(string.IsNullOrWhiteSpace(certificate) ? mosquitto_org : certificate); 
        // from https://test.mosquitto.org/ssl/mosquitto.org.crt

        _clientOptionsBuilder = _clientOptionsBuilder
            .WithTlsOptions(new MqttClientTlsOptionsBuilder().WithTrustChain(caChain).Build());

        return this;    
    }

  

    public MqttHubService Build()
    {
        if (_clientOptionsBuilder is null)
        {
            throw new Exception("TCP server or WebSocket server has not been initialized.");
        }

        _mqttClientOptions = _clientOptionsBuilder.Build();

        return this;
    }

    public MqttHubService UseAWS()
    {
        /*
         * This sample creates a simple MQTT client and connects to an Amazon Web Services broker.
         *
         * The broker requires special settings which are set here.
         */

        if (_mqttClient.IsConnected)
        {
            throw new Exception("Already connected to MQTT broker.");  
        }

        if (_clientOptionsBuilder is null)
        {
            throw new Exception("TCP server or WebSocket server has not been initialized.");
        }

        _clientOptionsBuilder = _clientOptionsBuilder
            // Disabling packet fragmentation is very important!
            .WithoutPacketFragmentation();

        return this;
    }

    public async Task DisconnectAsync(MqttClientDisconnectOptionsReason reason = MqttClientDisconnectOptionsReason.NormalDisconnection, string reasonString = "")
    {
        /*
         * This sample disconnects from the server with sending a DISCONNECT packet.
         * This way of disconnecting is treated as a clean disconnect which will not
         * trigger sending the last will etc.
         */

        await _semaphore.WaitAsync();

        try
        {
            if (!_mqttClient.IsConnected)
            {
                await LCall("Already disconnected from MQTT Broker"); 
                return;
            }

            await _mqttClient.DisconnectAsync(reason: reason, reasonString: reasonString);
        } 
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task PublishAsync(PublishOption topic)
    {
        if (!_mqttClient.IsConnected)
        {
            await ConnectAsync();
        }  
        
        var applicationMessage = new MqttApplicationMessageBuilder()
            .WithTopic(topic.Topic)
            .WithPayload(topic.Payload)
            .WithContentType(topic.PayloadContentType.Value)
            .WithQualityOfServiceLevel(topic.QoS) 
            .WithRetainFlag(topic.RetainFlag)
            .Build();

        var result = await CreatePublishRetryPolicy().ExecuteAsync(async() =>
        {
            return await _mqttClient.PublishAsync(applicationMessage);
        });

        if (!result.IsSuccess)
        {
            await LCall(topic); 
        } 
    } 
    
    public async Task SubscribeAsync(SubscribeOption topic)
    {
        if (!_mqttClient.IsConnected)
        {
            await LCall("Already disconnected from MQTT Broker");
            return;
        }

        var mqttSubscribeOptions = _mqttFactory.CreateSubscribeOptionsBuilder()
              .WithTopicFilter($"$share/{topic.Group}/{topic.Topic}", qualityOfServiceLevel: topic.QoS, noLocal: topic.NoLocal, retainAsPublished: topic.RetainAsPublished)  
              .Build();

        _mqttClientSubscribesOptions.Add(mqttSubscribeOptions);

        await CreateSubscribeRetryPolicy().ExecuteAsync(async() =>
        {
            await _mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);
        });

    }

    public async Task PingAsync()
    {
        /*
         * This sample sends a PINGREQ packet to the server and waits for a reply.
         *
         * This is only supported in MQTTv5.0.0+.
         */

        await _semaphore.WaitAsync();

        try
        {   
            await _mqttClient.PingAsync(CancellationToken.None);

            await LCall($"The MQTT server replied to the ping request"); 
        } 
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task ReconnectAsync()
    {  
        await _semaphore.WaitAsync(); 
        await Task.Delay(500);

        try
        {
            var timeout = new CancellationTokenSource(5000);

            int attempt = 0;
            const int maxAttempts = 20; // Batas maksimal percobaan

            while (!_mqttClient.IsConnected && attempt < maxAttempts && _mqttClientOptions is not null)
            {
                try
                {
                    await _mqttClient.ConnectAsync(_mqttClientOptions, timeout.Token);
                    if (_mqttClient.IsConnected)
                    { 
                        await LCall("Re-Connected successfully!");

                        if (_mqttClientSubscribesOptions.Count > 0)
                        {
                            foreach (var mqttClientSubscribeOptions  in _mqttClientSubscribesOptions)
                            { 
                                await _mqttClient.SubscribeAsync(mqttClientSubscribeOptions, CancellationToken.None); 
                            }
                        }

                        return;
                    }
                }
                catch (Exception ex)
                { 
                    await LCall($"Reconnect attempt {attempt + 1} failed: {ex.Message}");
                }

                attempt++;
                await Task.Delay(Math.Min(5000 * (int)Math.Pow(2, attempt), 30000), timeout.Token);
            }

            if (!_mqttClient.IsConnected)
            {
                await LCall("Failed to reconnect after maximum attempts.");
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task HandleConnectedAsync(MqttClientConnectedEventArgs args)
    {
        await LCall("Connected to broker."); 
    }

    private async Task HandleDisconnectedAsync(MqttClientDisconnectedEventArgs args)
    {
        await LCall("Disconnected from broker. Attempting to reconnect...");
        await ReconnectAsync();
    }

    private async Task HandleMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        try
        { 
            var topic = args.ApplicationMessage.Topic;
            var payload = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);
            args.AutoAcknowledge = false;

            var shutdownToken = new CancellationTokenSource(5000).Token;

            if (OnMessageReceived != null)
            { 
                var handlers = OnMessageReceived.GetInvocationList()
                    .Select(handler => ((Func<string, string, Task>) handler).Invoke(topic, payload));

                await Task.WhenAll(handlers);
                
                await args.AcknowledgeAsync(shutdownToken); 
            } 
        }
        catch (Exception ex)
        {
            await LCall($"Error in message handling: {ex.Message}");
        }
    } 

    private void ResetSubsribedTopics()
    {
        _mqttClientSubscribesOptions.Clear();
    } 

    private void ResetOptionBuilder()
    {
        _clientOptionsBuilder = null;
    }

    private void ResetClient()
    {
        // Inisialiasi IMqttClient dengan instance baru
        _mqttClient = _mqttFactory.CreateMqttClient();
    }

    private void ResetClientOptions()
    {
        // Reset mqttClient dengan instance baru
        _mqttClientOptions = null;
    }

    private X509Certificate2Collection LoadCertificates(ICollection<Certificate> certificates)
    {
        var collection = new X509Certificate2Collection();

        foreach (var cert in certificates) 
        {
            if (string.IsNullOrWhiteSpace(cert.Path))
            {
                continue;
            }

            if (!File.Exists(cert.Path))
            {
                continue;   
            }


            X509Certificate2 certificate2 = X509CertificateLoader.LoadCertificateFromFile(cert.Path);

            RSA rsa = RSA.Create();
            if (!string.IsNullOrEmpty(cert.Password))
            {
                rsa.ImportFromPem(cert.Password);
            }

            var finalCertificate = string.IsNullOrEmpty(cert.Password) ? certificate2 : certificate2.CopyWithPrivateKey(rsa);

            collection.Add(finalCertificate);
        } 

        return collection;
    }


    private bool ValidateServerCertificate(MqttClientCertificateValidationEventArgs args)
    {
        if (args.Certificate == null)
        {
            LCall("Certificate is empty").Wait(); 
            return false;
        }

        try
        {
            // Menggunakan X509CertificateLoader untuk memuat sertifikat
            X509Certificate2 cert = X509CertificateLoader.LoadCertificate(args.Certificate.Export(X509ContentType.Cert));

            using X509Chain chain = new();
            chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
            chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
            chain.ChainPolicy.VerificationTime = DateTime.Now;

            bool isValid = chain.Build(cert);

            if (!isValid)
            {
                LCall("Certificate validation failed!").Wait();
                foreach (var element in chain.ChainElements)
                {
                    foreach (var status in element.ChainElementStatus)
                    {
                        LCall($"Certificate error: {status.StatusInformation}").Wait();
                    }
                }
            }

            return isValid;
        }
        catch (Exception ex)
        {
            LCall($"Certificate validation exception: {ex.Message}").Wait();
            return false;
        }
    }
} 