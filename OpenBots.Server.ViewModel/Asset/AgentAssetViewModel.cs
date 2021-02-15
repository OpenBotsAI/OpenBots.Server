using Microsoft.AspNetCore.Http;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
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
        public IFormFile? File { get; set; }
        [Required]
        public Guid? AgentId { get; set; }
    }
}