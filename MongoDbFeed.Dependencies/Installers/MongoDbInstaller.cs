using System;
using System.Configuration;

using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace MongoDbFeed.Dependencies.Installers {
	internal class MongoDbInstaller : IWindsorInstaller {
		public void Install(IWindsorContainer container, IConfigurationStore store) {
			var serializer = new DateTimeSerializer(new DateTimeSerializationOptions {
				Kind = DateTimeKind.Local,
				Representation = BsonType.Int64
			});
			BsonSerializer.RegisterSerializer(typeof(DateTime), serializer);

			container.Register(
				Component.For<MongoUrl>()
					.DependsOn(
						new {
							url = ConfigurationManager.AppSettings["MongoClient.Url"]
						}),
				Component.For<MongoClient>(),
				Component.For<MongoServer>()
					.UsingFactoryMethod(kernel => kernel.Resolve<MongoClient>().GetServer()),
				Component.For<MongoDatabase>()
					.UsingFactoryMethod(
						kernel => kernel.Resolve<MongoServer>()
							.GetDatabase(ConfigurationManager.AppSettings["MongoClient.Database"])
					)
				);
		}
	}
}