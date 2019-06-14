#include<ESP8266WiFi.h>
#include<ESP8266WebServer.h>

#define IPADDRESS "---"

WiFiClient wifiClient;
ESP8266WebServer server(80);
char* ssid="---";
char* password="---";
byte buttonState = HIGH;

void setup(){
    pinMode(D5,INPUT_PULLUP);
    pinMode(16, OUTPUT);
    
    WiFi.begin(ssid,password);
    Serial.begin(115200);
    while(WiFi.status()!=WL_CONNECTED)
    {
        Serial.print(".");
        delay(500);
    }

    Serial.print("");
    Serial.println("IP Address:");
    Serial.println(WiFi.localIP());

    server.on("/", helloWorld);
    server.on("/toggleled", toggleLED);
    server.begin();
}

void loop(){
    server.handleClient();
    byte currentButtonState = digitalRead(D5);
    if(currentButtonState!=buttonState)
    {
        digitalWrite(16, !digitalRead(16));
        buttonState = currentButtonState;

        if (wifiClient.connect(IPADDRESS, 56210)) {
          Serial.println("connected");

          const char request[] = 
          "GET /api/timer/toggle HTTP/1.1\r\n" 
          "User-Agent: ESP8266/1.0\r\n"
          "Accept: */*\r\n"
          "Host: " IPADDRESS ":56210\r\n"
          "Connection: close\r\n"
          "\r\n";
          
          Serial.println(request);                        
          wifiClient.print(request);
          wifiClient.flush();
          wifiClient.stop();
      }
    }
}

void helloWorld()
{
  server.send(200,"text/plain","Hello User From ESP8266!");
}

void toggleLED()
{
    digitalWrite(16, !digitalRead(16));
    server.send(204,"");
}
