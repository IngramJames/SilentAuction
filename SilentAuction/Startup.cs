using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SilentAuction.Startup))]
namespace SilentAuction
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
