#ifndef MQTT_HANDLER
#include <mqtt_client.h>
#include <WiFi.h>
#define MQTT_HANDLER

// This is for automating mqtt 
struct MQTTConfig{
    MQTTConfig(const char* ssid, const char* passwd, const char* mqtt_server, 
        const int mqtt_port, const char* device_uid, const char* topic_telemetry, const char* topic_tasks):_ssid(ssid),_passwd(passwd),
        _mqtt_server(mqtt_server),_mqtt_port(mqtt_port),_device_uid(device_uid),_topic_telemetry(topic_telemetry),_topic_tasks(topic_tasks){}

    const char* ssid() const {return _ssid;}
    const char* passwd() const {return _passwd;}
    const char* mqtt_server() const {return _mqtt_server;}
    int mqtt_port() const {return _mqtt_port;}
    const char* device_uid() const {return _device_uid;}
    const char* telemetry() const {return _topic_telemetry;}
    const char* tasks() const {return _topic_tasks;}

    private:
        // wi-fi
        const char* _ssid;
        const char* _passwd;
        
        // server config
        const char* _mqtt_server;
        const int _mqtt_port;
        
        // device recognition
        const char* _device_uid;
        
        // topics
        const char* _topic_telemetry;
        const char* _topic_tasks;
};

// main "thing"
class MQTTHandler{
        MQTTConfig _cfg;
    public:
        MQTTHandler(const MQTTConfig& cfg) : _cfg(cfg) {}

        bool connect() {
            esp_mqtt_client_config_t mqtt_cfg  = {};
            mqtt_cfg.uri  = _cfg.mqtt_server()+_cfg.mqtt_port();

        }

        // handleIncomingMessage
        // disconnect
        // post

};

#endif