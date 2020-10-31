using EventProcessor.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventProcessor.Test
{
    class AlertsRepositoryMock : IRepository<Alert>
    {
        public AlertsRepositoryMock()
        {
            All = new List<Alert>().AsQueryable();
        }

        public Alert StoredAlert { get; private set; }

        public IQueryable<Alert> All { get; private set; }

        public Task AddAsync(Alert alert)
        {
            var all = All.ToList();
            all.Add(alert);

            All = all.AsQueryable();

            return Task.CompletedTask;
        }

        public Task DeleteAsync(Alert alert)
        {
            throw new System.NotImplementedException();
        }

        public Task UpsertAsync(Alert alert)
        {
            var existing = All.Where(a => a.id == alert.id).Single();
            existing.DisplayName = alert.DisplayName;
            existing.ApplicationUri = alert.ApplicationUri;
            existing.AverageValue = alert.AverageValue;
            existing.Occurrences = alert.Occurrences;

            return Task.CompletedTask;
        }

        public void PrePopulateAlerts(IList<Alert> alerts)
        {
            All = alerts.AsQueryable();
        }
    }
}
