using Microsoft.Azure.Devices.Client;
using System;
using System.Threading.Tasks;

namespace alerting
{
    public class ModuleClientWrapper : IModuleClientWrapper
    {
        private readonly ModuleClient _moduleClient;

        public ModuleClientWrapper(ModuleClient moduleClient)
        {
            _moduleClient = moduleClient;
        }

        public async Task SendEventAsync(string outputName, Message message)
        {
            message.ContentEncoding = "utf-8";
            message.ContentType = "application/json";
            await _moduleClient.SendEventAsync(outputName, message);
        }

        #region Disposable

        private bool _disposed = false;

        ~ModuleClientWrapper() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            // Dispose of managed resources here.
            if (disposing)
            {
                _moduleClient?.Dispose();
            }

            // Dispose of any unmanaged resources not wrapped in safe handles.

            _disposed = true;
        }

        #endregion
    }
}
