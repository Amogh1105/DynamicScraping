using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ScrapingMVC.Startup))]
namespace ScrapingMVC
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
