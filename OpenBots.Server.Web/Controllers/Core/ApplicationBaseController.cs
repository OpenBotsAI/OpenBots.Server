using System;
using System.Linq;
using System.Security.Claims;
using OpenBots.Server.Business;
using OpenBots.Server.Model.Identity;
using OpenBots.Server.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OpenBots.Server.Web;

namespace OpenBots.Server.WebAPI.Controllers
{
    /// <summary>
    /// Default base controller for controllers that are not implementing EntityController 
    /// </summary>
    public abstract class ApplicationBaseController : ControllerBase
    {
        protected UserSecurityContext SecurityContext { get; private set; }
        protected ApplicationUser applicationUser { get; set; }
        private readonly IHttpContextAccessor httpContextAccessor;
        protected readonly ApplicationIdentityUserManager userManager;
        private readonly IMembershipManager membershipManager;

        /// <summary>
        /// ApplicationBaseController constructor
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <param name="userManager"></param>
        /// <param name="membershipManager"></param>
        protected ApplicationBaseController(
            IHttpContextAccessor httpContextAccessor,
            ApplicationIdentityUserManager userManager,
            IMembershipManager membershipManager)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.userManager = userManager;
            this.membershipManager = membershipManager;
            //Initialize user security context  
            InitializeUserSecurityContext();
        }

        protected virtual void SetContext(UserSecurityContext securityContext)
        {
            SecurityContext = securityContext;
        }

        private void InitializeUserSecurityContext()
        {
            var userId = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                //Get logged in user 
                applicationUser = userManager.GetUserAsync(httpContextAccessor.HttpContext.User).Result;
                if (applicationUser != null)
                {
                    SecurityContext = new UserSecurityContext();
                    SecurityContext.PersonId = applicationUser.PersonId;
                    SecurityContext.OrganizationId = GetUserOrganization();
                    SetContext(SecurityContext);
                }
            }
        }

        private Guid[] GetUserOrganization()
        {
            var personOrgs = membershipManager.MyOrganizations(applicationUser.PersonId);

            var userOrganization = personOrgs?.Items?.Select(p=>p.Id)?.ToArray();
            return userOrganization;
        }

        public override BadRequestObjectResult BadRequest([ActionResultObjectValue] ModelStateDictionary modelState)
        {
            var problemDetails = new ServiceBadRequest(ModelState)
            {
                Status = StatusCodes.Status400BadRequest,
            };
            return base.BadRequest(problemDetails);
        }
    }
}