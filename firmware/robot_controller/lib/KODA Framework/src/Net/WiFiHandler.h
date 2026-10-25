#ifndef WIFI_HANDLER
#define WIFI_HANDLER

#include <string>
#include <atomic>
#include "esp_err.h"
#include "esp_event.h"
#include "esp_netif.h"

struct WiFiConfig {
    WiFiConfig(const char* ssid, const char* passwd) : _ssid(ssid), _passwd(passwd) {}
    const std::string& Ssid() const { return _ssid; }
    const std::string& Passwd() const { return _passwd; }
private:
    std::string _ssid;
    std::string _passwd;
};

class WiFiHandler {
    static constexpr const char* TAG = "WiFiHandler";

    WiFiConfig _cfg;
    std::atomic<bool> _connected{false};
    std::atomic<bool> _stopping{false};
    bool _initialized = false;

    esp_netif_t* _netif = nullptr;
    esp_event_handler_instance_t _wifiInst = nullptr;
    esp_event_handler_instance_t _ipInst   = nullptr;

    static void EventHandler(void* arg, esp_event_base_t event_base, int32_t event_id, void* data);
    void handleEvent(esp_event_base_t event_base, int32_t event_id, void* data);

public:
    explicit WiFiHandler(const WiFiConfig& cfg) : _cfg(cfg) {}
    ~WiFiHandler();

    WiFiHandler(const WiFiHandler&) = delete;
    WiFiHandler& operator=(const WiFiHandler&) = delete;

    esp_err_t Connect();
    void Stop();
    bool IsConnected() const { return _connected; }
};

#endif