using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using System;
using System.Collections.Generic;

namespace OpenBots.Server.ViewModel
{
    public class AllQueueItemsViewModel : IViewModel<QueueItemModel, AllQueueItemsViewModel>
    {
		public string Name { get; set; }
		public Guid? Id { get; set; }
		public string State { get; set; }
		public string StateMessage { get; set; }
		public bool IsLocked { get; set; }
		public Guid? LockedBy { get; set; }
		public DateTime? LockedOnUTC { get; set; }
		public DateTime? LockedUntilUTC { get; set; }
		public DateTime? LockedEndTimeUTC { get; set; }
		public DateTime? ExpireOnUTC { get; set; }
		public DateTime? PostponeUntilUTC { get; set; }
		public string? ErrorCode { get; set; }
		public string? ErrorMessage { get; set; }
		public string? ErrorSerialized { get; set; }
		public string? Source { get; set; }
		public string? Event { get; set; }
		public string? ResultJSON { get; set; }
		public List<Guid>? BinaryObjectIds { get; set; }
		public Guid? QueueId { get; set; }
		public DateTime? CreatedOn { get; set; }

		public AllQueueItemsViewModel Map(QueueItemModel entity)
		{
			AllQueueItemsViewModel queueItemViewModel = new AllQueueItemsViewModel();

			queueItemViewModel.Name = entity.Name;
			queueItemViewModel.Id = entity.Id;
			queueItemViewModel.State = entity.State;
			queueItemViewModel.StateMessage = entity.StateMessage;
			queueItemViewModel.IsLocked = entity.IsLocked;
			queueItemViewModel.LockedBy = entity.LockedBy;
			queueItemViewModel.LockedOnUTC = entity.LockedOnUTC;
			queueItemViewModel.LockedUntilUTC = entity.LockedUntilUTC;
			queueItemViewModel.LockedEndTimeUTC = entity.LockedEndTimeUTC;
			queueItemViewModel.ExpireOnUTC = entity.ExpireOnUTC;
			queueItemViewModel.PostponeUntilUTC = entity.PostponeUntilUTC;
			queueItemViewModel.ErrorCode = entity.ErrorCode;
			queueItemViewModel.ErrorMessage = entity.ErrorMessage;
			queueItemViewModel.ErrorSerialized = entity.ErrorSerialized;
			queueItemViewModel.Source = entity.Source;
			queueItemViewModel.Event = entity.Event;
			queueItemViewModel.ResultJSON = entity.ResultJSON;
			queueItemViewModel.QueueId = entity.QueueId;
			queueItemViewModel.CreatedOn = entity.CreatedOn;

			return queueItemViewModel;
		}
	}
}
