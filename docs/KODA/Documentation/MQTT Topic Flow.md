  
> [!abstract]  
> MQTT topic flow defines how robot-originated messages enter the server, how they are classified, and how they move into backend processing.  
> In the current architecture, MQTT is used as the runtime communication channel between robots and the server.  
  
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

From the server perspective, MQTT is used to receive:  
- telemetry  
- task-related updates  
- presence-related runtime information  
  
This allows the backend to react to live robot behavior without relying only on HTTP or database polling.  
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
| Telemetry | `smartwh/robots/+/telemetry` | runtime robot state and live updates | medium |  
| Task updates | `smartwh/robots/+/task/update` | task execution progress and task-related events | high |  
  
The topic split reflects that not all MQTT messages have the same backend importance.  
## Message routing  
Incoming MQTT messages are first handled by the MQTT client receive callback.  
  
The callback does not perform heavy processing directly.  
Instead, it inspects the topic and routes the message to the appropriate in-memory channel.  
### Current routing rules  
  
| Topic ending  | Destination               |     |
| :------------ | :------------------------ | --- |
| `task/update` | task channel              |     |
| `telemetry`   | telemetry channel         |     |
| anything else | warning / unhandled topic |     |
  
This keeps the receive path lightweight and avoids blocking the MQTT client.  
## Processing path  
The current server-side path is:  
1. robot publishes message to MQTT broker  
2. `MqttBackgroundService` receives the message  
3. topic is inspected  
4. message is routed into the appropriate channel  
5. background worker loop reads from that channel  
6. server-side processing updates runtime or persistent state  
  
This separates:  
- transport reception  
- message classification  
- business processing  
## Channel split  
The current implementation uses separate channels for different message types.  
  
| Channel | Message type | Buffer behavior | Reason |  
| :--- | :--- | :--- | :--- |  
| task channel | task-related updates | waits when full | task updates should not be dropped casually |  
| telemetry channel | live telemetry | drops oldest when full | fresh telemetry is more valuable than stale telemetry |  
  
This is an important architectural choice.  
  
Task updates are treated more conservatively because they affect execution correctness.  
Telemetry is treated as high-frequency runtime data that may lose value quickly.  
  
## Reconnect behavior  
MQTT connection handling runs in a dedicated loop.  
The server behavior is:  
1. check whether the client is connected  
2. if disconnected, attempt reconnect  
3. after reconnect, subscribe again to required topics  
4. continue monitoring connection state  
  
This allows the backend to recover from temporary network or broker failures without requiring a full server restart.  
## Why topic flow matters  
Topic flow is important because it defines how robot events become backend state changes.  
  
It directly affects:  
- runtime responsiveness  
- robot state freshness  
- task progress handling  
- system resilience under load  
- separation between communication transport and service logic  
## Current implementation observations  
The current branch already establishes:  
- broker connection handling  
- topic subscription  
- topic-based routing  
- separated processing channels  
- room for task and telemetry worker expansion  
  
What is still evolving:  
- full task update processing logic  
- full telemetry processing logic  
- richer topic contracts and payload handling  
  
So this note describes both the current message path and the intended processing structure.  
## Constraints  
  
- MQTT receive callbacks must stay lightweight  
- message classification should happen before heavier processing  
- task and telemetry flows should remain separated  
- unknown topics should not silently disappear  
- MQTT transport flow should not become the place where business orchestration lives  
  
## Related notes  
  
- [[MQTT Background Service Architecture]]  
- [[RobotBuffer]]  
- [[Robot State Management]]  
- [[Server and Firmware Boundary]]  
- [[Service Boundaries]]  
- [[Server Structure]]