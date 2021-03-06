﻿using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using System;
using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.ViewModel
{
    public class AgentSettingViewModel : IViewModel<AgentSettingViewModel, AgentSetting>
    {
        [Range(30, int.MaxValue, ErrorMessage = "Please enter valid a number greater than or equal to 30")]
        public int? HeartbeatInterval { get; set; }

        [Range(5, int.MaxValue, ErrorMessage = "Please enter valid a number greater than or equal to 5")]
        public int? JobLoggingInterval { get; set; }

        public bool? VerifySslCertificate { get; set; }

        public AgentSetting Map(AgentSettingViewModel viewModel)
        {
            AgentSetting agentSetting = new AgentSetting
            {
                HeartbeatInterval = viewModel.HeartbeatInterval,
                JobLoggingInterval = viewModel.JobLoggingInterval,
                VerifySslCertificate = viewModel.VerifySslCertificate
            };

            return agentSetting;
        }
        public AgentSettingViewModel MapFromModel(AgentSetting entity)
        {
            AgentSettingViewModel viewModel = new AgentSettingViewModel
            {
                HeartbeatInterval = entity.HeartbeatInterval,
                JobLoggingInterval = entity.JobLoggingInterval,
                VerifySslCertificate = entity.VerifySslCertificate
            };

            return viewModel;
        }

    }
}


