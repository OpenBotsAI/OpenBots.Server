using Microsoft.AspNetCore.Http;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using System;
using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.ViewModel
{
    public class GlobalAssetViewModel : IViewModel<GlobalAssetViewModel, Asset>
    {
        public Guid? Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Type { get; set; }
        public string? TextValue { get; set; }
        public double? NumberValue { get; set; }
        public string? JsonValue { get; set; }
        public Guid? BinaryObjectID { get; set; }
        public long? SizeInBytes { get; set; }

        public Asset Map(GlobalAssetViewModel? viewModel)
        {
            Asset asset = new Asset
            {
                Id = viewModel.Id,
                Name = viewModel.Name,
                Type = viewModel.Type,
                TextValue = viewModel.TextValue,
                NumberValue = viewModel.NumberValue,
                JsonValue = viewModel.JsonValue,
                BinaryObjectID = viewModel.BinaryObjectID,
                SizeInBytes = viewModel.SizeInBytes
            };
            return asset;
        }
    }
}
