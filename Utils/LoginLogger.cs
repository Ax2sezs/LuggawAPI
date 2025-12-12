using System;
using System.IO;
using System.Threading.Tasks;

namespace backend.Utils
{
    public class LoginLogger
    {
        private readonly string _logDirectory;
        private readonly string _logFile;

        public LoginLogger()
        {
            // เก็บ log ไว้ในโฟลเดอร์ "Logs" ของโปรเจกต์
            _logDirectory = Path.Combine(AppContext.BaseDirectory, "Logs");

            // ตั้งชื่อไฟล์ log ตามวัน เช่น login-2025-08-21.txt
            var fileName = $"login-{DateTime.Now:yyyy-MM-dd}.txt";
            _logFile = Path.Combine(_logDirectory, fileName);

            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        public async Task WriteLogAsync(string module, string message)
        {
            string fileName = $"{module.ToLower()}-{DateTime.Now:yyyy-MM-dd}.txt";
            string logFile = Path.Combine(_logDirectory, fileName);

            string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            Console.WriteLine($"[{module}] {logMessage}"); // ดูบน console ด้วย

            await File.AppendAllTextAsync(logFile, logMessage + Environment.NewLine);
        }
    }
}
