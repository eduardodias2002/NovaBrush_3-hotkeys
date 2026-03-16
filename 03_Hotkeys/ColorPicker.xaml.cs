using _NovaBrush;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NovaBrush._03_Hotkeys
{
    public partial class ColorPicker : UserControl
    {
        private bool _isDragging = false;
        private Point _dragStartScreenPoint;
        private Point _elementStartPosition;

        private static readonly Regex _regex = new Regex("^[0-9]+$"); // Only digits

        public ColorPicker()
        {
            InitializeComponent();
        }

        // When holding the tip
        private void Wnd_ColorWindowTip_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Mouse.Capture(this, CaptureMode.Element);

                _dragStartScreenPoint = e.GetPosition(Application.Current.MainWindow);
                _elementStartPosition = new Point(Canvas.GetLeft(this), Canvas.GetTop(this));

                _isDragging = true;
                e.Handled = true;
            }
        }

        // When moving while holding the tip
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_isDragging)
            {
                Point currentScreenPoint = e.GetPosition(Application.Current.MainWindow);

                double offsetX = currentScreenPoint.X - _dragStartScreenPoint.X;
                double offsetY = currentScreenPoint.Y - _dragStartScreenPoint.Y;

                double newLeft = _elementStartPosition.X + offsetX;
                double newTop = _elementStartPosition.Y + offsetY;

                double maxWidth = Application.Current.MainWindow.ActualWidth - this.ActualWidth;
                double maxHeight = Application.Current.MainWindow.ActualHeight - this.ActualHeight - 64;

                newLeft = Math.Clamp(newLeft, 0, maxWidth);
                newTop = Math.Clamp(newTop, 0, maxHeight);

                Canvas.SetLeft(this, newLeft);
                Canvas.SetTop(this, newTop);

            }
        }

        // Disable when left mouse releases
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            _isDragging = false;
            Mouse.Capture(null);
            e.Handled = true;
        }
    private void ToolButton_Click(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is FrameworkElement button)
            {
                string colorName = button.Tag.ToString();
                double newValue = e.NewValue;
                int newColor = (int)newValue;
                switch (colorName)
                {
                    case "Red":
                        Globals.curColor = Color.FromArgb(Globals.curColor.A, (byte)newColor, Globals.curColor.G,Globals.curColor.B);
                        ui_colorVisual.Fill = new SolidColorBrush(Color.FromRgb((byte)newColor, Globals.curColor.G, Globals.curColor.B));
                        break;
                    case "Green":
                        Globals.curColor = Color.FromArgb(Globals.curColor.A, Globals.curColor.R, (byte)newColor, Globals.curColor.B);
                        ui_colorVisual.Fill = new SolidColorBrush(Color.FromRgb(Globals.curColor.R, (byte)newColor, Globals.curColor.B));
                        break;
                    case "Blue":
                        Globals.curColor = Color.FromArgb(Globals.curColor.A, Globals.curColor.R, Globals.curColor.G, (byte)newColor);
                        ui_colorVisual.Fill = new SolidColorBrush(Color.FromRgb(Globals.curColor.R, Globals.curColor.G, (byte)newColor));
                        break;
                    case "Alpha":
                        Globals.curColor = Color.FromArgb((byte)newColor, Globals.curColor.R, Globals.curColor.G, Globals.curColor.B);
                        //ui_colorVisual.Fill = new SolidColorBrush(Color.FromArgb((byte)newColor, Globals.curColor.R, Globals.curColor.G, Globals.curColor.B));
                        break;
                }
            }
        }
    }
}
