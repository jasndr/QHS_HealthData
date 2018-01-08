using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(HealthData2.Startup))]
namespace HealthData2
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
