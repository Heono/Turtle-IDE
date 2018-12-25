using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Prism.Events;
using Wide.Interfaces.Controls;
using Wide.Interfaces.Events;
using Wide.Interfaces;

namespace Turtle_IDE.Core
{
    public sealed class SaveAsMenuItemViewModel : AbstractMenuItem
    {
        #region CTOR

        /// <summary>
        /// Initializes a new instance of the <see cref="MenuItemViewModel"/> class.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <param name="priority">The priority.</param>
        /// <param name="icon">The icon.</param>
        /// <param name="command">The command.</param>
        /// <param name="gesture">The gesture.</param>
        /// <param name="isCheckable">if set to <c>true</c> this menu acts as a checkable menu.</param>
        /// <param name="hideDisabled">if set to <c>true</c> this menu is not visible when disabled.</param>
        /// <param name="container">The container.</param>
        public SaveAsMenuItemViewModel(string header, int priority, ImageSource icon = null, ICommand command = null,
                                 KeyGesture gesture = null, bool isCheckable = false, bool hideDisabled = false,
                                 IUnityContainer container = null)
            : base(header, priority, icon, command, gesture, isCheckable, hideDisabled)
        {
            if (container != null)
            {
                IEventAggregator eventAggregator = container.Resolve<IEventAggregator>();
                eventAggregator.GetEvent<ActiveContentChangedEvent>().Subscribe(SaveAs);
            }
        }
        #endregion

        private void SaveAs(ContentViewModel cvm)
        {
            if (cvm != null)
            {
                this.Header = "Save " + cvm.Title + " As...";
            }
            else
            { this.Header = "Save As..."; }
        }
    }
}
