#include <Net/WiFiHandler.h>
#include <cstring>
#include "esp_log.h"
#include "esp_wifi.h"
#include "nvs_flash.h"

// > Passes the ball for handleEvent
void WiFiHandler::EventHandler(void* arg, esp_event_base_t event_base, int32_t event_id, void* data) {
    static_cast<WiFiHandler*>(arg)->handleEvent(event_base, event_id, data);
}

// > Catches it, and inspects
void WiFiHandler::handleEvent(esp_event_base_t event_base, int32_t event_id, void* data) {
    // Abt WIFI
    if (event_base == WIFI_EVENT) {
        switch (event_id) {
            // If starting -> use st. IDF connect 
            case WIFI_EVENT_STA_START:
                esp_wifi_connect();
                break;
            // on connect
            case WIFI_EVENT_STA_CONNECTED: {
                // get the data
                // Remark: we have to cast this, because of C. "data" var is pointer on event payload
                // in handler reads but does not save it somewhere
                // Payload depends on event_id, in C is equal to void* 
                auto* ev = static_cast<wifi_event_sta_connected_t*>(data);
                ESP_LOGI(TAG, "Connected to AP: %.*s, ch %d",
                         ev->ssid_len, (const char*)ev->ssid, ev->channel);
                break;
            }
            // on disconnect
            case WIFI_EVENT_STA_DISCONNECTED: {
                _connected = false;
                if (_stopping) break;   // if was on demand
                // get data again
                auto* ev = static_cast<wifi_event_sta_disconnected_t*>(data);
                ESP_LOGW(TAG, "Disconnected, reason %d, reconnecting...", ev->reason);
                // recon
                esp_wifi_connect();
                break;
            }

            default:
                break;
        }
    }
    // Abt IP stuff
    // here is only when obtained, but still can expand
    else if (event_base == IP_EVENT && event_id == IP_EVENT_STA_GOT_IP) {
        auto* ev = static_cast<ip_event_got_ip_t*>(data);
        ESP_LOGI(TAG, "Got IP: " IPSTR, IP2STR(&ev->ip_info.ip));
        _connected = true;
    }
}

// The "meat" -> Does the "dirty deeds"
esp_err_t WiFiHandler::Connect() {
    if (!_initialized) {
        esp_err_t err = nvs_flash_init(); // init space for WiFi driver
        // If our storage (NVS) has no free space or has different data stored 
        if (err == ESP_ERR_NVS_NO_FREE_PAGES || err == ESP_ERR_NVS_NEW_VERSION_FOUND) {
            ESP_ERROR_CHECK(nvs_flash_erase());
            err = nvs_flash_init();
        }
        // Tests
        ESP_ERROR_CHECK(err);

        ESP_ERROR_CHECK(esp_netif_init());
        ESP_ERROR_CHECK(esp_event_loop_create_default());
        
        // Interface in mode STATION (connect to Access Point, further AP)
        _netif = esp_netif_create_default_wifi_sta();

        // Load config
        wifi_init_config_t init = WIFI_INIT_CONFIG_DEFAULT();
        ESP_ERROR_CHECK(esp_wifi_init(&init));

        ESP_ERROR_CHECK(esp_event_handler_instance_register(
            WIFI_EVENT, ESP_EVENT_ANY_ID, &WiFiHandler::EventHandler, this, &_wifiInst));
        ESP_ERROR_CHECK(esp_event_handler_instance_register(
            IP_EVENT, IP_EVENT_STA_GOT_IP, &WiFiHandler::EventHandler, this, &_ipInst));

        // create some space, then fulfill 
        wifi_config_t wc = {};
        // quick reminder: char * strcpy( char * dest, const char * src );
        strncpy((char*)wc.sta.ssid, _cfg.Ssid().c_str(), sizeof(wc.sta.ssid) - 1);
        strncpy((char*)wc.sta.password, _cfg.Passwd().c_str(), sizeof(wc.sta.password) - 1);
        // Authmode is type of protection. Like WPA2/WPA3 or open (no pass)
        wc.sta.threshold.authmode = _cfg.Passwd().empty() ? WIFI_AUTH_OPEN : WIFI_AUTH_WPA2_PSK;

        ESP_ERROR_CHECK(esp_wifi_set_mode(WIFI_MODE_STA));
        ESP_ERROR_CHECK(esp_wifi_set_config(WIFI_IF_STA, &wc));
        _initialized = true;
    }
    _stopping = false;
    // actual start of "module"
    return esp_wifi_start();
}

void WiFiHandler::Stop() {
    if (!_initialized) return;
    _stopping = true;
    esp_wifi_stop();
    _connected = false;
}

WiFiHandler::~WiFiHandler() {
    if (!_initialized) return;
    _stopping = true;
    // REMEMBER TO UNREGISTER ALL THIS SHIT 
    esp_event_handler_instance_unregister(WIFI_EVENT, ESP_EVENT_ANY_ID, _wifiInst);
    esp_event_handler_instance_unregister(IP_EVENT, IP_EVENT_STA_GOT_IP, _ipInst);
    esp_wifi_stop();
    esp_wifi_deinit();
    esp_netif_destroy_default_wifi(_netif);
    // IM SERIOUS
}