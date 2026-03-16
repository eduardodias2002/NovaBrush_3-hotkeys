using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Diagnostics;
using NovaBrush._02_BrushSys.Tools;


namespace _NovaBrush {
    public class CanvasPanel : FrameworkElement {
        public WriteableBitmap BitmapToDraw { get; set; }
        public MainWindow mainWindow;

        private byte[] pixelData;
        private int stride;
        private Point _lastMousePosition;
        private Point _dragOffset;
        private ScaleTransform _scaleTransform;

        private bool _isPanning = false;
        private bool _isDrawing = false;
        private Point? _lastPoint = null;
        private double _currentZoom = 1.0;
        public float PixelVal { get; set; } = 1.0f;

        public int CanvasHeight = 720;
        public int CanvasWidth = 1280;

        // Load file
        public void LoadBitmap(WriteableBitmap bitmap) {
            this.BitmapToDraw = bitmap;
            Globals.BitmapToDraw = bitmap;
            InvalidateVisual();
        }

        // Render when file opened
        protected override void OnRender(DrawingContext dc) {
            base.OnRender(dc);

            if (BitmapToDraw != null) {
                dc.DrawImage(BitmapToDraw, new Rect(0, 0, BitmapToDraw.PixelWidth * PixelVal, BitmapToDraw.PixelHeight * PixelVal));
                Globals.imageWidth = BitmapToDraw.PixelWidth;
                Globals.imageHeight = BitmapToDraw.PixelHeight;
                Globals.ImageSizeLabel.Text = $"Size: {Globals.imageWidth}px x {Globals.imageHeight}px";
            }
        }
        // The canvas panel itself
        public CanvasPanel() {
            // Find the window and attach to it once

            PreviewMouseWheel += Window_PreviewMouseWheel;

            Loaded += (_, __) =>
            {
                Window.GetWindow(this).PreviewMouseWheel += Window_PreviewMouseWheel;
                Window.GetWindow(this).MouseDown += CanvasPanel_MouseDown;
                Window.GetWindow(this).MouseMove += CanvasPanel_MouseMove;
                Window.GetWindow(this).MouseUp += CanvasPanel_MouseUp;
                Window.GetWindow(this).KeyDown += Window_KeyDown;
                Globals.BitmapToDraw = BitmapToDraw;
            };


            _scaleTransform = new ScaleTransform(_currentZoom, _currentZoom);
            RenderTransform = _scaleTransform;
            RenderTransformOrigin = new Point(0, 0);
        }

        // Zooming in and out
        private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            if (Keyboard.Modifiers == ModifierKeys.Control) {
                e.Handled = true;
                Point mousePos = e.GetPosition(Parent as UIElement);
                double oldZoom = _currentZoom;

                double worldX = (mousePos.X - Canvas.GetLeft(this)) / oldZoom;
                double worldY = (mousePos.Y - Canvas.GetTop(this)) / oldZoom;

                double zoomFactor = e.Delta > 0f ? 1.1f : 1f / 1.1f;
                _currentZoom = Math.Clamp(_currentZoom * zoomFactor, 0.1f, 10.0f);

                double newLeft = mousePos.X - (worldX * _currentZoom);
                double newTop = mousePos.Y - (worldY * _currentZoom);

                _scaleTransform.ScaleX = _currentZoom;
                _scaleTransform.ScaleY = _currentZoom;

                Canvas.SetLeft(this, newLeft);
                Canvas.SetTop(this, newTop);

                Canvas.SetLeft(Globals.CheckerboardPatternRect, newLeft);
                Canvas.SetTop(Globals.CheckerboardPatternRect, newTop);

                Globals.CheckerboardPatternRect.Width = Globals.imageWidth * _currentZoom;
                Globals.CheckerboardPatternRect.Height = Globals.imageHeight * _currentZoom; // Scale up and down

                Globals.ZoomLabel.Text = $"Zoom: {Math.Round(_currentZoom * 100, 2)} %";

            }
        }

        // When middle mouse is pressed, un/set panning mode
        private void CanvasPanel_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                _dragOffset = e.GetPosition(Parent as UIElement);
                _isDrawing = true;
                Mouse.Capture(this);
            }

            if (e.MiddleButton == MouseButtonState.Pressed) {
                _dragOffset = e.GetPosition(this);
                _isPanning = !_isPanning;
                Mouse.Capture(this);
            }
        }

        // While moving and panning...
        private void CanvasPanel_MouseMove(object sender, MouseEventArgs e) {
            Point worldPos = e.GetPosition(this);
            Globals.MousePositionLabel.Text = $"X: {(int)worldPos.X}  Y: {(int)worldPos.Y}";

            if (_isPanning)
            {
                Point mousePos = e.GetPosition(Parent as UIElement);

                double newLeft = mousePos.X - _dragOffset.X * (_currentZoom);
                double newTop = mousePos.Y - _dragOffset.Y * (_currentZoom);

                double clampNewLeft = Math.Clamp(newLeft, (0) - BitmapToDraw.PixelWidth * (_currentZoom), Globals.CanvasPanelLabel.ActualWidth);
                double clampNewTop = Math.Clamp(newTop, (0) - BitmapToDraw.PixelHeight * (_currentZoom), Globals.CanvasPanelLabel.ActualHeight); // Temp panning limit

                Canvas.SetLeft(this, (int)clampNewLeft);
                Canvas.SetTop(this, (int)clampNewTop);

                Canvas.SetLeft(Globals.CheckerboardPatternRect, newLeft);
                Canvas.SetTop(Globals.CheckerboardPatternRect, newTop);
            }

            if (_isDrawing)
            {
                Point pos = e.GetPosition(this);
                int x = (int)pos.X;
                int y = (int)pos.Y;

                if (_lastPoint.HasValue)
                {
                    // draw a line instead of a single pixel
                    Globals.CurrentTool?.DrawLine(
                        Globals.BitmapToDraw,
                        (int)_lastPoint.Value.X,
                        (int)_lastPoint.Value.Y,
                        x, y,
                        Globals.curColor // or Globals.CurrentColor
                    );
                }
                else
                {
                    Globals.CurrentTool?.SetPixel(Globals.BitmapToDraw, x, y, Globals.curColor);
                }

                _lastPoint = pos;
            }
        }


        // When the mouse is released
        private void CanvasPanel_MouseUp(object sender, MouseButtonEventArgs e) {

            if (e.ChangedButton == MouseButton.Left)
            {
                _isDrawing = false;
                _lastPoint = null;
                Mouse.Capture(null);
            }

            if (e.ChangedButton == MouseButton.Middle)
            {
                _isPanning = false;
                Mouse.Capture(null);
            }
        }
    
        // Prepares the canvas, color palette and such
        public void prepareCanvas(){
            BitmapToDraw = new WriteableBitmap(
            CanvasWidth,
            CanvasHeight,
            96, // DPI X
            96, // DPI Y
            PixelFormats.Bgra32, // 32-bit color (8 bits per channel)
            null
            );

            FillCanvasWithColor(0xFFFFFFFF); // Fully white
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);
        }

        // Fills every pixel with the #FFFFFF
        public void FillCanvasWithColor(uint argbColor){
            BitmapToDraw.Lock();

            unsafe
            {
                uint* pBackBuffer = (uint*)BitmapToDraw.BackBuffer;

                for (int y = 0; y < CanvasHeight; y++) // 32px tall
                {
                    for (int x = 0; x < CanvasWidth; x++) //32px wide, so a 32x32 canvas for now
                    {
                        // Calculate index: row * width + column
                        int index = y * CanvasWidth + x;

                        // Write color directly to buffer
                        pBackBuffer[index] = argbColor;
                    }
                }
            }
            BitmapToDraw.AddDirtyRect(new Int32Rect(0, 0, CanvasWidth, CanvasHeight));
            BitmapToDraw.Unlock();
            Globals.imageWidth = CanvasWidth;
            Globals.imageHeight = CanvasHeight;
        }

        // Debug key
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == null)
            {
                Globals.curColor = Color.FromArgb(
                    (byte)Math.Clamp(Globals.curColor.R + 10, 0, 255),
                    Globals.curColor.R,
                    Globals.curColor.G,
                    Globals.curColor.B
                    );

                Globals.MousePositionLabel.Text = $"X: {Convert.ToString(Globals.curColor)}";
                //MessageBox.Show(Convert.ToString(Globals.curColor));
            }

        }
    }
}