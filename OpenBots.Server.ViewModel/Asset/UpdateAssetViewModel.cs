using Microsoft.AspNetCore.Http;
using System;

namespace OpenBots.Server.ViewModel.ViewModels
{
    public class UpdateAssetViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string? TextValue { get; set; }
        public double? NumberValue { get; set; }
        public string? JsonValue { get; set; }
        public Guid? BinaryObjectID { get; set; }
        public IFormFile? file { get; set; }
        public Guid? OrganizationId { get; set; }
    }
}
