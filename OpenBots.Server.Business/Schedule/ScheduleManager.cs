using Hangfire;
using Microsoft.AspNetCore.JsonPatch;
using OpenBots.Server.DataAccess.Exceptions;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace OpenBots.Server.Business
{
    public class ScheduleManager : BaseManager, IScheduleManager
    {
        private readonly IScheduleRepository _repo;
        private readonly IScheduleParameterRepository _scheduleParameterRepository;
        private readonly IAgentRepository _agentRepository;
        private readonly IAutomationRepository _automationRepository;
        private readonly IAgentGroupRepository _agentGroupRepository;
        private readonly ITimeZoneIdRepository _timeZoneIdRepository;

        public ScheduleManager(IScheduleRepository repo, IScheduleParameterRepository scheduleParameterRepository, 
            IAgentRepository agentRepository,
            IAutomationRepository automationRepository,
            IAgentGroupRepository agentGroupRepository,
            ITimeZoneIdRepository timeZoneIdRepository)
        {
            _repo = repo;
            _scheduleParameterRepository = scheduleParameterRepository;
            _agentRepository = agentRepository;
            _automationRepository = automationRepository;
            _agentGroupRepository = agentGroupRepository;
            _timeZoneIdRepository = timeZoneIdRepository;
        }

        /// <summary>
        /// Takes a Schedule and returns it for addition
        /// </summary>
        /// <param name="schedule"></param>
        /// <returns>The Schedule to be added</returns>
        public Schedule AddSchedule(CreateScheduleViewModel createScheduleView)
        {
            createScheduleView.CRONExpressionTimeZone = GetTimeZoneId(createScheduleView.CRONExpressionTimeZone);
            var existingSchedule = _repo.Find(null, d => d.Name.ToLower() == createScheduleView.Name.ToLower())?.Items?.FirstOrDefault();
            if (existingSchedule != null)
            {
                throw new EntityAlreadyExistsException("Schedule name already exists");
            }
            Schedule newSchedule = createScheduleView.Map(createScheduleView); //assign request to schedule entity

            return newSchedule;
        }

        /// <summary>
        /// Updates a Schedule entity 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public Schedule UpdateSchedule(string id, CreateScheduleViewModel request)
        {
            Guid entityId = new Guid(id);

            var existingSchedule = _repo.GetOne(entityId);
            if (existingSchedule == null)
            {
                throw new EntityDoesNotExistException("No Schedule exists for the specified id");
            }

            var namedSchedule = _repo.Find(null, d => d.Name.ToLower() == request.Name.ToLower() && d.Id != entityId)?.Items?.FirstOrDefault();
            if (namedSchedule != null && namedSchedule.Id != entityId)
            {
                throw new EntityAlreadyExistsException("Schedule already exists");
            }

            existingSchedule.Name = request.Name;
            existingSchedule.AgentId = request.AgentId;
            existingSchedule.AgentGroupId = request.AgentGroupId;
            existingSchedule.CRONExpression = request.CRONExpression;
            existingSchedule.CRONExpressionTimeZone = GetTimeZoneId(request.CRONExpressionTimeZone);
            existingSchedule.LastExecution = request.LastExecution;
            existingSchedule.NextExecution = request.NextExecution;
            existingSchedule.IsDisabled = request.IsDisabled;
            existingSchedule.ProjectId = request.ProjectId;
            existingSchedule.StartingType = request.StartingType;
            existingSchedule.Status = request.Status;
            existingSchedule.ExpiryDate = request.ExpiryDate;
            existingSchedule.StartDate = request.StartDate;
            existingSchedule.AutomationId = request.AutomationId;
            existingSchedule.MaxRunningJobs = request.MaxRunningJobs;

            return existingSchedule;
        }

        /// <summary>
        /// Verifies that the patch update can be completed
        /// </summary>
        /// <param name="request"></param>
        /// <param name="id"></param>
        public void AttemptPatchUpdate(JsonPatchDocument<Schedule> request, string id)
        {
            for (int i = 0; i < request.Operations.Count; i++)
            {
                Guid entityId = new Guid(id);

                if (request.Operations[i].op.ToString().ToLower() == "replace" && request.Operations[i].path.ToString().ToLower() == "/name")
                {
                    var namedSchedule = _repo.Find(null, d => d.Name.ToLower() == request.Operations[i].value.ToString().ToLower() && d.Id != entityId)?.Items?.FirstOrDefault();
                    if (namedSchedule != null && namedSchedule.Id != entityId)
                    {
                        throw new EntityAlreadyExistsException("Schedule name already exists");
                    }
                }
            }
        }

        public PaginatedList<AllSchedulesViewModel> GetScheduleAgentsandAutomations(Predicate<AllSchedulesViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            return _repo.FindAllView(predicate, sortColumn, direction, skip, take);
        }

        public void DeleteExistingParameters(string scheduleId)
        {
            Guid entityId = new Guid(scheduleId);
            var schedulParameters = GetScheduleParameters(entityId);
            foreach (var parmeter in schedulParameters)
            {
                _scheduleParameterRepository.Delete(parmeter.Id ?? Guid.Empty);
            }
        }

        public IEnumerable<ScheduleParameter> GetScheduleParameters(Guid scheduleId)
        {
            var scheduleParameters = _scheduleParameterRepository.Find(0, 1)?.Items?.Where(p => p.ScheduleId == scheduleId);
            return scheduleParameters;
        }

        public PaginatedList<ScheduleParameter> GetScheduleParameters(string scheduleId)
        {
           return _scheduleParameterRepository.Find(null, p => p.ScheduleId == Guid.Parse(scheduleId));
        }

        public ScheduleViewModel GetScheduleViewModel(ScheduleViewModel scheduleView)
        {
            scheduleView.AgentName = _agentRepository.GetOne(scheduleView.AgentId ?? Guid.Empty)?.Name;
            scheduleView.AgentGroupName = _agentGroupRepository.GetOne(scheduleView.AgentGroupId ?? Guid.Empty)?.Name;
            scheduleView.AutomationName = _automationRepository.GetOne(scheduleView.AutomationId ?? Guid.Empty)?.Name;
            scheduleView.ScheduleParameters = GetScheduleParameters(scheduleView.Id ?? Guid.Empty);

            return scheduleView;
        }

        /// <summary>
        /// Gets the timezone id for the server's operating system
        /// </summary>
        /// <param name="cronExpression"></param>
        /// <returns>The corresponding timezone for the current operating system</returns>
        public string GetTimeZoneId(string cronExpressionTimeZone)
        {
            if (String.IsNullOrEmpty(cronExpressionTimeZone))
            {
                cronExpressionTimeZone = "UTC";
            }

            var timeZoneIds = _timeZoneIdRepository.Find(null, t => t.WindowsTimeZone == cronExpressionTimeZone || t.LinuxTimeZone == cronExpressionTimeZone).Items?.FirstOrDefault();
            if (timeZoneIds == null)
            {
                throw new InvalidOperationException("The CRONExpressionTimeZone property is not a valid time zone");
            }

            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            if (isWindows)
            {
                return timeZoneIds?.WindowsTimeZone;
            }
            else
            {
                return timeZoneIds?.LinuxTimeZone;
            }
        }
    }
}
