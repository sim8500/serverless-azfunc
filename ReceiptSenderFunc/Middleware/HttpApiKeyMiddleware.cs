using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ReceiptSenderFunc.Middleware
{
    internal sealed class HttpApiKeyMiddleware : IFunctionsWorkerMiddleware
    {
        public const string ApiKeyHeaderName = "x-receipt-sender-api-key";

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var requestData = await context.GetHttpRequestDataAsync();
            if (requestData?.Headers.TryGetValues(ApiKeyHeaderName, out var headerValues) ?? false)
            {
                var apiKey = headerValues.First();

                if (!string.IsNullOrEmpty(apiKey) && IsValidKey(apiKey))
                {
                    await next(context);
                    return;
                }
            }

            var responseData = requestData!.CreateResponse();
            responseData.StatusCode = System.Net.HttpStatusCode.Unauthorized;
            context.GetInvocationResult().Value = responseData;
        }

        private static bool IsValidKey(string key) => true;
    }
}
