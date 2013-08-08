using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

using EventFeedDistributor.Core;
using EventFeedDistributor.Core.MongoDb;

using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace MongoDbFeed.Dependencies.Installers {
	internal class MongoDbActivityFeedServiceInstaller : IWindsorInstaller {
		public void Install(IWindsorContainer container, IConfigurationStore store) {
			container.Register(
				GetCollectionComponent("Events")
					.OnCreate(
						@event => {
							// ReSharper disable once ConvertToLambdaExpression
							@event.EnsureIndex(new IndexKeysBuilder().Ascending("SourceId"));
						}),
				GetCollectionComponent("Relations")
					.OnCreate(
						relations => {
							// ReSharper disable once ConvertToLambdaExpression
							relations.EnsureIndex(new IndexKeysBuilder().Ascending("SourceId"));
						}),
				GetCollectionComponent("Feed")
					.OnCreate(
						feed => {
							feed.EnsureIndex(new IndexKeysBuilder().Ascending("FeedId").Descending("Relevancy"));
							feed.EnsureIndex(new IndexKeysBuilder().Ascending("EventId"));
						}),
				Component.For(typeof(IActivityFeedService<,,,>), typeof(MongoDbActivityFeedService<,,,>))
					.ImplementedBy(typeof(MongoDbActivityFeedService<, , ,>))
					.DependsOn(
						Dependency.OnComponent("feed", GetCollectionComponentName("Feed")),
						Dependency.OnComponent("relations", GetCollectionComponentName("Relations")),
						Dependency.OnComponent("events", GetCollectionComponentName("Events")),
						Dependency.OnValue("fanOutWriteConcern", WriteConcern.Unacknowledged)
					)
				);
		}

		private ComponentRegistration<MongoCollection> GetCollectionComponent(string collectionName) {
			return Component.For<MongoCollection>()
				.Named(GetCollectionComponentName(collectionName))
				.UsingFactoryMethod(
					kernel => kernel.Resolve<MongoDatabase>().GetCollection(collectionName)
				);
		}

		private string GetCollectionComponentName(string collectionName) {
			return "MongoCollection." + collectionName;
		}
	}
}