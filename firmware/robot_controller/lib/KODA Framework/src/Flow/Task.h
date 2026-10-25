#ifndef TASK_H
#include <freertos/FreeRTOS.h>
#include <freertos/task.h>
#include <functional>
#include <string>
#define TASK_H

class Task{
    TaskHandle_t taskHandle = nullptr; // reference
    std::function<void()> _fn; 

    // Static adapter-method (FRTOS see this as C-func)
    static void TaskAdapter(void* pvParameters);

    virtual void Run() { if (_fn) _fn(); }

    public:
        Task() = default;

        explicit Task(std::function<void()> fn):_fn(std::move(fn)){}
        // Some FAQ:
        // fn is a temporary std::function wrapper around whatever was PASSED.
        // std::move lets us steal its CONTENTS into _fn instead of copying them;
        // the original callable (if it was a named function) is UNTOUCHED.

        virtual ~Task();
        TaskHandle_t GetHandle() const;
        void Start(const std::string& taskName, uint16_t stackSize = 2048, UBaseType_t priority = 1, BaseType_t coreId = 1);
        void Delete();
        void Pause();
        void Resume();
};

#endif