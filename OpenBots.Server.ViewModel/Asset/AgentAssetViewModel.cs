using Microsoft.AspNetCore.Http;
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
        public string? DriveName { get; set; }
        [Required]
        public Guid? AgentId { get; set; }
    }
}