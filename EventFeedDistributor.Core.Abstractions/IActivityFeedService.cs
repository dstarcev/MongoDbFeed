using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventFeedDistributor.Core {
	public interface IActivityFeedService<TEventBase, in TEventKey, in TSourceKey, in TFeedKey>
		where TEventBase : IFeedItem<TEventKey, TSourceKey> {
		Task AddEvent(TEventBase @event);
		Task UpdateEvent(TEventBase @event);
		Task RemoveEvent(TEventKey eventId);
		Task AddRelation(TSourceKey sourceId, TFeedKey feedId);
		Task RemoveRelation(TSourceKey sourceId, TFeedKey feedId);
		Task<ICollection<TEventBase>> GetFeed(TFeedKey feedId, int? skip = null, int? take = null);
	}
}