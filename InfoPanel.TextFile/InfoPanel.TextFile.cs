// InfoPanel.TextFile v1.0.0 - InfoPanel Plugin Template
using InfoPanel.Plugins;
using InfoPanel.TextFile.Services;
using InfoPanel.TextFile.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace InfoPanel.TextFile
{
    /// <summary>
    /// Template plugin for InfoPanel - Read text from text file
    /// 
    /// This template provides a solid foundation for creating new InfoPanel plugins with:
    /// - Service-based architecture
    /// - Event-driven data flow
    /// - Thread-safe sensor updates
    /// - Proper resource management
    /// - Configuration support
    /// 
    /// TODO: Customize this plugin for your specific monitoring needs
    /// </summary>
    public class TextFileMain : BasePlugin
    {
        #region Configuration
        
        // Configuration file path - exposed to InfoPanel for direct file access
        private string? _configFilePath;
        
        /// <summary>
        /// Exposes the configuration file path to InfoPanel for the "Open Config" button
        /// </summary>
        public override string? ConfigFilePath => _configFilePath;
        
        #endregion

        #region Sensors
        
        // Text file monitoring sensors
        private readonly PluginSensor _fileSizeSensor = new("file-size", "File Size", 0, "bytes");
        private readonly PluginSensor _lineCountSensor = new("line-count", "Line Count", 0, "lines");
        private readonly PluginText _contentSensor = new("content", "File Content", "No content loaded");
        private readonly PluginText _statusSensor = new("status", "File Status", "Initializing...");
        
        #endregion

        #region Services
        
        private MonitoringService? _monitoringService;
        private SensorManagementService? _sensorService;
        private ConfigurationService? _configService;
        private CancellationTokenSource? _cancellationTokenSource;
        
        #endregion

        #region Constructor & Initialization
        
        public TextFileMain() : base("InfoPanel.TextFile", "InfoPanel TextFile Monitor", "Read text from text file")
        {
            try
            {
                // Note: _configFilePath will be set in Initialize()
                // ConfigurationService will be initialized after we have the path
                
                // TODO: Add any additional initialization logic here that doesn't require configuration
                
            }
            catch (Exception ex)
            {
                // Log initialization errors
                Console.WriteLine($"[TextFile] Error during initialization: {ex.Message}");
                throw;
            }
        }

        public override void Initialize()
        {
            try
            {
                // Set up configuration file path for InfoPanel integration
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                string basePath = assembly.ManifestModule.FullyQualifiedName;
                _configFilePath = $"{basePath}.ini";
                
                Console.WriteLine($"[TextFile] Config file path: {_configFilePath}");
                
                // Initialize services now that we have the config path
                _configService = new ConfigurationService(_configFilePath);
                _sensorService = new SensorManagementService(_configService);
                _monitoringService = new MonitoringService(_configService);
                
                // Subscribe to events
                _monitoringService.DataUpdated += OnDataUpdated;
                
                // Start monitoring
                _cancellationTokenSource = new CancellationTokenSource();
                _ = StartMonitoringAsync(_cancellationTokenSource.Token);
                
                Console.WriteLine("[TextFile] Plugin initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TextFile] Error during plugin initialization: {ex.Message}");
                throw;
            }
        }
        
        #endregion

        #region Monitoring
        
        private async Task StartMonitoringAsync(CancellationToken cancellationToken)
        {
            try
            {
                // TODO: Implement your monitoring logic here
                // This is where you start your data collection process
                
                if (_monitoringService != null)
                {
                    await _monitoringService.StartMonitoringAsync(cancellationToken);
                }
                
                // Example: You might also start additional monitoring tasks
                // _ = MonitorSystemResourcesAsync(cancellationToken);
                // _ = MonitorNetworkConnectivityAsync(cancellationToken);
                
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                Console.WriteLine("[TextFile] Monitoring cancelled");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TextFile] Error in monitoring: {ex.Message}");
            }
        }
        
        #endregion

        #region Event Handlers
        
        private void OnDataUpdated(object? sender, DataUpdatedEventArgs e)
        {
            try
            {
                if (_sensorService != null)
                {
                    _sensorService.UpdateSensors(
                        _fileSizeSensor,
                        _lineCountSensor,
                        _contentSensor,
                        _statusSensor,
                        e.Data
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TextFile] Error updating sensors: {ex.Message}");
                _statusSensor.Value = "Error updating data";
            }
        }
        
        #endregion

        #region BasePlugin Implementation
        
        /// <summary>
        /// Gets the update interval for the plugin (not used in our async implementation)
        /// </summary>
        public override TimeSpan UpdateInterval => TimeSpan.Zero; // Not used since we handle monitoring internally
        
        /// <summary>
        /// Legacy Update method (not used in our async implementation)
        /// </summary>
        public override void Update()
        {
            // Not used - we handle updates through async monitoring
        }
        
        /// <summary>
        /// Legacy UpdateAsync method (not used in our async implementation)
        /// </summary>
        public override Task UpdateAsync(CancellationToken cancellationToken)
        {
            // Not used - we handle updates through async monitoring
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Loads the plugin containers (called by InfoPanel)
        /// </summary>
        public override void Load(List<IPluginContainer> containers)
        {
            try
            {
                // Create sensor container
                var container = new PluginContainer("TextFile", "Text File Monitor");
                
                // Add sensors to container
                container.Entries.Add(_fileSizeSensor);
                container.Entries.Add(_lineCountSensor);
                container.Entries.Add(_contentSensor);
                container.Entries.Add(_statusSensor);
                
                // Add container to the list
                containers.Add(container);
                
                Console.WriteLine("[TextFile] Container loaded with sensors");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TextFile] Error loading containers: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Closes the plugin
        /// </summary>
        public override void Close()
        {
            try
            {
                _cancellationTokenSource?.Cancel();
                _monitoringService?.StopMonitoring();
                Console.WriteLine("[TextFile] Plugin closed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TextFile] Error during plugin close: {ex.Message}");
            }
        }
        
        #endregion

        #region Cleanup & Disposal
        
        public void Dispose()
        {
            try
            {
                // Cancel monitoring
                _cancellationTokenSource?.Cancel();
                
                // Unsubscribe from events
                if (_monitoringService != null)
                {
                    _monitoringService.DataUpdated -= OnDataUpdated;
                }
                
                // Dispose services
                _monitoringService?.Dispose();
                _cancellationTokenSource?.Dispose();
                
                Console.WriteLine("[TextFile] Plugin disposed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TextFile] Error during disposal: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("[TextFile] Cleanup completed");
            }
        }
        
        #endregion

        #region TODO: Add Your Custom Methods Here
        
        // TODO: Add any plugin-specific methods you need
        // Examples:
        
        // private async Task MonitorSystemResourcesAsync(CancellationToken cancellationToken)
        // {
        //     // Monitor CPU, memory, disk, etc.
        // }
        
        // private async Task MonitorNetworkConnectivityAsync(CancellationToken cancellationToken)
        // {
        //     // Monitor network connectivity, bandwidth, etc.
        // }
        
        // private void ProcessSpecialEvents(SpecialEventData eventData)
        // {
        //     // Handle special events or conditions
        // }
        
        #endregion
    }
}