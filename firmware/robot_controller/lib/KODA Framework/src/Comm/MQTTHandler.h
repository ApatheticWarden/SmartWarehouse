#ifndef MQTT_HANDLER
#include <mqtt_client.h>
#include <Arduino.h>
#define MQTT_HANDLER

// This is for automating mqtt 
struct MQTTConfig{
    MQTTConfig(const char* mqtt_server, 
        const uint32_t mqtt_port, const char* device_uid, const char* topic_telemetry, const char* topic_tasks):
        _mqtt_server(mqtt_server),_mqtt_port(mqtt_port),_device_uid(device_uid),_topic_telemetry(topic_telemetry),_topic_tasks(topic_tasks){}

    const char* mqtt_server() const {return _mqtt_server;}
    uint32_t mqtt_port() const {return _mqtt_port;}
    const char* device_uid() const {return _device_uid;}
    const char* telemetry() const {return _topic_telemetry;}
    const char* tasks() const {return _topic_tasks;}

    private:
        // server config
        const char* _mqtt_server;
        const uint32_t _mqtt_port;
        
        // device recognition
        const char* _device_uid;
        
        // topics
        const char* _topic_telemetry;
        const char* _topic_tasks;
};

// main "thing"
class MQTTHandler{
        MQTTConfig _cfg;
        esp_mqtt_client_handle_t _client = nullptr;

        static void eventTrampoline(
            void* arg,
            esp_event_base_t base,
            int32_t event_id,
            void* data
        );

        void onEvent(esp_mqtt_event_handle_t event);
    public:
        explicit MQTTHandler(const MQTTConfig& cfg) : _cfg(cfg) {}

        // Connect
        bool Connect();
        // Handle all events
        void HandleEvents(esp_mqtt_event_handle_t event);
        // disconnect
        void Disconnect();
        // post
        int Post(const char* topic, const char* message, int qos, int retain);
        // qos - Quaity of Service. What should we do: post several times or "shot once" (0 - forget ,1 - at least once, 2 - = once)
        // retain - what brocker would do after publish. 0 - basic, only for subs / 1 - for ev1 who will sub further

};

#endif