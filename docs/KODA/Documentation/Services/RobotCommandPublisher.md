> [!abstract]  
> `IRobotCommandPublisher` and `MqttRobotCommandPublisher` define the outbound transport boundary used to send task commands from server to robot over MQTT.  
  
## Purpose  
This boundary exists so that orchestration logic does not need to know MQTT transport details directly.  
  
`DeliveryService` decides:  
- what to send  
- when to send  
- which robot receives it  
  
The publisher decides:  
- how to serialize the payload  
- which topic to use  
- how to publish it over MQTT  
  
## Current role  
The current V1 publisher is responsible for:  
- serializing outbound task command payloads  
- building the robot-specific MQTT topic  
- publishing the command to the broker  
  
## Current topic  
Current outbound task command topic:  
- `smartwh/robots/{robotId}/task/command`  
  
This topic is separate from inbound robot update topics.  
  
## Current V1 payload  
The current V1 outbound payload includes:  
- `taskId`  
- `robotId`  
- `command`  
- `startNodeId`  
- `endNodeId`  
  
This gives the robot the minimum information needed to identify and execute the assigned task.  
  
## Why it exists  
Without this boundary, `DeliveryService` would have to know:  
- MQTT topic naming  
- MQTT client usage  
- JSON serialization details  
- outbound publish mechanics  
  
That would mix orchestration logic with transport execution details.  
  
## What it should not own  
The publisher should not:  
- decide task creation  
- decide robot selection  
- own delivery orchestration  
- interpret task completion  
- manage task lifecycle state  
  
It is a transport helper, not a workflow owner.  
  
## Current V1 limitation  
The V1 implementation may create a fresh MQTT client per publish call.  
  
This is acceptable as a first working version, but it is not the final intended performance model.  
  
Later improvements may include:  
- connection reuse  
- shared outbound MQTT lifecycle management  
- better separation between inbound and outbound MQTT infrastructure  
  
## Related notes  
  
- [[Service Boundaries]]  
- [[MQTT Topic Flow]]  
- [[MQTT Background Service Architecture]]  
- [[Services/DeliveryService]]  
- [[Server and Firmware Boundary]]