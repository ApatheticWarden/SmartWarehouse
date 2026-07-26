using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWarehouse.Services.Interfaces {
    public interface ITestService {
        Task<string> PingAsync();
    }
}
