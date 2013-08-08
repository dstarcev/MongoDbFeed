using System;

using MongoDB.Bson;

namespace MongoDbFeed.Playground {
	public class MongoFeedItem {
		public ObjectId Id { get; set; }

		public ObjectId UserId { get; set; }

		public ObjectId EventId { get; set; }

		public DateTime Relevancy { get; set; }
	}
}