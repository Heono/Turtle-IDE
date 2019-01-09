using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Wide.Interfaces;
using Wide.Interfaces.Events;
using System.ComponentModel;
using Wide.Interfaces.Services;

namespace Turtle_IDE
{
    internal class IDEWorkspace : AbstractWorkspace
    {
        private string _document;
        private ILoggerService _logger;
        private const string _title = "Turtle IDE";

        public IDEWorkspace(IUnityContainer container, IEventAggregator eventAggregator)
            : base(container, eventAggregator)
        {
            IEventAggregator aggregator = container.Resolve<IEventAggregator>();
            aggregator.GetEvent<ActiveContentChangedEvent>().Subscribe(ContentChanged);
            _document = "";
        }

        public override ImageSource Icon
        {
            get
            {
                ImageSource imageSource = new BitmapImage(new Uri("pack://application:,,,/Turtle-IDE;component/Pycraft.ico"));
                return imageSource;
            }
        }

        public override string Title
        {
            get
            {
                string newTitle = _title;
                if (_document != "")
                {
                    newTitle += " - " + _document;
                }
                return newTitle;
            }
        }

        private ILoggerService Logger
        {
            get
            {
                if (_logger == null)
                    _logger = _container.Resolve<ILoggerService>();
                return _logger;
            }
        }

        private void ContentChanged(ContentViewModel model)
        {
            _document = model == null ? "" : model.Title;
            RaisePropertyChanged("Title");
            if (model != null)
            {
                Logger.Log("Active document changed to " + model.Title, LogCategory.Info, LogPriority.None);
            }
        }

        protected override void ModelChangedEventHandler(object sender, PropertyChangedEventArgs e)
        {
            string newValue = ActiveDocument == null ? "" : ActiveDocument.Title;
            if (_document != newValue)
            {
                _document = newValue;
                RaisePropertyChanged("Title");
                base.ModelChangedEventHandler(sender, e);
            }
        }
    }
}
