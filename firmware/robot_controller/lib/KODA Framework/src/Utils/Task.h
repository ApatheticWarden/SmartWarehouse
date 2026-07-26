#include <freertos/FreeRTOS.h>
#include <freertos/task.h>
#include <atomic>

#ifndef TASK_H
#define TASK_H

class Task{
    TaskHandle_t taskHandle = nullptr; // reference

    // Static adapter-method (FRTOS see this as C-func)
    static void TaskAdapter(void* pvParameters);

    // Definition what to do (in class)
    virtual void Run() = 0;
    public:
        Task();
        virtual ~Task();
        void Start(const std::string& taskName, uint16_t stackSize = 2048, UBaseType_t priority = 1, BaseType_t coreId = 1);
        void Delete();
        void Pause();
        void Resume();
};

#endif