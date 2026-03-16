using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using NovaBrush;
using NovaBrush._02_BrushSys;
using NovaBrush._02_BrushSys.Tools;
using System.Security.Cryptography;
using NovaBrush._03_Hotkeys;

namespace _NovaBrush
{

    public partial class MainWindow : Window
    {

        private CanvasPanel canvasPanel;
        private WriteableBitmap _canvasBitmap;
        private ITool itool;

        public MainWindow()
        {
            InitializeComponent();
            CreateCanvas();
            createCheckerboard();
            Globals.Initialize(this);
        }

        // If you close the window
        private void Window_Closing(object sender, CancelEventArgs e){
            MessageBoxResult result = MessageBox.Show("Do you want to save changes before closing?", "Exit", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.No){
                Application.Current.Shutdown();
            }
            else{
                SaveFile();
            }
        }

            // Places the Canvas
            public void CreateCanvas(){
            canvasPanel = new CanvasPanel { };

            // Add it to the named parent container from XAML
            MainCanvas.Children.Add(canvasPanel);
            canvasPanel.prepareCanvas();
        }

        // Get Hotkeys
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            HotkeyManager.OnKeyPressed(e); // Just one line
        }

        // Open button click
        public void btn_open_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }


        // Open the open file prompt
        public void OpenFile()
        {
            var ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.FileName = "Image"; // Default file name
            ofd.DefaultExt = ".png"; // Default file extension
            ofd.Filter = ".Image Files (*.png)|*.png"; // Filter files by extension

            bool? result = ofd.ShowDialog();

            if (result == true)
            {
                try
                {
                    string filename = ofd.FileName;

                    BitmapImage bitmapImage = new BitmapImage(new Uri(filename)); // This makes the image support alpha
                    var convertedBitmap = new FormatConvertedBitmap(
                        bitmapImage,
                        PixelFormats.Bgra32,
                        null,
                        0);

                    WriteableBitmap writable = new WriteableBitmap(convertedBitmap);

                    canvasPanel.LoadBitmap(writable);

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        // Hide/Unhide Function, Reuse it
        private void ToggleVisibility_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement button)
            {
                if (button.Tag is UIElement target)
                {
                    target.Visibility = target.Visibility == Visibility.Visible
                        ? Visibility.Collapsed
                        : Visibility.Visible;
                }
            }
        }

        // If you shift the slider
        private void sdr_brushsize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double newValue = e.NewValue;
            int size = (int)newValue;

            Globals.Size = size;
            if (Globals.BrushSizeLabel != null)
            {
                Globals.BrushSizeLabel.Text = $"Size: {size}px";
            }
            
        }

        // The Anti Aliasing button
        private void btn_AA_Click(object sender, RoutedEventArgs e)
        {
            Globals.AA = !Globals.AA;
            Globals.AntiAliasingLabel.Text = "AA: " + (Globals.AA ? "ON" : "OFF");
        }

        // File saving prompt
        public void SaveFile()
        {
            var sfd = new Microsoft.Win32.SaveFileDialog();
            sfd.Filter = ".Image Files (*.png)|*.png"; // Filter files by extension
            sfd.DefaultExt = ".png"; // Default file extension

            if (sfd.ShowDialog() == true)
            {
                string filename = sfd.FileName;
                SaveBitmapToFile(Globals.BitmapToDraw, filename);
            }
        }

        public void btn_save_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }

        // Saving attempt
        private static void SaveBitmapToFile(WriteableBitmap bitmap, String filepath)
        {
            try
            {
                if (bitmap == null) throw new ArgumentNullException(nameof(bitmap));

                BitmapEncoder encoder = GetEncoder(filepath);

                //bitmap.Freeze(); //this freezes the program, hmm

                encoder.Frames.Add(BitmapFrame.Create(bitmap));

                using (var fileStream = new System.IO.FileStream(filepath, System.IO.FileMode.Create))
                {
                    encoder.Save(fileStream);
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save image: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Get extension, and then encode
        private static BitmapEncoder GetEncoder(string filepath)
        {
            string ext = System.IO.Path.GetExtension(filepath).ToLower();

            return ext switch
            {
                ".png" => new PngBitmapEncoder(),
                _ => new PngBitmapEncoder() // more file types soon
            };
        }

        private void showOnLoad(object sender, RoutedEventArgs e)
        {
            if (sender is TextBlock textBlock && textBlock.Tag is string propertyName && !string.IsNullOrEmpty(propertyName))
            {
                // Get format 
                string format = textBlock.Text ?? "{0}";

                var field = typeof(Globals).GetField(propertyName);
                object value = null;

                if (field != null){
                    value = field.GetValue(null);
                }
                else{
                    var property = typeof(Globals).GetProperty(propertyName);
                    if (property != null){
                        value = property.GetValue(null);
                    }
                    else{
                        value = "Error";
                    }
                }
                textBlock.Text = string.Format(format, value);
            }
        }

        // Generates the checkeboard
        private void createCheckerboard()
        {
            int tileSize = 8;

            var brush = new DrawingBrush();
            var drawing = new DrawingGroup();

            var light = new SolidColorBrush(Color.FromRgb(128, 128, 128));
            var dark = new SolidColorBrush(Color.FromRgb(90, 85, 85));

            drawing.Children.Add(new GeometryDrawing(light, null, new RectangleGeometry(new Rect(0, 0, tileSize, tileSize))));
            drawing.Children.Add(new GeometryDrawing(dark, null, new RectangleGeometry(new Rect(tileSize, 0, tileSize, tileSize))));
            drawing.Children.Add(new GeometryDrawing(dark, null, new RectangleGeometry(new Rect(0, tileSize, tileSize, tileSize))));
            drawing.Children.Add(new GeometryDrawing(light, null, new RectangleGeometry(new Rect(tileSize, tileSize, tileSize, tileSize))));

            brush.Drawing = drawing;
            brush.TileMode = TileMode.Tile;
            brush.Viewport = new Rect(0, 0, tileSize * 2, tileSize * 2);
            brush.ViewportUnits = BrushMappingMode.Absolute;
            brush.Stretch = Stretch.None;

            ui_CheckerboardPattern.Fill = brush;
            ui_CheckerboardPattern.Width = Globals.imageWidth;
            ui_CheckerboardPattern.Height = Globals.imageHeight;
        }
    }
}