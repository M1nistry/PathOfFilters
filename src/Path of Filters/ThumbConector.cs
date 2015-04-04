using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace PathOfFilters
{
    class ThumbConector: Thumb
    {
        public List<LineGeometry> EndLines { get; private set; }
        public List<LineGeometry> StartLines { get; private set; }

        public ThumbConector() : base() 
        {
            StartLines = new List<LineGeometry>();
            EndLines = new List<LineGeometry>();
        }
    }
}
