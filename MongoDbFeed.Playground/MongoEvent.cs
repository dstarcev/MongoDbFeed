using System;

using MongoDB.Bson;

namespace MongoDbFeed.Playground {
	public class MongoEvent {
		public ObjectId Id { get; set; }

		public ObjectId OwnerId { get; set; }

		public DateTime Date { get; set; }

		public string Text { get; set; }

		public FanOutStatus FanOutStatus { get; set; }
	}
}