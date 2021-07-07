using Microsoft.AspNetCore.Http;
using OpenBots.Server.Model;
using System;
using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.ViewModel.ViewModels
{
    public class AgentAssetViewModel
    {
        [Required]
        public string Name { get; set; }
        public string? TextValue { get; set; }
        public double? NumberValue { get; set; }
        public string? JsonValue { get; set; }
        public Guid? FileId { get; set; }
        public IFormFile? File { get; set; }
        public Guid? DriveId { get; set; }
        [Required]
        public Guid? AgentId { get; set; }

        public AgentAssetViewModel Map(Asset asset, IFormFile file, Guid? driveId)
        {
            var agentAssetView = new AgentAssetViewModel()
            {
                AgentId = asset.Id,
                DriveId = driveId,
                File = file,
                Name = asset.Name
            };

            return agentAssetView;
        }
    }
}