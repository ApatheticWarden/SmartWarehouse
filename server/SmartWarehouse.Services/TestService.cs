
namespace SmartWarehouse.Services {
    public class TestService : Interfaces.ITestService {
        public async Task<string> PingAsync() {
            return "pong";
        }
    }
}
