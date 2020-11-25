#nullable enable
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using System;
using System.Collections.Generic;

namespace OpenBots.Server.ViewModel
{
	public class QueueItemViewModel : NamedEntity, IViewModel<QueueItemModel, QueueItemViewModel>
	{
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
		public Guid QueueId { get; set; }
		public string Type { get; set; }
		public string JsonType { get; set; }
		public string DataJson { get; set; }
		public Guid? LockTransactionKey { get; set; }
		public int RetryCount { get; set; }
		public int Priority { get; set; }
		public List<Guid> BinaryObjectIds { get; set; }

		public QueueItemViewModel Map(QueueItemModel entity)
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
			queueItemViewModel.ResultJSON = entity.ResultJSON;
			queueItemViewModel.CreatedBy = entity.CreatedBy;
			queueItemViewModel.CreatedOn = entity.CreatedOn;
			queueItemViewModel.DeletedBy = entity.DeletedBy;
			queueItemViewModel.DeleteOn = entity.DeleteOn;
			queueItemViewModel.IsDeleted = entity.IsDeleted;
			queueItemViewModel.Timestamp = entity.Timestamp;
			queueItemViewModel.UpdatedBy = entity.UpdatedBy;
			queueItemViewModel.UpdatedOn = entity.UpdatedOn;
			queueItemViewModel.QueueId = entity.QueueId;
			queueItemViewModel.Type = entity.Type;
			queueItemViewModel.JsonType = entity.JsonType;
			queueItemViewModel.DataJson = entity.DataJson;
			queueItemViewModel.LockTransactionKey = entity.LockTransactionKey;
			queueItemViewModel.RetryCount = entity.RetryCount;
			queueItemViewModel.Priority = entity.Priority;

			return queueItemViewModel;
		}
	}
}
