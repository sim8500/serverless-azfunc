using EDI.InboundMsgProxy.Config;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EDI.InboundMsgProxy.Services
{
    public interface IInboundMsgProxyService
    {
        Task<HttpResponseMessage> Process(string targetUrl, string content);
    }
    class InboundMsgProxyService : IInboundMsgProxyService
    {
        public InboundMsgProxyService(IOptions<ProxyConfig> config, HttpClient httpClient)
        {
            _config = config.Value;

            _httpClient = httpClient;
        }
        public async Task<HttpResponseMessage> Process(string targetUrl, string content)
        {
            var targetUri = new Uri(new Uri(_config.BaseUrl), targetUrl);

            var req = new HttpRequestMessage(HttpMethod.Post, targetUri)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            };

            return await _httpClient.SendAsync(req);
        }

        private readonly HttpClient _httpClient;
        private readonly ProxyConfig _config;
    }
}
