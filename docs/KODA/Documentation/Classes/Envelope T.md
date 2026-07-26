# Envelope T (Result Wrapper Pattern)
#architecture #patterns #csharp #utils #classes
[[Delivery Orchestration Architecture]]
### Description
The `Envelope<T>` wrapper class implements the Result pattern. It is used to safely encapsulate the outcomes of method executions (especially within services and business logic). 

The main problem it solves: the need to return either a typed object (e.g., `Package`) or an error message (e.g., during a database transaction `Rollback`) from a method, without resorting to throwing exceptions and without violating the strict typing of the `Task<T>` return value.

### Class Structure
*   **`Payload` (`T?`)**: The actual data payload. Contains the requested object (e.g., a database entity) on success, or `default` (`null`) on failure.
*   **`IsSuccess` (`bool`)**: Simplifies validation checks in controllers. If `true`, the operation completed normally; if `false`, the operation was aborted.
*   **`Message` (`string?`)**: A textual description of the result. On success, it defaults to "Success". On failure, it holds the specific reason for the error, which can be returned to the client or written to logs.
*   **`TimeStamp` (`DateTime`)**: A UTC timestamp recording the exact moment the response was generated. Highly useful for debugging and auditing.

### Factory Methods (Usage Guide)
Instead of manually initializing the object every time, the class provides two static methods for quick and clean response generation.

**1. Successful Execution (Instead of returning the object directly)**
Called at the end of a successful operation (e.g., after `transaction.Commit()`).
```csharp
// Service method signature example: 
// public async Task<Envelope<Package>> CreatePackage(...)

return Envelope<Package>.Ok(newPackage);

// You can also pass a custom message: 
// Envelope<Package>.Ok(newPackage, "Package created successfully");
```

## Class view:
```csharp
// Class that realises all work with Task<(string? Error, UserProfile Profile)>
// And other kind of stuff
public class Envelope<T> {
    // Data we want to transfer
    // Defined by T
    public T? Payload { get; set; }

    // Just to make logic "easier"
    public bool IsSuccess { get; set; }

    // Message we will deliver in different situation
    public string? Message { get; set; } = string.Empty;

    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

    public Envelope() { }

    // Usefull methods:

    // Action success
    public static Envelope<T> Ok(T payload, string m = "Success") {
        return new Envelope<T> {
            Payload = payload,
            IsSuccess = true,
            Message = m
        };
    }

    // Action error
    public static Envelope<T> Error(string errorMessage) {
        return new Envelope<T> {
            Payload = default,
            IsSuccess = false,
            Message = errorMessage
        };
    }
}
```
