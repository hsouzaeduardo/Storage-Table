using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ServiceBusRelay.Client.Startup))]
namespace ServiceBusRelay.Client
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
