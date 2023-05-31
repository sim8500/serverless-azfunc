using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReceiptSenderFunc.Config;
using ReceiptSenderFunc.DeliveryStatus;
using ReceiptSenderFunc.EmailSender.SendGrid;
using ReceiptSenderFunc.Middleware;
using ReceiptSenderFunc.QueueHelpers;
using ReceiptSenderFunc.SendGrid;
using ReceiptSenderFunc.Workflows;
using SendGrid.Extensions.DependencyInjection;
using SendGrid.Helpers.Mail;
using System.Text.Json;
using System.Text.Json.Serialization;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(app =>
    {
        app.UseWhen<HttpApiKeyMiddleware>((context) =>
        {
            // We want to use this middleware only for http trigger invocations.
            return context.FunctionDefinition.InputBindings.Values
                          .First(a => a.Type.EndsWith("Trigger")).Type == "httpTrigger";
        });
    })
   .ConfigureServices(services =>
   {
       services.AddApplicationInsightsTelemetryWorkerService();
       services.Configure<JsonSerializerOptions>(options =>
       {
           options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
           options.PropertyNameCaseInsensitive = true;
           options.Converters.Add(new JsonStringEnumConverter());
       });

       services.AddHttpClient<IReceiptSenderWorkflow, ReceiptSenderSendGridWorkflow>();
       services.AddSingleton<IEmailSender<SendGridMessage>, EmailSendGridSender>();
       services.AddSingleton<IEmailCreator<SendGridMessage>, EmailSendGridCreator>();
       services.AddSingleton<IMessageRedirectHandler, QueueMsgRedirectHandler>();
       services.AddSingleton<IReceiptDeliveryStatusRepository, ReceiptDeliveryStatusRepository>();
  
       var config = new ConfigurationBuilder()
                       .AddEnvironmentVariables()  
                       .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                       .Build();

       services.AddOptions<QueueConfig>().Bind(config.GetSection("QueueConfig"));
       services.AddOptions<MessageRedirectConfig>().Bind(config.GetSection("MessageRedirectConfig"));
       services.AddOptions<ReceiptSenderConfig>().Bind(config.GetSection("ReceiptSenderConfig"));
       services.AddSendGrid(op => op.ApiKey = config.GetValue<string>("SendGrid:ApiKey"));
   })
    .Build();

host.Run();
