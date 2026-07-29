  
> [!abstract]  
> `Package` is the persistent server-side entity that represents a warehouse item tracked by the system.  
> It stores stable package data, current warehouse-related status, and an optional relation to the rack where the package is currently located.  
  
## Purpose  
`Package` exists to represent a tracked item in the warehouse domain.  
It is used to:  
- register packages in the system  
- identify them through a tracking number  
- store basic physical characteristics  
- track warehouse-related status  
- connect a package to its current rack when applicable  
## What it represents  
`Package` is the server’s persistent record of a physical item handled inside the warehouse.  

It answers questions such as:  
- which package is this  
- what is its tracking identifier  
- what are its basic physical properties  
- what is its current warehouse status  
- is it currently assigned to a rack  
  
## Current model  
  
| Field | Meaning |  
| :--- | :--- |  
| `Id` | persistent database identifier |  
| `TrackingNumber` | package tracking identifier |  
| `Size` | size category |  
| `WeightKg` | package weight |  
| `Status` | current package state in the warehouse flow |  
| `RackId` | optional current rack reference |  
| `Rack` | navigation link to current rack |  
  
## Why it matters  
`Package` is one of the central warehouse entities because many warehouse actions begin with package intent.  
  
Examples:  
- a worker needs a package  
- a package must be found  
- a package may need to be moved  
- a package may be linked to a task or delivery-related workflow  
  
Without a package entity, the backend cannot tie warehouse requests to a concrete tracked item.  
## Relation to racks  
A package may optionally belong to a rack.  
  
| Relation | Meaning |  
| :--- | :--- |  
| `Package -> Rack` | current package placement in warehouse storage |  
  
The relation is nullable, which allows the system to represent cases where:  
- a package has not yet been placed  
- a package is in transfer  
- a package is temporarily outside rack storage  
  
This makes the package model more flexible than forcing rack presence at all times.  
## Relation to tasks  
A package is not currently linked directly to `WarehouseTask` in the model, but it still plays an important role in task-oriented workflows.  
  
In practice:  
- package-related intent may lead to task creation  
- package state may influence whether a warehouse action is valid  
- delivery or retrieval flows may begin from package lookup  
  
That means the package entity is part of execution context even when task linkage is indirect.  
## Package status  
`Status` represents package state in the warehouse flow.  
The exact meaning depends on the current enum design, but the general role is to express where the package stands operationally.  
  
This may include ideas such as:  
- newly created  
- stored  
- moving  
- prepared  
- delivered  
  
The package note should describe the purpose of status even if the full lifecycle evolves later.  
## Current design direction  
The current model keeps `Package` focused on:  
- identity  
- physical metadata  
- storage relation  
- operational status  
  
This is enough for the current system stage and avoids mixing package identity with unrelated workflow logic.  
## Constraints  
- `Package` should remain a persistent warehouse entity  
- package state should describe package-related status, not robot runtime state  
- rack placement should stay optional when the workflow requires it  
- package workflow logic should be handled through services, not embedded into the entity itself  
  
## Related notes  
  
- [[Rack]]  
- [[Warehouse Tasks]]  
- [[Database structure]]  
- [[Service Boundaries]]  
- [[Server Structure]]