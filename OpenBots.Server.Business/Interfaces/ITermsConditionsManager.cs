using OpenBots.Server.Model.Identity;
using System;
using System.Threading.Tasks;

namespace OpenBots.Server.Business
{
    public interface ITermsConditionsManager : IManager
    {
        Task<UserAgreement> GetUserAgreement();
        Task<bool> IsAccepted(Guid userAgreementId, Guid personId);
    }
}