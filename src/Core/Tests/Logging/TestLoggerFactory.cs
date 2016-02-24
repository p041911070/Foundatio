using System;
using System.Collections.Generic;
using Foundatio.Logging;
using Foundatio.Logging.Abstractions.Internal;
using Xunit.Abstractions;

namespace Foundatio.Tests.Logging {
    public class TestLoggerFactory : ILoggerFactory {
        private readonly Dictionary<string, LogLevel> _logLevels = new Dictionary<string, LogLevel>();
        private readonly List<LogEntry> _logEntries = new List<LogEntry>();
        private readonly ITestOutputHelper _testOutputHelper;

        public TestLoggerFactory(ITestOutputHelper output) {
            _testOutputHelper = output;
        }

        public LogLevel MinimumLevel { get; set; } = LogLevel.Information;
        public IReadOnlyList<LogEntry> LogEntries => _logEntries;
        public int MaxLogEntries = 1000;

        public void AddLogEntry(LogEntry logEntry) {
            if (_logEntries.Count >= MaxLogEntries)
                return;

            lock (_logEntries)
                _logEntries.Add(logEntry);

            if (!ShouldWriteToTestOutput)
                return;

            try {
                _testOutputHelper.WriteLine(logEntry.ToString());
            } catch (Exception) { }
        }

        public ILogger CreateLogger(string categoryName) {
            return new TestLogger(categoryName, this);
        }

        public bool ShouldWriteToTestOutput { get; set; } = true;

        public bool IsEnabled(string category, LogLevel logLevel) {
            LogLevel categoryLevel;
            if (_logLevels.TryGetValue(category, out categoryLevel))
                return logLevel >= categoryLevel;

            return logLevel >= MinimumLevel;
        }

        public void SetLogLevel(string category, LogLevel minLogLevel) {
            _logLevels[category] = minLogLevel;
        }

        public void SetLogLevel<T>(LogLevel minLogLevel) {
            _logLevels[TypeNameHelper.GetTypeDisplayName(typeof(T))] = minLogLevel;
        }

        public void Dispose() {}
    }
}