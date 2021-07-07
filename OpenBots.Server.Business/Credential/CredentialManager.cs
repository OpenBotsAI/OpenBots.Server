using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Configuration;
using OpenBots.Server.DataAccess.Exceptions;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Identity;
using OpenBots.Server.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenBots.Server.Business
{
    public class CredentialManager : BaseManager, ICredentialManager
    {
        private readonly ICredentialRepository _repo;
        private readonly IPersonRepository _personRepository;
        private readonly IAgentRepository _agentRepository;
        private readonly IOrganizationSettingRepository _organizationSettingRepository;
        private readonly IOrganizationManager _organizationManager;
        private readonly IConfiguration _configuration;

        public CredentialManager(ICredentialRepository repo,
            IPersonRepository personRepository, 
            IAgentRepository agentRepository,
            IOrganizationSettingRepository organizationSettingRepository,
            IOrganizationManager organizationManager,
            IConfiguration configuration)
        {
            _repo = repo;
            _personRepository = personRepository;
            _agentRepository = agentRepository;
            _organizationSettingRepository = organizationSettingRepository;
            _organizationManager = organizationManager;
            _configuration = configuration;
        }

        public bool ValidateRetrievalDate(Credential credential)//ensure current date falls within start-end date range
        {
            if (credential.StartDate != null)
            {
                if (DateTime.UtcNow < credential.StartDate)
                {
                    return false;
                }
            }

            if (credential.EndDate != null)
            {
                if (DateTime.UtcNow > credential.EndDate)
                {
                    return false;
                }
            }

            return true;
        }

        public bool ValidateStartAndEndDates(Credential credential) //validate start and end date values
        {
            if (credential.StartDate == null || credential.EndDate == null) //valid if either wasn't provided
            {
                return true;
            }
            if (credential.StartDate < credential.EndDate)
            {
                return true;
            }
            return false;
        }

        public Credential CreateGlobalCredential(GlobalCredentialViewModel request)
        {
            if (!String.IsNullOrEmpty(request.PasswordSecret))
            {
                //get encryption key
                var orgId = _organizationManager.GetDefaultOrganization().Id;
                _organizationSettingRepository.ForceIgnoreSecurity();

                var organizationKey = _organizationSettingRepository.Find(null, o => o.OrganizationId == orgId).Items.FirstOrDefault().EncryptionKey;
                var applicationKey = _configuration.GetSection("ApplicationEncryption:Key").Value;
                var encryptionKey = applicationKey + organizationKey;

                //generate salt
                request.HashSalt = CredentialHasher.CreateSalt(32); //create 32 byte salt

                //generate hash
                request.PasswordHash = CredentialHasher.GenerateSaltedHash(request.PasswordSecret, request.HashSalt);

                // Encrypt and decrypt the sample text via the Aes256CbcEncrypter class.
                request.PasswordSecret = CredentialsEncrypter.Encrypt(request.PasswordSecret, encryptionKey);               
            }

            Credential credential = new Credential();
            credential = request.Map(request);

            CredentialNameAvailability(credential);

            if (!ValidateStartAndEndDates(credential))
            {
                throw new EntityOperationException("Start and End Date are not valid");
            }

            return credential;
        }

        public Credential CreateAgentCredential(AgentCredentialViewModel request)
        {
            if (!String.IsNullOrEmpty(request.PasswordSecret))
            {
                var encryptionKey = GetEncryptionKey();

                //generate salt
                request.HashSalt = CredentialHasher.CreateSalt(32); //create 32 byte salt

                //generate hash
                request.PasswordHash = CredentialHasher.GenerateSaltedHash(request.PasswordSecret, request.HashSalt);

                // Encrypt and decrypt the sample text via the Aes256CbcEncrypter class.
                request.PasswordSecret = CredentialsEncrypter.Encrypt(request.PasswordSecret, encryptionKey);
            }

            Credential globalCredential = _repo.Find(null, a => a.Name == request.Name && a.AgentId == null).Items?.FirstOrDefault();
            Credential agentCredential = request.Map(request);

            if (globalCredential == null)
            {
                throw new EntityDoesNotExistException("No global credential exists with the given name");
            }

            CredentialNameAvailability(agentCredential);

            if (!ValidateStartAndEndDates(agentCredential))
            {
                throw new EntityOperationException("Start and End Date are not valid");
            }

            agentCredential.Provider = globalCredential.Provider;
            return agentCredential;
        }

        public Credential DeleteCredential(string id)
        {
            var existingCredential = _repo.GetOne(Guid.Parse(id));
            if (existingCredential == null)
            {
                throw new EntityDoesNotExistException ("Credential cannot be found or does not exist");
            }

            if (existingCredential.AgentId == null)//credential is a global credential
            {
                var childCredentials = _repo.Find(null, a => a.Name == existingCredential.Name && a.AgentId != null)?.Items;

                if (childCredentials.Count > 0)
                    throw new EntityOperationException("Child credentials exist for this credential, please delete those first");
            }

            return existingCredential;
        }


        public void CredentialNameAvailability(Credential request)
        {
            if (request.AgentId != null) //agent credential
            {
                var credential = _repo.Find(null, d => d.Name.ToLower(null) == request.Name.ToLower(null)
                    && d.AgentId == request.AgentId)?.Items?.FirstOrDefault();

                if (credential != null && credential.Id != request.Id)
                {
                    throw new EntityAlreadyExistsException("A credential with that name already exists for this agent");
                }
            }
            else //global credential
            {
                var credential = _repo.Find(null, d => d.Name.ToLower(null) == request.Name.ToLower(null) && d.AgentId == null)
                .Items?.FirstOrDefault();

                if (credential != null && credential.Id != request.Id)
                {
                    throw new EntityAlreadyExistsException("A global credential already exists with that name");
                }
            }
        }

        public Credential GetMatchingCredential(string credentialName)
        {
            Guid? personId = SecurityContext.PersonId;
            Person callingPerson = _personRepository.Find(null, p => p.Id == personId)?.Items?.FirstOrDefault();

            List<Credential> credentials = _repo.Find(null, a => a.Name == credentialName)?.Items;

            if (credentials.Count == 0)
            {
                throw new EntityDoesNotExistException("No credential was found that matches the provided details");
            }

            if (callingPerson.IsAgent)
            {
                Agent currentAgent = _agentRepository.Find(null, a => a.Name == callingPerson.Name)?.Items?.FirstOrDefault();
                var agentCredential = credentials.Where(a => a.AgentId == currentAgent.Id)?.FirstOrDefault();

                if (agentCredential != null)
                {
                    return agentCredential;
                }
            }
            
            var matchingCredential = credentials.Where(a => a.AgentId == null).FirstOrDefault();
            matchingCredential.PasswordSecret = GetPassword(matchingCredential);

            return matchingCredential;
        }

        public string GetPassword(Credential request)
        {
            var encryptionKey = GetEncryptionKey();
            string stringPassword = request.PasswordSecret;

            if (String.IsNullOrEmpty(stringPassword))
            {
                return "";
            }

            if (!CredentialsEncrypter.IsBase64(request.PasswordSecret))//if encryption is not in base64
            {
                //encrypt existing password
                request.HashSalt = CredentialHasher.CreateSalt(32); //create 32 byte salt

                //generate hash
                request.PasswordHash = CredentialHasher.GenerateSaltedHash(request.PasswordSecret, request.HashSalt);

                //encrypt the provided password
                request.PasswordSecret = CredentialsEncrypter.Encrypt(request.PasswordSecret, encryptionKey);

                _repo.Update(request);

                return stringPassword;
            }  
            return CredentialsEncrypter.Decrypt(request.PasswordSecret, encryptionKey);
        }

        public Credential UpdateCredential(string id, Credential request)
        {
            Guid entityId = new Guid(id);

            var existingCredential = _repo.GetOne(entityId);
            if (existingCredential == null)
            {
                throw new EntityDoesNotExistException("Credential could not be found or does not exist");
            }

            request.Id = entityId;
            CredentialNameAvailability(request);

            if (!ValidateStartAndEndDates(request))
            {
                throw new InvalidOperationException("");
            }

            existingCredential.StartDate = request.StartDate;
            existingCredential.EndDate = request.EndDate;
            existingCredential.Domain = request.Domain;
            existingCredential.UserName = request.UserName;
            existingCredential.Certificate = request.Certificate;

            if (!String.IsNullOrEmpty(request.PasswordSecret))//password is not null or empty, then set a new password
            {
                var encryptionKey = GetEncryptionKey();
                string decryptedPassword = string.Empty;

                if (CredentialsEncrypter.IsBase64(existingCredential.PasswordSecret))//if encryption is in base64
                {
                    decryptedPassword = CredentialsEncrypter.Decrypt(existingCredential.PasswordSecret, encryptionKey);
                }

                if (decryptedPassword != request.PasswordSecret)
                {
                    //generate salt
                    existingCredential.HashSalt = CredentialHasher.CreateSalt(32); //create 32 byte salt

                    //generate hash
                    existingCredential.PasswordHash = CredentialHasher.GenerateSaltedHash(request.PasswordSecret, existingCredential.HashSalt);

                    //encrypt the provided password
                    existingCredential.PasswordSecret = CredentialsEncrypter.Encrypt(request.PasswordSecret, encryptionKey);
                }
            }

            if (request.PasswordSecret == "")//if password is an empty string, then remove password fields
            {
                existingCredential.HashSalt = null;
                existingCredential.PasswordHash = null;
                existingCredential.PasswordSecret = null;
            }

            return existingCredential;
        }

        public string GetEncryptionKey()
        {
            //get encryption key
            var orgId = _organizationManager.GetDefaultOrganization().Id;
            _organizationSettingRepository.ForceIgnoreSecurity();

            var organizationKey = _organizationSettingRepository.Find(null, o => o.OrganizationId == orgId).Items.FirstOrDefault().EncryptionKey;

            if (string.IsNullOrEmpty(organizationKey))
                throw new NullReferenceException("Organization encryption key does not exist");

            var applicationKey = _configuration.GetSection("ApplicationEncryption:Key").Value;
            return applicationKey + organizationKey;
        }

        public CredentialViewModel GetCredentialDetails(Guid? credentialId)
        {
            Credential existingCredential = _repo.Find(null, c => c.Id == credentialId).Items.FirstOrDefault();

            if (existingCredential == null)
            {
                throw new EntityDoesNotExistException("No credential was found for the specified id");
            }

            CredentialViewModel credentialView = new CredentialViewModel();

            credentialView = credentialView.Map(existingCredential);
            credentialView.AgentName = _agentRepository.Find(null, a => a.Id == credentialView.AgentId).Items.FirstOrDefault()?.Name;

            return credentialView;
        }
    }
}
