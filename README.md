# KeyShow

KeyShow is a small .NET desktop application that displays keyboard input and key information on-screen. It’s implemented with Avalonia (cross-platform UI) and includes platform-specific keyboard listening (Windows keyboard listener is included).

![HelloWorldGif](HelloWorld.gif)

## Contents

- `Program.cs` — application entry point
- `App.axaml`, `App.axaml.cs` — Avalonia application definition
- `MainWindow.axaml`, `MainWindow.axaml.cs` — main UI
- `IKeyboardListener.cs`, `WindowsKeyboardListener.cs` — keyboard-listening abstraction and Windows implementation
- `Settings.*` — app settings and view model
- `KeyInfo.cs` — key model

## Requirements

- .NET SDK 9.0 or later (installed)
- Windows when using the provided `WindowsKeyboardListener` implementation
- Recommended: Visual Studio 2022/2023, Rider, or VS Code with C# plugin for development

## Build and run (PowerShell)

From the repository root (`c:\Repos\KeyShow`):

```powershell
# Restore (optional)
dotnet restore .\KeyShow.csproj

# Build (Debug)
dotnet build .\KeyShow.csproj -c Debug -p:DefineConstants=WINDOWS

# Run
dotnet run --project .\KeyShow.csproj -c Debug -p:DefineConstants=WINDOWS
```

Alternatively use the provided VS Code / task or build the solution:

```powershell
dotnet build KeyShow.sln -c Debug
```

## Publish (create a self-contained Windows executable)

```powershell
# Example: publish for Windows x64
dotnet publish .\KeyShow.csproj -c Release -r win-x64 --self-contained true -o .\publish\win-x64
```

## Notes

- The project includes a `.gitignore` to exclude build artifacts and IDE-specific files.
- If you run on non-Windows platforms, the Windows-specific keyboard listener will not be used; add/implement a platform listener as needed.

## Troubleshooting

- If `git push` fails with authentication/remote errors, verify the `origin` remote and your credentials:

```powershell
git remote -v
# If origin is missing, add it:
# git remote add origin <your-repo-url>
```

- If you see missing package/runtime errors when running, ensure the .NET 9 SDK is installed and available on PATH:

```powershell
dotnet --info
```

## Contributing

Contributions are welcome. Open an issue or submit a pull request with a clear description of the change.

## License

Add a LICENSE file to indicate the project license. If none is provided, assume source is under no specific license until one is added.

---

Generated on October 28, 2025
