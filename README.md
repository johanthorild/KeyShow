# KeyShow

KeyShow is a small .NET desktop application that displays keyboard input and key information on-screen. It’s implemented with Avalonia (cross-platform UI) and includes platform-specific keyboard listening (Windows keyboard listener is included).

![HelloWorldGif](HelloWorld.gif)

## Features

- Shows the last pressed key and active modifiers on-screen.
- Adjustable display duration via Settings (Display time in milliseconds).
- Drag the window anywhere; the position is persisted and restored on next run.
- Settings are persisted to `%APPDATA%\KeyShow\settings.json` on Windows.
- Context menu includes Settings and Quit actions.

## Requirements

- .NET SDK 9.0 or later (installed)
- Windows when using the provided `WindowsKeyboardListener` implementation

## Contents

- `Program.cs` — application entry point
- `App.axaml`, `App.axaml.cs` — Avalonia application definition
- `MainWindow.axaml`, `MainWindow.axaml.cs` — main UI
- `IKeyboardListener.cs`, `WindowsKeyboardListener.cs` — keyboard-listening abstraction and Windows implementation
- `Settings.*` — app settings and view model
- `KeyInfo.cs` — key model

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

Or use the included helper script to produce and zip artifacts locally:

```powershell
.\scripts\publish-local.ps1 -Version 0.1.0
```

## Notes

- The project includes a `.gitignore` to exclude build artifacts and IDE-specific files.
- If you run on non-Windows platforms, the Windows-specific keyboard listener will not be used; add/implement a platform listener as needed.

## Release

- The project is configured for release automation. Push an annotated tag (for example `v0.1.0`) and the included GitHub Actions workflow (`.github/workflows/release.yml`) will build and publish Windows artifacts and create a GitHub Release.
- You can also run `scripts/publish-local.ps1` locally to produce zipped artifacts in `./publish/`.

## Changelog & License

- See `CHANGELOG.md` for what changed in each release.
- This project includes an `LICENSE` file (MIT) — add or change if you prefer another license.

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

Generated on October 29, 2025
