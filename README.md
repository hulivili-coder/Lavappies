# Lavappies

An easy app launcher for Windows.

## Features

- Add executables (.exe) or shortcuts (.lnk) to your launcher.
- If you add a shortcut, it automatically finds the original .exe and uses its icon.
- Auto-start on PC login.
- Setup wizard on first run.
- Dark mode toggle.
- Custom logo support (add logo.ico to project root).

## Installation

Download the latest release from [GitHub Releases](https://github.com/hulivili-coder/Lavappies/releases).

Run the installer (LavappiesSetup.exe) for a guided setup wizard, or use the portable EXE if preferred.

To create your own installer:
1. Install [Inno Setup](https://jrsoftware.org/isinfo.php).
2. Run the app and click "Create Portable EXE".
3. Run `ISCC.exe setup.iss` to build the installer.

## Usage

1. Launch Lavappies.
2. Click "Add App" to select an .exe or .lnk file.
3. Select an app and click "Launch Selected".
4. Check "Auto-start on login" to enable auto-start.
5. Check "Dark Mode" to switch to dark theme.
6. To customize logo, add a logo.ico file to the application directory.

## Development

Built with C# and WPF.

To build:

```bash
dotnet build
```

To run:

```bash
dotnet run
```

## License

MIT