
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;

namespace Watch.Examples.Reading
{
    public partial class WatchFaceExample
    {
        readonly Dictionary<int,DrawTool> _selections = new Dictionary<int, DrawTool>();
        readonly Dictionary<int,Border> _ui = new Dictionary<int, Border>();
        private int _counter =0;
        public WatchFaceExample()
        {
            InitializeComponent();
            _selections.Add(0, DrawTool.Pen);
            _selections.Add(1, DrawTool.Brush);
            _selections.Add(2, DrawTool.Marker);
            _selections.Add(3, DrawTool.Eraser);

            _ui.Add(0, Pen);
            _ui.Add(1, Brush);
            _ui.Add(2, Marker);
            _ui.Add(3, Eraser);
        }
        public DrawTool SelectNextMode()
        {
            _counter++;
            if (_counter > 3)
                _counter = 0;

            foreach (var ui in _ui.Values)
            {
                ui.BorderBrush = Brushes.Transparent;
            }
            _ui[_counter].BorderBrush = Brushes.Black;
            return _selections[_counter];

        }
        public DrawTool SelectPreviousMode()
        {
            _counter--;
            if (_counter < 0)
                _counter = 3;

            foreach (var ui in _ui.Values)
            {
                ui.BorderBrush = Brushes.Transparent;
            }
            _ui[_counter].BorderBrush = Brushes.Black;
            return _selections[_counter];

        }
    }
}
