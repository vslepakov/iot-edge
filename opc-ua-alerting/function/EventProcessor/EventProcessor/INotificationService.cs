using System.Threading.Tasks;

namespace EventProcessor
{
    public interface  INotificationService
    {
        Task NotifyAsync(Alert alert);
    }
}
