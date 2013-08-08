using EventFeedDistributor.Core;

namespace MongoDbFeed.Playground {
	public class TestFeedItem : SimpleFeedItem<string, string> {
		public string Description { get; set; }
	}
}