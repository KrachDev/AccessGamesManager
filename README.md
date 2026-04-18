# AccessGames Manager

A modern, lightweight Windows desktop application for managing and organizing your game library with powerful Steam integration, account management, and advanced firewall control.

![.NET](https://img.shields.io/badge/.NET-9.0-blue?logo=.net)
![Platform](https://img.shields.io/badge/Platform-Windows-0078D4?logo=windows)
![License](https://img.shields.io/badge/License-MIT-green)
![Size](https://img.shields.io/badge/Size-42.84MB-brightgreen)

## 🎮 Features

### Core Features
- **Game Library Management** - Organize and manage your entire game collection
- **Multi-Account Support** - Seamlessly switch between different gaming accounts
- **Steam Integration** - Direct integration with Steam API for real-time data
- **Advanced Settings** - Customize firewall blocking, launch modes, and preferences
- **Firewall Control** - Block games from accessing network with one click
- **Auto-Updates** - Automatic application updates without user intervention
- **Multi-Language Support** - Support for multiple languages including Arabic

### Performance
- ⚡ **Lightweight** - Only 42.84 MB as a single executable
- 🚀 **Fast Startup** - ReadyToRun compilation for instant launch
- 💾 **Low Memory** - Optimized runtime with partial trimming
- 📦 **Self-Contained** - No dependencies or .NET SDK required on user machines

## 📥 Installation

### Download
1. Download the latest `AccessGames Manager.exe` from [Releases](https://github.com/KrachDev/AccessGamesManager/releases)
2. Run the executable - no installation needed!
3. Start managing your games immediately

### System Requirements
- **OS**: Windows 7 SP1 or later
- **Architecture**: 64-bit (x64)
- **RAM**: 500 MB minimum (1 GB recommended)
- **Storage**: 50 MB free space

## 🚀 Quick Start

### First Launch
1. Run `AccessGames Manager.exe`
2. Connect your Steam account
3. Grant firewall permissions (Windows security prompt may appear)
4. Start organizing your game library!

### Basic Usage
- **View Games**: Browse your complete game collection
- **Switch Accounts**: Click account button to switch gaming profiles
- **Block Games**: Select a game and toggle firewall blocking
- **Search**: Use search functionality to quickly find games
- **Settings**: Customize language, firewall behavior, and launch preferences

## 🔧 Build Information

### Technology Stack
- **.NET 9.0** - Latest .NET framework
- **Avalonia 11.3.10** - Modern cross-platform UI framework
- **SteamKit2** - Steam API integration
- **Newtonsoft.Json** - JSON serialization
- **MVVM Community Toolkit** - MVVM pattern support

### Build Configuration
The project is optimized for minimal size:
- Single-file executable
- ReadyToRun compilation
- Partial runtime trimming
- Compression enabled
- No debug symbols
- Windows x64 target

### Building from Source

**Prerequisites:**
- .NET 9.0 SDK or later
- Visual Studio 2022 or Visual Studio Code

**Clone and Build:**
```bash
git clone https://github.com/KrachDev/AccessGamesManager.git
cd AccessGames\ Manager
dotnet build -c Release
```

**Publish (Slim Build):**
```bash
cd "AccessGames Manager"
dotnet publish -c Release -r win-x64 --self-contained `
  -p:PublishSingleFile=true `
  -p:PublishReadyToRun=true `
  -p:PublishTrimmed=true `
  -p:TrimMode=partial `
  -p:EnableCompressionInSingleFile=true `
  -o "..\bin\PublishSlim"
```

**Run:**
```powershell
.\bin\PublishSlim\AccessGames Manager.exe
```

## 📊 Project Structure

```
AccessGames Manager/
├── Views/                 # UI Components (XAML)
│   ├── MainWindow.axaml
│   ├── GamesView.axaml
│   ├── AccountsView.axaml
│   └── SettingsView.axaml
├── ViewModels/           # MVVM ViewModels
├── Misc/                 # Utilities & Services
│   ├── SteamData.cs
│   ├── AutoUpdater.cs
│   ├── LocalWebServer.cs
│   └── Analytics.cs
├── Helpers/              # Helper Functions
├── Assets/               # Resources & Images
└── Program.cs            # Application Entry Point
```

## 🔐 Features Detail

### Account Management
- Switch between multiple Steam accounts instantly
- Store account credentials securely
- Quick account switching without restarting the app
- Support for limited and full Steam accounts

### Firewall Integration
- Windows Defender Firewall integration
- One-click game blocking
- Automatic rule management
- Batch block/unblock operations

### Game Library
- Display all installed games from Steam
- Real-time library synchronization
- Search and filter capabilities
- Game launch preferences

### Settings
- **Language**: English, Arabic, and more
- **Auto-Updates**: Enable/disable automatic updates
- **Firewall**: Configure default blocking behavior
- **Launch Mode**: Select game launch preferences
- **Theme**: Light/Dark mode support

## 🐛 Troubleshooting

### App won't start
- Ensure Windows 7 SP1 or later
- Check system architecture is 64-bit
- Try running as Administrator

### Steam connection issues
- Verify Steam is installed and running
- Check internet connection
- Restart the application
- Clear application cache

### Firewall rules not working
- Run as Administrator
- Check Windows Firewall is enabled
- Verify game path is correct
- Review Windows security logs

## 📈 Performance Metrics

### Size Reduction (Latest Optimization)
| Build Type | Size | Reduction |
|-----------|------|-----------|
| Original Release | 61.00 MB | baseline |
| Slim Build | 42.84 MB | 29.8% ⬇️ |
| Savings | 18.16 MB | - |

### Startup Time
- Cold Start: ~500ms (ReadyToRun optimized)
- Warm Start: ~200ms
- Game Load: Instant

## 🤝 Contributing

We welcome contributions! Here's how you can help:

1. **Report Bugs** - Open an issue with details
2. **Suggest Features** - Share your ideas in discussions
3. **Improve Code** - Submit pull requests
4. **Improve Docs** - Help with documentation
5. **Translations** - Add support for more languages

### Development Guidelines
- Follow C# naming conventions (PascalCase for classes, camelCase for locals)
- Write unit tests for new features
- Update documentation for API changes
- Create meaningful commit messages

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👨‍💻 Author

**KrachDev** - [GitHub Profile](https://github.com/KrachDev)

## 🔗 Links

- **GitHub Repository**: [AccessGamesManager](https://github.com/KrachDev/AccessGamesManager)
- **Issues**: [Report a bug](https://github.com/KrachDev/AccessGamesManager/issues)
- **Releases**: [Download latest](https://github.com/KrachDev/AccessGamesManager/releases)

## 💝 Support

If you find this project helpful, please consider:
- ⭐ Starring the repository
- 🐛 Reporting bugs
- 💡 Suggesting features
- 📢 Sharing with others

## 📞 Contact & Support

- **Report Issues**: [GitHub Issues](https://github.com/KrachDev/AccessGamesManager/issues)
- **Discussions**: [GitHub Discussions](https://github.com/KrachDev/AccessGamesManager/discussions)

---

**AccessGames Manager** - Manage your games, take control of your library! 🎮
