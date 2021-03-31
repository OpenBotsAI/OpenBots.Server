using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Webhooks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenBots.Server.ViewModel
{
    public class CreateBusinessEventViewModel : IViewModel<CreateBusinessEventViewModel, IntegrationEvent>
    {
        [Required]
        public string Name { get; set; }

        [StringLength(2048, ErrorMessage = "The Description cannot exceed 2048 characters. ")]
        public string Description { get; set; }

        [StringLength(256, ErrorMessage = "The EntityType cannot exceed 256 characters. ")]
        public string? EntityType { get; set; }

        public string? PayloadSchema { get; set; }

        public IntegrationEvent Map(CreateBusinessEventViewModel businessEventViewModel)
        {
            IntegrationEvent integrationEvent = new IntegrationEvent();

            integrationEvent.Name = businessEventViewModel.Name;
            integrationEvent.Description = businessEventViewModel.Description;
            integrationEvent.EntityType = businessEventViewModel.EntityType;
            integrationEvent.PayloadSchema = businessEventViewModel.PayloadSchema;
            integrationEvent.IsSystem = false;

            return integrationEvent;
        }
    }
}
