> [!abstract]  
> MQTT topic flow defines how robot-originated messages enter the server, how they are classified, how task and telemetry updates are processed, and how outbound robot task commands are published.  
> In the current V1 implementation, MQTT is used for:  
> - inbound task update handling  
> - inbound telemetry handling  
> - outbound task command publishing  
  
## Purpose  
  
This note describes the server-side flow of MQTT messages.  
  
It is responsible for explaining:  
- which topics the server listens to  
- how incoming messages are classified  
- how message types are separated  
- how messages move from transport into processing  
- how outbound robot commands are published  
- how reconnect and re-subscribe behavior is handled  
  
## Role in the server  
  
MQTT is the runtime communication entry path for robot-side events and the outbound transport path for robot task commands.  
  
From the server perspective, MQTT is currently used to:  
- receive task update messages  
- receive telemetry messages  
- publish outbound task commands to robots  
  
This allows the backend to both react to live robot behavior and initiate robot execution flow.  
  
## Current subscribed topics  
  
The current MQTT background service subscribes to:  
  
| Topic                          | Meaning                      |     |
| :----------------------------- | :--------------------------- | --- |
| `smartwh/robots/+/telemetry`   | live robot telemetry updates |     |
| `smartwh/robots/+/task/update` | task-related robot updates   |     |
  
The `+` segment means the server listens for messages from multiple robots under the same topic structure.  
  
The server currently subscribes only to inbound runtime topics.  
  
Outbound robot task commands are published separately and are not part of the subscription list.  
  
## Current outbound publish topic  
  
The current outbound robot task command topic is:  
  
| Topic | Meaning |  
| :--- | :--- |  
| `smartwh/robots/{robotId}/task/command` | outbound server-to-robot task command |  
  
This topic is used after `DeliveryService` successfully starts delivery orchestration.  
  
## Topic categories  
  
| Category      | Topic pattern                           | Purpose                                           | Processing priority |     |
| :------------ | :-------------------------------------- | :------------------------------------------------ | :------------------ | --- |
| Telemetry     | `smartwh/robots/+/telemetry`            | runtime robot state updates                       | medium              |     |
| Task updates  | `smartwh/robots/+/task/update`          | task execution events and task lifecycle progress | high                |     |
| Task commands | `smartwh/robots/{robotId}/task/command` | outbound task command from server to robot        | high                |     |
  
The topic split reflects that not all MQTT messages have the same backend importance.  
  
Task commands are published by the server and are not consumed through the inbound worker routing path.  
  
## Message routing  
  
Incoming MQTT messages are first handled by the MQTT client receive callback.  
  
The callback does not perform heavy processing directly.  
Instead, it inspects the topic and routes the message to the appropriate in-memory channel.  
  
### Current routing rules  
  
| Topic ending | Destination |  
| :--- | :--- |  
| `task/update` | task channel |  
| `telemetry` | telemetry channel |  
| anything else | warning / unhandled topic |  
  
This keeps the receive path lightweight and avoids blocking the MQTT client.  
  
## Processing path  
  
The current inbound server-side path is:  
  
1. robot publishes message to MQTT broker  
2. `MqttBackgroundService` receives the message  
3. topic is inspected  
4. message is routed into the appropriate channel  
5. background worker loop reads from that channel  
6. the worker deserializes a DTO  
7. a scoped service is resolved  
8. the service applies the update to runtime or persistent state  
  
This separates:  
- transport reception  
- message classification  
- contract deserialization  
- business processing  
  
## Outbound task command path  
  
The current outbound server-side path is:  
  
1. client starts a delivery request through HTTP  
2. `DeliveryController` calls `DeliveryService`  
3. `DeliveryService` resolves package, rack, robot, and task  
4. `DeliveryService` calls `IRobotCommandPublisher`  
5. the command payload is serialized  
6. the command is published to `smartwh/robots/{robotId}/task/command`  
  
This keeps outbound command transport separate from inbound runtime event handling.  
  
## V1 task update flow  
  
Task updates are currently deserialized into `TaskAnswerDTO`.  
  
The worker branches on `RobotTaskStatus`.  
  
### Robot task statuses  
  
| Robot task status | Meaning | Current server reaction |  
| :--- | :--- | :--- |  
| `Accepted` | robot acknowledged the task | log runtime handshake only |  
| `Started` | robot began execution | mark task as `InProgress` |  
| `Completed` | robot finished execution | mark task as `Completed`, set completion time, unassign robot |  
| `Failed` | robot failed to complete execution | mark task as `Failed`, log failure message |  
  
### Important distinction  
  
`RobotTaskStatus` is not the same as `TaskStatus`.  
  
- `RobotTaskStatus` describes what the robot reports to the server  
- `TaskStatus` describes the server-side persistent state of the task  
  
This keeps transport events separate from persistent workflow state.  
  
## V1 telemetry flow  
  
Telemetry messages are currently deserialized into `RobotTelemetryDTO`.  
  
The worker dispatches telemetry handling to `TelemetryService`.  
  
The telemetry flow currently:  
- validates/deserializes the DTO  
- updates `RobotBuffer`  
- updates `RobotServerState`  
- logs first-seen robot registration in the runtime buffer  
  
Telemetry currently affects runtime state only.  
  
It does not currently:  
- write telemetry to the database  
- perform route checking  
- perform pathfinding  
- make orchestration decisions  
  
## V1 outbound task command flow  
  
Outbound robot task commands are currently built from a structured DTO and published after delivery-start orchestration succeeds.  
  
The current payload contains:  
- `taskId`  
- `robotId`  
- `command`  
- `startNodeId`  
- `endNodeId`  
  
This gives the robot the minimum execution data needed to identify and execute the assigned task.  
  
## Task-related MQTT direction split  
  
Task-related MQTT communication currently has two directions:  
  
| Direction | Topic pattern | Meaning |  
| :--- | :--- | :--- |  
| server -> robot | `smartwh/robots/{robotId}/task/command` | outbound command to execute task |  
| robot -> server | `smartwh/robots/{robotId}/task/update` | task lifecycle update and execution feedback |  
  
This split is important because command publication and task update processing belong to different backend responsibilities.  
  
## Channel split  
  
The current implementation uses separate channels for different message types.  
  
| Channel | Message type | Buffer behavior | Reason |  
| :--- | :--- | :--- | :--- |  
| task channel | task-related updates | waits when full | task updates should not be dropped casually |  
| telemetry channel | live telemetry | drops oldest when full | fresh telemetry is more valuable than stale telemetry |  
  
This is an important architectural choice.  
  
Task updates affect workflow correctness.  
Telemetry is treated as high-frequency runtime data that may lose value quickly.  
  
## Reconnect behavior  
  
MQTT connection handling runs in a dedicated loop.  
  
The server behavior is:  
1. check whether the client is connected  
2. if disconnected, attempt reconnect  
3. after reconnect, subscribe again to required topics  
4. continue monitoring connection state  
  
This allows the backend to recover from temporary network or broker failures without requiring a full server restart.  
  
## Current implementation observations  
  
The current branch already establishes:  
- broker connection handling  
- topic subscription  
- topic-based routing  
- separated processing channels  
- task update DTO handling  
- telemetry DTO handling  
- outbound robot task command publication  
- separate outbound publisher boundary  
- scoped service resolution per message  
  
What is still evolving:  
- retry and recovery policy for failed tasks  
- delivery-level orchestration after task events  
- richer telemetry semantics  
- route-aware runtime behavior  
- better outbound MQTT client reuse  
- tighter delivery-to-MQTT orchestration polish  
  
## Constraints  
  
- MQTT receive callbacks must stay lightweight  
- message classification should happen before heavier processing  
- task and telemetry flows should remain separated  
- unknown topics should not silently disappear  
- MQTT transport flow should not become the place where business orchestration lives  
- runtime telemetry should remain separate from persistent task state  
- outbound robot command publication should stay separate from delivery decision logic  
- inbound worker routing and outbound publish responsibilities should not collapse into one transport blob  
  
## Related notes  
  
- [[MQTT Background Service Architecture]]  
- [[RobotBuffer]]  
- [[Robot State Management]]  
- [[Service Boundaries]]  
- [[Server Structure]]  
- [[Delivery Orchestration Architecture]]  
- [[Services/RobotCommandPublisher]]  
- [[Services/DeliveryService]]
