using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CpsBoostAgile.Startup))]
namespace CpsBoostAgile
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            ConfigureAuth(appBuilder);
            appBuilder.MapSignalR();
        }
    }
}
