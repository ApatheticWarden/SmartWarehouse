> [!abstract]  
> The MQTT background service is responsible for maintaining the broker connection, receiving robot messages, and dispatching them for asynchronous processing.  
> It exists to support continuous robot communication without coupling MQTT traffic to HTTP request handling.  
  
![[MQTT Background Service Architecture.png]]  
  
## Purpose  
This service handles the runtime side of robot communication.  
  
Its responsibilities are:  
- maintain a persistent MQTT connection  
- subscribe to robot-related topics  
- receive incoming messages  
- route messages by topic  
- process messages asynchronously  
- isolate high-frequency traffic from the rest of the application  
## Runtime role  
The service runs as a long-lived background process.  
  
This is necessary because:  
- robot communication must continue independently of HTTP requests  
- broker connection state must survive across the application lifetime  
- telemetry and task updates may arrive continuously  
## Processing model  
The service uses a producer-consumer approach.  
### Ingestion stage  
Incoming MQTT messages are received by the MQTT client event handler.  
  
The handler should stay lightweight:  
- identify the topic  
- choose the correct processing path  
- enqueue the message  
- return immediately  
  
The handler should not perform heavy parsing, database work, or long-running business logic.  
### Processing stage  
Separate worker loops consume queued messages and execute the actual processing logic.  
  
This separation prevents:  
- blocking the MQTT client  
- delayed message reception  
- increased risk of disconnects under load  
## Topic handling  
  
| Topic category | Example purpose | Processing priority | Notes |  
| :--- | :--- | :--- | :--- |  
| Task updates | robot task events and task lifecycle progress | High | Must be handled reliably |  
| Telemetry | battery, stance, live robot data | Medium | High volume, time-sensitive |  
| Presence / heartbeat | online-offline visibility | High | Not fully implemented yet |  
  
## Current V1 task update flow  
Task update messages are currently routed into the task channel and processed by `ProcessTasksAsync`.  
  
The V1 task flow currently:  
- deserializes `TaskAnswerDTO`  
- uses `RobotTaskStatus` as the incoming robot-side event contract  
- resolves `ITaskService` from a scoped service provider  
- dispatches task update handling to service-level methods  
### Current robot task event meanings  
  
| Robot task status | Meaning | Current server reaction |  
| :--- | :--- | :--- |  
| `Accepted` | robot acknowledged task reception | runtime handshake only |  
| `Started` | robot began execution | task becomes `InProgress` |  
| `Completed` | robot finished execution | task becomes `Completed`, completion time is stored, robot is unassigned |  
| `Failed` | robot failed to execute the task | task becomes `Failed`, failure is logged |  
  
### Important distinction  
The task update flow separates:  
- `RobotTaskStatus` as an incoming robot-side event contract  
- `TaskStatus` as server-side persistent task state  
  
This prevents the transport contract from being treated as the same thing as the persistent workflow model.  
## Current V1 telemetry flow  
Telemetry messages are currently routed into the telemetry channel and processed by `ProcessTelemetryAsync`.  
  
The V1 telemetry flow currently:  
- deserializes `RobotTelemetryDTO`  
- resolves `ITelemetryService` from a scoped service provider  
- updates `RobotBuffer`  
- updates `RobotServerState`  
  
Telemetry currently affects only runtime state.  
  
It does not currently:  
- persist telemetry to the database  
- perform route checking  
- perform pathfinding decisions  
- trigger broader orchestration behavior  
## Channel strategy  
The service uses separate channels for different message types.  
  
| Channel | Typical payload | Buffer strategy | Overflow behavior | Reason |  
| :--- | :--- | :--- | :--- | :--- |  
| Task channel | task update events | bounded | wait for space | task updates should not be lost |  
| Telemetry channel | live runtime state updates | bounded | drop oldest | fresh telemetry matters more than stale telemetry |  
  
This separation reflects different operational priorities.  
  
Task updates affect workflow correctness.  
Telemetry mainly reflects current state and loses value quickly when outdated.  
## Scoped dependency handling  
The background service itself is long-lived, but many application dependencies should remain short-lived.  
  
| Component type | Lifetime | Usage rule |  
| :--- | :--- | :--- |  
| MQTT background service | Singleton | owns connection and worker loops |  
| database context | Scoped | created only when processing a message |  
| business services | Scoped / transient | resolved inside a processing scope |  
  
This matters because scoped dependencies should not be held directly by a singleton for the full application lifetime.  
  
For message processing, the safe pattern is:  
1. dequeue message  
2. deserialize the transport DTO  
3. create scope  
4. resolve required service  
5. process message  
6. dispose scope  
## Connection management  
Connection lifecycle should be managed in its own loop.  
  
Main responsibilities:  
- connect to the broker  
- detect disconnects  
- retry after failure  
- restore subscriptions after reconnect  
  
A reconnect loop should assume that broker availability is not guaranteed.  
  
Typical behavior:  
1. detect lost connection  
2. log the failure  
3. wait briefly  
4. retry connection  
5. resubscribe to required topics  
## Failure considerations  
The service should tolerate:  
- broker disconnects  
- malformed payloads  
- temporary database errors  
- transient processing failures  
  
Failures should be isolated so that:  
- one bad message does not stop the whole service  
- connection handling continues  
- other queued messages can still be processed  
## Current limitations  
The current V1 implementation still leaves several things for later:  
  
- retry policy for failed tasks  
- delivery-level orchestration after task events  
- telemetry-aware routing logic  
- heartbeat/presence modeling as a first-class flow  
- richer robot runtime data beyond stance and battery  
## Constraints  
- MQTT receive handlers must stay lightweight  
- DTO deserialization should happen in the worker, not in downstream services  
- task and telemetry flows should remain separated  
- long-lived singleton state must not directly hold scoped services  
- task updates should be handled more conservatively than telemetry  
- reconnection must restore subscriptions automatically  
- MQTT transport flow should not become the place where business orchestration lives  
## Related notes  
  
- [[MQTT Topic Flow]]  
- [[RobotBuffer]]  
- [[Robot State Management]]  
- [[Service Boundaries]]  
- [[Server Structure]]  
- [[Delivery Orchestration Architecture]]