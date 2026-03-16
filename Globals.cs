using NovaBrush._02_BrushSys;
using NovaBrush._02_BrushSys.Tools;
using NovaBrush._03_Hotkeys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace _NovaBrush
{
    public class Globals {
        public static TextBlock MousePositionLabel { get; set; }
        public static TextBlock BrushSizeLabel { get; set; }
        public static TextBlock AntiAliasingLabel { get; set; }
        public static TextBlock ImageSizeLabel { get; set; }
        public static TextBlock ZoomLabel { get; set; }
        public static ToolWindow wnd_Tool { get; set; }
        public static ColorPicker wnd_colorPicker { get; set; }
        public static Canvas CanvasPanelLabel { get; set; }
        public static WriteableBitmap BitmapToDraw;
        public static Rectangle CheckerboardPatternRect { get; set; }
        public static HotkeyManager hotkeyManager { get; set; }
        public static ToolWindow toolWindow; 
        public static ITool itool { get; set; }
        public static int Size = 1;
        public static int imageWidth;
        public static int imageHeight;
        public static bool AA = true;
        public static ITool CurrentTool { get; set; }

        public static Color curColor { get; set; }

        public static readonly Dictionary<string, ITool> Tools = new()
        {
            { "Pencil", new bsh_pencil() },
            { "Eraser", new bsh_eraser() },
            { "Brush", new bsh_brush() },
        };

        public static void InitializeTools()
        {
            CurrentTool = Tools["Pencil"];
            CurrentTool = Tools["Eraser"];
            CurrentTool = Tools["Brush"];
        }

        public static void Initialize(MainWindow mainWindow){
            MousePositionLabel = mainWindow.lbl_MousePos;
            CanvasPanelLabel = mainWindow.MainCanvas;
            BrushSizeLabel = mainWindow.lbl_brushsize;
            AntiAliasingLabel = mainWindow.lbl_aa;
            wnd_Tool = mainWindow.wnd_Tool;
            wnd_colorPicker = mainWindow.wnd_colorPicker;
            CheckerboardPatternRect = mainWindow.ui_CheckerboardPattern;
            ImageSizeLabel = mainWindow.lbl_imageSize;
            ZoomLabel = mainWindow.lbl_zoom;
            Globals.curColor = Color.FromArgb(12, 0, 0, 0);
        }
    }
}
