using System.Configuration;

using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

using MongoDB.Driver;

namespace MongoDbFeed.Dependencies.Installers {
	internal class MongoDbInstaller : IWindsorInstaller {
		public void Install(IWindsorContainer container, IConfigurationStore store) {
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