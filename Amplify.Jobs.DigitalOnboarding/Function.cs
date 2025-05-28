using System;
using System.IO;
using System.Text;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Amplify.Jobs.DigitalOnboarding.Onboarding.Utils;

namespace Amplify.Jobs.DigitalOnboarding.Onboarding
{
    public class Function
    {
        private readonly ILogger<Function> _logger;

        public Function(ILogger<Function> logger)
        {
            _logger = logger;
        }

        [Function("OnboardingQueueTrigger")]
        public void Run(
            [QueueTrigger("onboarding-event", Connection = "AzureWebJobsStorage")] QueueMessage message)
        {
            string decodedMessage = Encoding.UTF8.GetString(Convert.FromBase64String(message.MessageText));

            using var logWriter = new StringWriter();
            _logger.LogInformation("Processing onboarding-event queue message: {Message}", decodedMessage);

            ProcessQueueMessage(decodedMessage, logWriter);
            _logger.LogInformation(logWriter.ToString());
        }

        private void ProcessQueueMessage(string message, TextWriter log)
        {
            var args = message.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (args.Length == 0)
            {
                log.WriteLine("Empty message");
                return;
            }

            switch (args[0].ToLowerInvariant())
            {
                case "onboardingevent":
                    Onboarding.HandleEvent(args, log);
                    break;
                default:
                    log.WriteLine($"Unknown command |{args[0]}|");
                    break;
            }
        }

        [Function("DocusignStatusUpdateTimer")]
        public void RunDocusignStatusUpdate([TimerTrigger("0 */5 * * * *")] TimerInfo timer)
        {
            using var logWriter = new StringWriter();

            if (DateTime.UtcNow < new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, 20, 0))
            {
                logWriter.WriteLine("Performing update for last 180 days");
                Onboarding.UpdateDocusignSignerStatus(180, logWriter);
                Onboarding.UpdateDocusignPackageStatus(180, logWriter);
            }
            else if (DateTime.UtcNow.Minute < 20)
            {
                logWriter.WriteLine("Performing update for last 45 days");
                Onboarding.UpdateDocusignSignerStatus(45, logWriter);
                Onboarding.UpdateDocusignPackageStatus(45, logWriter);
            }
            else
            {
                logWriter.WriteLine("Performing update for last 1 day");
                Onboarding.UpdateDocusignSignerStatus(logWriter);
            }

            _logger.LogInformation(logWriter.ToString());
        }
    }
}
