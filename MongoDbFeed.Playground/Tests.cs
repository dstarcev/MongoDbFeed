using System;
using System.Linq;

using MongoDB.Bson;
using MongoDB.Driver;

using NUnit.Framework;

namespace MongoDbFeed.Playground {
	[TestFixture]
	public class Tests {
		private MongoCollection _events;

		[SetUp]
		public void SetUp() {
			_events = Configuration.Database.GetCollection("Events");
		}

		[TearDown]
		public void TearDown() {
			_events = null;
		}

		[Test]
		public void CreateThousandOfActionsForSingleUser() {
			var userId = ObjectId.GenerateNewId();

			foreach (var i in Enumerable.Range(1, 1000)) {
				_events.Insert(
					new {
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
					new {
						OwnerId = ObjectId.GenerateNewId(),
						Date = DateTime.Now,
						Text = "Mamba namba " + i,
						FanOutStatus = FanOutStatus.Pending
					});
			}
		}
	}
}