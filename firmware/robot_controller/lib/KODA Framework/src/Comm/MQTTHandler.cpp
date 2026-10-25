#include <Comm/MQTTHandler.h>
#include "esp_log.h"

static const char* TAG = "MQTT";

void MQTTHandler::EventHandler(void* arg, esp_event_base_t base, int32_t event_id, void* data){
        MQTTHandler* handler = static_cast<MQTTHandler*>(arg);
        handler->handleEvent(static_cast<esp_mqtt_event_handle_t>(data));
}

void MQTTHandler::handleEvent(esp_mqtt_event_handle_t event)
{
    switch (event->event_id)
    {
        case MQTT_EVENT_CONNECTED:
            ESP_LOGI(TAG, "CONNECTED");
            _connected = true;
            esp_mqtt_client_subscribe(_client, _cfg.telemetry(), 1);
            // Post(_cfg.telemetry(), "{\"robotId\":1,\"stance\":1,\"battery\":80}", 2, 0);
            break;

        case MQTT_EVENT_DISCONNECTED:
            _connected = false;
            break;

        case MQTT_EVENT_ERROR:
            
            break;

        default:
            break;
    }
}

void MQTTHandler::Disconnect(){
    if (_client == nullptr) return;

    esp_mqtt_client_stop(_client);
    esp_mqtt_client_destroy(_client);

    _client = nullptr;
}

int MQTTHandler::Post(const char* topic, const char* message, int qos, int retain)
{
    if (_client == nullptr)
        return -1;

    return esp_mqtt_client_publish(
        _client,
        topic,
        message,
        0,
        qos,
        retain
    );
}

esp_err_t MQTTHandler::Connect(){
    esp_mqtt_client_config_t mqtt_cfg = {};
    mqtt_cfg.broker.address.uri  = _cfg.mqtt_server();
    mqtt_cfg.broker.address.port = _cfg.mqtt_port();

    _client = esp_mqtt_client_init(&mqtt_cfg);

    if (_client == nullptr) return ESP_ERR_NOT_FINISHED;

    esp_err_t err = esp_mqtt_client_register_event(
        _client,
        MQTT_EVENT_ANY,
        EventHandler,
        this
    );

    if (err != ESP_OK) return err;

    err = esp_mqtt_client_start(_client);

    return err;
}