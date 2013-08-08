using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

using MongoDB.Bson;
using MongoDB.Driver;

using NUnit.Framework;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MongoDbFeed.Playground {
	[TestFixture]
	public class Tests {
		private MongoCollection<MongoEvent> _events;
		private MongoCollection<MongoFeedItem> _feed;

		[SetUp]
		public void SetUp() {
			_events = Configuration.Database.GetCollection<MongoEvent>("Events");
			_feed = Configuration.Database.GetCollection<MongoFeedItem>("Feed");
		}

		[TearDown]
		public void TearDown() {
			_events = null;
			_feed = null;
		}

		[Test]
		public void CreateThousandOfActionsForSingleUser() {
			var userId = ObjectId.GenerateNewId();

			foreach (var i in Enumerable.Range(1, 1000)) {
				_events.Insert(
					new MongoEvent {
						OwnerId = userId,
						Date = DateTime.Now,
						Text = "Mamba namba " + i,
						FanOutStatus = FanOutStatus.Pending
					});
			}
		}

		[Test]
		public void CreateThousandOfActionsForMultipleUsers() {
			foreach (var i in Enumerable.Range(1, 1000)) {
				_events.Insert(
					new MongoEvent {
						OwnerId = ObjectId.GenerateNewId(),
						Date = DateTime.Now,
						Text = "Mamba namba " + i,
						FanOutStatus = FanOutStatus.Pending
					});
			}
		}

		[Test]
		public void CreateNewEventAndFanOutToFollowers() {
			var userId = ObjectId.GenerateNewId();
			var friendCount = 10000;
			var followerIds = Enumerable.Range(1, friendCount).Select(ObjectId.GenerateNewId).ToList();

			var newEvent = new MongoEvent {
				OwnerId = userId,
				Date = DateTime.Now,
				Text = "Mamba namba five",
				FanOutStatus = FanOutStatus.Pending
			};

			_events.Insert(newEvent);

			Assert.NotNull(newEvent.Id);

			var feedItems = followerIds.Select(
				followerId => new MongoFeedItem {
						EventId = newEvent.Id,
						UserId = followerId,
						Relevancy = newEvent.Date
					}).ToList();

			var preInsertCount = _feed.Count();
			
			var fanOutDurationWatch = new Stopwatch();
			fanOutDurationWatch.Start();

			

			_feed.InsertBatch(feedItems, new MongoInsertOptions {
				WriteConcern = WriteConcern.Unacknowledged
			});

			fanOutDurationWatch.Stop();

			var fanOutMillisecondsPerFriend = fanOutDurationWatch.ElapsedMilliseconds / (decimal)friendCount;

			var postInsertCount = _feed.Count();

			Assert.AreEqual(preInsertCount + feedItems.Count, postInsertCount);
			//Assert.LessOrEqual(fanOutMillisecondsPerFriend, 0.1);
		}


		[Test]
		public void RabbitMQTest() {
			var channel = Configuration.Container.Resolve<IModel>();
			var queueName = "eventFeedSistribution";
			channel.QueueDeclare(queueName, false, false, false, null);
			var input = "hello world!";
			channel.BasicPublish("", queueName, null, Encoding.UTF8.GetBytes(input));

			var consumer = new QueueingBasicConsumer(channel);
			channel.BasicConsume(queueName, true, consumer);

			var output = Encoding.UTF8.GetString(((BasicDeliverEventArgs)consumer.Queue.Dequeue()).Body);

			Assert.AreEqual(input, output);
		}
	}
}