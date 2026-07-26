using System.Collections.Concurrent;

namespace SmartWarehouse.Models {
    public class RobotBuffer {
        private readonly ConcurrentDictionary<int, RobotServerState> _states = new();

        public void UpdateState(int robotId, Action<RobotServerState> updateAction) {
            var state = _states.GetOrAdd(robotId, id => new RobotServerState { RobotId = id });
            lock (state) updateAction(state);
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
