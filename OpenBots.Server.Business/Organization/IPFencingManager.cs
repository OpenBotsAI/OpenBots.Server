using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Identity;
using OpenBots.Server.Model.Membership;
using OpenBots.Server.Security;
using OpenBots.Server.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace OpenBots.Server.Business
{
    public class IPFencingManager : BaseManager, IIPFencingManager
    {
        private readonly IIPFencingRepository repo;
        private readonly IOrganizationSettingRepository organizationSettingRepo;
        private readonly IOrganizationManager organizationManager;
        private readonly IHttpContextAccessor _accessor;
        private readonly IAspNetUsersRepository aspNetUsersRepository;
        private readonly IOrganizationMemberRepository organizationMemberRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public IPFencingManager(IIPFencingRepository repository,
            IOrganizationSettingRepository organizationSettingRepository,
            IOrganizationManager organizationManager,
            IHttpContextAccessor accessor,
            IAspNetUsersRepository aspNetUsersRepository,
            IOrganizationMemberRepository organizationMemberRepository,
            UserManager<ApplicationUser> userManager)
        {
            repo = repository;
            _accessor = accessor;
            _userManager = userManager;
            organizationSettingRepo = organizationSettingRepository;
            this.organizationManager = organizationManager;
            this.aspNetUsersRepository = aspNetUsersRepository;
        }

        /// <summary>
        /// Checks if the rule can be added based on the organization's IPFencingMode
        /// </summary>
        /// <param name="iPFencing"></param>
        /// <returns>True if rule can be added</returns>
        public bool CanBeAdded(IPFencing iPFencing)
        {
            organizationSettingRepo.ForceIgnoreSecurity();
            var orgSettings = organizationSettingRepo.Find(0,1).Items?.
                Where(s=>s.OrganizationId == iPFencing.OrganizationId)?.FirstOrDefault();
            organizationSettingRepo.ForceSecurity();

            if (orgSettings == null)
            {
                return false;
            }

            switch (orgSettings.IPFencingMode)
            {
                // If IPFencing mode is null, then set it based on the first added record
                case null:
                    if (iPFencing.Usage == UsageType.Allow)
                    {
                        orgSettings.IPFencingMode = IPFencingMode.DenyMode;
                        organizationSettingRepo.ForceIgnoreSecurity();
                        organizationSettingRepo.Update(orgSettings);
                        organizationSettingRepo.ForceSecurity();

                        return true;
                    }
                    else
                    {
                        orgSettings.IPFencingMode = IPFencingMode.AllowMode;
                        organizationSettingRepo.ForceIgnoreSecurity();
                        organizationSettingRepo.Update(orgSettings);
                        organizationSettingRepo.ForceSecurity();

                        return true;
                    }

                //If mode is AllowMode, then only Deny type rules can be added
                case IPFencingMode.AllowMode:
                    if (iPFencing.Usage == UsageType.Deny)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                //If mode is DenyMode, then only Allow type rules can be added.
                case IPFencingMode.DenyMode:
                    if (iPFencing.Usage == UsageType.Allow)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                default:
                    return false;
            }
        }

        /// <summary>
        /// Checks if the current IPAddress matches on any IPFencing rules
        /// </summary>
        /// <param name="iPAddress"></param>
        /// <param name="ipFencingRules"></param>
        /// <param name="headers"></param>
        /// <returns>True if IP/Headers match on a rule</returns>
        public bool MatchedOnRule(IPAddress iPAddress, List<IPFencing> ipFencingRules, IHeaderDictionary headers)
        {
            bool ipMatched = false;
            bool headersMatched = true;

            if (ipFencingRules == null)
            {
                return false;
            }
            else
            {
                foreach (var rule in ipFencingRules)
                {
                    if (rule.Rule == RuleType.IPv4 || rule.Rule == RuleType.IPv6)
                    {
                         if (iPAddress.Equals(IPAddress.Parse(rule.IPAddress)))
                        {
                            ipMatched = true;
                        }
                    }
                    if (rule.Rule == RuleType.IPv4Range || rule.Rule == RuleType.IPv6Range)
                    {
                        var rangeStrings = rule.IPRange.Split('/');
                        String lowerBound = rangeStrings[0];
                        String upperBound = lowerBound.Substring(0, lowerBound.LastIndexOf(".")) + "." + rangeStrings[1];
                        IPAddressRange range = new IPAddressRange(IPAddress.Parse(lowerBound),IPAddress.Parse(upperBound));

                        if (range.IsInRange(iPAddress))
                        {
                            ipMatched = true;
                        }
                    }

                    if (rule.Rule == RuleType.Header)
                    {
                        if (rule.Usage == UsageType.Deny)
                        {
                            headersMatched = false;
                        }
                        //If Usage is allow, then check the headers
                        else if (headers.ContainsKey(rule.HeaderName))
                        {
                            if (rule.HeaderValue == headers[rule.HeaderName].ToString())
                            {
                                headersMatched = true;
                            }
                        }
                    }
                }
                if (ipMatched && headersMatched)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }        
        }

        /// <summary>
        /// Checks if the IPAddress is allowed in the IPFencing rules
        /// </summary>
        /// <param name="iPAddress"></param>
        /// <returns>True if the IP is allowed for the current organization</returns>
        public bool IsRequestAllowed(IPAddress iPAddress)
        {
            List<IPFencing> ipFencingRules = new List<IPFencing>();
            Guid? organizationId = Guid.Empty;
            var user = _accessor.HttpContext.User;
            var requestHeaders = _accessor.HttpContext.Request.Headers;

            var defaultOrg = organizationManager.GetDefaultOrganization();
            if (defaultOrg != null)
            {
                organizationId = defaultOrg.Id;
            }
            //if there is no default organization
            else if (user != null)
            {
                Guid userId = Guid.Parse(_userManager.GetUserId(user));
                var aspUser = aspNetUsersRepository.GetOne(userId);
                organizationId = organizationMemberRepository.Find(0, 1).Items?.
                    Where(o => o.PersonId == aspUser.PersonId)?.FirstOrDefault()?.Id;
            }
            else if (organizationId == null || organizationId == Guid.Empty)
            {
                ipFencingRules = repo.Find(0, 1).Items?.Where(i => i.OrganizationId == null)?.ToList();
            }
            else
            {
                ipFencingRules = repo.Find(0, 1).Items?.Where(i => i.OrganizationId == organizationId)?.ToList();
            }

            return MatchedOnRule(iPAddress, ipFencingRules, requestHeaders); 
        }
    }
}
