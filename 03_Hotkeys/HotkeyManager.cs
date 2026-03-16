using _NovaBrush;
using NovaBrush._02_BrushSys;
using NovaBrush._02_BrushSys.Tools;
using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NovaBrush._03_Hotkeys
{
    public class HotkeyManager{
        private ToolWindow toolWindow;
        private static MainWindow mainWindow;
        public static void OnKeyPressed(KeyEventArgs e)
        {
            if (e == null) return;

            bool ctrl = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);

            switch (e.Key)
            {
                case Key.O when ctrl:
                    // Get window, get OpenFile() method
                    Application.Current.MainWindow?.Dispatcher.Invoke(() =>
                    {
                        (Application.Current.MainWindow as MainWindow)?.OpenFile();
                    });
                    e.Handled = true;
                    break;

                case Key.S when ctrl:
                    // Get window, get OpenFile() method
                    Application.Current.MainWindow?.Dispatcher.Invoke(() =>
                    {
                        (Application.Current.MainWindow as MainWindow)?.SaveFile();
                    });
                    e.Handled = true;
                    break;

                case Key.P:
                    Globals.CurrentTool = Globals.Tools["Pencil"];
                    Globals.toolWindow.HighlightActiveTool("Pencil");
                    e.Handled = true;
                    break;

                case Key.B:
                    Globals.CurrentTool = Globals.Tools["Brush"];
                    Globals.toolWindow.HighlightActiveTool("Brush");
                    e.Handled = true;
                    break;

                case Key.E:
                    Globals.CurrentTool = Globals.Tools["Eraser"];
                    Globals.toolWindow.HighlightActiveTool("Eraser");
                    e.Handled = true;
                    break;

                case Key.F4:
                    Globals.AA = !Globals.AA;
                    Globals.AntiAliasingLabel.Text = "AA: " + (Globals.AA ? "ON" : "OFF");
                    e.Handled = true;
                    break;

                case Key.F5:
                    Globals.wnd_colorPicker.Visibility = Globals.wnd_colorPicker.Visibility == Visibility.Visible
                        ? Visibility.Collapsed
                        : Visibility.Visible;
                    e.Handled = true;
                    break;

                case Key.F6:
                    Globals.wnd_Tool.Visibility = Globals.wnd_Tool.Visibility == Visibility.Visible
                        ? Visibility.Collapsed
                        : Visibility.Visible;
                    e.Handled = true;
                    break;
            }
        }
    }
}
