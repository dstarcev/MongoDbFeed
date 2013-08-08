using System;

namespace EventFeedDistributor.Core.MongoDb {
	internal class ActivityFeedItem<TFeedKey, TEventKey, TSourceKey> {
		public TFeedKey FeedId { get; set; }
		public TSourceKey SourceId { get; set; }
		public TEventKey EventId { get; set; }
		public DateTime Relevancy { get; set; }
	}
}