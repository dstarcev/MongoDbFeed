using MongoDB.Bson;

namespace EventFeedDistributor.Core.MongoDb {
	internal class ActivityFeedRelation<TFeedKey, TSourceKey> {
		public ObjectId Id { get; set; }
		public TFeedKey FeedId { get; set; }
		public TSourceKey SourceId { get; set; }
	}
}