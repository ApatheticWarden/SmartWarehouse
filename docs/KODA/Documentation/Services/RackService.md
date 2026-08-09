> [!abstract]  
> `RackService` is responsible for rack-related lookup and persistence access.  
> In the current V1 implementation, it mainly supports delivery orchestration by resolving rack data from the persistent model.  
  
## Purpose  
  
`RackService` exists to provide a dedicated backend boundary for rack-related access.  
  
Its role is to:  
- retrieve rack entities  
- isolate rack lookup from unrelated services  
- give future rack/storage logic a clear home  
  
## Current role  
  
In the current V1 implementation, `RackService` handles:  
- rack lookup by id  
- returning `Rack` entities needed for delivery flow  
  
This is currently narrow, but still useful as a separate rack-focused access layer.  
  
## Why it exists  
  
Even if rack usage is simple right now, rack concerns are different from:  
- package identity  
- robot runtime state  
- delivery orchestration  
  
A dedicated rack-focused service helps prevent rack/storage logic from leaking into unrelated services.  
  
## Current V1 use in delivery flow  
  
The current delivery-start flow uses rack data to:  
- resolve where a package is stored  
- obtain the rack’s current node  
- build task start location data  
  
This makes `RackService` a supporting service for `DeliveryService`, not an orchestration owner.  
  
## What it should not own  
  
`RackService` should not become responsible for:  
- package lifecycle  
- robot selection  
- MQTT transport  
- delivery orchestration  
- task lifecycle  
  
It should stay focused on rack-related access and, later, rack/storage rules.  
  
## Future growth  
  
Later, `RackService` may become the place for:  
- storage placement logic  
- rack occupancy rules  
- rack availability/usage checks  
- warehouse storage layout helpers  
- rack/node relation helpers  
  
That future growth is one of the reasons to keep rack concerns separate now.  
  
## Related notes  
  
- [[Service Boundaries]]  
- [[Classes/Rack]]  
- [[Warehouse Robotics Backend Architecture]]  
- [[Services/DeliveryService]]