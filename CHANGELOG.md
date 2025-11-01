# Changelog

All notable changes to the InfoPanel.TextFile plugin will be documented in this file.

## [1.0.0] - 2025-11-01

### Added
- **Real-time text file monitoring** - Monitor any text file for changes with automatic updates
- **Four informative sensors** - File size (bytes), line count, content preview, and status with timestamps
- **Service-based architecture** - Clean separation with MonitoringService, SensorManagementService, ConfigurationService, and FileLoggingService
- **Dual monitoring system** - Timer-based polling + FileSystemWatcher for instant change detection
- **Comprehensive configuration** - INI-based settings for file paths, intervals, content limits, and display options
- **TextFileData model** - Complete data model with file metadata, content handling, and validation
- **Thread-safe operations** - Safe concurrent access to sensors and file operations
- **Advanced error handling** - Graceful handling of missing files, permission errors, and file locks
- **Content truncation** - Configurable content preview length for large files
- **Debug logging** - Comprehensive logging with rotation and multiple levels

### Features
- **File monitoring** with configurable intervals (default: 5 seconds)
- **Real-time updates** via FileSystemWatcher when files change
- **Content reading** with size limits to handle large files (default: 10,000 characters)
- **Display truncation** for sensor content preview (default: 100 characters)
- **Status tracking** with file timestamps and formatted file sizes
- **Error recovery** with automatic retry and clear error messaging
- **Configuration validation** with sensible defaults and bounds checking

### Configuration Options
- `TextFilePath` - Path to the text file to monitor
- `MonitoringIntervalSeconds` - Check interval (1-3600 seconds)
- `ContinuousMonitoring` - Enable FileSystemWatcher (true/false)
- `MaxContentLength` - Maximum characters to read (100-1000000)
- `TruncateLength` - Content preview length (10-1000)
- `ShowFileInfo` - Show file size in status (true/false)
- `ShowTimestamp` - Show last modified time (true/false)

### Technical Implementation
- **MonitoringService** - File reading, change detection, and FileSystemWatcher integration
- **SensorManagementService** - Thread-safe sensor updates via event-driven architecture
- **ConfigurationService** - Text file specific settings with validation and defaults
- **TextFileData** - File content model with metadata, validation, and formatting methods

### Use Cases
- Monitor application log files in real-time
- Track configuration file changes
- Watch script outputs and temporary files
- Monitor document modifications
- Track data file updates

---

## Future Releases

### Planned Features for v1.1.0
- File encoding detection and support for different text encodings
- Multiple file monitoring support
- Content filtering and search capabilities
- File change notifications/alerts
- Content highlighting and syntax detection
- Historical content tracking

### Planned Features for v1.2.0
- Binary file support with hex preview
- Network file monitoring (UNC paths, mapped drives)
- File comparison and diff capabilities
- Export functionality for sensor data
- Custom content parsers and extractors

---

## Development Notes

This plugin was built by transforming the InfoPanel Plugin Template into a specialized text file monitoring solution. The architecture maintains the template's service-based design while implementing file-specific functionality.

### Version History Format
- **MAJOR** version for incompatible API changes
- **MINOR** version for backwards-compatible functionality additions  
- **PATCH** version for backwards-compatible bug fixes

All dates are in YYYY-MM-DD format following ISO 8601 standard.

### Added
- New temperature monitoring sensor
- Configuration setting for temperature unit (C/F)

### Changed  
- Improved error handling in data collection
- Updated sensor refresh rate to 500ms

### Fixed
- Memory leak in monitoring service disposal
- Thread safety issue in sensor updates
```

### Version Update Checklist:
- [ ] Update version in .csproj file
- [ ] Update version in PluginInfo.ini
- [ ] Update version references in source comments
- [ ] Add changelog entry with date
- [ ] Test build and ZIP creation
- [ ] Verify all features work correctly