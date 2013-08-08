using System.Configuration;

using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

using RabbitMQ.Client;

namespace MongoDbFeed.Dependencies.Installers {
	internal class RabbitMqInstaller : IWindsorInstaller {
		public void Install(IWindsorContainer container, IConfigurationStore store) {
			container.Register(
				Component.For<ConnectionFactory>()
					.DependsOn(
						new {
							HostName = ConfigurationManager.AppSettings["RabbitMQ.HostName"]
						}),
				Component.For<IConnection>().UsingFactoryMethod(k => k.Resolve<ConnectionFactory>().CreateConnection()),
				Component.For<IModel>().UsingFactoryMethod(k => k.Resolve<IConnection>().CreateModel())
			);
		}
	}
}