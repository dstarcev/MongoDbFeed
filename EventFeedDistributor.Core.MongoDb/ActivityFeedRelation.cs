namespace EventFeedDistributor.Core.MongoDb {
	internal class ActivityFeedRelation<TFeedKey, TSourceKey> {
		public TFeedKey FeedId { get; set; }
		public TSourceKey SourceId { get; set; }
	}
}