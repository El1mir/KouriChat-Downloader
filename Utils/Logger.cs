using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Linq;

namespace KouriChat_Downloader.Utils
{
    public static class Logger
    {
        private static readonly List<string> _logEntries = new List<string>();
        private static TextBox _logTextBox;
        private static string _logFilePath = Path.Combine(Application.StartupPath, "program.log");

        public static void Initialize(TextBox logTextBox)
        {
            _logTextBox = logTextBox;
            
            try
            {
                // 如果日志文件存在，则加载历史日志
                if (File.Exists(_logFilePath))
                {
                    string[] existingLogs = File.ReadAllLines(_logFilePath);
                    _logEntries.AddRange(existingLogs);
                    
                    // 将历史日志显示到文本框
                    if (_logTextBox != null)
                    {
                        _logTextBox.Text = string.Join(Environment.NewLine, existingLogs);
                        _logTextBox.SelectionStart = _logTextBox.Text.Length;
                        _logTextBox.ScrollToCaret();
                    }
                    
                    // 添加会话分隔符
                    string sessionSeparator = $"=== 新会话 {DateTime.Now} ===";
                    _logEntries.Add(sessionSeparator);
                    File.AppendAllText(_logFilePath, sessionSeparator + Environment.NewLine);
                    
                    if (_logTextBox != null)
                    {
                        _logTextBox.AppendText(Environment.NewLine + sessionSeparator + Environment.NewLine);
                        _logTextBox.SelectionStart = _logTextBox.Text.Length;
                        _logTextBox.ScrollToCaret();
                    }
                }
                else
                {
                    // 创建新日志文件
                    File.WriteAllText(_logFilePath, $"=== 日志开始 {DateTime.Now} ===\r\n");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法处理日志文件: {ex.Message}", "日志错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public static void Log(string message)
        {
            string entry = $"[{DateTime.Now:HH:mm:ss}] {message}";
            _logEntries.Add(entry);

            // 写入日志文件
            try
            {
                File.AppendAllText(_logFilePath, entry + "\r\n");
            }
            catch { /* 忽略文件写入错误 */ }

            // 如果配置了TextBox，则更新UI
            if (_logTextBox != null && _logTextBox.IsHandleCreated)
            {
                try
                {
                    if (_logTextBox.InvokeRequired)
                    {
                        _logTextBox.Invoke(new Action(() => UpdateLogTextBox(entry)));
                    }
                    else
                    {
                        UpdateLogTextBox(entry);
                    }
                }
                catch { /* 忽略UI更新错误 */ }
            }
        }

        private static void UpdateLogTextBox(string entry)
        {
            // 添加日志并滚动到底部
            _logTextBox.AppendText(entry + Environment.NewLine);
            _logTextBox.SelectionStart = _logTextBox.Text.Length;
            _logTextBox.ScrollToCaret();
            
            // 如果日志太长，清除前面部分
            if (_logTextBox.Lines.Length > 500)
            {
                string[] lines = _logTextBox.Lines;
                _logTextBox.Lines = lines[100..];
            }
        }

        public static string[] GetLogEntries()
        {
            return _logEntries.ToArray();
        }

        public static void ClearLogFile()
        {
            try
            {
                // 仅保留当前会话标记后的日志
                int currentSessionIndex = -1;
                
                for (int i = 0; i < _logEntries.Count; i++)
                {
                    if (_logEntries[i].Contains("=== 新会话"))
                    {
                        currentSessionIndex = i;
                    }
                }
                
                List<string> currentSessionLogs;
                if (currentSessionIndex >= 0)
                {
                    currentSessionLogs = _logEntries.Skip(currentSessionIndex).ToList();
                }
                else
                {
                    currentSessionLogs = _logEntries;
                }
                
                // 写入新的日志文件
                File.WriteAllText(_logFilePath, string.Join(Environment.NewLine, currentSessionLogs));
                Logger.Log("日志文件已清空，仅保留当前会话");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"清空日志文件时出错: {ex.Message}");
            }
        }
    }
} 