﻿using Microsoft.AspNetCore.JsonPatch;
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

        public CredentialManager(ICredentialRepository repo, IPersonRepository personRepository, IAgentRepository agentRepository)
        {
            _repo = repo;
            _personRepository = personRepository;
            _agentRepository = agentRepository;
        }

        public bool ValidateRetrievalDate(Credential credential) //ensure current date falls within start-end date range
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

            return agentCredential;
        }

        public Credential DeleteCredential(string id)
        {
            var existingCredential = _repo.GetOne(Guid.Parse(id));
            if (existingCredential == null)
            {
                throw new EntityDoesNotExistException ("Credential cannot be found or does not exist.");
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
            return credentials.Where(a => a.AgentId == null).FirstOrDefault();
        }
    }
}
