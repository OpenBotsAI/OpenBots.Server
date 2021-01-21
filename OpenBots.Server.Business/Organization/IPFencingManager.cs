using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Membership;
using OpenBots.Server.Security;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IIPFencingRepository iPFencingRepository;

        public IPFencingManager(IIPFencingRepository repository,
            IOrganizationSettingRepository organizationSettingRepository,
            IOrganizationManager organizationManager,
            IHttpContextAccessor accessor,
            UserManager<ApplicationUser> userManager)
        {
            repo = repository;
            _accessor = accessor;
            _userManager = userManager;
            organizationSettingRepo = organizationSettingRepository;
            this.organizationManager = organizationManager;
            this.iPFencingRepository = iPFencingRepository;
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
            bool headersMatched = true; //headers will match unless specified by a rule

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
                        //check if IP matches rule
                        case RuleType.IPv4:
                        case RuleType.IPv6:
                            if (rule.IPAddress == null) break;
                            if (iPAddress.Equals(IPAddress.Parse(rule.IPAddress)))
                            {
                                ipMatched = true;

                                if (rule.Usage == UsageType.Deny)
                                {
                                    return true; //if rule type is deny, then return true on any match
                                }
                            }
                            break;
                        //check if IP is in range
                        case RuleType.IPv4Range:
                        case RuleType.IPv6Range:
                            IPAddress lowerBoundIP;
                            IPAddress upperBoundIP;
                            if (rule.IPRange == null) break;

                            var rangeStrings = rule.IPRange.Split('/');
                            String lowerBound = rangeStrings[0];
                            bool isValidLowerIP = IPAddress.TryParse(lowerBound, out lowerBoundIP);
                            bool isValidUpperIP = false;

                            if (rangeStrings.Length == 1 || isValidLowerIP == false) break; //no upper bound was specified or lower bound was an invalid IP

                            if (rule.Rule == RuleType.IPv4Range)
                            {
                                String upperBound = lowerBound.Substring(0, lowerBound.LastIndexOf(".")) + "." + rangeStrings[1];
                                isValidUpperIP = IPAddress.TryParse(upperBound, out upperBoundIP);
                            }
                            else
                            {
                                String upperBound = lowerBound.Substring(0, lowerBound.LastIndexOf(":")) + ":" + rangeStrings[1];
                                isValidUpperIP = IPAddress.TryParse(upperBound, out upperBoundIP);
                            }

                            if (isValidUpperIP == false) break;
                            IPAddressRange range = new IPAddressRange(lowerBoundIP, upperBoundIP);


                            if (range.IsInRange(iPAddress))
                            {
                                ipMatched = true;

                                if (rule.Usage == UsageType.Deny)
                                {
                                    return true;
                                }
                            }
                            break;
                        //check if headers match rule
                        case RuleType.Header:
                            if (headers.ContainsKey(rule.HeaderName))
                            {
                                if (rule.IPRange == null) break;
                                if (rule.HeaderValue == headers[rule.HeaderName].ToString())
                                {
                                    headersMatched = true;

                                    if (rule.Usage == UsageType.Deny)
                                    {
                                        return true; //if rule type is deny, then return true on any match
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
        public bool IsRequestAllowed(IPAddress iPAddress,  IPFencingMode? fencingMode = null)
        {
            //local host addresses
            IPAddress localIPV4 = IPAddress.Parse("::1");
            IPAddress localIPV6 = IPAddress.Parse("127.0.0.1");

            //IP is local host
            if (iPAddress.Equals(localIPV4) || iPAddress.Equals(localIPV6))
            {
                return true;
            }

            List<IPFencing> ipFencingRules = new List<IPFencing>();
            Guid? organizationId = Guid.Empty;
            var user = _accessor.HttpContext.User;
            var requestHeaders = _accessor.HttpContext.Request.Headers;
            Guid userId = Guid.Empty;

            var defaultOrg = organizationManager.GetDefaultOrganization();
            if (defaultOrg != null)
            {
                organizationId = defaultOrg.Id;

                organizationSettingRepo.ForceIgnoreSecurity();
                var orgSettings = organizationSettingRepo.Find(0, 1).Items?.Where(q => q.OrganizationId == organizationId).FirstOrDefault();
                organizationSettingRepo.ForceSecurity();

                if (orgSettings == null)
                {
                    fencingMode = IPFencingMode.AllowMode;
                }
                else
                    fencingMode = orgSettings.IPFencingMode;
            }
            //if there is no default organization, find the current user's organization
            else if (user != null)
            {
                string userIdStr = _userManager.GetUserId(user);
                if (string.IsNullOrEmpty(userIdStr))
                {
                    //if there is no user or organization, then use default IP fencing rules
                    if (organizationId == null || organizationId == Guid.Empty)
                    {
                        ipFencingRules = repo.Find(0, 1).Items?.Where(i => i.OrganizationId == null)?.ToList();
                        //if no organization, user, or rules exist, allow user to access the site
                        //this means the user is accessing the server application for the first time
                        if (ipFencingRules.Count == 0)
                            return true;
                        else
                        {
                            foreach (var rule in ipFencingRules)
                            {
                                if (rule.IPAddress == iPAddress.ToString() && rule.Usage == UsageType.Allow)
                                    return true;
                            }
                        }
                    }
                }
            }
            else
                return false;

            if (fencingMode == IPFencingMode.AllowMode)
            {
                ipFencingRules = repo.Find(0, 1).Items?.Where(i => i.OrganizationId == organizationId
                    && i.Usage == UsageType.Deny)?.ToList();

                //if mode is allow, then any matched rules will be forbidden
                return !MatchedOnRule(iPAddress, ipFencingRules, requestHeaders);
            }
            else
            {
                ipFencingRules = repo.Find(0, 1).Items?.Where(i => i.OrganizationId == organizationId
                    && i.Usage == UsageType.Allow)?.ToList();

                //if mode is deny, then any matched rules will be allowed
                return MatchedOnRule(iPAddress, ipFencingRules, requestHeaders);
            }
        }

        public IPFencingMode? GetIPFencingMode(Guid organizationId)
        {
            //get organization settings
            organizationSettingRepo.ForceIgnoreSecurity();
            var orgSettings = organizationSettingRepo.Find(0, 1).Items?.
                Where(s => s.OrganizationId == organizationId)?.FirstOrDefault();
            organizationSettingRepo.ForceSecurity();

            if (orgSettings == null)
            {
                throw new Exception("No organization exists for the given id");
            }
            return orgSettings.IPFencingMode;
        }
    }
}
