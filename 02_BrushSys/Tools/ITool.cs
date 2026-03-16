using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NovaBrush._02_BrushSys.Tools
{
    public interface ITool
    {   
        // Uses methods that are in common with the tools set in the Globals dictionary
        public void SetPixel(WriteableBitmap bitmap, int x, int y, Color color);
        public void DrawLine(WriteableBitmap bitmap, int x1, int y1, int x2, int y2, Color color);
    }
}
