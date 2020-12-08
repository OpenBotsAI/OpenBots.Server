using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Webhooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenBots.Server.Business
{
    public class IntegrationEventSubscriptionAttemptManager: BaseManager, IIntegrationEventSubscriptionAttemptManager
    {
        private readonly IIntegrationEventSubscriptionAttemptRepository repo;
        public IntegrationEventSubscriptionAttemptManager(IIntegrationEventSubscriptionAttemptRepository repo)
        {
            this.repo = repo;
        }

        public int? SaveAndGetAttemptCount(IntegrationEventSubscriptionAttempt subscriptionAttempt, int? maxRetryCount)
        {
            int? attemptCount = 0;
            var existingAttempt = GetLastAttempt(subscriptionAttempt);

            //If no attempt exists, then this is the first attempt
            if (existingAttempt == null)
            {
                attemptCount = 1;
            }
            else
            {
                existingAttempt.Status = "Failed";
                attemptCount = existingAttempt.AttemptCounter;
                attemptCount++;

                if (existingAttempt.AttemptCounter > maxRetryCount)
                {
                    existingAttempt.Status = "FailedFataly";
                    return attemptCount;
                }
                repo.Update(existingAttempt);
            }
            subscriptionAttempt.AttemptCounter = attemptCount;
            repo.Add(subscriptionAttempt);
            return attemptCount;
        }

        public IntegrationEventSubscriptionAttempt GetLastAttempt(IntegrationEventSubscriptionAttempt subscriptionAttempt)
        {
            var result = repo.Find(0, 1).Items?
                .Where(a => a.EventLogID == subscriptionAttempt.EventLogID
           && a.IntegrationEventSubscriptionID == subscriptionAttempt.IntegrationEventSubscriptionID)?
                .OrderByDescending(a => a.AttemptCounter)
                .FirstOrDefault();

            return result;
        }
    }
}
