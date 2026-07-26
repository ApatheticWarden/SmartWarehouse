---
aliases:
  - Warehouse Backend
  - Architecture
  - Robot API
tags:
  - architecture
  - dotnet
  - csharp
  - mqtt
  - iot
  - backend
---

> [!abstract] Core Concept
> A single **ASP.NET Core Monolith** application that simultaneously fulfills two roles:
> 1. A classic **REST API** for human interaction, front-end dashboards, and business logic.
> 2. A background **MQTT Client** for persistent, real-time communication with the robots.

---

## 🏗️ Architecture Overview

The system operates as a unified monolith. Below is the high-level component diagram showing how the pieces fit together. 

> [!info] Tip for Obsidian
> If you have the PlantUML plugin installed, the block below will automatically render into an architecture diagram.

![[Relations of Delivery and Task services according to parts.png]]

## 🗄️ Structure & Data Storage

The system relies on a hybrid storage approach to separate high-frequency telemetry from persistent business data.

- **Database (EF Core):** Stores persistent, relational data.
    
    - Packages, Racks, Task History, Robot Registry.
        
- **In-Memory State (Singleton Cache):** A thread-safe buffer (`ConcurrentDictionary`).
    
    - Stores the real-time coordinates, battery level, and status of active robots.
        
    - Updates hundreds of times per second **without hitting the database**.
        

## 🚪 Communication Flows

### 1. Robot ➡️ Server (Telemetry & Presence)

> [!quote] Protocol: **MQTT** The robot continuously publishes its coordinates and battery level to a telemetry topic. The ASP.NET `BackgroundService` listens to this topic and updates the **Singleton Cache**.

**Presence (Heartbeat):** Handled via MQTT _Last Will and Testament (LWT)_. The robot connects with an "Online" status. If it disconnects abruptly, the broker automatically publishes an "Offline" message, prompting the server to update the cache and reassign tasks.
![[Heartbeat example.png]]
### 2. Human ➡️ Server (Commands & Orchestration)

> [!quote] Protocol: **HTTP (REST API)** An admin triggers a delivery. The `DeliveryService` pulls data from the DB, checks the cache for available robots, creates a `WarehouseTask` in the DB, and then publishes a command to the MQTT broker.

![[UseCase of delivery task creating.png]]

[[Delivery Orchestration Architecture]]
[[Robot State Management]]