using Microsoft.ServiceBus;
using ServiceBusRelay.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.Web.Mvc;

namespace ServiceBusRelay.Client.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SendMsg(string text) {

            var url = "sb://sbhsouza.servicebus.windows.net/chat";

            using (var channelFactory = new
                ChannelFactory<IContratoChat>(new NetTcpRelayBinding(),
                new EndpointAddress(url)))
            {
                channelFactory.Endpoint.Behaviors.Add(new TransportClientEndpointBehavior
                {
                    TokenProvider =
                        TokenProvider
                        .CreateSharedAccessSignatureTokenProvider("owner"
                        , "duMzZyoZ5SX+asyeQ5tzHXE7dZeXa2+LX5FcHnP2+kA=")
                });

                var proxy = channelFactory.CreateChannel();

                try
                {
                    proxy.Message(text);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.Write(ex.Message);
                }

                return RedirectToAction("Index");
            }

        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}