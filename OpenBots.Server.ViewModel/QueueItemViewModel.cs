#nullable enable
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using System;

namespace OpenBots.Server.ViewModel
{
	public class QueueItemViewModel : IViewModel<QueueItem, QueueItemViewModel>
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

		public QueueItemViewModel Map(QueueItem entity)
		{
			QueueItemViewModel queueItemViewModel = new QueueItemViewModel();

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

			return queueItemViewModel;
		}
	}
}
