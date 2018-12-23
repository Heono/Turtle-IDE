using System;
using MarkdownSharp;
using Wide.Interfaces;
using Wide.Interfaces.Services;
using System.Threading;

namespace Turtle_IDE.Core
{
    /// <summary>
    /// MDView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MDView : IContentView
    {
        private Markdown _md;
        private IStatusbarService _statusbar;
        private Thread t;

        public MDView(IStatusbarService statusbar)
        {
            _md = new Markdown();
            this._statusbar = statusbar;
            InitializeComponent();
            textEditor.TextArea.Caret.PositionChanged += Caret_PositionChanged;
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
                model.SetHtml(_md.Transform(textEditor.Text));
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
