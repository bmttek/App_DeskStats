using DLL_Support.Worker;
using System.ServiceModel;

namespace APP_DeskStats.Worker
{
    [ServiceContract(CallbackContract = typeof(IWorkerCallback))]
    public interface IWorkerService : IWorkerServiceAll
    {
        //[OperationContract]
        //List<UserRecord> GetAllUsers();
    }
}
