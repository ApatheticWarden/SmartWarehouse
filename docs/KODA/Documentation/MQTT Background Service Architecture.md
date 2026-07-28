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

| Topic category       | Example purpose                                          | Processing priority | Notes                         |
| :------------------- | :------------------------------------------------------- | :------------------ | :---------------------------- |
| Task updates         | robot task progress, task completion, task state changes | High                | Must be handled reliably      |
| Telemetry            | battery, position, live robot data                       | Medium              | High volume, time-sensitive   |
| Presence / heartbeat | online-offline visibility                                | High                | Used for runtime availability |

## Channel strategy  
The service uses separate channels for different message types.  

| Channel           | Typical payload              | Buffer strategy | Overflow behavior | Reason                                              |
| :---------------- | :--------------------------- | :-------------- | :---------------- | :-------------------------------------------------- |
| Task channel      | task state updates           | bounded         | wait for space    | task updates should not be lost                     |
| Telemetry channel | live status and measurements | bounded         | drop oldest       | fresh telemetry is more useful than stale telemetry |
  
This separation reflects different operational priorities.  

Task updates affect workflow correctness.  
Telemetry mainly reflects current state and loses value quickly when outdated.  
  
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
2. create scope  
3. resolve required services  
4. process message  
5. dispose scope  
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
## Constraints  
- MQTT receive handlers must stay lightweight  
- database work should happen outside the receive callback  
- long-lived singleton state must not directly hold scoped services  
- task updates should be handled more conservatively than telemetry  
- reconnection must restore subscriptions automatically  
  
## Related notes  
  
- [[Warehouse Robotics Backend Architecture]]  
- [[Delivery Orchestration Architecture]]  
- [[Robot State Management]]