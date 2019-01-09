using System.Windows.Controls;
using Wide.Core.TextDocument;
using Wide.Interfaces;
using Wide.Interfaces.Services;

namespace Turtle_IDE.Core.PythonView
{
    public class PyViewModel : TextViewModel
    {
        public PyViewModel(AbstractWorkspace workspace, ICommandManager commandManager, ILoggerService logger,
                           IMenuService menuService)
            : base(workspace, commandManager, logger, menuService)
        {
        }

        internal void SetModel(ContentModel model)
        {
            base.Model = model;
        }

        internal void SetView(UserControl view)
        {
            base.View = view;
        }

        internal void SetHandler(IContentHandler handler)
        {
            base.Handler = handler;
        }
    }
}
