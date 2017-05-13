using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBus
{
    class Program
    {
        private const string cCon = "Endpoint=sb://sbhsouza.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=I/WoCSlnyulcx/Fvdg9qZqegjlgWe+W9tpq4Q2RwCTk=";

        private const string NomeFila = "assinantes";
        private const string NomeTopico = "partidas";

        static void Main()
        {
            //ServiceBusQueue();
            //ServiceBusTopic();
            SBPublicarTopicoSANTOS();
            SbReciveTopicoSANTOS();
            Console.ReadKey();
        }

        private static void ServiceBusTopic()
        {
            var nmm = NamespaceManager.CreateFromConnectionString(cCon);
            if (!nmm.TopicExists(NomeTopico)) {
                var topico = nmm.CreateTopic(NomeTopico);
                nmm.CreateSubscription(topico.Path, "SANTOS");
                nmm.CreateSubscription(topico.Path, "CORINTHIANS");
            }

            SqlFilter highMessagesFilter =
            new SqlFilter("TIME = 'SANTOS'");

            nmm.CreateSubscription(NomeTopico , "torcida", highMessagesFilter);
            
            

           

        }

        private static void SbReciveTopicoSANTOS()
        {
            var Client =
                SubscriptionClient.CreateFromConnectionString
                        (cCon, NomeTopico ,"torcida");

            // Configure the callback options.
            var options = new OnMessageOptions();
            options.AutoComplete = false;
            options.AutoRenewTimeout = TimeSpan.FromMinutes(1);

            Client.OnMessage((message) =>
            {
                try
                {

                    var ass = message.GetBody<Assinantes>();
                    // Process message from subscription.
                    Console.WriteLine("\n**High Messages**");
                    
                    Console.WriteLine("MessageID: " + message.MessageId);
                    Console.WriteLine("Nome Torcedor: " + ass.Nome + 
                        message.Properties["Time"]);
                    
                    message.Complete();
                }
                catch (Exception)
                {
                    // Indicates a problem, unlock message in subscription.
                    message.Abandon();
                }
            }, options);
        }

        private static void SBPublicarTopicoSANTOS()
        {

            

            var topico = TopicClient.CreateFromConnectionString(cCon, NomeTopico);
            var ass = new Assinantes {
                DataAssinatura = DateTime.Now,
                Descricao = "TORCEDOR SANTOS",
                Nome = "HENRIQUE SOUZA",
                Time = "SANTOS"
            };

            using (var message = new BrokeredMessage(ass))
            {
                message.MessageId = Guid.NewGuid().ToString();
                message.Properties.Add("TIME", ass.Time);
                topico.Send(message);
            }
        }

        private static void ServiceBusQueue()
        {
            NamespaceManager nmm = NamespaceManager.CreateFromConnectionString(cCon);

            if (!nmm.QueueExists(NomeFila)) {
                nmm.CreateQueue(NomeFila);
            }

            ServiceBusEnviarMsg();
            ServiceBusReciverMsg();
            Console.ReadKey();
        }

        private static void ServiceBusReciverMsg()
        {
            var fila = QueueClient.CreateFromConnectionString(cCon, NomeFila);

            BrokeredMessage msg = null;

            while (true)
            {
                try
                {
                    msg = fila.Receive(TimeSpan.FromSeconds(5));

                    if (msg != null) {
                        var conteudo = msg.GetBody<Assinantes>();
                        Console.WriteLine($@"{conteudo.Nome} - 
                        {conteudo.Descricao} - 
                        {conteudo.DataAssinatura.ToShortDateString()}");

                        msg.Complete();
                    } else
                    {
                        Console.WriteLine("Concluído...");

                        break;
                    }
                }
                catch (MessagingException ex)
                {
                    if (!ex.IsTransient) {
                        Console.WriteLine(ex.Message);
                        throw;
                    }
                }
            }
        }

        private static void ServiceBusEnviarMsg()
        {
            var fila = QueueClient.CreateFromConnectionString(cCon, NomeFila);

            Assinantes assistente = new Assinantes()
            {
                DataAssinatura = new DateTime(2016, 01, 12),
                Descricao = "Assinatura Futebol",
                Nome = "HENRIQUE SOUZA"
            };

            BrokeredMessage bkm = new BrokeredMessage(assistente);

            bkm.MessageId = Guid.NewGuid().ToString();

            try
            {
                fila.Send(bkm);
            }
            catch (MessagingException ex)
            {
                if (!ex.IsTransient) {
                    Console.WriteLine(ex.Message);
                }
            }
            finally
            {
                fila.Close();
            }
        }
    }
}
