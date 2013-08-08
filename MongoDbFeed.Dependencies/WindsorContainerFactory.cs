using Castle.Windsor;

using MongoDbFeed.Dependencies.Installers;

namespace MongoDbFeed.Dependencies {
	public class WindsorContainerFactory {
		public IWindsorContainer Create() {
			var container = new WindsorContainer();
			container.Install(new MongoDbInstaller());
			container.Install(new MongoDbActivityFeedServiceInstaller());
			container.Install(new RabbitMqInstaller());
			return container;
		}
	}
}