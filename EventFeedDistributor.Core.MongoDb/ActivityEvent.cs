using System;

namespace EventFeedDistributor.Core.MongoDb {
	internal class ActivityEvent<TEventKey> {
		public TEventKey Id { get; set; }
		public DateTime Relevancy { get; set; }
	}
}