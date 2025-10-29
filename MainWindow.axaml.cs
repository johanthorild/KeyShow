using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KeyShow;

public partial class MainWindow : Window
{
    private readonly Border _border;
    private readonly TextBlock _text;
    private readonly Settings _settings;
    private readonly IKeyboardListener _keyboardListener;
    private readonly object _displayLock = new();
    private System.Threading.CancellationTokenSource? _displayCts;

    public MainWindow()
    {
        InitializeComponent();

        _border = this.FindControl<Border>("KeyBorder")!;
        _text = this.FindControl<TextBlock>("KeyText")!;

        _settings = Settings.Load();

        if (OperatingSystem.IsWindows())
        {
            _keyboardListener = new WindowsKeyboardListener();
        }
        else
        {
            _keyboardListener = new DummyKeyboardListener();
        }
        _keyboardListener.OnKeyPressed += ShowKey;
        _keyboardListener.Start();

        this.Opened += (_, _) => ApplyPosition();
    }

    private void Window_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var props = e.GetCurrentPoint(this).Properties;
        if (props.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }

    private void Window_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var props = e.GetCurrentPoint(this).Properties;

        if (props.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased)
        {
            try
            {
                var p = this.Position;
                _settings.WindowX = p.X;
                _settings.WindowY = p.Y;
                _settings.Save();
            }
            catch { }
        }
    }

    private void InitializeComponent()
    {
        Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);
    }

    private string NormalizeModifiers(KeyModifiers mods)
    {
        var parts = new List<string>();
        if (mods.HasFlag(KeyModifiers.Control)) parts.Add("CTRL");
        if (mods.HasFlag(KeyModifiers.Alt)) parts.Add("ALT");
        if (mods.HasFlag(KeyModifiers.Shift)) parts.Add("SHIFT");
        return string.Join(" + ", parts);
    }

    private async void ShowKey(KeyInfo keyInfo)
    {
        string modifiers = NormalizeModifiers(keyInfo.Modifiers);
        string displayText = !string.IsNullOrEmpty(modifiers)
            ? $"{modifiers} + {keyInfo.KeyName}"
            : keyInfo.KeyName;

        // Cancel any pending clear so rapid key presses don't get cleared
        // by an earlier ShowKey's delay. We create a fresh CTS per display
        // and cancel the previous one.
        var cts = new System.Threading.CancellationTokenSource();
        lock (_displayLock)
        {
            try { _displayCts?.Cancel(); } catch { }
            try { _displayCts?.Dispose(); } catch { }
            _displayCts = cts;
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            _text.Text = displayText;
            _border.Opacity = 1;
        });

        try
        {
            await Task.Delay(_settings.DisplayTimeMs, cts.Token);
        }
        catch (OperationCanceledException)
        {
            // A newer key was shown; do not clear.
            return;
        }

        // Only clear if this CTS is still the active one.
        lock (_displayLock)
        {
            if (!ReferenceEquals(_displayCts, cts))
                return;
            try { _displayCts?.Dispose(); } catch { }
            _displayCts = null;
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            _text.Text = string.Empty;
            _border.Opacity = 0;
        });
    }

    private async void OpenSettings_Click(object? sender, RoutedEventArgs e)
    {
        var win = new SettingsWindow(_settings);
        await win.ShowDialog(this);
        ApplyPosition();
    }

    private void Quit_Click(object? sender, RoutedEventArgs e)
    {

        try
        {
            if (_keyboardListener is WindowsKeyboardListener w)
                w.Stop();
        }
        catch { }


        Close();
    }

    private void ApplyPosition()
    {
        var screen = Screens.Primary;
        if (screen == null)
            return;
        var workArea = screen.WorkingArea;

        double windowWidth = Width;
        double windowHeight = Height;

        int x = 0, y = 0;

        if (_settings.WindowX.HasValue && _settings.WindowY.HasValue)
        {
            Position = new PixelPoint(_settings.WindowX.Value, _settings.WindowY.Value);
            return;
        }

        switch (_settings.Position)
        {
            case "TopLeft":
                x = workArea.X;
                y = workArea.Y;
                break;
            case "TopRight":
                x = workArea.X + (int)(workArea.Width - windowWidth);
                y = workArea.Y;
                break;
            case "BottomLeft":
                x = workArea.X;
                y = workArea.Y + (int)(workArea.Height - windowHeight);
                break;
            case "BottomRight":
            default:
                x = workArea.X + (int)(workArea.Width - windowWidth);
                y = workArea.Y + (int)(workArea.Height - windowHeight);
                break;
        }

        Position = new PixelPoint(x, y);
    }
}
