#include<iostream>
#include<Utils/Task.h>

Task::Task() : taskHandle(nullptr) {}
Task::~Task(){ Delete(); }

// This is C-like thingy
void Task::Start(const std::string& taskName, uint16_t stackSize, UBaseType_t priority, BaseType_t coreId){
    // "Exception"
    if(taskHandle != nullptr) return;
    
    xTaskCreatePinnedToCore(
        TaskAdapter, // what to do
        taskName.c_str(), // name
        stackSize,   // stack size
        this,   // object pointer
        priority,      // Priority
        &taskHandle, // make freertos know how to access task
        coreId       // which core
    );
}

void Task::Pause(){
    if (taskHandle != nullptr) {
        vTaskSuspend(taskHandle);
    }
}

void Task::Resume(){
    if(taskHandle != nullptr){
        vTaskResume(taskHandle);
    }
}

void Task::Delete(){
    if(taskHandle != nullptr){
        vTaskDelete(taskHandle);
        taskHandle = NULL;
    }
}

void Task::TaskAdapter(void* pvParameters){
    // pvParameters is a nameless void* holding the 'this' pointer, which FreeRTOS returns back to us
    // Extract object adress from it
    Task* instance = static_cast<Task*>(pvParameters);
    
    // Do tha thing with our object
    instance->Run();
}
