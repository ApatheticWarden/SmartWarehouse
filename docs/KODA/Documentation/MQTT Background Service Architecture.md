# 🧠 The Brain of the Warehouse: MQTT Background Service

> [!info] 🎯 Executive Summary
> Think of the `MqttBackgroundService` as the **Air Traffic Controller** for our warehouse robots. It’s a long-running, indestructible background process (`Singleton`) that keeps a persistent ear open to the MQTT broker, catches hundreds of messages per second, and routes them safely without blocking the main application.

---

## 🗺️ High-Level Architecture Flow

> [!abstract]- 📐 View PlantUML Architecture Diagram (PlantText)
> ![[MQTT Background Service Architecture.png]]

---

## 🧩 The Cast of Characters

| Component | Lifespan | What it actually does |
| :--- | :--- | :--- |
| 👑 **MqttBackgroundService** | `Singleton` | The immortal boss. Holds everything together and outlives HTTP requests. |
| 🔌 **Manage Connection** | `Infinite Loop` | The mechanic. Constantly checks if the broker is alive and reconnects if a cable drops. |
| ⚡ **The Router** | `Event Trigger` | The sorter. Looks at incoming topics and instantly throws payloads into the right buckets. |
| 🏋️ **The Workers** | `Infinite Loop` | The heavy lifters. They pull tasks out of the buckets, parse JSON, and run business logic. |
| 🛡️ **Scope Factory** | `Transient` | The memory-leak defuser. Spawns short-lived DI containers for database operations. |

---

## 🏎️ The Engine: `MQTTnet` Toolbox

We use **MQTTnet** because it's insanely fast and built for .NET. Here's our setup in the constructor:

* `MqttClientFactory`: The forge where we build our client.
* `MqttClientOptions`: Our ID card (`IP`, `Port`, `ClientId`). We use `.WithCleanSession()` so we never inherit ghost messages from past sessions after a reboot.
* `IMqttClient`: The actual pipeline. We hook into `DisconnectedAsync` for logging and `ApplicationMessageReceivedAsync` as our incoming message gate.

---

## 🤹‍♂️ 1. The "No-Blocking" Rule (`Task.WhenAll`)

> [!success] 🚀 Concurrent Orchestration
> A standard `while(true)` loop would freeze the whole service. Instead, our `ExecuteAsync` is an **orchestrator** that launches three independent parallel flows simultaneously:
> 
> 1. 🔌 **`ManageMqttConnectionAsync`** — Keeps the connection alive & stable.
> 2. 📦 **`ProcessTasksAsync`** — Consumes critical task queues.
> 3. 🛸 **`ProcessTelemetryAsync`** — Consumes real-time telemetry spam.

*If saving a task to the database takes a whole second, the MQTT client doesn't care — it keeps receiving telemetry in the background.*

---

## 🚦 2. The Conveyor Belts (`System.Threading.Channels`)

If you try to parse JSON and save to a database right when a message arrives in the event handler, the MQTT client will choke and drop connections. 

We solved this using the **Producer-Consumer pattern**. The Router just grabs messages and drops them onto fast in-memory conveyor belts (Channels).

### ⚖️ Backpressure Strategy Matrix

| Feature | 📥 The Task Channel | 🛸 The Telemetry Channel |
| :--- | :--- | :--- |
| **Topic Route** | `smartwh/robots/+/task/update` | `smartwh/robots/+/telemetry` |
| **Data Payload** | "Robot picked up item #45" | "Robot coordinates: X, Y & Battery" |
| **Buffer Limit** | 1,000 messages | 5,000 messages |
| **Overflow Mode** | `BoundedChannelFullMode.Wait` | `BoundedChannelFullMode.DropOldest` |
| **Push Method** | `await WriteAsync(...)` | `TryWrite(...)` |
| **Why this rule?** | Missing a task breaks the warehouse state. **We must wait for space.** | Nobody cares where the robot *was* 5 seconds ago. **Drop old spam, keep fresh coordinates.** |

---

## 💣 3. Defusing Memory Leaks (`IServiceScopeFactory`)

> [!warning] ⚠️ The Singleton Trap
> Background services live forever (`Singleton`). If you inject a Scoped service like Entity Framework's `DbContext` directly into its constructor, it stays alive forever. The tracking state bloats, RAM explodes, and the server crashes.

> [!tip] 💡 The Solution
> We inject `IServiceScopeFactory`. For every single message pulled from the channel, we build a tiny, temporary universe:

```text
📥 Pull Message  ──>  🧪 Create Scope  ──>  ⚙️ Resolve Service  ──>  💾 Execute DB/Cache  ──>  🗑️ Dispose Scope
```

1. **Pull:** Worker wakes up as a message hits the channel.
2. **Spawn:** `using var scope = _scopeFactory.CreateScope();`
3. **Resolve:** We get a fresh, isolated `DbContext` or Business Service.
4. **Execute:** Data is safely processed and saved.
5. **Destroy:** The `using` block ends. The scope dies, connection returns to the pool, RAM stays pristine.

---

## 🧟 4. The Self-Healing Connection

Networks are fragile. Cables get kicked, WiFi drops. The `ManageMqttConnectionAsync` loop acts as our survival mechanic. 

If the broker goes offline, this background loop automatically:
1. Catches the exception safely (no app crashes).
2. Sleeps for 1 second (`Task.Delay`).
3. Tries to `ConnectAsync` again.
4. Re-subscribes (`SubscribeAsync`) to all topics once the pipe is back up.

*Zero manual server restarts required*