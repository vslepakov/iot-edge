using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EventProcessor.Data
{
    public class AlertsRepository : IRepository<Alert>
    {
        private readonly IDocumentClient _documentClient;
        private readonly Uri _collectionUri;

        public AlertsRepository(IDocumentClient documentClient, string databaseName, string collectionName)
        {
            _documentClient = documentClient;
            _collectionUri = UriFactory.CreateDocumentCollectionUri(databaseName, collectionName);
        }

        public IQueryable<Alert> All => _documentClient.CreateDocumentQuery<Alert>(_collectionUri);

        public async Task AddAsync(Alert entity)
        {
            try
            {
                entity.id = Guid.NewGuid().ToString();
                await _documentClient.CreateDocumentAsync(_collectionUri, entity);
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.Conflict)
                {
                    throw new EntityAlreadyExistsException();
                }

                throw;
            }
        }

        public async Task UpsertAsync(Alert entity)
        {
            try
            {
                await _documentClient.UpsertDocumentAsync(_collectionUri, entity);
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new EntityNotFoundException();
                }

                throw;
            }
        }

        public Task DeleteAsync(Alert entity)
        {
            throw new NotImplementedException();
        }
    }
}
