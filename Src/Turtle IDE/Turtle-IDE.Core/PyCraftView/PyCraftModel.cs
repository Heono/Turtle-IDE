using Wide.Core.TextDocument;
using Wide.Interfaces.Services;

namespace Turtle_IDE.Core
{
    /// <summary>
    /// Class TextModel which contains the text of the document
    /// </summary>
    public class PyCraftModel : TextModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MDModel" /> class.
        /// </summary>
        /// <param name="commandManager">The injected command manager.</param>
        /// <param name="menuService">The menu service.</param>
        public PyCraftModel(ICommandManager commandManager, IMenuService menuService) : base(commandManager, menuService)
        {
        }

        internal void SetLocation(object location)
        {
            this.Location = location;
            RaisePropertyChanged("Location");
        }

        internal void SetDirty(bool value)
        {
            this.IsDirty = value;
        }
    }
}
