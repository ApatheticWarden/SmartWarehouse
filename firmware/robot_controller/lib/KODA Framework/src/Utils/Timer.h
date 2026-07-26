#include <freertos/FreeRTOS.h>
#include <freertos/task.h>
#include <atomic>
#include <Utils/Task.h>
#include <functional>

#ifndef TIMER_H
#define TIMER_H

class Timer: public Task{
    std::atomic<long> seconds;      
    // as ai says: volatile doesnt grant thread durability
    // So we should use atomic
    // P.S. It should sync our cores 
    std::atomic<long> secondsToStop;

    std::function<void()> onTickCallback = nullptr;

    std::atomic<long> callbackInterval{0}; 
    std::atomic<long> callbackCounter{0};

    void Run() override;
    public:
        Timer();
        long GetSeconds();
        float GetMinutes();
        float GetHours();
        void SetStopTime(long time);
        void SetTime(long time);
        void Stop();
        void SetOnTick(std::function<void()> callback, long intervalSeconds);
};

#endif