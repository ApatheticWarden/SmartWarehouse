> [!abstract]  
> MQTT topic flow defines how robot-originated messages enter the server, how they are classified, and how they move into backend processing.  
> In the current V1 implementation, MQTT is used for two runtime flows:  
> - task update handling  
> - telemetry handling  
  
## Purpose  
This note describes the server-side flow of MQTT messages.  
  
It is responsible for explaining:  
- which topics the server listens to  
- how incoming messages are classified  
- how message types are separated  
- how messages move from transport into processing  
- how reconnect and re-subscribe behavior is handled  
  
## Role in the server  
MQTT is the runtime communication entry path for robot-side events.  
  
From the server perspective, MQTT is currently used to receive:  
- task update messages  
- telemetry messages  
  
This allows the backend to react to live robot behavior without relying only on HTTP or database polling.  
  ![[MQTT Flow.png]]
## Current subscribed topics  
The current MQTT background service subscribes to:  
  
| Topic | Meaning |  
| :--- | :--- |  
| `smartwh/robots/+/telemetry` | live robot telemetry updates |  
| `smartwh/robots/+/task/update` | task-related robot updates |  
  
The `+` segment means the server listens for messages from multiple robots under the same topic structure.  
## Topic categories  
  
| Category | Topic pattern | Purpose | Processing priority |  
| :--- | :--- | :--- | :--- |  
| Telemetry | `smartwh/robots/+/telemetry` | runtime robot state updates | medium |  
| Task updates | `smartwh/robots/+/task/update` | task execution events and task lifecycle progress | high |  
  
The topic split reflects that not all MQTT messages have the same backend importance.  
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
The current server-side path is:  
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
- scoped service resolution per message  
  
What is still evolving:  
- retry and recovery policy for failed tasks  
- delivery-level orchestration after task events  
- richer telemetry semantics  
- route-aware runtime behavior  
## Constraints  
- MQTT receive callbacks must stay lightweight  
- message classification should happen before heavier processing  
- task and telemetry flows should remain separated  
- unknown topics should not silently disappear  
- MQTT transport flow should not become the place where business orchestration lives  
- runtime telemetry should remain separate from persistent task state  
  
## Related notes  
  
- [[MQTT Background Service Architecture]]  
- [[RobotBuffer]]  
- [[Robot State Management]]  
- [[Service Boundaries]]  
- [[Server Structure]]