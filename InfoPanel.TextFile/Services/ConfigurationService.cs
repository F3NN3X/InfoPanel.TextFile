using System;
using System.Diagnostics;
using System.IO;
using IniParser;
using IniParser.Model;

namespace InfoPanel.TextFile.Services
{
    /// <summary>
    /// Manages plugin configuration settings using INI file format
    /// Follows InfoPanel's standard pattern for configuration management
    /// </summary>
    public class ConfigurationService
    {
        #region Fields
        
        private readonly string? _configFilePath;
        private IniData? _config;
        private readonly FileIniDataParser _parser;
        
        #endregion

        #region Constructor
        
        /// <summary>
        /// Initializes the configuration service with the specified config file path
        /// </summary>
        /// <param name="configFilePath">Path to the INI configuration file</param>
        public ConfigurationService(string? configFilePath)
        {
            _configFilePath = configFilePath;
            _parser = new FileIniDataParser();
            LoadConfiguration();
        }
        
        #endregion

        #region Configuration Management
        
        /// <summary>
        /// Loads configuration from INI file
        /// </summary>
        private void LoadConfiguration()
        {
            if (string.IsNullOrEmpty(_configFilePath))
            {
                Debug.WriteLine("[ConfigurationService] Config file path is not set.");
                return;
            }

            try
            {
                if (!File.Exists(_configFilePath))
                {
                    CreateDefaultConfiguration();
                    Debug.WriteLine("[ConfigurationService] Config file created with default values.");
                }
                else
                {
                    // Load existing config with safe file reading
                    using var fileStream = new FileStream(_configFilePath, 
                        FileMode.Open, FileAccess.Read, FileShare.Read);
                    using var reader = new StreamReader(fileStream);
                    
                    string fileContent = reader.ReadToEnd();
                    _config = _parser.Parser.Parse(fileContent);
                    
                    Debug.WriteLine("[ConfigurationService] Configuration loaded successfully.");
                    
                    // Auto-add missing keys for backward compatibility
                    if (EnsureMissingKeys())
                    {
                        SaveConfiguration();
                        Debug.WriteLine("[ConfigurationService] Updated config with missing keys.");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ConfigurationService] Error loading configuration: {ex.Message}");
                CreateDefaultConfiguration();
            }
        }
        
        /// <summary>
        /// Creates default configuration file with sensible defaults
        /// </summary>
        private void CreateDefaultConfiguration()
        {
            if (string.IsNullOrEmpty(_configFilePath))
                return;

            try
            {
                _config = new IniData();
                
                // Debug Settings
                _config["Debug Settings"]["EnableDebugLogging"] = "false";
                _config["Debug Settings"]["DebugLogLevel"] = "Info";
                
                // Monitoring Settings
                _config["Monitoring Settings"]["TextFilePath"] = @"C:\temp\sample.txt";
                _config["Monitoring Settings"]["MonitoringIntervalSeconds"] = "5";
                _config["Monitoring Settings"]["ContinuousMonitoring"] = "true";
                _config["Monitoring Settings"]["MaxContentLength"] = "10000";
                
                // Display Settings
                _config["Display Settings"]["TruncateLength"] = "100";
                _config["Display Settings"]["ShowFileInfo"] = "true";
                _config["Display Settings"]["ShowTimestamp"] = "true";
                
                // TODO: Add your plugin-specific settings here
                // Examples:
                // _config["Plugin Settings"]["TemperatureUnit"] = "C";
                // _config["Plugin Settings"]["MaxHistoryEntries"] = "100";
                // _config["Plugin Settings"]["AutoRefreshInterval"] = "30";
                // _config["Plugin Settings"]["EnableNotifications"] = "true";
                // _config["Plugin Settings"]["NotificationThreshold"] = "90";
                // _config["Plugin Settings"]["CustomApiKey"] = "<your-api-key-here>";
                
                _parser.WriteFile(_configFilePath, _config);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ConfigurationService] Error creating default configuration: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Ensures all required configuration keys exist (for version upgrades)
        /// </summary>
        /// <returns>True if any keys were added</returns>
        private bool EnsureMissingKeys()
        {
            if (_config == null)
                return false;

            bool configUpdated = false;
            
            // Ensure Debug Settings
            if (!_config["Debug Settings"].ContainsKey("EnableDebugLogging"))
            {
                _config["Debug Settings"]["EnableDebugLogging"] = "false";
                configUpdated = true;
            }
            if (!_config["Debug Settings"].ContainsKey("DebugLogLevel"))
            {
                _config["Debug Settings"]["DebugLogLevel"] = "Info";
                configUpdated = true;
            }
            
            // Ensure Monitoring Settings
            if (!_config["Monitoring Settings"].ContainsKey("MonitoringIntervalMs"))
            {
                _config["Monitoring Settings"]["MonitoringIntervalMs"] = "1000";
                configUpdated = true;
            }
            if (!_config["Monitoring Settings"].ContainsKey("EnableAutoReconnect"))
            {
                _config["Monitoring Settings"]["EnableAutoReconnect"] = "true";
                configUpdated = true;
            }
            if (!_config["Monitoring Settings"].ContainsKey("ConnectionTimeoutMs"))
            {
                _config["Monitoring Settings"]["ConnectionTimeoutMs"] = "5000";
                configUpdated = true;
            }
            
            // Ensure Display Settings
            if (!_config["Display Settings"].ContainsKey("ShowStatusMessages"))
            {
                _config["Display Settings"]["ShowStatusMessages"] = "true";
                configUpdated = true;
            }
            if (!_config["Display Settings"].ContainsKey("ShowDetailedMetrics"))
            {
                _config["Display Settings"]["ShowDetailedMetrics"] = "true";
                configUpdated = true;
            }
            if (!_config["Display Settings"].ContainsKey("UseMetricSystem"))
            {
                _config["Display Settings"]["UseMetricSystem"] = "true";
                configUpdated = true;
            }
            
            // TODO: Add checks for your plugin-specific settings
            // Example:
            // if (!_config["Plugin Settings"].ContainsKey("TemperatureUnit"))
            // {
            //     _config["Plugin Settings"]["TemperatureUnit"] = "C";
            //     configUpdated = true;
            // }
            
            return configUpdated;
        }
        
        /// <summary>
        /// Saves current configuration to file
        /// </summary>
        public void SaveConfiguration()
        {
            if (string.IsNullOrEmpty(_configFilePath) || _config == null)
                return;

            try
            {
                _parser.WriteFile(_configFilePath, _config);
                Debug.WriteLine("[ConfigurationService] Configuration saved successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ConfigurationService] Error saving configuration: {ex.Message}");
            }
        }
        
        #endregion

        #region Setting Accessors
        
        /// <summary>
        /// Gets a string setting value
        /// </summary>
        public string GetSetting(string section, string key, string defaultValue = "")
        {
            try
            {
                if (_config == null)
                    return defaultValue;
                
                var value = _config[section][key];
                return string.IsNullOrEmpty(value) ? defaultValue : value;
            }
            catch
            {
                return defaultValue;
            }
        }
        
        /// <summary>
        /// Gets a boolean setting value
        /// </summary>
        public bool GetBoolSetting(string section, string key, bool defaultValue = false)
        {
            var value = GetSetting(section, key);
            return bool.TryParse(value, out var result) ? result : defaultValue;
        }
        
        /// <summary>
        /// Gets an integer setting value with validation
        /// </summary>
        public int GetIntSetting(string section, string key, int defaultValue = 0)
        {
            var value = GetSetting(section, key);
            return int.TryParse(value, out var result) ? result : defaultValue;
        }
        
        /// <summary>
        /// Gets a double setting value
        /// </summary>
        public double GetDoubleSetting(string section, string key, double defaultValue = 0.0)
        {
            var value = GetSetting(section, key);
            return double.TryParse(value, out var result) ? result : defaultValue;
        }
        
        /// <summary>
        /// Sets a setting value
        /// </summary>
        public void SetSetting(string section, string key, string value)
        {
            if (_config == null)
                return;
            
            _config[section][key] = value ?? "";
        }
        
        /// <summary>
        /// Sets a boolean setting value
        /// </summary>
        public void SetBoolSetting(string section, string key, bool value)
        {
            SetSetting(section, key, value.ToString().ToLowerInvariant());
        }
        
        /// <summary>
        /// Sets an integer setting value
        /// </summary>
        public void SetIntSetting(string section, string key, int value)
        {
            SetSetting(section, key, value.ToString());
        }
        
        /// <summary>
        /// Sets a double setting value
        /// </summary>
        public void SetDoubleSetting(string section, string key, double value)
        {
            SetSetting(section, key, value.ToString("F2"));
        }
        
        #endregion

        #region Common Settings Properties
        
        /// <summary>
        /// Gets whether debug logging is enabled
        /// </summary>
        public bool IsDebugLoggingEnabled => 
            GetBoolSetting("Debug Settings", "EnableDebugLogging", false);
        
        /// <summary>
        /// Gets the debug log level
        /// </summary>
        public string DebugLogLevel => 
            GetSetting("Debug Settings", "DebugLogLevel", "Info");
        
        /// <summary>
        /// Gets the monitoring interval in milliseconds
        /// </summary>
        public int MonitoringIntervalMs => 
            GetIntSetting("Monitoring Settings", "MonitoringIntervalMs", 1000);
        
        /// <summary>
        /// Gets whether auto-reconnect is enabled
        /// </summary>
        public bool EnableAutoReconnect => 
            GetBoolSetting("Monitoring Settings", "EnableAutoReconnect", true);
        
        /// <summary>
        /// Gets the connection timeout in milliseconds
        /// </summary>
        public int ConnectionTimeoutMs => 
            GetIntSetting("Monitoring Settings", "ConnectionTimeoutMs", 5000);
        
        /// <summary>
        /// Gets whether to show status messages
        /// </summary>
        public bool ShowStatusMessages => 
            GetBoolSetting("Display Settings", "ShowStatusMessages", true);
        
        /// <summary>
        /// Gets whether to show detailed metrics
        /// </summary>
        public bool ShowDetailedMetrics => 
            GetBoolSetting("Display Settings", "ShowDetailedMetrics", true);
        
        /// <summary>
        /// Gets whether to use metric system for units
        /// </summary>
        public bool UseMetricSystem => 
            GetBoolSetting("Display Settings", "UseMetricSystem", true);
        
        #endregion

        #region TODO: Add Plugin-Specific Settings
        
        // TODO: Add getters for your plugin-specific configuration settings
        // Examples:
        
        // /// <summary>
        // /// Gets the temperature unit (C or F)
        // /// </summary>
        // public string GetTemperatureUnit() => 
        //     GetSetting("Plugin Settings", "TemperatureUnit", "C");
        
        // /// <summary>
        // /// Gets the maximum number of history entries to keep
        // /// </summary>
        // public int GetMaxHistoryEntries() => 
        //     GetIntSetting("Plugin Settings", "MaxHistoryEntries", 100);
        
        // /// <summary>
        // /// Gets the auto-refresh interval in seconds
        // /// </summary>
        // public int GetAutoRefreshInterval() => 
        //     GetIntSetting("Plugin Settings", "AutoRefreshInterval", 30);
        
        // /// <summary>
        // /// Gets whether notifications are enabled
        // /// </summary>
        // public bool GetEnableNotifications() => 
        //     GetBoolSetting("Plugin Settings", "EnableNotifications", true);
        
        // /// <summary>
        // /// Gets the notification threshold percentage
        // /// </summary>
        // public double GetNotificationThreshold() => 
        //     GetDoubleSetting("Plugin Settings", "NotificationThreshold", 90.0);
        
        // /// <summary>
        // /// Gets the custom API key for external services
        // /// </summary>
        // public string GetCustomApiKey() => 
        //     GetSetting("Plugin Settings", "CustomApiKey", "");
        
        #endregion

        #region Text File Specific Settings
        
        /// <summary>
        /// Gets the text file path to monitor
        /// </summary>
        public string GetTextFilePath() => 
            GetSetting("Monitoring Settings", "TextFilePath", "");
        
        /// <summary>
        /// Gets the file monitoring interval in seconds
        /// </summary>
        public int GetMonitoringIntervalSeconds()
        {
            var interval = GetIntSetting("Monitoring Settings", "MonitoringIntervalSeconds", 5);
            return Math.Max(1, interval); // Minimum 1 second
        }
        
        /// <summary>
        /// Gets whether to monitor file for changes continuously
        /// </summary>
        public bool GetContinuousMonitoring() => 
            GetBoolSetting("Monitoring Settings", "ContinuousMonitoring", true);
        
        /// <summary>
        /// Gets the maximum content length to read from file
        /// </summary>
        public int GetMaxContentLength()
        {
            var length = GetIntSetting("Monitoring Settings", "MaxContentLength", 10000);
            return Math.Max(100, length); // Minimum 100 characters
        }
        
        /// <summary>
        /// Gets the content truncate length for display
        /// </summary>
        public int GetTruncateLength()
        {
            var length = GetIntSetting("Display Settings", "TruncateLength", 100);
            return Math.Max(10, length); // Minimum 10 characters
        }
        
        /// <summary>
        /// Gets whether to show file info in display
        /// </summary>
        public bool GetShowFileInfo() => 
            GetBoolSetting("Display Settings", "ShowFileInfo", true);
        
        /// <summary>
        /// Gets whether to show timestamp in display
        /// </summary>
        public bool GetShowTimestamp() => 
            GetBoolSetting("Display Settings", "ShowTimestamp", true);
        
        #endregion

        #region Validation Methods
        
        /// <summary>
        /// Validates that required settings are present and valid
        /// </summary>
        public bool ValidateConfiguration()
        {
            try
            {
                // Validate text file settings
                var textFilePath = GetTextFilePath();
                if (string.IsNullOrWhiteSpace(textFilePath))
                {
                    Debug.WriteLine("[ConfigurationService] TextFilePath is required but not set");
                    return false;
                }
                
                if (GetMonitoringIntervalSeconds() <= 0)
                {
                    Debug.WriteLine("[ConfigurationService] Invalid MonitoringIntervalSeconds value");
                    return false;
                }
                
                if (GetMaxContentLength() <= 0)
                {
                    Debug.WriteLine("[ConfigurationService] Invalid MaxContentLength value");
                    return false;
                }
                
                if (GetTruncateLength() <= 0)
                {
                    Debug.WriteLine("[ConfigurationService] Invalid TruncateLength value");
                    return false;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ConfigurationService] Error validating configuration: {ex.Message}");
                return false;
            }
        }
        
        #endregion
    }
}