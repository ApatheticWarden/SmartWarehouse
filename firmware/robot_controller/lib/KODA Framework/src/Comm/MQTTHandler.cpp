#include <Comm/MQTTHandler.h>

void MQTTHandler::eventTrampoline(void* arg, esp_event_base_t base, int32_t event_id, void* data){
        MQTTHandler* handler = static_cast<MQTTHandler*>(arg);
        handler->onEvent(static_cast<esp_mqtt_event_handle_t>(data));
}

void MQTTHandler::onEvent(esp_mqtt_event_handle_t event)
{
    Serial.print("MQTT EVENT: ");
    Serial.println(event->event_id);

    switch (event->event_id)
    {
        case MQTT_EVENT_CONNECTED:
            Serial.println("MQTT CONNECTED!");
            break;

        case MQTT_EVENT_DISCONNECTED:
            Serial.println("MQTT DISCONNECTED!");
            break;

        case MQTT_EVENT_ERROR:
            Serial.println("MQTT ERROR!");
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

bool MQTTHandler::Connect(){
    esp_mqtt_client_config_t mqtt_cfg = {
        .uri = _cfg.mqtt_server(),
        .port = _cfg.mqtt_port()
    };

    _client = esp_mqtt_client_init(&mqtt_cfg);

    if (_client == nullptr) return false;

    esp_err_t err = esp_mqtt_client_register_event(
        _client,
        MQTT_EVENT_ANY,
        eventTrampoline,
        this
    );

    if (err != ESP_OK) return false;

    err = esp_mqtt_client_start(_client);

    return err == ESP_OK;
}