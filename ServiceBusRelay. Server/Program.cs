using Microsoft.ServiceBus;
using ServiceBusRelay.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusRelay.Server
{
    class Program
    {
        static void Main()
        {
            var url = "sb://sbhsouza.servicebus.windows.net/";

            using (var host = new ServiceHost(typeof(ServerRelay)
                , new Uri(url)))
            {
                var endPoint = host
                    .AddServiceEndpoint(typeof(IContratoChat)
                    , new NetTcpRelayBinding(), "chat");

                endPoint.Behaviors.Add(new TransportClientEndpointBehavior
                {
                    TokenProvider = 
                        TokenProvider
                        .CreateSharedAccessSignatureTokenProvider("owner"
                        , "duMzZyoZ5SX+asyeQ5tzHXE7dZeXa2+LX5FcHnP2+kA=")
                });

                host.Open();

                Console.WriteLine("Server is running");
                Console.ReadKey();
                host.Close();
            }


        }
    }
}
