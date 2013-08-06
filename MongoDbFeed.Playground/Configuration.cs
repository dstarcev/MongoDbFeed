using Castle.Windsor;

using MongoDB.Driver;

using MongoDbFeed.Dependencies;

using NUnit.Framework;

namespace MongoDbFeed.Playground {
	[SetUpFixture]
	public class Configuration {
		public static IWindsorContainer Container { get; private set; }
		public static MongoDatabase Database { get; private set; }

		[SetUp]
		public void SetUp() {
			Container = new WindsorContainerFactory().Create();
			Database = Container.Resolve<MongoDatabase>();
		}

		[TearDown]
		public void TearDown() {
			Container.Release(Database);
			Container.Dispose();
			Container = null;
		}
	}
}