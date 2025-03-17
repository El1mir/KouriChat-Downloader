using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KouriChat_Downloader.Utils
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Http;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    namespace PythonTools
    {
        public class PythonEnvironmentManager
        {
            private readonly HttpClient _httpClient;
            private string _pythonExecutablePath;

            public PythonEnvironmentManager()
            {
                _httpClient = new HttpClient();
            }

            /// <summary>
            /// 下载指定版本的Python
            /// </summary>
            /// <param name="version">Python版本，如"3.10"，"3.11"</param>
            /// <param name="tempDir">下载临时目录</param>
            /// <returns>Python可执行文件路径</returns>
            public async Task<string> DownloadPythonAsync(string version, string tempDir)
            {
                // 如果已经有Python路径，先删除旧的安装
                if (!string.IsNullOrEmpty(_pythonExecutablePath) && File.Exists(_pythonExecutablePath))
                {
                    try
                    {
                        string pythonDir = Path.GetDirectoryName(_pythonExecutablePath);
                        if (Directory.Exists(pythonDir))
                        {
                            Logger.Log($"删除现有的Python安装: {pythonDir}");
                            Directory.Delete(pythonDir, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"删除旧Python目录时出错: {ex.Message}");
                        // 继续安装新版本
                    }
                }

                // 创建安装目录
                Directory.CreateDirectory(tempDir);

                try
                {
                    // 下载嵌入式Python (更改为嵌入式版本)
                    string fileName = $"python-{version}-embed-amd64.zip";
                    string downloadUrl = $"https://www.python.org/ftp/python/{version}/python-{version}-embed-amd64.zip";
                    string zipPath = Path.Combine(tempDir, fileName);
                    string extractDir = Path.Combine(tempDir, $"python{version.Replace(".", "")}");

                    Logger.Log($"开始下载Python {version} ({downloadUrl})...");

                    // 下载Python压缩包
                    using (var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        using (var fileStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await stream.CopyToAsync(fileStream);
                        }
                    }

                    // 解压Python
                    Logger.Log($"Python {version} 下载完成，开始解压...");
                    Directory.CreateDirectory(extractDir);

                    // 解压缩文件
                    try
                    {
                        System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, extractDir, true);
                        Logger.Log("Python解压完成");
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"解压Python时出错: {ex.Message}");
                        throw;
                    }

                    // 获取Python可执行文件路径
                    _pythonExecutablePath = Path.Combine(extractDir, "python.exe");

                    if (!File.Exists(_pythonExecutablePath))
                    {
                        throw new Exception($"Python解压成功但找不到可执行文件: {_pythonExecutablePath}");
                    }

                    // 设置Python PATH - 为嵌入式Python添加必要的配置
                    try
                    {
                        // 修改python<version>.zip配置
                        string pthFile = Path.Combine(extractDir, $"python{version.Split('.')[0]}{version.Split('.')[1]}._pth");
                        if (File.Exists(pthFile))
                        {
                            // 取消注释import site
                            string content = File.ReadAllText(pthFile);
                            content = content.Replace("#import site", "import site");
                            File.WriteAllText(pthFile, content);
                            Logger.Log("已启用site-packages导入");
                        }

                        // 创建或修改sitecustomize.py以添加pip支持
                        string sitecustomizePath = Path.Combine(extractDir, "sitecustomize.py");
                        File.WriteAllText(sitecustomizePath, @"
import sys
import os

# 添加Lib/site-packages到路径
lib_dir = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'Lib', 'site-packages')
if os.path.exists(lib_dir) and lib_dir not in sys.path:
    sys.path.append(lib_dir)

# 添加Scripts目录到路径
scripts_dir = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'Scripts')
if os.path.exists(scripts_dir) and scripts_dir not in sys.path:
    sys.path.append(scripts_dir)
");
                        Logger.Log("已创建自定义site配置");

                        // 创建Lib/site-packages目录
                        Directory.CreateDirectory(Path.Combine(extractDir, "Lib", "site-packages"));
                        // 创建Scripts目录
                        Directory.CreateDirectory(Path.Combine(extractDir, "Scripts"));
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"配置Python环境时出错: {ex.Message}");
                        // 继续执行，不中断流程
                    }

                    // 安装pip (嵌入式Python默认不包含pip)
                    Logger.Log("正在安装pip...");
                    await EnsurePipInstalledAsync();

                    Logger.Log($"Python {version} 安装成功: {_pythonExecutablePath}");
                    SavePythonPath(_pythonExecutablePath);

                    return _pythonExecutablePath;
                }
                catch (Exception ex)
                {
                    Logger.Log($"Python安装失败: {ex.Message}");
                    throw;
                }
            }

            /// <summary>
            /// 创建Python虚拟环境
            /// </summary>
            /// <param name="venvPath">虚拟环境目录</param>
            /// <returns>操作结果</returns>
            public async Task<bool> CreateVirtualEnvironmentAsync(string venvPath)
            {
                if (string.IsNullOrEmpty(_pythonExecutablePath))
                {
                    throw new InvalidOperationException("请先下载并安装Python");
                }

                Logger.Log($"正在创建虚拟环境: {venvPath}");

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = _pythonExecutablePath,
                    Arguments = $"-m virtualenv \"{venvPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                try
                {
                    using (var process = new Process { StartInfo = processStartInfo })
                    {
                        process.Start();
                        string output = await process.StandardOutput.ReadToEndAsync();
                        string error = await process.StandardError.ReadToEndAsync();
                        await process.WaitForExitAsync();

                        if (process.ExitCode != 0)
                        {
                            Logger.Log($"创建虚拟环境失败: {error}");
                            return false;
                        }

                        Logger.Log("虚拟环境创建成功");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log($"创建虚拟环境异常: {ex.Message}");
                    return false;
                }
            }

            /// <summary>
            /// 克隆Git仓库的指定分支
            /// </summary>
            /// <param name="repoUrl">仓库URL</param>
            /// <param name="branch">分支名称</param>
            /// <param name="targetDir">目标目录</param>
            /// <returns>操作结果</returns>
            public async Task<bool> CloneRepositoryAsync(string repoUrl, string branch, string targetDir)
            {
                Logger.Log($"正在克隆仓库 {repoUrl} 的 {branch} 分支到 {targetDir}");

                // 检查git命令是否可用
                try
                {
                    var checkGitInfo = new ProcessStartInfo
                    {
                        FileName = "git",
                        Arguments = "--version",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (var process = new Process { StartInfo = checkGitInfo })
                    {
                        process.Start();
                        string output = await process.StandardOutput.ReadToEndAsync();
                        await process.WaitForExitAsync();

                        if (process.ExitCode != 0)
                        {
                            Logger.Log("Git命令不可用，请确保已安装Git并添加到PATH中");
                            return false;
                        }

                        Logger.Log($"Git版本: {output.Trim()}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log($"检查Git时出错: {ex.Message}");
                    return false;
                }

                // 如果目标目录已存在，先清空或备份
                if (Directory.Exists(targetDir) && Directory.GetFileSystemEntries(targetDir).Length > 0)
                {
                    Logger.Log($"目标目录 {targetDir} 已存在，进行清理...");
                    try
                    {
                        // 创建一个备份文件夹
                        string backupDir = $"{targetDir}_backup_{DateTime.Now:yyyyMMdd_HHmmss}";
                        Directory.Move(targetDir, backupDir);
                        Directory.CreateDirectory(targetDir);
                        Logger.Log($"原目录已备份到 {backupDir}");
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"清理目录时出错: {ex.Message}");
                        // 继续执行，可能会导致部分文件冲突
                    }
                }

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"clone -b {branch} {repoUrl} \"{targetDir}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                try
                {
                    using (var process = new Process { StartInfo = processStartInfo })
                    {
                        process.Start();
                        string output = await process.StandardOutput.ReadToEndAsync();
                        string error = await process.StandardError.ReadToEndAsync();
                        await process.WaitForExitAsync();

                        Logger.Log($"Git执行结果: ExitCode={process.ExitCode}");
                        Logger.Log($"Git输出: {output}");

                        if (!string.IsNullOrEmpty(error))
                            Logger.Log($"Git错误: {error}");

                        if (process.ExitCode != 0)
                        {
                            Logger.Log($"克隆仓库失败: {error}");
                            return false;
                        }

                        // 验证克隆成功
                        bool hasFiles = Directory.Exists(targetDir) && Directory.GetFiles(targetDir, "*", SearchOption.AllDirectories).Length > 0;
                        Logger.Log($"仓库克隆验证: 目录存在={Directory.Exists(targetDir)}, 包含文件={hasFiles}");

                        if (!hasFiles)
                        {
                            Logger.Log("克隆成功但目录为空，可能是仓库本身为空或分支名错误");
                            return false;
                        }

                        Logger.Log("仓库克隆成功");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log($"克隆过程异常: {ex.Message}");
                    return false;
                }
            }

            /// <summary>
            /// 在虚拟环境中执行命令
            /// </summary>
            /// <param name="venvPath">虚拟环境路径</param>
            /// <param name="workingDirectory">工作目录</param>
            /// <param name="command">要执行的命令</param>
            /// <returns>命令执行输出</returns>
            public async Task<string> ExecutePythonCommandAsync(string workingDirectory, string command)
            {
                if (string.IsNullOrEmpty(_pythonExecutablePath))
                {
                    throw new InvalidOperationException("请先下载并安装Python");
                }

                Logger.Log($"正在执行命令: {command}");
                Logger.Log($"工作目录: {workingDirectory}");

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = _pythonExecutablePath,
                    Arguments = command,  // 直接传递命令，不添加-m
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDirectory
                };

                try
                {
                    using (var process = new Process { StartInfo = processStartInfo })
                    {
                        process.Start();
                        string output = await process.StandardOutput.ReadToEndAsync();
                        string error = await process.StandardError.ReadToEndAsync();
                        await process.WaitForExitAsync();

                        if (process.ExitCode != 0)
                        {
                            throw new Exception($"命令执行失败: {error}\n输出: {output}");
                        }

                        return output;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"执行命令时出错: {ex.Message}\n命令: {command}\n工作目录: {workingDirectory}", ex);
                }
            }

            private string GetPythonInstallerFileName(string version)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return $"python-{version}-embed-amd64.zip";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    return $"Python-{version}.tgz";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return $"python-{version}-macos11.pkg";
                }

                throw new PlatformNotSupportedException("不支持当前操作系统");
            }

            private string GetPythonDownloadUrl(string version, string fileName)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return $"https://www.python.org/ftp/python/{version}/python-{version}-embed-amd64.zip";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    return $"https://www.python.org/ftp/python/{version}/Python-{version}.tgz";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return $"https://www.python.org/ftp/python/{version}/python-{version}-macos11.pkg";
                }
                throw new PlatformNotSupportedException("不支持当前操作系统");
            }

            private async Task InstallPythonAsync(string installerPath, string installDir, string version)
            {
                // 删除可能存在的旧目录
                if (Directory.Exists(installDir))
                {
                    Directory.Delete(installDir, true);
                }
                Directory.CreateDirectory(installDir);

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // 解压缩嵌入式Python
                    Logger.Log($"正在解压Python到: {installDir}");
                    await Task.Run(() => System.IO.Compression.ZipFile.ExtractToDirectory(installerPath, installDir, true));

                    // 创建python._pth文件以启用导入系统
                    string pthFile = Path.Combine(installDir, "python._pth");
                    File.WriteAllText(pthFile,
                        $"python{version.Replace(".", "")}.zip\n" +
                        ".\n" +
                        "import site\n");

                    string pythonExe = Path.Combine(installDir, "python.exe");
                    Logger.Log($"Python路径: {pythonExe}");
                    if (!File.Exists(pythonExe))
                    {
                        throw new Exception($"Python可执行文件未找到: {pythonExe}");
                    }

                    // 从官方仓库下载并安装pip
                    Logger.Log("从官方源下载get-pip.py...");
                    string getpipUrl = "https://bootstrap.pypa.io/get-pip.py";
                    string getpipPath = Path.Combine(installDir, "get-pip.py");

                    using (var httpClient = new HttpClient())
                    using (var response = await httpClient.GetAsync(getpipUrl))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            throw new Exception($"无法下载get-pip.py: {response.StatusCode}");
                        }

                        var content = await response.Content.ReadAsByteArrayAsync();
                        File.WriteAllBytes(getpipPath, content);

                        if (!File.Exists(getpipPath))
                        {
                            throw new Exception($"get-pip.py下载成功但未找到文件: {getpipPath}");
                        }
                        Logger.Log($"get-pip.py下载成功: {getpipPath}, 大小: {content.Length}字节");
                    }

                    // 直接调用python.exe执行get-pip.py
                    Logger.Log("开始安装pip...");
                    try
                    {
                        await RunProcessAsync(pythonExe, getpipPath, installDir);
                        Logger.Log("pip安装成功");
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"安装pip失败: {ex.Message}");
                        // 即使失败也继续执行
                    }

                    // 将Python路径保存
                    _pythonExecutablePath = pythonExe;
                    return;
                }

                // 其他平台的处理
                ProcessStartInfo processStartInfo = null; // 在方法级别声明变量

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    // Linux环境下编译安装Python
                    string extractDir = Path.GetDirectoryName(installerPath);
                    string pythonDirName = Path.GetFileNameWithoutExtension(installerPath);
                    string pythonSourceDir = Path.Combine(extractDir, pythonDirName);

                    processStartInfo = new ProcessStartInfo
                    {
                        FileName = "tar",
                        Arguments = $"-xzf {installerPath} -C {extractDir}",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    processStartInfo = new ProcessStartInfo
                    {
                        FileName = "installer",
                        Arguments = $"-pkg {installerPath} -target {installDir}",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                }
                else
                {
                    throw new PlatformNotSupportedException("不支持当前操作系统");
                }

                // 仅为非 Windows 平台执行进程
                using (var process = new Process { StartInfo = processStartInfo })
                {
                    process.Start();
                    await process.WaitForExitAsync();
                }
            }

            private string GetPythonExecutablePath(string installDir)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    string pythonExe = Path.Combine(installDir, "python.exe");
                    if (!File.Exists(pythonExe))
                    {
                        pythonExe = Path.Combine(installDir, "Scripts", "python.exe");
                    }
                    if (!File.Exists(pythonExe))
                    {
                        throw new FileNotFoundException($"无法找到 Python 可执行文件: {pythonExe}");
                    }
                    return pythonExe;
                }
                else
                {
                    return Path.Combine(installDir, "bin", "python3");
                }
            }

            private string GetActivateScriptPath(string venvPath)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return Path.Combine(venvPath, "Scripts", "activate.bat");
                }
                else
                {
                    return Path.Combine(venvPath, "bin", "activate");
                }
            }

            // 辅助方法：运行进程
            public async Task RunProcessAsync(string fileName, string arguments, string workingDir)
            {
                Logger.Log($"运行进程: {fileName}");
                Logger.Log($"参数: {arguments}");
                Logger.Log($"工作目录: {workingDir}");

                // 检查工作目录是否存在
                if (!Directory.Exists(workingDir))
                {
                    Directory.CreateDirectory(workingDir);
                    Logger.Log($"创建了工作目录: {workingDir}");
                }

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDir
                };

                using (var process = new Process { StartInfo = processStartInfo })
                {
                    try
                    {
                        process.Start();
                        string output = await process.StandardOutput.ReadToEndAsync();
                        string error = await process.StandardError.ReadToEndAsync();
                        await process.WaitForExitAsync();

                        Logger.Log($"进程退出代码: {process.ExitCode}");

                        if (!string.IsNullOrEmpty(output))
                            Logger.Log($"标准输出: {output}");

                        if (!string.IsNullOrEmpty(error))
                            Logger.Log($"标准错误: {error}");

                        if (process.ExitCode != 0)
                        {
                            throw new Exception($"进程执行失败 (代码 {process.ExitCode}): {error}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"进程执行异常: {ex.Message}");
                        throw;
                    }
                }
            }

            // 增强Python检测功能
            public bool TryDetectExistingPython(string pythonDir, out string pythonPath)
            {
                pythonPath = string.Empty;
                try
                {
                    Logger.Log($"开始检测Python环境，目录: {pythonDir}");
                    // 先检查是否有已保存的Python路径
                    string configPath = Path.Combine(Application.StartupPath, "python_config.txt");
                    if (File.Exists(configPath))
                    {
                        string savedPath = File.ReadAllText(configPath).Trim();
                        if (File.Exists(savedPath))
                        {
                            _pythonExecutablePath = savedPath;
                            pythonPath = savedPath;
                            Logger.Log($"从配置文件中检测到Python: {pythonPath}");
                            return true;
                        }
                    }

                    // 检查传入的Python目录及其子目录
                    if (Directory.Exists(pythonDir))
                    {
                        // 检查当前目录
                        Logger.Log($"检查目录: {pythonDir}");
                        string possiblePath = Path.Combine(pythonDir, "python.exe");
                        if (File.Exists(possiblePath))
                        {
                            _pythonExecutablePath = possiblePath;
                            pythonPath = possiblePath;
                            Logger.Log($"直接找到Python: {pythonPath}");
                            SavePythonPath(pythonPath);
                            return true;
                        }

                        // 检查Scripts子目录
                        possiblePath = Path.Combine(pythonDir, "Scripts", "python.exe");
                        if (File.Exists(possiblePath))
                        {
                            _pythonExecutablePath = possiblePath;
                            pythonPath = possiblePath;
                            Logger.Log($"在Scripts目录找到Python: {pythonPath}");
                            SavePythonPath(pythonPath);
                            return true;
                        }

                        // 检查子目录
                        foreach (var dir in Directory.GetDirectories(pythonDir, "*", SearchOption.AllDirectories))
                        {
                            Logger.Log($"检查子目录: {dir}");
                            if (dir.Contains("python"))
                            {
                                // 直接检查目录
                                possiblePath = Path.Combine(dir, "python.exe");
                                if (File.Exists(possiblePath))
                                {
                                    _pythonExecutablePath = possiblePath;
                                    pythonPath = possiblePath;
                                    Logger.Log($"在子目录找到Python: {pythonPath}");
                                    SavePythonPath(pythonPath);
                                    return true;
                                }

                                // 检查Scripts子目录
                                possiblePath = Path.Combine(dir, "Scripts", "python.exe");
                                if (File.Exists(possiblePath))
                                {
                                    _pythonExecutablePath = possiblePath;
                                    pythonPath = possiblePath;
                                    Logger.Log($"在子目录的Scripts中找到Python: {pythonPath}");
                                    SavePythonPath(pythonPath);
                                    return true;
                                }
                            }
                        }
                    }

                    Logger.Log("未检测到已安装的Python");
                    return false;
                }
                catch (Exception ex)
                {
                    Logger.Log($"检测Python时出错: {ex.Message}");
                    return false;
                }
            }

            private void SavePythonPath(string pythonPath)
            {
                try
                {
                    string configPath = Path.Combine(Application.StartupPath, "python_config.txt");
                    File.WriteAllText(configPath, pythonPath);
                    Logger.Log($"已保存Python路径到配置文件: {configPath}");
                }
                catch (Exception ex)
                {
                    Logger.Log($"保存Python路径时出错: {ex.Message}");
                }
            }

            /// <summary>
            /// 检查Python依赖是否已安装
            /// </summary>
            public async Task<bool> CheckDependenciesInstalledAsync(string[] moduleNames)
            {
                if (string.IsNullOrEmpty(_pythonExecutablePath))
                {
                    throw new InvalidOperationException("请先下载并安装Python");
                }

                foreach (var moduleName in moduleNames)
                {
                    try
                    {
                        Logger.Log($"检查模块 {moduleName} 是否已安装...");

                        var processStartInfo = new ProcessStartInfo
                        {
                            FileName = _pythonExecutablePath,
                            Arguments = $"-c \"import {moduleName}; print('{moduleName} 已安装')\"",
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };

                        using (var process = new Process { StartInfo = processStartInfo })
                        {
                            process.Start();
                            string output = await process.StandardOutput.ReadToEndAsync();
                            string error = await process.StandardError.ReadToEndAsync();
                            await process.WaitForExitAsync();

                            if (process.ExitCode != 0 || !string.IsNullOrEmpty(error))
                            {
                                Logger.Log($"模块 {moduleName} 未安装或导入失败: {error}");
                                return false;
                            }

                            Logger.Log($"模块检查结果: {output.Trim()}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"检查模块 {moduleName} 时出错: {ex.Message}");
                        return false;
                    }
                }

                return true;
            }

            // 修改EnsurePipInstalledAsync方法
            public async Task<bool> EnsurePipInstalledAsync()
            {
                Logger.Log("开始为嵌入式Python安装pip...");

                try
                {
                    // 下载get-pip.py
                    string getTempDir = Path.GetTempPath();
                    string getPipPath = Path.Combine(getTempDir, "get-pip.py");

                    using (var httpClient = new HttpClient())
                    {
                        Logger.Log("下载get-pip.py...");
                        byte[] getPipData = await httpClient.GetByteArrayAsync("https://bootstrap.pypa.io/get-pip.py");
                        await File.WriteAllBytesAsync(getPipPath, getPipData);
                    }

                    // 获取Python安装目录
                    string pythonDir = Path.GetDirectoryName(_pythonExecutablePath);

                    // 执行get-pip.py安装pip
                    var installPipInfo = new ProcessStartInfo
                    {
                        FileName = _pythonExecutablePath,
                        Arguments = $"\"{getPipPath}\" --no-warn-script-location",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (var process = new Process { StartInfo = installPipInfo })
                    {
                        process.Start();
                        string output = await process.StandardOutput.ReadToEndAsync();
                        string error = await process.StandardError.ReadToEndAsync();
                        await process.WaitForExitAsync();

                        if (process.ExitCode == 0)
                        {
                            Logger.Log("pip安装成功");

                            // 检查pip是否可用
                            var checkPipInfo = new ProcessStartInfo
                            {
                                FileName = _pythonExecutablePath,
                                Arguments = "-m pip --version",
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                UseShellExecute = false,
                                CreateNoWindow = true
                            };

                            using (var checkProcess = new Process { StartInfo = checkPipInfo })
                            {
                                checkProcess.Start();
                                string checkOutput = await checkProcess.StandardOutput.ReadToEndAsync();
                                await checkProcess.WaitForExitAsync();

                                if (checkProcess.ExitCode == 0)
                                {
                                    Logger.Log($"pip验证成功: {checkOutput.Trim()}");
                                    return true;
                                }
                                else
                                {
                                    Logger.Log("pip安装验证失败，可能需要修复PATH");
                                }
                            }

                            return true;
                        }

                        Logger.Log($"pip安装失败: {error}");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log($"安装pip时出错: {ex.Message}");
                    return false;
                }
            }

            // 增强依赖安装的日志输出
            public async Task<bool> InstallPackagesWithLogging(string pythonPath, string requirementsPath, string targetDir)
            {
                Logger.Log("开始安装Python包，这可能需要几分钟时间...");
                
                try
                {
                    // 构建命令（添加-v参数获取更详细的输出）
                    string arguments = $"-m pip install -r \"{requirementsPath}\" --no-cache-dir --target=\"{targetDir}\" -v";
                    
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = pythonPath,
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    
                    using (var process = new Process { StartInfo = processInfo })
                    {
                        process.Start();
                        
                        // 实时读取输出并记录日志
                        process.OutputDataReceived += (sender, e) => 
                        {
                            if (!string.IsNullOrEmpty(e.Data))
                                Logger.Log($"安装输出: {e.Data}");
                        };
                        
                        process.ErrorDataReceived += (sender, e) => 
                        {
                            if (!string.IsNullOrEmpty(e.Data))
                                Logger.Log($"安装错误: {e.Data}");
                        };
                        
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                        
                        await process.WaitForExitAsync();
                        
                        Logger.Log($"安装进程已退出，代码: {process.ExitCode}");
                        return process.ExitCode == 0;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log($"安装包时出错: {ex.Message}");
                    return false;
                }
            }
        }
    }
}
