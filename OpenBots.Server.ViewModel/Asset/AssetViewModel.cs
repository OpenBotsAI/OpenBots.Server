using OpenBots.Server.Model;
using Microsoft.AspNetCore.Http;
using OpenBots.Server.Model.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenBots.Server.ViewModel
{
    public class AssetViewModel : IViewModel<Asset, AssetViewModel>
    {
        public Guid? Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Type { get; set; }
        public string? TextValue { get; set; }
        public double? NumberValue { get; set; }
        public string? JsonValue { get; set; }
        public Guid? FileId { get; set; }
        public long? SizeInBytes { get; set; }
        public Guid? AgentId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string AgentName { get; set; }
        public string FileName { get; set; }

        public AssetViewModel Map(Asset entity)
        {
            AssetViewModel assetsViewModel = new AssetViewModel();

            assetsViewModel.Id = entity.Id;
            assetsViewModel.Name = entity.Name;
            assetsViewModel.TextValue = entity.TextValue;
            assetsViewModel.NumberValue = entity.NumberValue;
            assetsViewModel.JsonValue = entity.JsonValue;
            assetsViewModel.FileId = entity.FileId;
            assetsViewModel.SizeInBytes = entity.SizeInBytes;
            assetsViewModel.AgentId = entity.AgentId;
            assetsViewModel.Name = entity.Name;
            assetsViewModel.CreatedBy = entity.CreatedBy;
            assetsViewModel.CreatedOn = entity.CreatedOn;

            return assetsViewModel;
        }
    }
}
