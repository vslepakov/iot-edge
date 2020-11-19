using Microsoft.Azure.Devices.Client;
using System.Threading.Tasks;

namespace alerting
{
    public interface IModuleClientWrapper
    {
        Task SendEventAsync(string outputName, Message message);
    }
}
