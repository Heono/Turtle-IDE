using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Wide.Interfaces;
using Wide.Interfaces.Events;
using Wide.Interfaces.Services;

namespace Turtle_IDE.Tools.ConsoleView
{
    internal class ConsoleViewModel : ToolViewModel
    {
        private readonly IUnityContainer _container;
        private readonly ConsoleModel _model;
        private readonly ConsoleView _view;
        private IWorkspace _workspace;

        public ConsoleViewModel(IUnityContainer container, AbstractWorkspace workspace)
        {
            _workspace = workspace;
            _container = container;
            Name = "Console";
            Title = "Console";
            ContentId = "Console";

            _view = new ConsoleView();
            _view.DataContext = _model;
            View = _view;

            _model = new ConsoleModel();
            Model = _model;
            IsVisible = false;
        }

        public override PaneLocation PreferredLocation
        {
            get { return PaneLocation.Bottom; }
        }
    }
}
