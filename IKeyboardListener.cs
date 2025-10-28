using System;
using System.Threading.Tasks;

namespace KeyShow;

public interface IKeyboardListener
{
    event Action<KeyInfo> OnKeyPressed;
    void Start();
    void Stop();
}

public class DummyKeyboardListener : IKeyboardListener
{
    public event Action<KeyInfo>? OnKeyPressed;

    public void Start()
    {
        // Testa med fake-key
        Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(2000);
                OnKeyPressed?.Invoke(new KeyInfo { KeyName = "A", Modifiers = Avalonia.Input.KeyModifiers.Shift });
            }
        });
    }

   public void Stop()
   {
       // Stop the fake-key simulation
   }
}
