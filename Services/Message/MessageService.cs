using HangfireSample.Services.Message.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WellaHealthCoreData.Models.Insurance;
using WellaHealthCoreRepository;
using WellaHealthCoreRepository.Interfaces;

namespace HangfireSample.Services.Message
{
    public class MessageService : IMessageService
    {
        private readonly IGenericRepository<InsuranceSubscription> _subscriptionRepo;

        private readonly ILogger<MessageService> _logger;

        public MessageService(IGenericRepository<InsuranceSubscription> subscriptionRepo,
            ILogger<MessageService> logger)
        {
            _subscriptionRepo = subscriptionRepo;
            _logger = logger;
        }

        public async Task<string> WelcomeMessage()
        {
            InsuranceSubscription subscription = await _subscriptionRepo.Query()
                                                                        .FirstOrDefaultAsync(subscription => subscription.Email.Equals("franklinezeji@gmail.com"));

            if (subscription == null)
            {
                _logger.LogInformation($"'{0}' was not found", nameof(subscription));
            }

            string message = $"Welcome {subscription.FirstName}, your subscription code is {subscription.SubscriptionCode}";

            //update subscription data
            subscription.Notes = message;
            subscription.DateUpdated = DateTime.UtcNow;

            await _subscriptionRepo.SaveChangesToDbAsync();

            return message;
        }

    }
}
