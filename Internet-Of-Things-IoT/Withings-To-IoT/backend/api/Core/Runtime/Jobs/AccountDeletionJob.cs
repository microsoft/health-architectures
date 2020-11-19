// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Runtime.Jobs
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using H3.Core.Domain;
    using H3.Core.Models.Api;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.DurableTask;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// This class implements a background job to delete a user's account from the system.
    /// This involves clearing state from various subsystems such as FHIR and the event feed
    /// as well as deregistering with the IoMT vendors.
    /// Note that to prevent race conditions, before the account deletion starts, the user's
    /// account is moved into a deleting state during which API interactions with the account
    /// will return a HTTP 409 response.
    /// </summary>
    public class AccountDeletionJob
    {
        private static readonly Func<object, bool> DeleteAll = _ => true;

        private readonly ILogger<AccountDeletionJob> log;
        private readonly IExceptionFilter exceptionFilter;
        private readonly IFhirClient fhirClient;
        private readonly IConsentStore consentStore;
        private readonly IVendorClient vendorClient;
        private readonly IEventFeed eventFeed;

        public AccountDeletionJob(
            ILoggerFactory log,
            IExceptionFilter exceptionFilter,
            IFhirClient fhirClient,
            IConsentStore consentStore,
            IVendorClient vendorClient,
            IEventFeed eventFeed)
        {
            this.log = log.CreateLogger<AccountDeletionJob>();
            this.exceptionFilter = exceptionFilter;
            this.fhirClient = fhirClient;
            this.consentStore = consentStore;
            this.vendorClient = vendorClient;
            this.eventFeed = eventFeed;
        }

        [FunctionName(nameof(AccountDeletionWorkflow))]
        public async Task AccountDeletionWorkflow(
            [OrchestrationTrigger] IDurableOrchestrationContext orchestrationContext)
        {
            var input = orchestrationContext.GetInput<StartAccountDeletionMessage>();

            await orchestrationContext.CallActivityAsync(nameof(AccountDeletionTakeLock), input);

            await Task.WhenAll(
                orchestrationContext.CallActivityAsync(nameof(AccountDeletionFeed), input),
                orchestrationContext.CallActivityAsync(nameof(AccountDeletionFhir), input),
                orchestrationContext.CallActivityAsync(nameof(AccountDeletionVendor), input));

            await orchestrationContext.CallActivityAsync(nameof(AccountDeletionReleaseLock), input);
        }

        [FunctionName(nameof(AccountDeletionTakeLock))]
        public Task AccountDeletionTakeLock(
            [ActivityTrigger] IDurableActivityContext context,
            CancellationToken cancellationToken)
        {
            return exceptionFilter.FilterExceptions(async () =>
            {
                var message = context.GetInput<StartAccountDeletionMessage>();
                var userId = message.UserId;

                var consent = await consentStore.FetchConsent(userId, cancellationToken);

                if (consent == null)
                {
                    return;
                }

                consent.IsDeleting = true;

                await consentStore.WriteConsent(consent, cancellationToken);
            });
        }

        [FunctionName(nameof(AccountDeletionFeed))]
        public Task AccountDeletionFeed(
            [ActivityTrigger] IDurableActivityContext context,
            CancellationToken cancellationToken)
        {
            return exceptionFilter.FilterExceptions(async () =>
            {
                var message = context.GetInput<StartAccountDeletionMessage>();
                var userId = message.UserId;

                var consent = await consentStore.FetchConsent(userId, cancellationToken);

                if (consent?.FhirId == null)
                {
                    return;
                }

                await eventFeed.DeleteUser(consent.FhirId, cancellationToken);
            });
        }

        [FunctionName(nameof(AccountDeletionFhir))]
        public Task AccountDeletionFhir(
            [ActivityTrigger] IDurableActivityContext context,
            CancellationToken cancellationToken)
        {
            return exceptionFilter.FilterExceptions(async () =>
            {
                var message = context.GetInput<StartAccountDeletionMessage>();
                var userId = message.UserId;

                var consent = await consentStore.FetchConsent(userId, cancellationToken);

                if (consent?.FhirId == null)
                {
                    return;
                }

                await fhirClient.DeleteObservations(userId, consent.FhirId, DeleteAll, cancellationToken);

                await fhirClient.DeletePatient(userId, consent.FhirId, cancellationToken);
            });
        }

        [FunctionName(nameof(AccountDeletionVendor))]
        public Task AccountDeletionVendor(
            [ActivityTrigger] IDurableActivityContext context,
            CancellationToken cancellationToken)
        {
            return exceptionFilter.FilterExceptions(() =>
            {
                var message = context.GetInput<StartAccountDeletionMessage>();
                var userId = message.UserId;

                return vendorClient.DeleteAccount(userId, cancellationToken);
            });
        }

        [FunctionName(nameof(AccountDeletionReleaseLock))]
        public Task AccountDeletionReleaseLock(
            [ActivityTrigger] IDurableActivityContext context,
            CancellationToken cancellationToken)
        {
            return exceptionFilter.FilterExceptions(() =>
            {
                var message = context.GetInput<StartAccountDeletionMessage>();
                var userId = message.UserId;

                return consentStore.DeleteConsent(userId, cancellationToken);
            });
        }
    }
}
