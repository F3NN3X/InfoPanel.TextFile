using InfoPanel.Plugins;
using InfoPanel.TextFile.Models;
using System;

namespace InfoPanel.TextFile.Services
{
    /// <summary>
    /// Manages sensor updates with thread-safe operations for text file monitoring
    /// </summary>
    public class SensorManagementService
    {
        #region Fields
        
        private readonly object _sensorLock = new();
        private readonly ConfigurationService _configService;
        private readonly FileLoggingService _loggingService;
        
        #endregion

        #region Constructor
        
        public SensorManagementService(ConfigurationService configService)
        {
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
            _loggingService = new FileLoggingService(_configService);
        }
        
        #endregion

        #region Sensor Updates
        
        /// <summary>
        /// Updates all sensors with new text file data in a thread-safe manner
        /// </summary>
        public void UpdateSensors(
            PluginSensor fileSizeSensor,
            PluginSensor lineCountSensor, 
            PluginText contentSensor,
            PluginText statusSensor,
            TextFileData data)
        {
            if (data == null) return;
            
            lock (_sensorLock)
            {
                try
                {
                    // Update file size sensor (in bytes)
                    UpdateFileSizeSensor(fileSizeSensor, data);
                    
                    // Update line count sensor
                    UpdateLineCountSensor(lineCountSensor, data);
                    
                    // Update content sensor with truncated content
                    UpdateContentSensor(contentSensor, data);
                    
                    // Update status sensor
                    UpdateStatusSensor(statusSensor, data);
                    
                    _loggingService.LogDebug($"Sensors updated - Size: {data.FileSize}, Lines: {data.LineCount}, Status: {data.Status}");
                }
                catch (Exception ex)
                {
                    _loggingService.LogDebug($"Error updating sensors: {ex.Message}");
                    
                    // Set error state for sensors
                    SetErrorState(contentSensor, statusSensor, ex.Message);
                }
            }
        }
        
        /// <summary>
        /// Updates the file size sensor
        /// </summary>
        private void UpdateFileSizeSensor(PluginSensor fileSizeSensor, TextFileData data)
        {
            if (data.IsValid)
            {
                fileSizeSensor.Value = data.FileSize;
            }
            else
            {
                fileSizeSensor.Value = 0;
            }
        }
        
        /// <summary>
        /// Updates the line count sensor
        /// </summary>
        private void UpdateLineCountSensor(PluginSensor lineCountSensor, TextFileData data)
        {
            if (data.IsValid)
            {
                lineCountSensor.Value = data.LineCount;
            }
            else
            {
                lineCountSensor.Value = 0;
            }
        }
        
        /// <summary>
        /// Updates the content sensor with truncated text
        /// </summary>
        private void UpdateContentSensor(PluginText contentSensor, TextFileData data)
        {
            if (data.IsValid)
            {
                var truncateLength = _configService.GetTruncateLength();
                var content = data.GetTruncatedContent(truncateLength);
                
                // Handle empty content
                contentSensor.Value = string.IsNullOrEmpty(content) ? "[Empty File]" : content;
            }
            else
            {
                contentSensor.Value = "[File Error]";
            }
        }
        
        /// <summary>
        /// Updates the status sensor with file information
        /// </summary>
        private void UpdateStatusSensor(PluginText statusSensor, TextFileData data)
        {
            if (data.IsValid)
            {
                var showTimestamp = _configService.GetShowTimestamp();
                var showFileInfo = _configService.GetShowFileInfo();
                
                var statusText = data.Status;
                
                if (showTimestamp && data.LastModified != default)
                {
                    statusText += $" - {data.LastModified:HH:mm:ss}";
                }
                
                if (showFileInfo)
                {
                    statusText += $" ({data.GetFormattedFileSize()})";
                }
                
                statusSensor.Value = statusText;
            }
            else
            {
                statusSensor.Value = data.ErrorMessage;
            }
        }
        
        /// <summary>
        /// Sets error state for text sensors
        /// </summary>
        private void SetErrorState(PluginText contentSensor, PluginText statusSensor, string errorMessage)
        {
            try
            {
                contentSensor.Value = "[Error]";
                statusSensor.Value = $"Error: {errorMessage}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SensorManagementService] Error setting error state: {ex.Message}");
            }
        }
        
        #endregion

        #region Validation
        
        /// <summary>
        /// Validates sensor update parameters
        /// </summary>
        public bool ValidateSensors(
            PluginSensor fileSizeSensor,
            PluginSensor lineCountSensor, 
            PluginText contentSensor,
            PluginText statusSensor)
        {
            try
            {
                return fileSizeSensor != null && 
                       lineCountSensor != null && 
                       contentSensor != null && 
                       statusSensor != null;
            }
            catch
            {
                return false;
            }
        }
        
        #endregion

        #region Utility Methods
        
        /// <summary>
        /// Resets all sensors to initial state
        /// </summary>
        public void ResetSensors(
            PluginSensor fileSizeSensor,
            PluginSensor lineCountSensor, 
            PluginText contentSensor,
            PluginText statusSensor)
        {
            lock (_sensorLock)
            {
                try
                {
                    fileSizeSensor.Value = 0;
                    lineCountSensor.Value = 0;
                    contentSensor.Value = "No content loaded";
                    statusSensor.Value = "Initializing...";
                    
                    _loggingService.LogDebug("Sensors reset to initial state");
                }
                catch (Exception ex)
                {
                    _loggingService.LogDebug($"Error resetting sensors: {ex.Message}");
                }
            }
        }
        
        #endregion

        #region Disposal
        
        public void Dispose()
        {
            _loggingService?.Dispose();
        }
        
        #endregion
    }
}