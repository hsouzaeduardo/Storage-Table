using ServiceBusRelay.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusRelay.Server
{

    public class ServerRelay : IContratoChat
    {
        public void Message(string text)
        {
            Console.WriteLine(text);
        }   
    }
}
