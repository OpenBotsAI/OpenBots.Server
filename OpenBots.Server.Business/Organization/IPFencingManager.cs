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
            var orgSettings = organizationSettingRepo.Find(0, 1).Items?.
                Where(s => s.OrganizationId == iPFencing.OrganizationId)?.FirstOrDefault();
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
        /// Checks if the current request matches on any IPFencing rules
        /// </summary>
        /// <param name="iPAddress"></param>
        /// <param name="ipFencingRules"></param>
        /// <param name="headers"></param>
        /// <returns>True if IP/Headers match on a rule</returns>
        public bool MatchedOnRule(IPAddress iPAddress, List<IPFencing> ipFencingRules, IHeaderDictionary headers)
        {
            bool ipMatched = false;
            bool headersMatched = true; //Headers will match unless specified by a rule

            if (ipFencingRules.Count == 0)
            {
                return false;
            }
            else
            {
                foreach (var rule in ipFencingRules)
                {
                    switch (rule.Rule)
                    {
                        //Check if IP matches rule
                        case RuleType.IPv4:
                        case RuleType.IPv6:
                            if (rule.IPAddress == null) break;
                            if (iPAddress.Equals(IPAddress.Parse(rule.IPAddress)))
                            {
                                ipMatched = true;

                                if (rule.Usage == UsageType.Deny)
                                {
                                    return true; //If rule type is deny, then return true on any match
                                }
                            }
                            break;
                        //Check if IP is in range
                        case RuleType.IPv4Range:
                        case RuleType.IPv6Range:
                            IPAddress lowerBoundIP;
                            IPAddress upperBoundIP;
                            if (rule.IPRange == null) break;

                            var rangeStrings = rule.IPRange.Split('/');
                            String lowerBound = rangeStrings[0];
                            bool isValidLowerIP = IPAddress.TryParse(lowerBound, out lowerBoundIP);

                            if (rangeStrings[1] == null || isValidLowerIP == false) break;

                            String upperBound = lowerBound.Substring(0, lowerBound.LastIndexOf(".")) + "." + rangeStrings[1];
                            bool isValidUpperIP = IPAddress.TryParse(upperBound, out upperBoundIP);

                            if (isValidUpperIP == false) break;
                            IPAddressRange range = new IPAddressRange(lowerBoundIP, upperBoundIP);

                            if (rule.Rule == RuleType.IPv6Range)
                            {
                                upperBound = lowerBound.Substring(0, lowerBound.LastIndexOf(":")) + ":" + rangeStrings[1];
                                range = new IPAddressRange(IPAddress.Parse(lowerBound), IPAddress.Parse(upperBound));
                            }

                            if (range.IsInRange(iPAddress))
                            {
                                ipMatched = true;

                                if (rule.Usage == UsageType.Deny)
                                {
                                    return true;
                                }
                            }
                            break;
                        //Check if headers match rule
                        case RuleType.Header:
                            if (headers.ContainsKey(rule.HeaderName))
                            {
                                if (rule.IPRange == null) break;
                                if (rule.HeaderValue == headers[rule.HeaderName].ToString())
                                {
                                    headersMatched = true;

                                    if (rule.Usage == UsageType.Deny)
                                    {
                                        return true; //If rule type is deny, then return true on any match
                                    }
                                }
                                else
                                {
                                    headersMatched = false;
                                }
                            }
                            else
                            {
                                headersMatched = false;
                            }
                            break;
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
            // Localhost addresses
            IPAddress localIPV4 = IPAddress.Parse("::1");
            IPAddress localIPV6 = IPAddress.Parse("127.0.0.1");

            //Ip is localhost
            if (iPAddress.Equals(localIPV4) || iPAddress.Equals(localIPV6))
            {
                return true;
            }

            List<IPFencing> ipFencingRules = new List<IPFencing>();
            Guid? organizationId = Guid.Empty;
            IPFencingMode? fencingMode = IPFencingMode.AllowMode;
            var user = _accessor.HttpContext.User;
            var requestHeaders = _accessor.HttpContext.Request.Headers;

            var defaultOrg = organizationManager.GetDefaultOrganization();
            if (defaultOrg != null)
            {
                organizationId = defaultOrg.Id;
            }
            //If there is no default organization find the current user's organization
            else if (user != null)
            {
                Guid userId = Guid.Parse(_userManager.GetUserId(user));
                var aspUser = aspNetUsersRepository.GetOne(userId);
                organizationId = organizationMemberRepository.Find(0, 1).Items?.
                    Where(o => o.PersonId == aspUser.PersonId)?.FirstOrDefault()?.Id;
            }

            //If there is no user, then use default IPFencing rules
            if (organizationId == null || organizationId == Guid.Empty)
            {
                ipFencingRules = repo.Find(0, 1).Items?.Where(i => i.OrganizationId == null)?.ToList();
            }
            else
            {
                //Get Organization Settings
                organizationSettingRepo.ForceIgnoreSecurity();
                var orgSettings = organizationSettingRepo.Find(0, 1).Items?.
                    Where(s => s.OrganizationId == organizationId)?.FirstOrDefault();
                organizationSettingRepo.ForceSecurity();

                fencingMode = orgSettings.IPFencingMode;

                if  (fencingMode == IPFencingMode.AllowMode)
                {
                    ipFencingRules = repo.Find(0, 1).Items?.Where(i => i.OrganizationId == organizationId 
                        && i.Usage == UsageType.Deny)?.ToList();
                }
                else
                {
                    ipFencingRules = repo.Find(0, 1).Items?.Where(i => i.OrganizationId == organizationId
                        && i.Usage == UsageType.Allow)?.ToList();
                }


            }

            if (fencingMode == IPFencingMode.AllowMode)
            {
                //If mode is allow, then any matched rules will be forbidden
                return !MatchedOnRule(iPAddress, ipFencingRules, requestHeaders);
            }
            else
            {
                //If mode is deny, then any matched rules will be allowed
                return MatchedOnRule(iPAddress, ipFencingRules, requestHeaders);
            }
        }
    }
}
