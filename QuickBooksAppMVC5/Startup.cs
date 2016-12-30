using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(QuickBooksAppMVC5.Startup))]
namespace QuickBooksAppMVC5
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
