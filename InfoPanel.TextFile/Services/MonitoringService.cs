using InfoPanel.TextFile.Models;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SystemTimer = System.Threading.Timer;

namespace InfoPanel.TextFile.Services
{
    /// <summary>
    /// Event arguments for data update events
    /// </summary>
    public class DataUpdatedEventArgs : EventArgs
    {
        public TextFileData Data { get; }
        
        public DataUpdatedEventArgs(TextFileData data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }
    }

    /// <summary>
    /// Service responsible for monitoring text file changes and reading content
    /// </summary>
    public class MonitoringService : IDisposable
    {
        #region Events
        
        /// <summary>
        /// Triggered when new data is available
        /// </summary>
        public event EventHandler<DataUpdatedEventArgs>? DataUpdated;
        
        #endregion

        #region Fields
        
        private readonly ConfigurationService _configService;
        private readonly FileLoggingService _loggingService;
        private SystemTimer? _monitoringTimer;
        private FileSystemWatcher? _fileWatcher;
        private DateTime _lastReadTime = DateTime.MinValue;
        private string _currentFilePath = string.Empty;
        private volatile bool _isMonitoring;
        private readonly object _lockObject = new();
        
        #endregion

        #region Constructor
        
        public MonitoringService(ConfigurationService configService)
        {
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
            _loggingService = new FileLoggingService(_configService);
            
            Console.WriteLine("[MonitoringService] Text file monitoring service initialized");
        }
        
        #endregion

        #region Monitoring Control
        
        /// <summary>
        /// Starts the text file monitoring process
        /// </summary>
        public async Task StartMonitoringAsync(CancellationToken cancellationToken)
        {
            try
            {
                lock (_lockObject)
                {
                    if (_isMonitoring)
                    {
                        _loggingService.LogDebug("Monitoring already started");
                        return;
                    }
                    _isMonitoring = true;
                }
                
                _loggingService.LogDebug("Starting text file monitoring");
                
                // Get configuration
                _currentFilePath = _configService.GetTextFilePath();
                var intervalSeconds = _configService.GetMonitoringIntervalSeconds();
                var continuousMonitoring = _configService.GetContinuousMonitoring();
                
                if (string.IsNullOrWhiteSpace(_currentFilePath))
                {
                    _loggingService.LogDebug("No text file path configured");
                    OnDataUpdated(CreateErrorData("No text file path configured. Please set TextFilePath in configuration."));
                    return;
                }
                
                // Initial read
                await ReadFileAsync();
                
                // Set up periodic monitoring
                var intervalMs = intervalSeconds * 1000;
                _monitoringTimer = new SystemTimer(async _ => await ReadFileAsync(), 
                    null, 
                    intervalMs, 
                    intervalMs);
                
                // Set up file system watcher for real-time updates if enabled
                if (continuousMonitoring && File.Exists(_currentFilePath))
                {
                    SetupFileWatcher();
                }
                
                _loggingService.LogDebug($"Monitoring started for: {_currentFilePath} (interval: {intervalSeconds}s, continuous: {continuousMonitoring})");
                
                // Keep monitoring until cancellation
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _loggingService.LogDebug("Monitoring cancelled");
            }
            catch (Exception ex)
            {
                _loggingService.LogDebug($"Error in monitoring: {ex.Message}");
                OnDataUpdated(CreateErrorData($"Monitoring error: {ex.Message}"));
            }
            finally
            {
                lock (_lockObject)
                {
                    _isMonitoring = false;
                }
            }
        }
        
        /// <summary>
        /// Stops the monitoring process
        /// </summary>
        public void StopMonitoring()
        {
            lock (_lockObject)
            {
                _isMonitoring = false;
            }
            
            _monitoringTimer?.Dispose();
            _monitoringTimer = null;
            
            _fileWatcher?.Dispose();
            _fileWatcher = null;
            
            _loggingService.LogDebug("Monitoring stopped");
        }
        
        #endregion

        #region File Monitoring Implementation
        
        /// <summary>
        /// Sets up FileSystemWatcher for real-time file change detection
        /// </summary>
        private void SetupFileWatcher()
        {
            try
            {
                var directory = Path.GetDirectoryName(_currentFilePath);
                var fileName = Path.GetFileName(_currentFilePath);
                
                if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName))
                    return;
                
                _fileWatcher = new FileSystemWatcher(directory, fileName)
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                    EnableRaisingEvents = true
                };
                
                _fileWatcher.Changed += async (sender, e) => 
                {
                    // Debounce rapid file changes
                    await Task.Delay(500);
                    await ReadFileAsync();
                };
                
                _fileWatcher.Error += (sender, e) =>
                {
                    _loggingService.LogDebug($"FileSystemWatcher error: {e.GetException()?.Message}");
                    // Try to recreate the watcher
                    Task.Run(() =>
                    {
                        _fileWatcher?.Dispose();
                        Task.Delay(2000).ContinueWith(_ => SetupFileWatcher());
                    });
                };
                
                _loggingService.LogDebug("File system watcher setup complete");
            }
            catch (Exception ex)
            {
                _loggingService.LogDebug($"Could not setup file watcher: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Reads the text file and updates data
        /// </summary>
        private async Task ReadFileAsync()
        {
            if (!_isMonitoring) return;
            
            try
            {
                var data = new TextFileData
                {
                    FilePath = _currentFilePath
                };
                
                if (!File.Exists(_currentFilePath))
                {
                    data.FileExists = false;
                    data.Status = "File Not Found";
                    data.ErrorMessage = $"File does not exist: {_currentFilePath}";
                    OnDataUpdated(data);
                    return;
                }
                
                var fileInfo = new FileInfo(_currentFilePath);
                
                // Check if file has been modified since last read
                if (fileInfo.LastWriteTime <= _lastReadTime)
                {
                    return; // No changes
                }
                
                data.FileExists = true;
                data.LastModified = fileInfo.LastWriteTime;
                data.FileSize = fileInfo.Length;
                _lastReadTime = fileInfo.LastWriteTime;
                
                // Read file content with size limit
                var maxLength = _configService.GetMaxContentLength();
                
                using var reader = new StreamReader(_currentFilePath);
                var buffer = new char[maxLength];
                var charsRead = await reader.ReadAsync(buffer, 0, maxLength);
                
                data.Content = new string(buffer, 0, charsRead);
                data.Status = charsRead >= maxLength ? "Truncated" : "Complete";
                
                _loggingService.LogDebug($"File read successfully: {charsRead} characters, {data.LineCount} lines");
                OnDataUpdated(data);
            }
            catch (IOException ex)
            {
                _loggingService.LogDebug($"IO error reading file: {ex.Message}");
                OnDataUpdated(CreateErrorData($"Cannot read file: {ex.Message}"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _loggingService.LogDebug($"Access denied reading file: {ex.Message}");
                OnDataUpdated(CreateErrorData($"Access denied: {ex.Message}"));
            }
            catch (Exception ex)
            {
                _loggingService.LogDebug($"Error reading file: {ex.Message}");
                OnDataUpdated(CreateErrorData($"Error: {ex.Message}"));
            }
        }
        
        /// <summary>
        /// Creates error data instance
        /// </summary>
        private TextFileData CreateErrorData(string errorMessage)
        {
            return new TextFileData
            {
                FilePath = _currentFilePath,
                FileExists = false,
                Status = "Error",
                ErrorMessage = errorMessage,
                Content = string.Empty
            };
        }
        
        /// <summary>
        /// Raises the DataUpdated event
        /// </summary>
        private void OnDataUpdated(TextFileData data)
        {
            try
            {
                DataUpdated?.Invoke(this, new DataUpdatedEventArgs(data));
            }
            catch (Exception ex)
            {
                _loggingService.LogDebug($"Error raising DataUpdated event: {ex.Message}");
            }
        }
        
        #endregion

        #region Disposal
        
        public void Dispose()
        {
            StopMonitoring();
            _loggingService?.Dispose();
            Console.WriteLine("[MonitoringService] Disposed");
        }
        
        #endregion
    }
}