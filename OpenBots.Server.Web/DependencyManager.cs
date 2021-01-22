using OpenBots.Server.Business;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Infrastructure.Azure.Email;
using OpenBots.Server.Infrastructure.Email;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OpenBots.Server.Web.Hubs;
using OpenBots.Server.Model.Configuration;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using OpenBots.Server.Web.Webhooks;
using OpenBots.Server.Business.Interfaces;
using OpenBots.Server.DataAccess.Repositories.File;
using OpenBots.Server.WebAPI.Controllers;

namespace OpenBots.Server.Web
{
    public static class DependencyManager
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            //component repositories and managers
            services.AddTransient(typeof(IAccessRequestRepository), typeof(AccessRequestRepository));
            services.AddTransient(typeof(IOrganizationRepository), typeof(OrganizationRepository));

            services.AddTransient(typeof(IOrganizationUnitRepository), typeof(OrganizationUnitRepository));
            services.AddTransient(typeof(IOrganizationMemberRepository), typeof(OrganizationMemberRepository));
            services.AddTransient(typeof(IOrganizationUnitMemberRepository), typeof(OrganizationUnitMemberRepository));
            services.AddTransient(typeof(IOrganizationSettingRepository), typeof(OrganizationSettingRepository));
            services.AddTransient(typeof(IOrganizationSettingManager), typeof(OrganizationSettingManager));

            services.AddTransient(typeof(IPersonRepository), typeof(PersonRepository));
            services.AddTransient(typeof(IPersonEmailRepository), typeof(PersonEmailRepository));
            services.AddTransient(typeof(IEmailVerificationRepository), typeof(EmailVerificationRepository));
            services.AddTransient(typeof(IAspNetUsersRepository), typeof(AspNetUsersRepository));
            services.AddTransient(typeof(IAutomationRepository), typeof(AutomationRepository));
            services.AddTransient(typeof(IScheduleRepository), typeof(ScheduleRepository));
            services.AddTransient(typeof(IScheduleManager), typeof(ScheduleManager));
            services.AddTransient(typeof(IScheduleParameterRepository), typeof(ScheduleParameterRepository));

            services.AddTransient(typeof(IMembershipManager), typeof(MembershipManager));
            services.AddTransient(typeof(IAccessRequestsManager), typeof(AccessRequestsManager));
            services.AddTransient(typeof(ITermsConditionsManager), typeof(TermsConditionsManager));
            services.AddTransient(typeof(IPasswordPolicyRepository), typeof(PasswordPolicyRepository));
            services.AddTransient(typeof(IOrganizationManager), typeof(OrganizationManager));
            services.AddTransient(typeof(IAutomationManager), typeof(AutomationManager));

            services.AddTransient(typeof(ILookupValueRepository), typeof(LookupValueRepository));
            services.AddTransient(typeof(IApplicationVersionRepository), typeof(ApplicationVersionRepository));
            services.AddTransient(typeof(IQueueItemRepository), typeof(QueueItemRepository));
            services.AddTransient(typeof(IQueueItemManager), typeof(QueueItemManager));
            services.AddTransient(typeof(IBinaryObjectRepository), typeof(BinaryObjectRepository));
            services.AddTransient(typeof(IAgentHeartbeatRepository), typeof(AgentHeartbeatRepository));
            services.AddTransient(typeof(IAgentRepository), typeof(AgentRepository));
            services.AddTransient(typeof(IAgentManager), typeof(AgentManager));
            services.AddTransient(typeof(IAssetRepository), typeof(AssetRepository));
            services.AddTransient(typeof(IAssetManager), typeof(AssetManager));

            services.AddTransient(typeof(IJobRepository), typeof(JobRepository));
            services.AddTransient(typeof(IJobManager), typeof(JobManager));
            services.AddTransient(typeof(IJobParameterRepository), typeof(JobParameterRepository));
            services.AddTransient(typeof(IJobCheckpointRepository), typeof(JobCheckpointRepository));
            services.AddTransient(typeof(ICredentialRepository), typeof(CredentialRepository));
            services.AddTransient(typeof(ICredentialManager), typeof(CredentialManager));
            services.AddTransient(typeof(IAutomationExecutionLogRepository), typeof(AutomationExecutionLogRepository));
            services.AddTransient(typeof(IAutomationExecutionLogManager), typeof(AutomationExecutionLogManager));
            services.AddTransient(typeof(IUserAgreementRepository), typeof(UserAgreementRepository));
            services.AddTransient(typeof(IUserConsentRepository), typeof(UserConsentRepository));
            services.AddTransient(typeof(IAuditLogRepository), typeof(AuditLogRepository));
            services.AddTransient(typeof(IAuditLogManager), typeof(AuditLogManager));
            services.AddTransient(typeof(IQueueRepository), typeof(QueueRepository));
            services.AddTransient(typeof(IQueueManager), typeof(QueueManager));
            services.AddTransient(typeof(IAutomationLogRepository), typeof(AutomationLogRepository));
            services.AddTransient(typeof(IAutomationLogManager), typeof(AutomationLogManager));
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient(typeof(IBinaryObjectManager), typeof(BinaryObjectManager));
            services.AddTransient(typeof(IAutomationVersionRepository), typeof(AutomationVersionRepository));
            services.AddTransient(typeof(IConfigurationValueRepository), typeof(ConfigurationValueRepository));
            services.AddTransient(typeof(IIPFencingRepository), typeof(IPFencingRepository));
            services.AddTransient(typeof(IIPFencingManager), typeof(IPFencingManager));
            services.AddTransient(typeof(IQueueItemAttachmentRepository), typeof(QueueItemAttachmentRepository));

            //webHooks
            services.AddTransient(typeof(IIntegrationEventRepository), typeof(IntegrationEventRepository));
            services.AddTransient(typeof(IIntegrationEventLogRepository), typeof(IntegrationEventLogRepository));
            services.AddTransient(typeof(IIntegrationEventSubscriptionRepository), typeof(IntegrationEventSubscriptionRepository));
            services.AddTransient(typeof(IIntegrationEventSubscriptionAttemptRepository), typeof(IntegrationEventSubscriptionAttemptRepository));
            services.AddTransient(typeof(IIntegrationEventSubscriptionAttemptManager), typeof(IntegrationEventSubscriptionAttemptManager));


            //blob storage
            services.AddTransient(typeof(IBlobStorageAdapter), typeof(BlobStorageAdapter));
            services.AddTransient(typeof(IFileSystemAdapter), typeof(FileSystemAdapter));
            services.AddTransient(typeof(IDirectoryManager), typeof(DirectoryManager));
            services.AddTransient(typeof(IWebhookPublisher), typeof(WebhookPublisher));
            services.AddTransient(typeof(IWebhookSender), typeof(WebhookSender));

            //email services
            services.AddTransient(typeof(EmailSettings), typeof(EmailSettings));
            services.AddTransient(typeof(EmailAccount), typeof(EmailAccount));
            services.AddTransient(typeof(ISendEmailChore), typeof(AzureSendEmailChore));
            services.AddTransient(typeof(IEmailManager), typeof(EmailManager));
            services.AddTransient(typeof(IEmailAccountRepository), typeof(EmailAccountRepository));
            services.AddTransient(typeof(IEmailRepository), typeof(EmailRepository));
            services.AddTransient(typeof(IEmailSettingsRepository), typeof(EmailSettingsRepository));
            services.AddTransient(typeof(IHubManager), typeof(HubManager));
            services.AddTransient(typeof(IEmailAttachmentRepository), typeof(EmailAttachmentRepository));

            //files
            services.AddTransient(typeof(IFileAttributeRepository), typeof(FileAttributeRepository));
            services.AddTransient(typeof(IServerDriveRepository), typeof(ServerDriveRepository));
            services.AddTransient(typeof(IServerFolderRepository), typeof(ServerFolderRepository));
            services.AddTransient(typeof(IServerFileRepository), typeof(ServerFileRepository));
        }
    }
}
