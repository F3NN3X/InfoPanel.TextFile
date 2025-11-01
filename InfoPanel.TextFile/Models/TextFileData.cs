using System;
using System.IO;

namespace InfoPanel.TextFile.Models
{
    /// <summary>
    /// Data model for text file content and metadata
    /// </summary>
    public class TextFileData
    {
        #region Core Properties
        
        /// <summary>
        /// The text content read from the file
        /// </summary>
        public string Content { get; set; } = string.Empty;
        
        /// <summary>
        /// When the file was last modified
        /// </summary>
        public DateTime LastModified { get; set; }
        
        /// <summary>
        /// File size in bytes
        /// </summary>
        public long FileSize { get; set; }
        
        /// <summary>
        /// Path to the monitored text file
        /// </summary>
        public string FilePath { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether the file exists and can be read
        /// </summary>
        public bool FileExists { get; set; }
        
        /// <summary>
        /// Current status of file monitoring
        /// </summary>
        public string Status { get; set; } = "Unknown";
        
        /// <summary>
        /// Error message if there's an issue reading the file
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;
        
        /// <summary>
        /// Timestamp when this data was collected
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;
        
        #endregion

        #region Computed Properties
        
        /// <summary>
        /// Indicates if the data represents a valid file read
        /// </summary>
        public bool IsValid => FileExists && string.IsNullOrEmpty(ErrorMessage);
        
        /// <summary>
        /// Gets the number of lines in the content
        /// </summary>
        public int LineCount 
        {
            get
            {
                if (string.IsNullOrEmpty(Content))
                    return 0;
                    
                return Content.Split('\n').Length;
            }
        }
        
        /// <summary>
        /// Gets the number of characters in the content
        /// </summary>
        public int CharacterCount => Content?.Length ?? 0;
        
        #endregion

        #region Constructors
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public TextFileData()
        {
        }
        
        /// <summary>
        /// Constructor for successful file read
        /// </summary>
        public TextFileData(string filePath, string content, FileInfo fileInfo)
        {
            FilePath = filePath;
            Content = content;
            FileExists = true;
            LastModified = fileInfo.LastWriteTime;
            FileSize = fileInfo.Length;
            Status = "Complete";
        }
        
        /// <summary>
        /// Constructor for error states
        /// </summary>
        public TextFileData(string filePath, string errorMessage)
        {
            FilePath = filePath;
            FileExists = false;
            Status = "Error";
            ErrorMessage = errorMessage;
            Content = string.Empty;
        }
        
        #endregion

        #region Methods
        
        /// <summary>
        /// Gets a truncated version of content for display purposes
        /// </summary>
        public string GetTruncatedContent(int maxLength = 100)
        {
            if (string.IsNullOrEmpty(Content))
                return "Empty";
                
            if (Content.Length <= maxLength)
                return Content;
                
            return Content.Substring(0, maxLength) + "...";
        }
        
        /// <summary>
        /// Gets formatted file size as human-readable string
        /// </summary>
        public string GetFormattedFileSize()
        {
            if (FileSize == 0) return "0 B";
            
            string[] sizes = { "B", "KB", "MB", "GB" };
            double bytes = FileSize;
            int order = 0;
            
            while (bytes >= 1024 && order < sizes.Length - 1)
            {
                order++;
                bytes = bytes / 1024;
            }
            
            return $"{bytes:0.##} {sizes[order]}";
        }
        
        /// <summary>
        /// Gets formatted status with timestamp
        /// </summary>
        public string GetFormattedStatus()
        {
            var timeStr = LastModified != default ? LastModified.ToString("HH:mm:ss") : "N/A";
            return IsValid ? $"{Status} - {timeStr}" : ErrorMessage;
        }
        
        /// <summary>
        /// Validates that the data is in a consistent state
        /// </summary>
        public bool IsValidData()
        {
            try
            {
                // Basic validation rules
                if (!FileExists && string.IsNullOrWhiteSpace(ErrorMessage))
                    return false;
                
                if (FileExists && string.IsNullOrEmpty(FilePath))
                    return false;
                
                if (FileSize < 0)
                    return false;
                
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Determines if this data is significantly different from another instance
        /// </summary>
        public bool HasSignificantChange(TextFileData? other)
        {
            if (other == null) return true;
            if (FileExists != other.FileExists) return true;
            if (!FileExists && !other.FileExists) return ErrorMessage != other.ErrorMessage;
            
            return Content != other.Content || 
                   LastModified != other.LastModified || 
                   FileSize != other.FileSize ||
                   Status != other.Status;
        }
        
        #endregion

        #region String Representation
        
        /// <summary>
        /// Returns a string representation of the data
        /// </summary>
        public override string ToString()
        {
            if (!IsValid)
            {
                return $"TextFileData[Error: {ErrorMessage}]";
            }
            
            return $"TextFileData[Path: {FilePath}, Size: {GetFormattedFileSize()}, Lines: {LineCount}, Modified: {LastModified:HH:mm:ss}]";
        }
        
        #endregion
    }
}