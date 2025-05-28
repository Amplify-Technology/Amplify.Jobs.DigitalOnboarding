using Amplify.Jobs.DigitalOnboarding.Onboarding.Utils;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

AppConfig.Initialize(builder.Configuration);

builder.ConfigureFunctionsWebApplication();

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
