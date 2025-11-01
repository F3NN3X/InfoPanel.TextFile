# InfoPanel.TextFile

**Version:** 1.0.0  
**Author:** F3NN3X  
**Website:** https://myplugin.com

## Description

A real-time text file monitoring plugin for InfoPanel that reads and displays content from any text file with live updates when the file changes.

## Features

- **Real-time file monitoring** - Automatically detects and displays file changes instantly
- **Four informative sensors** - File size, line count, content preview, and status with timestamps
- **Configurable monitoring** - Adjustable check intervals and file size limits
- **Error handling** - Graceful handling of missing files, permission errors, and file locks
- **FileSystemWatcher integration** - Instant updates when monitored files change
- **Content truncation** - Configurable content preview length for large files
- **Service-based architecture** - Clean separation of concerns with proper resource management
- **Thread-safe operations** - Safe concurrent access to sensors and file operations

## Installation

1. Build the plugin in Release mode:
   ```powershell
   dotnet build -c Release
   ```

2. The plugin will be built to:
   ```
   bin\Release\net8.0-windows\InfoPanel.TextFile-v1.0.0\InfoPanel.TextFile\
   ```

3. A distribution ZIP file will also be created:
   ```
   bin\Release\net8.0-windows\InfoPanel.TextFile-v1.0.0.zip
   ```

4. Extract the ZIP file to your InfoPanel plugins directory, or copy the plugin folder manually

5. Restart InfoPanel to load the plugin

## Configuration

After first run, the plugin creates a configuration file:
```
InfoPanel.TextFile.dll.ini
```

### Key Settings

**Monitoring Settings:**
- `TextFilePath` - Path to the text file to monitor (e.g., `C:\logs\app.log`)
- `MonitoringIntervalSeconds` - How often to check for changes (default: 5 seconds)
- `ContinuousMonitoring` - Enable real-time FileSystemWatcher (default: true)
- `MaxContentLength` - Maximum characters to read from file (default: 10000)

**Display Settings:**
- `TruncateLength` - Content preview length in sensors (default: 100)
- `ShowFileInfo` - Show file size in status (default: true)
- `ShowTimestamp` - Show last modified time (default: true)

**Debug Settings:**
- `EnableDebugLogging` - Enable detailed logging (default: false)
- `LogLevel` - Logging level: Debug, Info, Warning, Error (default: Info)

### Example Configuration

```ini
[Monitoring Settings]
TextFilePath=C:\logs\application.log
MonitoringIntervalSeconds=3
ContinuousMonitoring=true
MaxContentLength=5000

[Display Settings]
TruncateLength=150
ShowFileInfo=true
ShowTimestamp=true

[Debug Settings]
EnableDebugLogging=false
LogLevel=Info
```

Use InfoPanel's "Open Config" button to easily access and edit the configuration file.

## Building from Source

### Prerequisites
- .NET 8.0 SDK or later
- Visual Studio 2022 or JetBrains Rider (optional)
- InfoPanel installed with InfoPanel.Plugins.dll available

### Build Commands

**Debug Build:**
```powershell
dotnet build -c Debug
```

**Release Build:**
```powershell
dotnet build -c Release
```

**Clean Build:**
```powershell
dotnet clean
dotnet build -c Release
```

## Development

### Architecture

The plugin follows a service-based architecture for clean separation of concerns:

```
InfoPanel.TextFile.cs           # Main plugin class - coordinates services & lifecycle
├── Services/
│   ├── MonitoringService.cs    # File monitoring with Timer + FileSystemWatcher
│   ├── SensorManagementService.cs # Thread-safe sensor updates via events
│   ├── ConfigurationService.cs    # INI-based config with section organization
│   └── FileLoggingService.cs      # Debug logging with rotation
├── Models/
│   └── TextFileData.cs             # File data model with validation
└── PluginInfo.ini                  # Plugin metadata for InfoPanel
```

### Key Components

- **MonitoringService**: Handles file reading, change detection, and FileSystemWatcher
- **SensorManagementService**: Thread-safe updates to InfoPanel sensors via events
- **ConfigurationService**: Manages INI file configuration with text file specific settings
- **TextFileData**: Data model with file content, metadata, and validation methods

### Data Flow

1. **MonitoringService** reads file and detects changes
2. **DataUpdated** event raised with TextFileData
3. **SensorManagementService** receives event and updates sensors
4. **InfoPanel** displays updated sensor values to user

## Sensors

The plugin provides four sensors in InfoPanel:

1. **File Size** - Shows the file size in bytes
2. **Line Count** - Shows the number of lines in the file
3. **File Content** - Shows a truncated preview of the file content
4. **File Status** - Shows monitoring status with timestamp and file info

## Usage

1. **Set the file path** in the configuration:
   ```ini
   [Monitoring Settings]
   TextFilePath=C:\path\to\your\file.txt
   ```

2. **Restart InfoPanel** or reload the plugin

3. **Monitor in real-time** - The sensors will update automatically when:
   - The file content changes
   - The file size changes
   - The file is created or deleted
   - File access errors occur

4. **View content** - The content sensor shows a preview of the file with configurable truncation

## Use Cases

- **Log file monitoring** - Watch application logs in real-time
- **Configuration file tracking** - Monitor config file changes
- **Data file updates** - Track when data files are updated
- **Script output monitoring** - Watch script outputs or temporary files
- **Document change tracking** - Monitor text documents for modifications

## Troubleshooting

### Enable Debug Logging

Edit the configuration file and set:
```ini
[Debug Settings]
EnableDebugLogging=true
LogLevel=Debug
```

Check the log file: `InfoPanel.TextFile-debug.log`

### Common Issues

**Plugin Not Loading:**
- Ensure InfoPanel.Plugins.dll reference is correct
- Verify all dependencies are in the plugin directory
- Check that .NET 8.0 runtime is installed

**No Data Appearing:**
- Verify the `TextFilePath` setting points to an existing file
- Check file permissions - ensure InfoPanel can read the file
- Enable debug logging and check for errors
- Verify monitoring service is starting correctly

**File Not Updating:**
- Check if `ContinuousMonitoring` is enabled for real-time updates
- Verify `MonitoringIntervalSeconds` is set to a reasonable value (1-10 seconds)
- Some applications lock files - check if file is accessible
- Review debug logs for FileSystemWatcher errors

**Content Not Showing:**
- Check `MaxContentLength` setting - increase if file is large
- Verify `TruncateLength` for content sensor display
- Ensure file contains readable text (not binary)
- Check file encoding compatibility

**Performance Issues:**
- Increase `MonitoringIntervalSeconds` for large files
- Reduce `MaxContentLength` for very large files
- Disable `ContinuousMonitoring` if FileSystemWatcher causes issues
- Monitor debug logs for excessive file access

## Version History

See [CHANGELOG.md](CHANGELOG.md) for detailed version history.

## License

See [LICENSE](LICENSE) for license information.

## Support

For issues, questions, or contributions:
- Check debug logs first (`InfoPanel.TextFile-debug.log`)
- Review configuration settings in the `.ini` file
- Verify file paths and permissions
- Contact: F3NN3X (https://myplugin.com)

## Acknowledgments

Built using the InfoPanel Plugin Framework - Transformed from template to text file monitoring plugin v1.0.0.
