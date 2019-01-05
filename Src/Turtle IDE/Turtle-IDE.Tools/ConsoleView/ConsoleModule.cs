using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;
using Wide.Interfaces;
using Wide.Interfaces.Events;

namespace Turtle_IDE.Tools.ConsoleView
{
    [Module(ModuleName = "Turtle-IDE.Tools")]
    public sealed class ConsoleModule : IModule
    {
        private readonly IUnityContainer _container;

        public ConsoleModule(IUnityContainer container)
        {
            _container = container;
        }

        private IEventAggregator EventAggregator
        {
            get { return _container.Resolve<IEventAggregator>(); }
        }

        #region IModule Members

        public void Initialize()
        {
            EventAggregator.GetEvent<SplashMessageUpdateEvent>().Publish(new SplashMessageUpdateEvent
            { Message = "Loading Console Module" });
            _container.RegisterType<ConsoleViewModel>();
            IWorkspace workspace = _container.Resolve<AbstractWorkspace>();
            workspace.Tools.Add(_container.Resolve<ConsoleViewModel>());
        }

        #endregion
    }
}
