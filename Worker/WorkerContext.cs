using DLL_Support.Logging;
using DLL_Support.Worker;
using System;
using System.ServiceModel;

namespace APP_DeskStats.Worker
{
    public class WorkerContext : IWorkerContext
    {
        public dynamic Service { get; set; }

        public IWorkerCallback Callback { get; set; }

        public System.Diagnostics.Process Process { get; set; }

        public void Dispose()
        {
            try
            {
                ((IDisposable)Service)?.Dispose();
            }
            catch (CommunicationObjectFaultedException)
            {
            }
            catch (Exception e)
            {
                Log.ErrorException("Error cleaning up worker", e);
            }
            try
            {
                Process.Kill();
            }
            catch (Exception e)
            {
                Log.ErrorException("Error cleaning up worker", e);
            }
        }
    }
}
