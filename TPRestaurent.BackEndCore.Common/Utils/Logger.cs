using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.Utils
{
    public class LogDto
    {
        public Guid Id { get; set; } 
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
    }

    public static class Logger
    {
        private static readonly string LogFilePath = "logs.txt";

        // Method to write a log DTO
        public static void WriteLog(LogDto log)
        {
            try
            {
                log.Id = Guid.NewGuid();
                string logMessage = $"{log.Id} - {log.Timestamp:yyyy-MM-dd HH:mm:ss} - {log.Message}\n";
                File.AppendAllText(LogFilePath, logMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing log: {ex.Message}");
            }
        }

        // Method to read logs as a list of LogDto
        public static async Task<List<LogDto>> ReadLogs()
        {
            var logs = new List<LogDto>();

            try
            {
                if (File.Exists(LogFilePath))
                {
                    var lines = await File.ReadAllLinesAsync(LogFilePath); // Async file read
                    logs = lines
                        .Where(line => !string.IsNullOrWhiteSpace(line))
                        .Select(line =>
                        {
                            var parts = line.Split(new[] { " - " }, 3, StringSplitOptions.None);
                            return new LogDto
                            {
                                Id = Guid.Parse(parts[0]),
                                Timestamp = DateTime.Parse(parts[1]),
                                Message = string.Join("\n", parts[2].Split(" |")),
                            };
                        })
                        .OrderByDescending(l => l.Timestamp)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading logs: {ex.Message}");
            }

            return logs;
        }
        public static async Task<LogDto> ReadLogById(Guid id)
        {
            var logs = await ReadLogs();
            return logs.FirstOrDefault(log => log.Id == id);
        }
    }

}
