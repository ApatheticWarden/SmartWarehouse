![[KODA SMART WAREHOUSE.png]]
<center> Initial version V1 </center>
# Database Schema: KODA SMART WAREHOUSE
#database #schema #koda
## 1. Racks
> [!abstract] Purpose
> Tracks mobile storage units.

*   **Fields:**
    *   `Id` (PK)
    *   `CurrentNodeId` (Graph location reference)

---

## 2. Packages
> [!abstract] Purpose
> Tracks inventory items and their properties.

*   **Fields:**
    *   `Id` (PK)
    *   `TrackingNumber`
    *   `Size`
    *   `WeightKg`
    *   `Status`
    *   `RackId` (FK, Nullable)
*   **Relations:**
    *   `RackId` > [[Racks]].Id (Many-to-One)

---

## 3. Robots
> [!abstract] Purpose
> Registers active autonomous hardware.

*   **Fields:**
    *   `Id` (PK)
    *   `Serial`

---

## 4. WarehouseTasks
> [!abstract] Purpose
> Core transactional table for movement operations.

*   **Fields:**
    *   `Id` (PK)
    *   `RackId` (FK)
    *   `AssignedRobotId` (FK, Nullable)
    *   `Command` (TaskCommand Enum)
    *   `StartNodeId`
    *   `EndNodeId`
    *   `Status` (TaskStatus Enum)
    *   `CreatedAt`
    *   `CompletedAt` (Nullable)
*   **Relations:**
    *   `RackId` > [[Racks]].Id (Many-to-One)
    *   `AssignedRobotId` > [[Robots]].Id (Many-to-One)

---

## 5. Users
> [!abstract] Purpose
> Access control and authorization.

*   **Fields:**
    *   `Id` (PK)
    *   `Username`
    *   `Role` (UserRole Enum)

---

## 6. ActionLog
> [!abstract] Purpose
> System audit trail.

*   **Fields:**
    *   `Id` (PK)
    *   `TimeStamp`
    *   `EventType` (LogEventType Enum)
    *   `Message`