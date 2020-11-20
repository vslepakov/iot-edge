using Microsoft.Azure.Devices.Client;
using System;
using System.Threading.Tasks;

namespace alerting
{
    public interface IModuleClientWrapper : IDisposable
    {
        Task SendEventAsync(string outputName, Message message);
    }
}
