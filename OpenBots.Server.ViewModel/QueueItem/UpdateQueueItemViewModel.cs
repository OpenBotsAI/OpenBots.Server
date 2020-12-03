using Microsoft.AspNetCore.Http;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using System;
using System.Collections.Generic;

namespace OpenBots.Server.ViewModel.QueueItem
{
    public class UpdateQueueItemViewModel : IViewModel<QueueItemModel, UpdateQueueItemViewModel>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid? QueueId { get; set; }
        public string? Source { get; set; }
        public string? Event { get; set; }
        public DateTime? ExpireOnUTC { get; set; }
        public DateTime? PostponeUntilUTC { get; set; }
        public string? Type { get; set; }
        public string? DataJson { get; set; }
        public string? State { get; set; }
        public List<Guid>? BinaryObjectIds { get; set; }
        public IFormFile[]? Files { get; set; }

        public UpdateQueueItemViewModel Map(QueueItemModel entity)
        {
            UpdateQueueItemViewModel viewModel = new UpdateQueueItemViewModel()
            {
                Id = (Guid)entity.Id,
                Name = entity.Name,
                QueueId = entity.QueueId,
                Source = entity.Source,
                Event = entity.Event,
                ExpireOnUTC = entity.ExpireOnUTC,
                PostponeUntilUTC = entity.PostponeUntilUTC,
                Type = entity.Type,
                DataJson = entity.DataJson,
                State = entity.State
            };

            return viewModel;
        }
    }
}
