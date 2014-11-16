using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Fable.Startup))]
namespace Fable
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
