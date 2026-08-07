using System.Collections.Concurrent;

namespace SmartWarehouse.Models {
    public class RobotBuffer {
        private readonly ConcurrentDictionary<int, RobotServerState> _states = new();

        public (RobotServerState, bool) UpdateOrCreateState(int robotId, Action<RobotServerState> updateAction) {
            bool wasCreated = false;
            // Try to get
            if (!_states.TryGetValue(robotId, out var state)) {
                // Create instance
                state = new RobotServerState { RobotId = robotId };
                // try to add
                // tryadd - returns false if already exists
                if (_states.TryAdd(robotId, state)) {
                    wasCreated = true;
                } else {
                    state = _states[robotId];
                }
            }
            lock (state) updateAction(state);
            return (state, wasCreated);
        }

        // get only one
        public RobotServerState? GetState(int robotId) {
            _states.TryGetValue(robotId, out var state);
            return state;
        }
        // get all
        public IEnumerable<RobotServerState> GetAllStates() {
            return _states.Values;
        }
    }
}
