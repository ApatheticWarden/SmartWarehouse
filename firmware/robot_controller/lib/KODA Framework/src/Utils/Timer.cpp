#include<Utils/Timer.h>

Timer::Timer():seconds(0),secondsToStop(0){}
long Timer::GetSeconds(){
    return seconds.load(std::memory_order_relaxed);
}   

float Timer::GetMinutes(){
    return seconds.load(std::memory_order_relaxed)/60.0f;
}

float Timer::GetHours(){
    return seconds.load(std::memory_order_relaxed)/3600.0f;
}

void Timer::Run(){
    while(true){
        long currentSeconds = seconds.load(std::memory_order_relaxed);
        long stopTime = secondsToStop.load(std::memory_order_relaxed);
        long interval = callbackInterval.load(std::memory_order_relaxed);

        if(stopTime > 0 && currentSeconds >= stopTime){
            SetStopTime(0);
            Pause(); 
        }
        
        seconds.fetch_add(1, std::memory_order_relaxed);
        
        if (onTickCallback != nullptr && interval > 0) {    // onTickCallback some function that we pass through
            long currentCount = callbackCounter.fetch_add(1, std::memory_order_relaxed) + 1; 
            // basicaly will make juice from your brain, but
            // we combine 3 steps into one, to guarantee that only WE use that "box". And storing old value, so we add 1 localy to achieve new val
            // at least I`ve tried
            if (currentCount >= interval) {
                onTickCallback();   // using this passed function
                callbackCounter.store(0, std::memory_order_relaxed); // zero our flag. Could i have this changed? Yes. How? Dunno. Do I want? Nope
            }
        }

        vTaskDelay(pdMS_TO_TICKS(1000));
    }
}

void Timer::SetTime(long time){
    seconds.store(time, std::memory_order_relaxed);
}

void Timer::SetStopTime(long time){
    secondsToStop.store(time, std::memory_order_relaxed);
}

void Timer::SetOnTick(std::function<void()> callback, long intervalSeconds = 1) {
    Pause(); 
    onTickCallback = callback;
    callbackInterval.store(intervalSeconds, std::memory_order_relaxed);
    callbackCounter.store(0, std::memory_order_relaxed);
    Resume();
}

void Timer::Stop(){
    SetTime(0);
    SetStopTime(0);
    Pause();
}
