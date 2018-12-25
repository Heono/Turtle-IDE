using System;
using Wide.Interfaces;
using Wide.Interfaces.Services;
using System.Threading;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Highlighting;
using System.Xml;

namespace Turtle_IDE.Core
{
    /// <summary>
    /// MDView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PYView : IContentView
    {
        private IStatusbarService _statusbar;
        private Thread t;
        public static ICSharpCode.AvalonEdit.TextEditor editor;
        public static PythonConsoleControl.IronPythonConsoleControl console;

        public PYView(IStatusbarService statusbar)
        {
            this._statusbar = statusbar;
            InitializeComponent();
            textEditor.TextArea.Caret.PositionChanged += Caret_PositionChanged;
            editor = textEditor;
            console = pythonConsole;

            //textEditor.SyntaxHighlighting = HighlightingLoader.Load(new XmlTextReader(Environment.CurrentDirectory + "\\Python-Mode.xshd"), HighlightingManager.Instance);
        }

        private void Caret_PositionChanged(object sender, EventArgs e)
        {
            Update();
        }

        private void textEditor_TextChanged(object sender, EventArgs e)
        {
            var model = this.DataContext as IDEModel;
            if (model != null)
            {
                // Add autocomplete 
            }
        }

        private void Update()
        {
            _statusbar.LineNumber = textEditor.Document.GetLineByOffset(textEditor.CaretOffset).LineNumber;
            _statusbar.ColPosition = textEditor.TextArea.Caret.VisualColumn + 1;
            _statusbar.CharPosition = textEditor.CaretOffset;
            _statusbar.InsertMode = false;
            if (t == null || !t.IsAlive)
                Run();
        }

        private void Run()
        {
            t = new Thread(SimpleRun);
            t.Start();
        }

        private void SimpleRun(object obj)
        {
            uint i = 0;
            while (i < 1000)
            {
                _statusbar.Progress(true, i, 1000);
                Thread.Sleep(10);
                i++;
            }
            _statusbar.Progress(false, i, 1000);
        }
    }
}
