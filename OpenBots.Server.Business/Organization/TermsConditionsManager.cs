using System;
using System.Linq;
using System.Threading.Tasks;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Identity;

namespace OpenBots.Server.Business
{
    public class TermsConditionsManager : BaseManager, ITermsConditionsManager
    {

        public TermsConditionsManager() : base()
        {
        }

        public async Task<UserAgreement> GetUserAgreement()
        {
            UserAgreement userAgreement = new UserAgreement()
            {
                Id = Guid.Empty,
                ExpiresOnUTC = DateTime.Today.AddDays(364),
                EffectiveOnUTC = DateTime.Today.AddDays(-1),
                Title = "Terms & Conditions",
                ContentStaticUrl = "",
                IsDeleted = false
            };
            return userAgreement;
        }

        public async Task<bool> IsAccepted(Guid userAgreementId, Guid personId)
        {
            return true;
        }
    }
}
