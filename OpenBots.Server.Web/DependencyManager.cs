using OpenBots.Server.Business;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Infrastructure.Azure.Email;
using OpenBots.Server.Infrastructure.Email;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OpenBots.Server.Web.Hubs;
using OpenBots.Server.Model.Configuration;

namespace OpenBots.Server.Web
{
    public static class DependencyManager
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            //Repositories and Managers
            services.AddTransient(typeof(IAccessRequestRepository), typeof(AccessRequestRepository));
            services.AddTransient(typeof(IOrganizationRepository), typeof(OrganizationRepository));

            services.AddTransient(typeof(IOrganizationUnitRepository), typeof(OrganizationUnitRepository));
            services.AddTransient(typeof(IOrganizationMemberRepository), typeof(OrganizationMemberRepository));
            services.AddTransient(typeof(IOrganizationUnitMemberRepository), typeof(OrganizationUnitMemberRepository));
            services.AddTransient(typeof(IOrganizationSettingRepository), typeof(OrganizationSettingRepository));

            services.AddTransient(typeof(IPersonRepository), typeof(PersonRepository));
            services.AddTransient(typeof(IPersonEmailRepository), typeof(PersonEmailRepository));
            services.AddTransient(typeof(IEmailVerificationRepository), typeof(EmailVerificationRepository));
            services.AddTransient(typeof(IAspNetUsersRepository), typeof(AspNetUsersRepository));
            services.AddTransient(typeof(IProcessRepository), typeof(ProcessRepository));
            services.AddTransient(typeof(IScheduleRepository), typeof(ScheduleRepository));


            services.AddTransient(typeof(IMembershipManager), typeof(MembershipManager));
            services.AddTransient(typeof(IAccessRequestsManager), typeof(AccessRequestsManager));
            services.AddTransient(typeof(ITermsConditionsManager), typeof(TermsConditionsManager));
            services.AddTransient(typeof(IPasswordPolicyRepository), typeof(PasswordPolicyRepository));
            services.AddTransient(typeof(IOrganizationManager), typeof(OrganizationManager));
            services.AddTransient(typeof(IProcessManager), typeof(ProcessManager));
            services.AddTransient(typeof(IScheduleManager), typeof(ScheduleManager));

            services.AddTransient(typeof(ILookupValueRepository), typeof(LookupValueRepository));
            services.AddTransient(typeof(IApplicationVersionRepository), typeof(ApplicationVersionRepository));
            services.AddTransient(typeof(IQueueItemRepository), typeof(QueueItemRepository));
            services.AddTransient(typeof(IQueueItemManager), typeof(QueueItemManager));
            services.AddTransient(typeof(IBinaryObjectRepository), typeof(BinaryObjectRepository));
            services.AddTransient(typeof(IAgentRepository), typeof(AgentRepository));
            services.AddTransient(typeof(IAgentManager), typeof(AgentManager));
            services.AddTransient(typeof(IAssetRepository), typeof(AssetRepository));

            services.AddTransient(typeof(IJobRepository), typeof(JobRepository));
            services.AddTransient(typeof(IJobManager), typeof(JobManager));
            services.AddTransient(typeof(ICredentialRepository), typeof(CredentialRepository));
            services.AddTransient(typeof(ICredentialManager), typeof(CredentialManager));
            services.AddTransient(typeof(IProcessExecutionLogRepository), typeof(ProcessExecutionLogRepository));
            services.AddTransient(typeof(IProcessExecutionLogManager), typeof(ProcessExecutionLogManager));
            services.AddTransient(typeof(IUserAgreementRepository), typeof(UserAgreementRepository));
            services.AddTransient(typeof(IUserConsentRepository), typeof(UserConsentRepository));
            services.AddTransient(typeof(IAuditLogRepository), typeof(AuditLogRepository));
            services.AddTransient(typeof(IAuditLogManager), typeof(AuditLogManager));
            services.AddTransient(typeof(IQueueRepository), typeof(QueueRepository));
            services.AddTransient(typeof(IQueueManager), typeof(QueueManager));
            services.AddTransient(typeof(IProcessLogRepository), typeof(ProcessLogRepository));
            services.AddTransient(typeof(IProcessLogManager), typeof(ProcessLogManager));
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient(typeof(IBinaryObjectManager), typeof(BinaryObjectManager));

            //Blob Storage
            services.AddTransient(typeof(IBlobStorageAdapter), typeof(BlobStorageAdapter));
            services.AddTransient(typeof(IFileSystemAdapter), typeof(FileSystemAdapter));
            services.AddTransient(typeof(IDirectoryManager), typeof(DirectoryManager));

            //Email Services
            services.AddTransient(typeof(EmailSettings), typeof(EmailSettings));
            services.AddTransient(typeof(EmailAccount), typeof(EmailAccount));
            services.AddTransient(typeof(ISendEmailChore), typeof(AzureSendEmailChore));
            services.AddTransient(typeof(IEmailManager), typeof(EmailManager));
            services.AddTransient(typeof(IEmailAccountRepository), typeof(EmailAccountRepository));
            services.AddTransient(typeof(IEmailLogRepository), typeof(EmailLogRepository));
            services.AddTransient(typeof(IEmailSettingsRepository), typeof(EmailSettingsRepository));
            services.AddTransient(typeof(IHubManager), typeof(HubManager));
        }
    }
}
