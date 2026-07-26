#architecture #services #koda
### System Flow Diagram
![[Delivery Service Functioning Diagram.png]]
### Architectural Concept: The Orchestrator Pattern (Master-Slave)

This module utilizes an **Orchestrator Pattern** where the **Delivery Service** acts as the supreme controller (Master/Big Boss), managing the domain-specific operations of the **Package Service** and **Task Service** (Slaves/Workers).

- **Role Segregation:** The Package and Task services are strictly isolated. They do not interact with each other and are unaware of the broader business context. They only execute specific, localized operations in memory when commanded.

- **Single Source of Truth:** All complex, cross-domain operations and state mutations are orchestrated exclusively by the Delivery Service.

- **Data Synchronization and Safety:** The Delivery Service guarantees complete data correctness. External requests interacting with the Delivery Controller will not corrupt the system state, as the Master service precisely manages the sequence of execution.

- **Transactional Immutability (Fail-Safe):** The Delivery Service acts as a Unit of Work. It is the ONLY service permitted to execute database commits (`SaveChanges`). If any part of the process fails (e.g., a network drop, a validation error in Task creation), the Delivery Service aborts the entire operation. No partial data is saved, ensuring strict data immutability and preventing ghost records or desynchronization in the database.



[[Database structure]]