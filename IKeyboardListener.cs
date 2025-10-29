using System;
using System.Threading.Tasks;

namespace KeyShow;

public interface IKeyboardListener
{
    event Action<KeyInfo> OnKeyPressed;
    void Start();
}

public class DummyKeyboardListener : IKeyboardListener
{
    public event Action<KeyInfo>? OnKeyPressed;

    public void Start()
    {
        Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(2000);
                OnKeyPressed?.Invoke(new KeyInfo { KeyName = "K", Modifiers = Avalonia.Input.KeyModifiers.Control });
            }
        });
    }
}
