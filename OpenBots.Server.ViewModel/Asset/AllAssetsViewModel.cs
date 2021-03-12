using OpenBots.Server.Model;
using Microsoft.AspNetCore.Http;
using OpenBots.Server.Model.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenBots.Server.ViewModel
{
    public class AllAssetsViewModel : IViewModel<Asset, AllAssetsViewModel>
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

        public AllAssetsViewModel Map(Asset entity)
        {
            AllAssetsViewModel assetsViewModel = new AllAssetsViewModel();

            assetsViewModel.Id = entity.Id;
            assetsViewModel.Name = entity.Name;
            assetsViewModel.TextValue = entity.TextValue;
            assetsViewModel.JsonValue = entity.JsonValue;
            assetsViewModel.FileId = entity.FileId;
            assetsViewModel.Name = entity.Name;
            assetsViewModel.CreatedBy = entity.CreatedBy;
            assetsViewModel.CreatedOn = entity.CreatedOn;

            return assetsViewModel;
        }
    }
}
