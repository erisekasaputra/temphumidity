#include <WiFi.h>
#include <PubSubClient.h>
#include <ModbusMaster.h>
#include <ArduinoJson.h>
#include <NTPClient.h>
#include <WiFiUdp.h>
#include <DFRobot_RTU.h>
 
const char* ssid = "Riscon 12.";
const char* password = "8984175308";

// MQTT Broker
const char* mqtt_server = "192.168.68.117"; 

// Modbus Configuration
#define RX2 16  // GPIO for RS485 RX
#define TX2 17  // GPIO for RS485 TX
#define DE_RE 4  // GPIO for RS485 DE/RE
 
ModbusMaster node;

DFRobot_RTU Modbus(/*s =*/&Serial1);
uint16_t Read_Holding_Reg_5;
uint16_t Read_Holding_Reg_4;

// WiFi and MQTT Client
WiFiClient espClient;
PubSubClient client(espClient);

// NTP Configuration
WiFiUDP ntpUDP; 

void setup() {
    Serial.begin(115200); 
    Serial1.begin(9600, SERIAL_8N1, RX2, TX2);
    pinMode(DE_RE, OUTPUT);
    digitalWrite(DE_RE, LOW);
 
    // Connect to WiFi
    setup_wifi();
    
    // Setup MQTT
    client.setServer(mqtt_server, 1883); 
}

void setup_wifi() {
    WiFi.begin(ssid, password);
    while (WiFi.status() != WL_CONNECTED) {
        delay(500);
        Serial.print(".");
    }
    Serial.println("\nWiFi connected");
}

void reconnect() {
    while (!client.connected()) {
        Serial.print("Connecting to MQTT...");
        if (client.connect("ESP32Client")) {
            Serial.println("connected");
        } else {
            Serial.print("failed, rc=");
            Serial.print(client.state());
            Serial.println(" retrying in 5 seconds");
            delay(5000);
        }
    }
}

void loop() {
    if (!client.connected()) {
        reconnect();
    }
    client.loop(); 
    
 
    
    //Temp = dht.readTemperature();     // Membaca nilai Suhu
    Read_Holding_Reg_4= Modbus.readInputRegister(1,1);
    float A=10;
    float MD20_Suhu =Read_Holding_Reg_4;
    float Temp = MD20_Suhu/A;
    //delay(500);

    
    //Hum = dht.readHumidity();         // Membaca nilai Humidity
    Read_Holding_Reg_5= Modbus.readInputRegister(1,2);
    float B=10;
    float MD20_Hum =Read_Holding_Reg_5;
    float Hum = MD20_Hum/B;
     
        
        Serial.print("Temperature: ");
        Serial.print(Temp);
        Serial.println(" °C");
        Serial.print("Humidity: ");
        Serial.print(Hum);
        Serial.println(" %");
         
        StaticJsonDocument<256> doc;
        
        doc["SensorId"] = "fcf5d89e-c0d8-4785-8e54-54f217987798"; // Example GUID
        doc["SensorName"] = "Humidity";
        doc["SensorValue"] = Hum;
        doc["SensorUnitValue"] = "%"; 

        char jsonBuffer[256];
        serializeJson(doc, jsonBuffer);
        client.publish("sensor/fcf5d89e-c0d8-4785-8e54-54f217987798", jsonBuffer);

        doc["SensorId"] = "fcf5d89e-c0d8-4785-8e54-54f217987799";
        doc["SensorName"] = "Temperature";
        doc["SensorValue"] = Temp;
        doc["SensorUnitValue"] = "°C";

        serializeJson(doc, jsonBuffer);
        client.publish("sensor/fcf5d89e-c0d8-4785-8e54-54f217987799", jsonBuffer);

    delay(1000); // Delay between readings
}
