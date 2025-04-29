void initOLED(){
  if(!display.begin(SSD1306_SWITCHCAPVCC, 0x3C)) { // Address 0x3D for 128x64
    Serial.println(F("SSD1306 allocation failed"));
    for(;;);
  }
  delay(2000);
  display.clearDisplay();
}

void splashScreen(float size_1, char* row_1, float size_2, char* row_2, int jeda){
  
  display.clearDisplay();

  display.setTextColor(WHITE);
  display.setTextSize(size_1);
  displayCenterText(row_1, 20);

  display.setTextSize(size_2);
  displayCenterText(row_2, 40);

  display.display();
  delay(jeda);
}

// Fungsi untuk menampilkan teks terpusat
void displayCenterText(const char *text, int y) {
  int16_t x1, y1;
  uint16_t w, h;

  // Hitung lebar dan tinggi teks
  display.getTextBounds(text, 0, 0, &x1, &y1, &w, &h);

  // Hitung posisi x agar teks berada di tengah
  int x = (SCREEN_WIDTH - w) / 2;

  // Set posisi kursor dan tampilkan teks
  display.setCursor(x, y);
  display.println(text);
}


void connecting2WiFi(const char* SSID, const char* PASS) {
  WiFi.mode(WIFI_STA);
  int length = strlen(SSID);
  int padding = (128 - (length * 6)) / 2; // Hitung padding horizontal (font default ukuran 6 px)
  
  display.clearDisplay();
  display.setTextSize(1);
  display.setTextColor(SSD1306_WHITE);

  Serial.print("Connecting to WiFi ..");
  display.setCursor(0, 0);
  display.println("Connecting to WiFi...");
  display.setCursor(padding, 20);
  display.println(SSID);
  display.display();
  delay(2000);
  
  WiFi.begin(SSID, PASS);
  int dotCount = 0;
  while (WiFi.status() != WL_CONNECTED) {
    digitalWrite(LEDIND,HIGH);
    delay(250);
    // Tampilkan titik-titik loading
    display.setCursor((dotCount * 6), 40); // Titik-titik bergerak horizontal
    display.print(".");
    display.display();
    dotCount++;
    if (dotCount > 20) {
      display.fillRect(0, 40, 128, 8, SSD1306_BLACK); // Hapus area titik
      dotCount = 0;
    }
    Serial.print('.');
    digitalWrite(LEDIND,LOW);
    delay(250);
  }
  
  display.clearDisplay();
  display.setTextSize(1.5);
  displayCenterText("WiFi Connected!", 20);
   // Convert IPAddress to string before displaying it
  String ip = WiFi.localIP().toString();
  displayCenterText(ip.c_str(), 40);
  display.display();
  Serial.println(WiFi.localIP());
 // delay(3000);
  delay(1000);

  display.clearDisplay();
}

//void displayOnLCD(float data1, float data2, float data3){
void displayOnLCD(float data1, float data2){
  display.clearDisplay();
  display.setTextSize(1.5);
  display.setTextColor(WHITE);
  display.setCursor(10, 5);
  // Display static text
  display.println("HASIL PENGUKURAN :");
  // Draw a line below the "Nilai Parameter" text
  display.drawLine(0, 15, 128, 15, WHITE);  // Membuat garis horizontal pada y = 18

  display.setTextSize(2);
  display.setCursor(0,20);
  display.print("Temp:");
  display.print(data1,1);
  display.println("C");

  display.setCursor(0,40);
  display.print("Hum :");
  display.print(data2,1);
  display.println("%");

 // display.setCursor(10, 40);
 // display.print("CO2 : ");
 // display.print(data3,0);
 // display.println(" PPM");

  display.display();
}

//void displayOnSerialMonitor(float data1, float data2, float data3){
void displayOnSerialMonitor(float data1, float data2){
  Serial.print("Temp: " + String(data1) + " C" + "\t");
  Serial.print("Hum : " + String(data2) + " %" + "\t");
  //Serial.print("CO2 : " + String(data3) + " PPM" + "\t\n");
     
}


//void sendDataToThingSpeak(float data1,float data2,float data3){
// void sendDataToThingSpeak(float data1,float data2){
//   HTTPClient http; // Inisialisasi HTTP CLient

//  // String url = serverName + APIKey + "&field1=" + String(data1) + "&field2=" + String(data2) + "&field3=" + String(data3);
//   String url = serverName + APIKey + "&field1=" + String(data1) + "&field2=" + String(data2);

//    http.begin(url.c_str()); // Send HTTP Reques / Inisialisasi
//   int httpResponseCode = http.GET(); // Check status data

//   Serial.println("Mengirim data ke Thingspeak");
//   if (httpResponseCode > 0){
//     Serial.print("HTTP Response code:");
//     Serial.println(httpResponseCode);
//   }
//   else{
//      Serial.print("Error COde");
//      Serial.println(httpResponseCode);
//   }
//   http.end();
// }
