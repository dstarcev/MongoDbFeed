using System;
using System.Linq;

using EventFeedDistributor.Core;
using EventFeedDistributor.Core.MongoDb;

using NUnit.Framework;

namespace MongoDbFeed.Playground {
	[TestFixture]
	public class MongoDbActivityFeedServiceTests {
		private IActivityFeedService<SimpleFeedItem<string, string>, string, string, string> _service;

		[SetUp]
		public void SetUp() {
			_service =
				Configuration.Container
					.Resolve<MongoDbActivityFeedService<SimpleFeedItem<string, string>, string, string, string>>();

			Configuration.Database.GetCollection("Events").RemoveAll();
			Configuration.Database.GetCollection("Relations").RemoveAll();
			Configuration.Database.GetCollection("Feed").RemoveAll();
		}

		[TearDown]
		public void TearDown() {
			_service = null;
		}

		[Test]
		public async void Test() {
			const string sourceId = "user_0";
			
			var eventCount = 1000;

			var events = Enumerable.Range(1, eventCount)
				.Select(i => new TestFeedItem {
					Id = "event_" + i,
					Relevancy = DateTime.Now.AddMilliseconds(i),
					SourceId = sourceId,
					Description = "Mamba namba " + i
				})
				.ToList();

			const int followerCount = 100;
			var followerFeedIds = Enumerable.Range(1, followerCount)
				.Select(i => "user_" + i)
				.ToList();

			foreach (var feedIds in followerFeedIds) {
				await _service.AddRelation(sourceId, feedIds);
			}


			foreach (var @event in events) {
				await _service.AddEvent(@event);
			}

			followerFeedIds.Add("user_new");

			await _service.AddRelation("user_0", "user_new");

			await _service.RemoveRelation("user_0", "user_3");

			await _service.RemoveEvent("event_5");
			eventCount--;

			
			await _service.UpdateEvent(
				new TestFeedItem {
					Id = "event_2",
					Relevancy = DateTime.Today,
					SourceId = sourceId,
					Description = "Updated!"
				});
			

			foreach (var followerFeedId in followerFeedIds) {
				var followerFeed = await _service.GetFeed(followerFeedId);

				if (followerFeedId == "user_3") {
					Assert.AreEqual(0, followerFeed.Count);
					continue;
				}
				
				Assert.AreEqual(eventCount, followerFeed.Count);

				foreach (var @event in events) {
					var loadedEvent = followerFeed.FirstOrDefault(e => e.Id == @event.Id) as TestFeedItem;
					
					if (@event.Id == "event_5") {
						Assert.Null(loadedEvent);
					}
					else {
						Assert.NotNull(loadedEvent);
						if (loadedEvent.Id == "event_2") {
							Assert.AreEqual(loadedEvent.Relevancy, DateTime.Today);
							Assert.AreEqual(loadedEvent.Description, "Updated!");
						}
						else {
							Assert.AreEqual(loadedEvent.Description, @event.Description);
							Assert.AreEqual(loadedEvent.Relevancy, @event.Relevancy);
						}

						Assert.AreEqual(loadedEvent.SourceId, @event.SourceId);
					}
				}
			}

			var removedFollowerFeed = await _service.GetFeed("user_3");
			Assert.IsEmpty(removedFollowerFeed);
		}
	}
}