#include <WiFi.h>
#include <PubSubClient.h>
#include <ModbusMaster.h>
#include <ArduinoJson.h>
#include <FastLED.h>

// ======== Konstanta ID Sensor ==========
#define HUMIDITY_SENSOR_ID "fcf5d89e-c0d8-4785-8e54-54f217987798"
#define TEMPERATURE_SENSOR_ID "fcf5d89e-c0d8-4785-8e54-54f217987799"

// ======== Konfigurasi RS485 XY-MD02 =========
#define RX2 18
#define TX2 17
#define MODBUS_BAUD 9600
#define SLAVE_ID 1

ModbusMaster node;

#define DATA_PIN 38       // Pin data LED WS2812
#define LED_TYPE WS2812
#define NUM_LEDS 1
#define COLOR_ORDER RGB

CRGB leds[NUM_LEDS];
uint8_t brightness = 128;

// ======== Konfigurasi WiFi & MQTT =========== 
const char* ssid = "RUSA RAYA";
const char* password = "89841753050776";
const char* mqtt_server = "192.168.1.6";
const int mqtt_port = 1883;

bool is_sensor1_error = false;
bool is_sensor2_error = false; 

WiFiClient espClient;
PubSubClient client(espClient);

// ======== LED Indikator ========== 
#define RELAY_ALERT 2

// ======== READY Indikator ========
#define RELAY_READY 1

// ======== Konfigurasi Retry ========== 
const int WIFI_MAX_RETRY = 20;
const int MQTT_MAX_RETRY = 10;

void setLEDStatusNonBlocking(CRGB color, int duration_ms = 1000) {
  leds[0] = color;
  FastLED.show();

  unsigned long start = millis();
  while (millis() - start < duration_ms) {
    client.loop();
    delay(10);
  }

  leds[0] = CRGB::Black;
  FastLED.show();
}

void callback(char* topic, byte* payload, unsigned int length) {
  String topicStr = String(topic);

  Serial.print("Message arrived [");
  Serial.print(topicStr);
  Serial.print("] ");

  String message;
  for (unsigned int i = 0; i < length; i++) {
    Serial.print((char)payload[i]);
    message += (char)payload[i];
  }
  Serial.println();
  
  if (topicStr.indexOf(HUMIDITY_SENSOR_ID) != -1) {
    is_sensor1_error = (message != "Normal");
  } 

  if (topicStr.indexOf(TEMPERATURE_SENSOR_ID) != -1) {
    is_sensor2_error = (message != "Normal");
  }
}

void setup() {
  pinMode(RELAY_ALERT, OUTPUT);
  digitalWrite(RELAY_ALERT, LOW);
  
  pinMode(RELAY_READY, OUTPUT);
  digitalWrite(RELAY_READY, HIGH);

  Serial.begin(115200);
  Serial2.begin(MODBUS_BAUD, SERIAL_8N1, RX2, TX2);
  node.begin(SLAVE_ID, Serial2);

  FastLED.addLeds<LED_TYPE, DATA_PIN, COLOR_ORDER>(leds, NUM_LEDS);
  FastLED.setBrightness(brightness);
  leds[0] = CRGB::Black;
  FastLED.show();

  setupWiFiWithLED();
  client.setServer(mqtt_server, mqtt_port);
  client.setKeepAlive(120);
  client.setCallback(callback);

  Serial.println(F("System Ready - XY-MD02 Monitoring"));
}

void safeRestart(const char* reason) {
  Serial.printf("System Restart - Reason: %s\n", reason);
  delay(2000);
  ESP.restart();
}

void safeRestartWithLED(const char* reason, CRGB color) {
  Serial.printf("System Restart - Reason: %s\n", reason);
  setLEDStatusNonBlocking(color, 2000);
  ESP.restart();
}

void setupWiFiWithLED() {
  WiFi.mode(WIFI_STA);
  WiFi.begin(ssid, password);
  Serial.print(F("Connecting to WiFi"));

  int retryCount = 0;
  while (WiFi.status() != WL_CONNECTED) {
    digitalWrite(RELAY_READY, HIGH);
    setLEDStatusNonBlocking(CRGB::Blue, 200);
    retryCount++;
    Serial.print(".");
    if (retryCount >= WIFI_MAX_RETRY) {
      setLEDStatusNonBlocking(CRGB::Red, 2000);
      safeRestart("WiFi Connection Failed");
    }
  } 

  digitalWrite(RELAY_READY, LOW);
  Serial.println(F("\nWiFi Connected"));
  Serial.print(F("IP Address: "));
  Serial.println(WiFi.localIP());
}

void reconnectWiFi() {
  if (WiFi.status() != WL_CONNECTED) {
    Serial.println(F("WiFi Disconnected, Attempting Reconnect..."));
    setupWiFiWithLED();
  }
}

void reconnectMQTTWithLED() {
  int retryCount = 0;
  while (!client.connected()) {
    digitalWrite(RELAY_READY, HIGH);
    Serial.print(F("Connecting to MQTT..."));
    if (client.connect("ESP32_XYMD02")) {
      Serial.println(F("MQTT Connected"));
      client.subscribe((String("monitor/sensor/alert/") + TEMPERATURE_SENSOR_ID).c_str());
      client.subscribe((String("monitor/sensor/alert/") + HUMIDITY_SENSOR_ID).c_str());
    } else {
      Serial.printf("Failed, rc=%d\n", client.state());
      retryCount++;
      setLEDStatusNonBlocking(CRGB::Yellow, 500);
      if (retryCount >= MQTT_MAX_RETRY) {
        setLEDStatusNonBlocking(CRGB::Orange, 2000);
        safeRestart("MQTT Connection Failed");
      }
    }
  }

  
  digitalWrite(RELAY_READY, LOW);
}

bool publishSensorWithRetry(const char* topic, const char* sensorId, const char* sensorName, float value, const char* unit, const int maxAttempts = 2) {
  StaticJsonDocument<256> doc;
  char payload[256];

  doc["SensorId"] = sensorId;
  doc["SensorName"] = sensorName;
  doc["SensorValue"] = value;
  doc["SensorUnitValue"] = unit;

  serializeJson(doc, payload);

  for (int attempt = 1; attempt <= maxAttempts; attempt++) {
    if (!client.connected()) reconnectMQTTWithLED();

    if (client.publish(topic, payload)) {
      Serial.printf("Published to %s: %s\n", topic, payload);
      setLEDStatusNonBlocking(CRGB::Green, 200);
      return true;
    } else {
      Serial.printf("Publish failed to %s, attempt %d\n", topic, attempt);
      reconnectMQTTWithLED();
    }
  }
  Serial.printf("Final failure to publish to %s after %d attempts\n", topic, maxAttempts);
  safeRestartWithLED("Publish Failed", CRGB::Red);
  return false;
}

float readSensorRegisterWithLED(uint16_t reg) {
  uint8_t result = node.readInputRegisters(reg, 1);
  if (result == node.ku8MBSuccess) {
    return node.getResponseBuffer(0) / 10.0;
  } else {
    Serial.printf("Failed to read register %u, Error: %02X\n", reg, result);
    setLEDStatusNonBlocking(CRGB::Purple, 500);
    return NAN;
  }
}

void loop() {
  reconnectWiFi();

  if (!client.connected()) 
    reconnectMQTTWithLED();
  
  client.loop();

  static unsigned long lastReadTime = 0;
  if (millis() - lastReadTime > 1000) {
    lastReadTime = millis();

    float suhu = readSensorRegisterWithLED(1);
    float hum = readSensorRegisterWithLED(2);

    leds[0] = CRGB::Blue;
    FastLED.show();

    if (!isnan(suhu) && !isnan(hum)) {
      Serial.printf("Temperature: %.1f C, Humidity: %.1f %%\n", suhu, hum);

      publishSensorWithRetry((String("sensor/") + HUMIDITY_SENSOR_ID).c_str(), HUMIDITY_SENSOR_ID, "Humidity", hum, "%");
      publishSensorWithRetry((String("sensor/") + TEMPERATURE_SENSOR_ID).c_str(), TEMPERATURE_SENSOR_ID, "Temperature", suhu, "C");

      leds[0] = CRGB::Black;
      FastLED.show();
    } else {
      Serial.println(F("Sensor reading failed, skipping publish..."));
    }

    digitalWrite(RELAY_ALERT, (!is_sensor1_error && !is_sensor2_error) ? LOW : HIGH);
  }
}
