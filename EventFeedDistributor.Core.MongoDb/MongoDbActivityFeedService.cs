using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace EventFeedDistributor.Core.MongoDb {
	public class MongoDbActivityFeedService<TEventBase, TEventKey, TSourceKey, TFeedKey>
		: IActivityFeedService<TEventBase, TEventKey, TSourceKey, TFeedKey>
		where TEventBase : IFeedItem<TEventKey, TSourceKey> {

		private readonly MongoCollection _events;
		private readonly MongoCollection _relations;
		private readonly MongoCollection _feed;
		private readonly WriteConcern _fanOutWriteConcern;

		public MongoDbActivityFeedService(
			MongoCollection events,
			MongoCollection relations,
			MongoCollection feed,
			WriteConcern fanOutWriteConcern) {
			Contract.Requires(events != null);
			Contract.Requires(relations != null);
			Contract.Requires(feed != null);
			Contract.Requires(fanOutWriteConcern != null);

			_events = events;
			_relations = relations;
			_feed = feed;
			_fanOutWriteConcern = fanOutWriteConcern;
		}

		public Task AddEvent(TEventBase @event) {
			return UpdateEvent(@event);
		}

		private Task AddToRelatedFeeds(TEventBase @event) {
			var relatedFeeds = _relations.FindAs<ActivityFeedRelation<TFeedKey, TSourceKey>>(
				Query.EQ("SourceId", BsonValue.Create(@event.SourceId))
			).SetFields("FeedId");

			if (relatedFeeds.Size() == 0) {
				return StubTask.Void;
			}

			var feedItems = relatedFeeds.Select(
				relation => new ActivityFeedItem<TFeedKey, TEventKey, TSourceKey> {
					FeedId = relation.FeedId,
					EventId = @event.Id,
					SourceId = @event.SourceId,
					Relevancy = @event.Relevancy
				});

			_feed.InsertBatch(feedItems, _fanOutWriteConcern).ThrowIfNotOk();

			return StubTask.Void;
		}

		public Task UpdateEvent(TEventBase @event) {
			var result = UpsertEvent(@event);
			if (result.UpdatedExisting) {
				return UpdateRelevancy(@event);
			}

			return AddToRelatedFeeds(@event);
		}

		private WriteConcernResult UpsertEvent(TEventBase @event) {
			return _events
				.Update(
					Query.EQ("_id", BsonValue.Create(@event.Id)),
					Update.Replace(@event),
					UpdateFlags.Upsert
				)
				.ThrowIfNotOk();
		}

		public Task RemoveEvent(TEventKey eventId) {
			_events.Remove(Query.EQ("_id", BsonValue.Create(eventId)));
			return RemoveFromAllFeeds(eventId);
		}

		private Task RemoveFromAllFeeds(TEventKey eventId) {
			_feed
				.Remove(Query.EQ("EventId", BsonValue.Create(eventId)), _fanOutWriteConcern)
				.ThrowIfNotOk();

			return StubTask.Void;
		}

		public Task AddRelation(TSourceKey sourceId, TFeedKey feedId) {
			var result = _relations.Update(
				Query.And(
					Query.EQ("SourceId", BsonValue.Create(sourceId)),
					Query.EQ("FeedId", BsonValue.Create(feedId))),
				Update.Replace(
					new {
						FeedId = feedId,
						SourceId = sourceId
					}),
				UpdateFlags.Upsert
			);

			if (result.UpdatedExisting) {
				return StubTask.Void;
			}

			return AddAllEventsFromSourceToFeed(sourceId, feedId);
		}

		private Task AddAllEventsFromSourceToFeed(TSourceKey sourceId, TFeedKey feedId) {
			var allEventsFromSource = _events
				.FindAs<ActivityEvent<TEventKey>>(
					Query.EQ("SourceId", BsonValue.Create(sourceId))
				)
				.SetFields("Relevancy");

			if (allEventsFromSource.Size() == 0) {
				return StubTask.Void;
			}

			var feedItems = allEventsFromSource.Select(
				@event => new ActivityFeedItem<TFeedKey, TEventKey, TSourceKey> {
					FeedId = feedId,
					EventId = @event.Id,
					SourceId = sourceId,
					Relevancy = @event.Relevancy
				});
			
			_feed.InsertBatch(feedItems, _fanOutWriteConcern).ThrowIfNotOk();

			return StubTask.Void;
		}

		public Task RemoveRelation(TSourceKey sourceId, TFeedKey feedId) {
			_relations.Remove(
				Query.And(
					Query.EQ("SourceId", BsonValue.Create(sourceId)),
					Query.EQ("FeedId", BsonValue.Create(feedId))
				)
			);

			return RemoveAllSourceEventsFromFeed(sourceId, feedId);
		}

		public Task<ICollection<TEventBase>> GetFeed(TFeedKey feedId, int? skip = null, int? take = null) {
			var query = _feed
				.FindAs<ActivityFeedItem<TFeedKey, TEventKey, TSourceKey>>(
					Query.EQ("FeedId", BsonValue.Create(feedId))
				)
				.SetSortOrder(SortBy.Descending("Relevancy"))
				.SetFields("EventId");

			if (skip != null) {
				query = query.SetSkip(skip.Value);
			}

			if (take != null) {
				query = query.SetLimit(take.Value);
			}

			var eventIds = query.Select(e => BsonValue.Create(e.EventId)).ToList();
			var result = _events.FindAs<TEventBase>(Query.In("_id", eventIds))
				.SetSortOrder(SortBy.Descending("Relevancy"))
				.ToList();

			return StubTask.Value((ICollection<TEventBase>)result);
		}

		private Task RemoveAllSourceEventsFromFeed(TSourceKey sourceId, TFeedKey feedId) {
			_feed.Remove(
				Query.And(
					Query.EQ("SourceId", BsonValue.Create(sourceId)),
					Query.EQ("FeedId", BsonValue.Create(feedId))
				)
			);

			return StubTask.Void;
		}

		private Task UpdateRelevancy<T>(T @event) where T : IFeedItem<TEventKey, TSourceKey> {
			_feed.Update(
				Query.EQ("EventId", BsonValue.Create(@event.Id)),
				Update.Set("Relevancy", @event.Relevancy),
				UpdateFlags.Multi
			);

			return StubTask.Void;
		}
	}
}