using System.ServiceModel;

namespace ServiceBusRelay.Contract
{
    [ServiceContract]
    public interface IContratoChat
    {
        [OperationContract]
        void Message(string text);
    }
}
