using _NovaBrush;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NovaBrush._02_BrushSys.Tools{
    public class bsh_brush : ITool {

        private const byte Pressure = 10;

        // When I hold down the mouse, it draws a line that follows the cursor
        public void DrawLine(WriteableBitmap bitmap, int x1, int y1, int x2, int y2, Color color)
        {
            if (bitmap == null) return;

            int dx = Math.Abs(x2 - x1);
            int dy = Math.Abs(y2 - y1);
            int sx = x1 < x2 ? 1 : -1;
            int sy = y1 < y2 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                SetPixel(bitmap, x1, y1, color); // Calls SetPixel method

                if (x1 == x2 && y1 == y2) break;

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x1 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y1 += sy;
                }
            }
        }

        public void SetPixel(WriteableBitmap bitmap, int x, int y, Color color)
        {
            DrawWithSize(bitmap, x, y, Globals.Size, color);
        }

        private void DrawWithSize(WriteableBitmap bitmap, int centerX, int centerY, int size, Color color)
        {
            int radius = size / 2;

            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    if (dx * dx + dy * dy <= Math.Pow(radius, 2))
                    {
                        int px = centerX + dx;
                        int py = centerY + dy;
                        if (Globals.AA == true)
                            SetPixelAA(bitmap, px, py, color);
                        else
                            SetPixelNoAA(bitmap, px, py, color);
                    }
                }
            }
        }


        // Places pixels as usual
        public void SetPixelAA(WriteableBitmap bitmap, int x, int y, Color color) {
            color = Globals.curColor; // Set color before preparing painting
            if (x < 0 || y < 0 || x >= Globals.BitmapToDraw.PixelWidth || y >= Globals.BitmapToDraw.PixelHeight) return;

            Globals.BitmapToDraw.Lock(); // reserves buffer for updates

            unsafe {
            int bytesPerPixel = (Globals.BitmapToDraw.Format.BitsPerPixel + 7) / 8;
            int stride = Globals.BitmapToDraw.BackBufferStride;
            IntPtr buffer = Globals.BitmapToDraw.BackBuffer;

            byte* pixel = (byte*)buffer + y * stride + x * bytesPerPixel;

            // get old pixel data
            byte oldB = pixel[0];
            byte oldG = pixel[1];
            byte oldR = pixel[2];
            byte oldA = pixel[3];

            // get current pixel data
            byte newR = color.R;
            byte newG = color.G;
            byte newB = color.B;
            byte newA = color.A;

            // convert alpha to 0-1.0
            double alpha = newA / 255.0;
            double invAlpha = 1.0 - alpha;

            // Blend it and then place it
            byte resultR = (byte)(newR * alpha + oldR * invAlpha);
            byte resultG = (byte)(newG * alpha + oldG * invAlpha);
            byte resultB = (byte)(newB * alpha + oldB * invAlpha);
            // Exception for painting on alpha
            byte resultA = (byte)(newA + oldA * (1 - newA / 255.0));

            pixel[0] = resultB;
            pixel[1] = resultG;
            pixel[2] = resultR;
            pixel[3] = resultA;
            }

            Globals.BitmapToDraw.AddDirtyRect(new Int32Rect(x, y, 1, 1));
            Globals.BitmapToDraw.Unlock();

            Globals.CanvasPanelLabel.InvalidateVisual(); // So it triggers an update
        }

        public void SetPixelNoAA(WriteableBitmap bitmap, int x, int y, Color color)
        {
            color = Globals.curColor; // Set color before preparing painting
            if (x < 0 || y < 0 || x >= Globals.BitmapToDraw.PixelWidth || y >= Globals.BitmapToDraw.PixelHeight) return;

            Globals.BitmapToDraw.Lock(); // reserves buffer for updates

            unsafe
            {
                int bytesPerPixel = (Globals.BitmapToDraw.Format.BitsPerPixel + 7) / 8;
                int stride = Globals.BitmapToDraw.BackBufferStride;
                IntPtr buffer = Globals.BitmapToDraw.BackBuffer;

                byte* pixel = (byte*)buffer + y * stride + x * bytesPerPixel;

                pixel[0] = color.B;
                pixel[1] = color.G;
                pixel[2] = color.R;
                pixel[3] = 255;
            }

            Globals.BitmapToDraw.AddDirtyRect(new Int32Rect(x, y, 1, 1));
            Globals.BitmapToDraw.Unlock();

            Globals.CanvasPanelLabel.InvalidateVisual(); // So it triggers an update
        }

    }
}