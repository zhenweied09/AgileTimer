#include <FS.h>                   //this needs to be first, or it all crashes and burns...
#include <ESP8266WiFi.h>          //https://github.com/esp8266/Arduino
#include <DNSServer.h>            //Local DNS Server used for redirecting all requests to the configuration portal
#include <ESP8266WebServer.h>     //Local WebServer used to serve the configuration portal
#include <WiFiManager.h>          //https://github.com/tzapu/WiFiManager WiFi Configuration Magic
#include <ArduinoJson.h>          //https://github.com/bblanchon/ArduinoJson

#define PIN_TRIGGER_BUTTON D5
#define PIN_CONFIG_BUTTON D6
#define PIN_ONBOARD_LED   16

char* apName = "AgileTimer-Setup";
char* apPassword = "12345678";

char mqtt_server[40] = "";
char mqtt_port[6] = "80";

bool shouldSaveConfig = false; 

const long MIN_RESPONSE_TIME = 500;
int lastTriggerButtonStatus = HIGH;
long lastTriggerResponseTime = 0;
int lastConfigButtonStatus = HIGH;
long lastConfigResponseTime = 0;

void setup(){
    Serial.begin(115200);
    Serial.println("------------SETUP START-------------");
    
    pinMode(PIN_TRIGGER_BUTTON,INPUT_PULLUP);
    pinMode(PIN_CONFIG_BUTTON,INPUT_PULLUP);
    
    pinMode(PIN_ONBOARD_LED, OUTPUT);
    digitalWrite(PIN_ONBOARD_LED, HIGH);

    readConfig();
    configWifiManager(false);
    
    Serial.println("------------SETUP FINISH-------------");
    Serial.println();
}

void loop(){
  
    byte currentTriggerButtonStatus = digitalRead(PIN_TRIGGER_BUTTON);
    if(currentTriggerButtonStatus==LOW){      
        long triggerGap = millis() - lastTriggerResponseTime;
        
        if(lastTriggerButtonStatus == HIGH && triggerGap > MIN_RESPONSE_TIME){
          digitalWrite(PIN_ONBOARD_LED, LOW);
        
          String request = 
            String("GET /api/timer/toggle HTTP/1.1\r\n") +
            "User-Agent: ESP8266/1.0\r\n" +
            "Accept: */*\r\n" +
            "Host: " + mqtt_server + ":"+ mqtt_port +"\r\n" +
            "Connection: close\r\n" +
            "\r\n";
          
           Serial.println("Trying to deliver request:");
           Serial.println(request);
        
          WiFiClient wifiClient;
          int result = wifiClient.connect(mqtt_server,atoi(mqtt_port));
          if (result) {
            Serial.println("Server connect succeed.");                 
            wifiClient.print(request);
            wifiClient.flush();
            wifiClient.stop();
          }
          else{
            Serial.println("Server connect failed."); 
          }

          lastTriggerResponseTime = millis();
        }
    }else{
      digitalWrite(PIN_ONBOARD_LED, HIGH);
    }


    byte currentConfigButtonStatus = digitalRead(PIN_CONFIG_BUTTON);
    if(currentConfigButtonStatus==LOW){      
        long configGap = millis() - lastConfigResponseTime;
        
        if(lastConfigButtonStatus == HIGH && configGap > MIN_RESPONSE_TIME){
          digitalWrite(PIN_ONBOARD_LED, LOW);
          configWifiManager(true);
          lastConfigResponseTime = millis();
          delay(1000);
        }
    }else {
      digitalWrite(PIN_ONBOARD_LED, HIGH);
    }

    lastTriggerButtonStatus = currentTriggerButtonStatus;
    lastConfigButtonStatus = currentConfigButtonStatus;
    delay(200);
}

void configWifiManager(bool isOnDemandConfig)
{
    WiFiManager wifiManager;  
    wifiManager.setAPCallback(configModeCallback);
    wifiManager.setAPStaticIPConfig(IPAddress(192,168,31,1), IPAddress(192,168,31,1), IPAddress(255,255,255,0));
    wifiManager.setSaveConfigCallback(saveConfigCallback);       
    
    //add all your parameters here
    WiFiManagerParameter custom_mqtt_server("server", "Server Address", mqtt_server, 40);
    WiFiManagerParameter custom_mqtt_port("port", "Port", mqtt_port, 6);
    wifiManager.addParameter(&custom_mqtt_server);
    wifiManager.addParameter(&custom_mqtt_port);
    wifiManager.setTimeout(20);

    bool isConnected = false;
    if(!isOnDemandConfig){
      isConnected = wifiManager.autoConnect(apName, apPassword);
    }else{
      isConnected = wifiManager.startConfigPortal(apName, apPassword);
    }
    
    if(!isConnected){
      Serial.println("Wifi connection failed.");
      delay(500);
      //reset and try again, or maybe put it to deep sleep
      ESP.restart();
      delay(5000);  
    }else{
      Serial.println("Wifi has connected.");
    }
    
    //read updated parameters
    strcpy(mqtt_server, custom_mqtt_server.getValue());
    strcpy(mqtt_port, custom_mqtt_port.getValue());

    //save the custom parameters to FS
    if (shouldSaveConfig) {
      saveConfig();
    }
}

void configModeCallback(WiFiManager *wifiManager)
{
   Serial.println("Entered config mode");
   Serial.println(WiFi.softAPIP());
   
   //You could indicate on your screen or by an LED you are in config mode here 
}

//callback notifying us of the need to save config
void saveConfigCallback () {
  Serial.println("Should save config");
  shouldSaveConfig = true;
}

void readConfig()
{
    //read configuration from FS json
    Serial.println("mounting FS...");

    if (!SPIFFS.begin()) {
      Serial.println("failed to mount FS");
      return;
    }
      
    Serial.println("mounted file system");
      
    if (SPIFFS.exists("/config.json")) {
      Serial.println("file not exists!");
      return;
    } 
        
    //file exists, reading and loading
    Serial.println("reading config file");
        
    File configFile = SPIFFS.open("/config.json", "r");
    if (configFile) {    
      Serial.println("opened config file");
      size_t size = configFile.size();
      
      // Allocate a buffer to store contents of the file.
      std::unique_ptr<char[]> buf(new char[size]);

      configFile.readBytes(buf.get(), size);
      DynamicJsonDocument doc(200);
      DeserializationError error = deserializeJson(doc, buf.get());
      if(error)
      {
         Serial.print(F("deserializeJson() failed: "));
         Serial.println(error.c_str());
         return;
      }

      serializeJson(doc,Serial);

      Serial.println("\nparsed json");
      strcpy(mqtt_server, doc["mqtt_server"]);
      strcpy(mqtt_port, doc["mqtt_port"]);
      configFile.close();
    }    
}

void saveConfig()
{
      Serial.println("saving config");
      
      DynamicJsonDocument doc(200);
      doc["mqtt_server"] = mqtt_server;
      doc["mqtt_port"] = mqtt_port;

      File configFile = SPIFFS.open("/config.json", "w");
      if (!configFile) {
        Serial.println("failed to open config file for writing");
      }

      serializeJson(doc,Serial);
      serializeJson(doc,configFile);
      configFile.close();
}
