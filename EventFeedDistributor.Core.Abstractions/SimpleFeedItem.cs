using System;

namespace EventFeedDistributor.Core {
	public class SimpleFeedItem<TKey, TSourceKey> : IFeedItem<TKey, TSourceKey> {
		public TKey Id { get; set; }
		public TSourceKey SourceId { get; set; }
		public DateTime Relevancy { get; set; }
	}
}