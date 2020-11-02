using OpenBots.Server.Model.Core;
using System;

namespace OpenBots.Server.Model
{
	/// <summary>
	/// QueueItem Model
	/// </summary>
	public class QueueItem : NamedEntity
	{
		/// <summary>
		/// Whether a QueueItem is locked by a job or not
		/// </summary>
		public bool IsLocked { get; set; }

		/// <summary>
		/// When the QueueItem was locked
		/// </summary>
		public DateTime? LockedOnUTC { get; set; }

		/// <summary>
		/// When to lock QueueItem if still being executed
		/// </summary>
		public DateTime? LockedUntilUTC { get; set; }

		/// <summary>
		/// Which Agent locked the QueueItem
		/// </summary>
		public Guid? LockedBy { get; set; }

		/// <summary>
		/// Which Queue the QueueItem belongs to
		/// </summary>
		public Guid QueueId { get; set; }

		/// <summary>
		/// Format of Data
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		/// Describes the type of item the queue is dealing with
		/// </summary>
		public string JsonType { get; set; }

		/// <summary>
		/// Data in JSON or Text format
		/// </summary>
		public string DataJson { get; set; }

		/// <summary>
		/// Failed, Expired, Successful, New
		/// </summary>
		public string State { get; set; }

		/// <summary>
		/// Message given to user after state of QueueItem was changed
		/// </summary>
		public  string StateMessage { get; set; }

		/// <summary>
		/// Guid generated when item is dequeued
		/// </summary>
		public Guid? LockTransactionKey { get; set; }

		/// <summary>
		/// Tells when QueueItem has been executed and when IsLocked has been turned back to false
		/// </summary>
		public DateTime? LockedEndTimeUTC { get; set; }
		/// <summary>
		/// Number of time a QueueItem has been retried
		/// </summary>
		public int RetryCount { get; set; }
		/// <summary>
		/// Priority of when queue item should be dequeued
		/// </summary>
		public int Priority { get; set; }
		/// <summary>
		/// DateTime the queue item will expire on
		/// </summary>
		public DateTime? ExpireOnUTC { get; set; }
        /// <summary>
        /// DateTime to postpone the queue item from being processed until
        /// </summary>
        public DateTime? PostponeUntilUTC { get; set; }
		/// <summary>
		/// Error Code received when processing a queue item
		/// </summary>
		public string? ErrorCode { get; set; }
		/// <summary>
		/// Error message received when processing a queue item
		/// </summary>
		public string? ErrorMessage { get; set; }
		/// <summary>
		/// ErrorCode and ErrorMessage serialized into JSON string
		/// </summary>
		public string? ErrorSerialized { get; set; }
		/// <summary>
		/// System of event that was raised (ex: "Employee.Onboarded")
		/// </summary>
		public string? Source { get; set; }
		/// <summary>
		/// Event raised from an application (ex: "New employee joins the company.")
		/// </summary>
		public string? Event { get; set; }
    }
}