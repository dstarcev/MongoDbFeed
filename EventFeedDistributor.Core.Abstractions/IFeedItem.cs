using System;

namespace EventFeedDistributor.Core {
	public interface IFeedItem<out TKey, out TSourceKey> {
		TKey Id { get; }
		TSourceKey SourceId { get; }
		DateTime Relevancy { get; }
	}
}