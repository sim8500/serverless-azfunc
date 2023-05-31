using EDI.InboundMsgProxy.Config;
using EDI.InboundMsgProxy.Services;
using InboundMsgProxy;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(InboundMsgProxyStartup))]
namespace InboundMsgProxy
{
    public class InboundMsgProxyStartup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var services = builder.Services;

            services.AddHttpClient<IInboundMsgProxyService, InboundMsgProxyService>();

            var config = new ConfigurationBuilder()
                        .AddEnvironmentVariables()
                        .Build();

            services.Configure<ProxyConfig>(config.GetSection("Proxy"));
        }
    }
}
