using System.Xml.Serialization;

namespace SmartWarehouse.Models.Wrappers {
    // Class that realises all work with Task<(string? Error, UserProfile Profile)>
    // And other kind of stuff
    public class Envelope<T> {
        // Data we want to transfer
        // Defined by T
        public T? Payload { get; init; }

        // Just to make logic "easier"
        public bool IsSuccess { get; init; }

        // Message we will deliver in different situation
        public string? Message { get; init; } = string.Empty;

        public DateTime TimeStamp { get; init; } = DateTime.UtcNow;

        // "init" grants immutability of data
        // e.x. someone decided to change data between actions. And now it is fucked up
        // So, use it

        public Envelope() { }

        // Operators
        public static implicit operator Envelope<T>(T payload) => Ok(payload);
        
        // So I don`t need to write that whole shit everytime :) 
        // E.x. return Envelope<List<RobotServerState>>.Ok(robots); => return robots;

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
}
