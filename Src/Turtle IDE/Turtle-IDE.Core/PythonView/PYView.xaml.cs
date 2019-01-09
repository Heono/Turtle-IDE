using System;
using Wide.Interfaces;
using Wide.Interfaces.Services;
using System.Threading;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Highlighting;
using System.Xml;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using ICSharpCode.AvalonEdit.Rendering;
using System.Windows.Forms;
using System.Windows.Media;
using System.Text;

namespace Turtle_IDE.Core.PythonView
{
    /// <summary>
    /// PYView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PyView : IContentView
    {
        private IStatusbarService _statusbar;
        private Thread t;
        public static ICSharpCode.AvalonEdit.TextEditor editor;
        public static PythonConsoleControl.IronPythonConsoleControl console;

        public PyView(IStatusbarService statusbar)
        {
            InitializeComponent();

            this._statusbar = statusbar;
            InitializeComponent();
            textEditor.TextArea.Caret.PositionChanged += Caret_PositionChanged;
            editor = textEditor;

            try
            {
                byte[] r = Encoding.Default.GetBytes("30");
                byte[] g = Encoding.Default.GetBytes("40");
                byte[] b = Encoding.Default.GetBytes("34");
                textEditor.Background = new SolidColorBrush(Color.FromRgb(r[0], g[0], b[0]));
                IHighlightingDefinition pythonHighlighting;
                using (Stream s = typeof(PyView).Assembly.GetManifestResourceStream("Turtle_IDE.Core.Resource.Python.xshd"))
                {
                    if (s == null)
                        throw new InvalidOperationException("Could not find embedded resource");
                    using (XmlReader reader = new XmlTextReader(s))
                    {
                        pythonHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.
                            HighlightingLoader.Load(reader, HighlightingManager.Instance);
                    }
                }

                HighlightingManager.Instance.RegisterHighlighting("Python Highlighting", new string[] { ".cool" }, pythonHighlighting);
                textEditor.SyntaxHighlighting = pythonHighlighting;
                IList<IVisualLineTransformer> transformers = textEditor.TextArea.TextView.LineTransformers;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Caret_PositionChanged(object sender, EventArgs e)
        {
            Update();
        }

        private void textEditor_TextChanged(object sender, EventArgs e)
        {
            var model = this.DataContext as PyModel;
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
