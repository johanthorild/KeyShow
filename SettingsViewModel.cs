using Avalonia.Controls;
using ReactiveUI;
using System.Reactive;

namespace KeyShow;

public class SettingsViewModel : ReactiveObject
{
    private readonly Settings _settings;
    private readonly Window _window;

    private int _displayTimeMs;
    private string _position = "TopLeft";

    public int DisplayTimeMs
    {
        get => _displayTimeMs;
        set => this.RaiseAndSetIfChanged(ref _displayTimeMs, value);
    }

    public string Position
    {
        get => _position;
        set => this.RaiseAndSetIfChanged(ref _position, value);
    }

    public ReactiveCommand<Unit, Unit> SaveCommand { get; }

    public SettingsViewModel(Settings settings, Window window)
    {
        _settings = settings;
        _window = window;
        _displayTimeMs = settings.DisplayTimeMs;
        _position = settings.Position;

        SaveCommand = ReactiveCommand.Create(SaveAndClose);
    }

    private void SaveAndClose()
    {
        _settings.DisplayTimeMs = _displayTimeMs;
        _settings.Position = _position;

        _window.Close();
    }
}
