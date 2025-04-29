#include <WiFi.h>
#include <PubSubClient.h>
#include <ModbusMaster.h>
#include <ArduinoJson.h>
#include <NTPClient.h>
#include <WiFiUdp.h>
#include <DFRobot_RTU.h>
#include <Adafruit_SSD1306.h>
#include <Adafruit_GFX.h>
#include <HTTPClient.h>
#include <TroykaMQ.h>
 
const char* ssid = "KAI";
const char* password = "J4kaRta8F";

// MQTT Broker
const char* mqtt_server = "192.168.50.165"; 

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

//=============== OLED LCD Declaration ================//
#define SCREEN_WIDTH 128 // OLED display width, in pixels
#define SCREEN_HEIGHT 64 // OLED display height, in pixels
// Declaration for an SSD1306 display connected to I2C (SDA, SCL pins)
Adafruit_SSD1306 display(SCREEN_WIDTH, SCREEN_HEIGHT, &Wire, -1);

//========= LED Indikator ================//
#define LEDIND 2

float Temp, Hum;

 



void setup() {
    Serial.begin(115200); 
    Serial1.begin(9600, SERIAL_8N1, RX2, TX2);
    pinMode(DE_RE, OUTPUT);
    digitalWrite(DE_RE, LOW);

// Connect to WiFi
    setup_wifi();
    client.setServer(mqtt_server, 1883);

  initOLED();
  splashScreen(1.5, "Iot Monitoring", 2, "PT.KATOLEC", 3000);
  splashScreen(2, "IoT", 1.5, "TEMPERATURE+HUMIDITY", 3000);
  connecting2WiFi(ssid, password);   // Connecting to Wi-Fi


    

 


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
    
   // Serial.println("WiFi terputus, mencoba menyambung...");
   // connecting2WiFi(ssid, password);       // Melakukan koneksi kembali ke WiFI
    
    //Temp = dht.readTemperature();     // Membaca nilai Suhu
    Read_Holding_Reg_4= Modbus.readInputRegister(1,0);
    float A=100;
    float MD20_Suhu =Read_Holding_Reg_4;
    float Temp = MD20_Suhu/A;
    //delay(500);

    
    //Hum = dht.readHumidity();         // Membaca nilai Humidity
    Read_Holding_Reg_5= Modbus.readInputRegister(1,1);
    float B=100;
    float MD20_Hum =Read_Holding_Reg_5;
    float Hum = MD20_Hum/B;

        //display.clearDisplay();
        // display.setTextSize(1);
        displayOnLCD(Temp, Hum);
        
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
