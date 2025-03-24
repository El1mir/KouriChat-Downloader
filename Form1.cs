using KouriChat_Downloader.Utils.PythonTools;
using System.Diagnostics;
using KouriChat_Downloader.Utils;
using System.Net.Http;

namespace KouriChat_Downloader
{
    public partial class Form1 : Form
    {
        private PythonEnvironmentManager manager = new PythonEnvironmentManager();
        private bool _pythonInstalled = false;
        private bool _repoCloned = false;
        private bool _dependenciesInstalled = false;
        private string _pythonPath = string.Empty;

        public Form1()
        {
            InitializeComponent();

            // 初始化日志系统
            Logger.Initialize(textBoxLog);
            Logger.Log("程序启动...");

            // 启动时检测已安装的Python和项目状态
            Task.Run(() => CheckExistingComponents());

            // 添加表单关闭事件
            this.FormClosing += Form1_FormClosing;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Load_ComboBox();
        }

        // 添加Load_ComboBox方法
        private void Load_ComboBox()
        {
            // 初始化combo box
            // 支持选择box
            comboBox1.Items.Add("KouriChat-Dev");
            comboBox1.Items.Add("WeChat-wxauto");
            comboBox1.Items.Add("KouriChat-Test");
            comboBox1.Items.Add("QQ-NoneBot");
            comboBox1.Items.Add("KouriChat-Dev-SetupFixed");
            comboBox1.SelectedItem = "KouriChat-Dev-SetupFixed";
            // 初始化选择版本box
            comboBox2.Items.Add("3.11.9");
            comboBox2.Items.Add("3.10.9");
            comboBox2.Items.Add("3.9.13");
            comboBox2.SelectedItem = "3.11.9";
        }

        // 清理项目按钮点击事件
        private void btnCleanup_Click(object sender, EventArgs e)
        {
            string baseDir = Application.StartupPath;
            string projectDir = Path.Combine(baseDir, "KouriChat");
            string pythonDir = Path.Combine(baseDir, ".python");

            // 检查项目或Python环境是否存在
            if (!Directory.Exists(projectDir) && !Directory.Exists(pythonDir))
            {
                MessageBox.Show("没有找到需要清理的项目或Python环境", "无法清理", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            CleanupProject();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string baseDir = Application.StartupPath;
            string projectDir = Path.Combine(baseDir, "KouriChat");

            // 检查项目是否存在
            if (!Directory.Exists(projectDir) || !Directory.GetFiles(projectDir, "*", SearchOption.AllDirectories).Any())
            {
                MessageBox.Show("请先部署项目再运行", "项目未部署", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 检查Python是否可用
            if (string.IsNullOrEmpty(_pythonPath) || !File.Exists(_pythonPath))
            {
                MessageBox.Show("Python环境未准备就绪，请先部署项目", "环境错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 禁用所有操作按钮
            button1.Enabled = false;
            button2.Enabled = false;
            btnCleanup.Enabled = false;

            // 在后台线程启动脚本
            Task.Run(() =>
            {
                try
                {
                    // 检查Web配置脚本是否存在
                    string webConfigPath = Path.Combine(projectDir, "run_config_web.py");

                    if (!File.Exists(webConfigPath))
                    {
                        throw new Exception($"找不到Web配置脚本: {webConfigPath}");
                    }

                    Logger.Log("正在启动项目...");

                    // 只启动Web配置脚本
                    RunPythonScriptInTerminal(webConfigPath, projectDir);

                    Logger.Log("Web配置脚本已启动在独立终端中");

                    // 在UI线程中重新启用按钮
                    this.Invoke(() =>
                    {
                        button1.Enabled = true;
                        button2.Enabled = true;
                        btnCleanup.Enabled = true;
                    });
                }
                catch (Exception ex)
                {
                    // 处理错误
                    this.Invoke(() =>
                    {
                        button1.Enabled = true;
                        button2.Enabled = true;
                        btnCleanup.Enabled = true;

                        MessageBox.Show($"启动项目时出错:\n{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    });
                }
            });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // 检查是否已选择分支和Python版本
            if (comboBox1.SelectedItem == null || comboBox2.SelectedItem == null)
            {
                MessageBox.Show("请先选择分支和Python版本", "参数缺失", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var branch = comboBox1.SelectedItem.ToString();
            var pyv = comboBox2.SelectedItem.ToString();

            // 验证选择项目不为空
            if (string.IsNullOrEmpty(branch) || string.IsNullOrEmpty(pyv))
            {
                MessageBox.Show("分支或Python版本无效", "参数错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 禁用所有操作按钮
            button1.Enabled = false;
            button2.Enabled = false;
            btnCleanup.Enabled = false;

            try
            {
                string baseDir = Application.StartupPath;
                string pythonDir = Path.Combine(baseDir, ".python");
                string projectDir = Path.Combine(baseDir, "KouriChat");

                UpdateUI("准备开始部署...", 0);

                Task.Run(async () =>
                {
                    try
                    {
                        // 下载/检查Python
                        if (!_pythonInstalled)
                        {
                            UpdateUI("正在下载环境配置", 0);
                            string pythonPath = await manager.DownloadPythonAsync(pyv, pythonDir);
                            _pythonPath = pythonPath;
                            _pythonInstalled = true;
                        }
                        else
                        {
                            UpdateUI("使用已安装的Python环境", 25);
                        }

                        // 克隆仓库
                        if (!_repoCloned)
                        {
                            // 确保项目目录存在
                            Directory.CreateDirectory(projectDir);

                            // 检查Git依赖
                            UpdateUI("检查环境依赖...", 25);
                            bool gitAvailable = await CheckGitInstalledAsync(false, true);

                            if (gitAvailable)
                            {
                                // 使用Git克隆
                                UpdateUI("正在克隆项目文件", 50);
                                bool cloneSuccess = await manager.CloneRepositoryAsync("https://github.com/1Eliver/KouriChat-dev.git", branch, projectDir);
                                if (!cloneSuccess)
                                {
                                    // Git克隆失败，尝试ZIP下载
                                    UpdateUI("克隆项目失败，尝试ZIP下载方式...", 50);
                                    Logger.Log("Git克隆失败，切换到ZIP下载方式");
                                    bool zipSuccess = await DeployFromGitHubZipAsync(projectDir, branch);
                                    if (!zipSuccess)
                                    {
                                        UpdateUI("部署项目失败，请检查网络连接", 0);
                                        throw new Exception("部署项目失败，请检查网络连接");
                                    }
                                }
                            }
                            else
                            {
                                // 直接使用ZIP下载
                                UpdateUI("使用ZIP下载方式部署项目...", 50);
                                bool zipSuccess = await DeployFromGitHubZipAsync(projectDir, branch);
                                if (!zipSuccess)
                                {
                                    UpdateUI("部署项目失败，请检查网络连接", 0);
                                    throw new Exception("部署项目失败，请检查网络连接");
                                }
                            }
                            _repoCloned = true;
                        }
                        else
                        {
                            UpdateUI("使用已克隆的项目", 50);
                        }

                        // 安装依赖
                        if (!_dependenciesInstalled)
                        {
                            UpdateUI("正在安装项目依赖", 75);
                            bool success = await InstallDependenciesAsync(projectDir);
                            if (success)
                            {
                                _dependenciesInstalled = true;
                            }
                            else
                            {
                                UpdateUI("依赖安装失败", 75);
                                throw new Exception("项目依赖安装失败");
                            }
                        }
                        else
                        {
                            UpdateUI("使用已安装的依赖", 75);
                        }

                        UpdateUI("部署完成", 100);

                        // 在部署完成后启用按钮
                        this.Invoke(() =>
                        {
                            button1.Enabled = true;
                            button2.Enabled = true;
                            btnCleanup.Enabled = true;
                        });
                    }
                    catch (Exception ex)
                    {
                        // 出错时也要启用按钮
                        this.Invoke(() =>
                        {
                            button1.Enabled = true;
                            button2.Enabled = true;
                            btnCleanup.Enabled = true;

                            MessageBox.Show($"发生错误\n{ex.Message}\n发生在：\n{ex.StackTrace}\n请联系开发人员解决",
                                           "运行时发生错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                // 启用按钮
                button1.Enabled = true;
                button2.Enabled = true;
                btnCleanup.Enabled = true;

                MessageBox.Show($"启动任务失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<bool> CheckAndInstallDependenciesAsync(bool showGitWarning = true)
        {
            // 检查git是否已安装
            bool gitInstalled = await CheckGitInstalledAsync(showGitWarning);
            if (!gitInstalled && showGitWarning)
            {
                UpdateUI("未检测到Git，请先安装Git并确保添加到PATH环境变量", 25);
                MessageBox.Show("未检测到Git。请先安装Git并将其添加到系统PATH中，然后重试。\n可以从 https://git-scm.com/downloads 下载Git。",
                               "缺少Git", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return gitInstalled || !showGitWarning;
        }

        private async Task<bool> CheckGitInstalledAsync(bool showWarning = false, bool autoSwitch = false)
        {
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = new Process { StartInfo = processStartInfo })
                {
                    process.Start();
                    string output = await process.StandardOutput.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    Logger.Log($"Git版本检测结果: {output.Trim()}");

                    if (process.ExitCode != 0)
                    {
                        if (showWarning && !autoSwitch)
                        {
                            this.Invoke(() =>
                            {
                                MessageBox.Show("未检测到Git。请先安装Git并将其添加到系统PATH中，然后重试。\n可以从 https://git-scm.com/downloads 下载Git。",
                                       "缺少Git", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            });
                        }
                        else if (autoSwitch)
                        {
                            Logger.Log("未检测到Git，将自动使用ZIP下载方式");
                            UpdateUI("未检测到Git，将使用ZIP下载方式", 25);
                        }
                    }

                    return process.ExitCode == 0;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Git检测异常: {ex.Message}");

                if (showWarning && !autoSwitch)
                {
                    this.Invoke(() =>
                    {
                        MessageBox.Show("未检测到Git。请先安装Git并将其添加到系统PATH中，然后重试。\n可以从 https://git-scm.com/downloads 下载Git。",
                               "缺少Git", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    });
                }
                else if (autoSwitch)
                {
                    Logger.Log("Git检测异常，将自动使用ZIP下载方式");
                    UpdateUI("Git检测异常，将使用ZIP下载方式", 25);
                }

                return false;
            }
        }

        private void UpdateUI(string message, int progressValue)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(() => UpdateUI(message, progressValue));
                return;
            }

            textBox1.Text = message;
            progressBar1.Value = progressValue;
        }

        private async Task CheckExistingComponents()
        {
            try
            {
                string baseDir = Application.StartupPath;
                string pythonDir = Path.Combine(baseDir, ".python");
                string projectDir = Path.Combine(baseDir, "KouriChat");

                // 检查Python
                if (Directory.Exists(pythonDir))
                {
                    var pythonFiles = Directory.GetFiles(pythonDir, "python.exe", SearchOption.AllDirectories);
                    if (pythonFiles.Length > 0)
                    {
                        _pythonPath = pythonFiles[0];
                        _pythonInstalled = true;
                        Logger.Log($"找到已安装的Python: {_pythonPath}");
                    }
                }

                // 检查项目目录
                if (Directory.Exists(projectDir) && Directory.GetFiles(projectDir, "*", SearchOption.AllDirectories).Any())
                {
                    _repoCloned = true;
                    Logger.Log("找到已克隆的项目目录");
                }

                // 不在启动时检查Git，只在实际需要使用时检查
                // 这一行注释掉或删除：await CheckGitInstalledAsync();
            }
            catch (Exception ex)
            {
                Logger.Log($"检查现有组件时出错: {ex.Message}");
            }
        }

        // 添加关闭事件处理
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 清空日志文件
            Logger.ClearLogFile();
        }

        private void CleanupProject()
        {
            // 禁用所有操作按钮
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            btnCleanup.Enabled = false;

            try
            {
                string baseDir = Application.StartupPath;
                string pythonDir = Path.Combine(baseDir, ".python");
                string projectDir = Path.Combine(baseDir, "KouriChat");
                bool cleaned = false;

                // 确保所有Python进程已终止
                try
                {
                    foreach (var process in Process.GetProcessesByName("python"))
                    {
                        try
                        {
                            process.Kill();
                            process.WaitForExit(3000); // 等待进程终止
                            Logger.Log($"已终止Python进程: {process.Id}");
                        }
                        catch (Exception ex)
                        {
                            Logger.Log($"无法终止Python进程: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log($"查找Python进程时出错: {ex.Message}");
                }

                // 稍微延迟以确保文件解锁
                System.Threading.Thread.Sleep(1000);

                // 清理Python环境
                if (Directory.Exists(pythonDir))
                {
                    Logger.Log("正在清理Python环境...");
                    try
                    {
                        // 使用具有更高容错性的递归删除方法
                        SafeDeleteDirectory(pythonDir);
                        Logger.Log("Python环境已清理");

                        _pythonInstalled = false;
                        _pythonPath = string.Empty;

                        cleaned = true;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"清理Python环境时出错: {ex.Message}");
                    }

                    // 删除Python配置文件
                    try
                    {
                        string configPath = Path.Combine(Application.StartupPath, "python_config.txt");
                        if (File.Exists(configPath))
                        {
                            File.Delete(configPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"删除配置文件时出错: {ex.Message}");
                    }
                }

                // 清理项目文件
                if (Directory.Exists(projectDir))
                {
                    Logger.Log("正在清理项目文件...");
                    try
                    {
                        SafeDeleteDirectory(projectDir);
                        Logger.Log("项目文件已清理");

                        _repoCloned = false;
                        _dependenciesInstalled = false;

                        cleaned = true;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"清理项目文件时出错: {ex.Message}");
                    }
                }

                if (cleaned)
                {
                    MessageBox.Show("项目和环境已清理完毕，可以重新部署", "清理完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    UpdateUI("已清理项目", 0);
                }
                else
                {
                    MessageBox.Show("清理过程中出现一些问题，可能需要手动删除文件或重启应用程序", "清理警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // 在完成后启用按钮
                button1.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = true;
                btnCleanup.Enabled = true;
            }
            catch (Exception ex)
            {
                // 出错时也要启用按钮
                button1.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = true;
                btnCleanup.Enabled = true;

                Logger.Log($"清理项目时出错: {ex.Message}");
                MessageBox.Show($"清理项目时出错: {ex.Message}\n可能需要以管理员身份运行程序或手动删除文件", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 安全地递归删除目录，即使有一些文件可能被锁定
        private void SafeDeleteDirectory(string directory)
        {
            try
            {
                foreach (string file in Directory.GetFiles(directory))
                {
                    try
                    {
                        File.SetAttributes(file, FileAttributes.Normal); // 清除只读属性
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"无法删除文件 {file}: {ex.Message}");
                    }
                }

                foreach (string subDir in Directory.GetDirectories(directory))
                {
                    try
                    {
                        SafeDeleteDirectory(subDir);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"无法处理子目录 {subDir}: {ex.Message}");
                    }
                }

                Directory.Delete(directory, false); // 尝试删除空目录
            }
            catch (Exception ex)
            {
                Logger.Log($"无法删除目录 {directory}: {ex.Message}");
                throw; // 重新抛出异常以供上层处理
            }
        }

        private async Task<bool> InstallDependenciesAsync(string projectDir)
        {
            try
            {
                // 检查requirements.txt是否存在
                string requirementsPath = Path.Combine(projectDir, "requirements.txt");
                if (!File.Exists(requirementsPath))
                {
                    Logger.Log("找不到requirements.txt文件，跳过依赖安装");
                    return true;
                }

                // 获取Python目录和site-packages路径
                string pythonDir = Path.GetDirectoryName(_pythonPath);
                string sitePackagesDir = Path.Combine(pythonDir, "Lib", "site-packages");

                // 确保site-packages目录存在
                Directory.CreateDirectory(sitePackagesDir);

                // 使用新方法获取实时日志输出
                bool installResult = await manager.InstallPackagesWithLogging(
                    _pythonPath,
                    requirementsPath,
                    sitePackagesDir,
                    new List<string> {
                        "https://mirrors.aliyun.com/pypi/simple",
                        "https://pypi.tuna.tsinghua.edu.cn/simple",
                        "https://repo.huaweicloud.com/repository/pypi/simple",
                        "https://mirrors.cloud.tencent.com/pypi/simple",
                        "https://pypi.mirrors.ustc.edu.cn/simple",
                        "https://pypi.org/simple"
                    }
                    );

                if (!installResult)
                {
                    Logger.Log("依赖安装过程返回错误代码，尝试验证是否部分安装成功");
                }

                // 验证依赖安装
                bool checkResult = await manager.CheckDependenciesInstalledAsync(new[] { "flask", "requests" });
                if (checkResult)
                {
                    Logger.Log("依赖安装验证成功");
                    return true;
                }
                else
                {
                    Logger.Log("依赖安装后验证失败，尝试使用备用安装方法");

                    // 使用pip直接安装核心包
                    foreach (var package in new[] { "flask", "requests", "werkzeug" })
                    {
                        Logger.Log($"单独安装核心包: {package}");
                        await manager.RunProcessAsync(
                            _pythonPath,
                            $"-m pip install {package} --no-cache-dir --target=\"{sitePackagesDir}\"",
                            pythonDir);
                    }

                    // 再次验证
                    checkResult = await manager.CheckDependenciesInstalledAsync(new[] { "flask", "requests" });
                    if (checkResult)
                    {
                        Logger.Log("核心依赖安装验证成功");
                        return true;
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"安装依赖时出错: {ex.Message}");
                return false;
            }
        }

        // 修改启动脚本方法，避免重复创建
        private void RunPythonScriptInTerminal(string scriptPath, string projectDir)
        {
            Logger.Log($"在独立终端中启动脚本: {scriptPath}");

            // 获取Python目录
            string pythonDir = Path.GetDirectoryName(_pythonPath);
            string sitePackagesDir = Path.Combine(pythonDir, "Lib", "site-packages");

            // 获取脚本名称
            string scriptName = Path.GetFileName(scriptPath);

            // 使用固定名称的启动脚本和bat文件
            string launcherPath = Path.Combine(projectDir, $"_launcher_{scriptName}_downloader.py");
            string batPath = Path.Combine(projectDir, $"run_{scriptName}_downloader.bat");

            // 检查启动脚本是否存在，不存在才创建
            if (!File.Exists(launcherPath))
            {
                Logger.Log($"创建启动脚本: {launcherPath}");
                File.WriteAllText(launcherPath, $@"
import sys
import os
import time

print('=========== 启动脚本初始化 ===========')
print(f'正在启动脚本: {scriptName}')
print(f'项目目录: {projectDir.Replace("\\", "\\\\")}')

# 设置项目根目录为当前工作目录
os.chdir('{projectDir.Replace("\\", "\\\\")}')

# 第一步: 暴力添加所有可能的源代码目录到Python路径
# 1. 添加项目根目录
project_dir = '{projectDir.Replace("\\", "\\\\")}'
if project_dir not in sys.path:
    sys.path.insert(0, project_dir)
    print(f'已添加到路径: {{project_dir}}')

# 2. 添加src目录及其所有子目录
src_dir = os.path.join(project_dir, 'src')
if os.path.exists(src_dir) and src_dir not in sys.path:
    sys.path.insert(0, src_dir)
    print(f'已添加到路径: {{src_dir}}')
    # 添加src的子目录
    for root, dirs, files in os.walk(src_dir):
        if root not in sys.path:
            sys.path.insert(0, root)
            print(f'已添加到路径: {{root}}')

# 3. 递归添加整个KouriChat目录及其所有子目录到路径
# 这会添加所有可能包含模块的目录
for root, dirs, files in os.walk(project_dir):
    if root not in sys.path:
        sys.path.insert(0, root)
        print(f'已添加到路径: {{root}}')

# 4. 特殊处理：直接导入可能直接导入的包
try:
    print('尝试直接导入src作为模块')
    import src
    print('导入src成功')
except ImportError as e:
    print(f'导入src失败: {{e}}')
    # 尝试创建src模块
    if not os.path.exists(os.path.join(project_dir, 'src')):
        os.makedirs(os.path.join(project_dir, 'src'))
        with open(os.path.join(project_dir, 'src', '__init__.py'), 'w') as f:
            f.write('# 自动创建的初始化文件\n')
        print('已创建src目录和__init__.py文件')
        try:
            import src
            print('创建后导入src成功')
        except ImportError as e:
            print(f'创建后导入仍然失败: {{e}}')

# 显示当前所有路径
print('\\n当前Python路径:')
for i, path in enumerate(sys.path):
    print(f'  {{i}}: {{path}}')

print('\\n=========== 开始执行脚本 ===========')

# 直接使用__file__导入目标脚本
script_path = '{scriptPath.Replace("\\", "\\\\")}'
try:
    # 给执行脚本一个全局变量，标记这是从启动器启动的
    globals()['KOURICHAT_LAUNCHER'] = True
    
    # 从源文件读取内容执行
    with open(script_path, 'r', encoding='utf-8') as f:
        script_content = f.read()
        # 使用exec执行，提供全局和局部环境
        exec(script_content, globals())
except Exception as e:
    print(f'执行脚本时出错: {{e}}')
    import traceback
    traceback.print_exc()

print('\\n=========== 脚本执行完成 ===========')
print('按回车键关闭窗口...')
try:
    input()
except:
    # 防止某些环境下input出错
    time.sleep(30)  # 等待30秒
");
            }
            else
            {
                Logger.Log($"使用已存在的启动脚本: {launcherPath}");
            }

            // 检查bat文件是否存在，不存在才创建
            if (!File.Exists(batPath))
            {
                Logger.Log($"创建批处理文件: {batPath}");
                File.WriteAllText(batPath, $@"
@echo off
echo 启动Python脚本: {scriptName}
set PYTHONPATH={projectDir};{sitePackagesDir}
""{_pythonPath}"" ""{launcherPath}""
pause
");
            }
            else
            {
                Logger.Log($"使用已存在的批处理文件: {batPath}");
            }

            try
            {
                // 创建进程信息，使用cmd运行bat文件
                var processInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c \"{batPath}\"",
                    WorkingDirectory = projectDir,
                    UseShellExecute = true,
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal
                };

                // 启动进程
                var process = new Process { StartInfo = processInfo };
                process.Start();

                Logger.Log($"已在独立终端中启动 {scriptName}");
            }
            catch (Exception ex)
            {
                Logger.Log($"启动 {scriptName} 时出错: {ex.Message}");
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {
            // 这是GroupBox1的Enter事件处理程序
            // 通常可以留空，除非需要特定功能
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {
            // 这是GroupBox2的Enter事件处理程序
            // 通常可以留空，除非需要特定功能
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {
            // 这是ProgressBar1的Click事件处理程序
            // 通常可以留空，除非需要特定功能
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string baseDir = Application.StartupPath;
            string projectDir = Path.Combine(baseDir, "KouriChat");

            // 检查项目是否已经存在
            if (!Directory.Exists(projectDir) || !Directory.GetFiles(projectDir, "*", SearchOption.AllDirectories).Any())
            {
                MessageBox.Show("本地项目不存在，请先部署项目", "无法更新", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 禁用操作按钮
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            btnCleanup.Enabled = false;

            // 异步执行更新操作
            Task.Run(async () =>
            {
                try
                {
                    // 先检查是否可以使用Git更新，但不显示警告
                    bool gitInstalled = await CheckGitInstalledAsync();
                    // 如果需要检查其他依赖但不显示Git警告，可以这样调用
                    // await CheckAndInstallDependenciesAsync(false);

                    bool isGitRepo = false;

                    if (gitInstalled)
                    {
                        isGitRepo = await CheckIsGitRepositoryAsync(projectDir);

                        if (isGitRepo)
                        {
                            // 如果Git可用且是Git仓库，使用Git更新
                            UpdateUI("使用Git更新项目...", 25);
                            Logger.Log("使用Git方式更新项目...");

                            // 执行git pull命令更新代码
                            bool updateSuccess = await UpdateRepositoryAsync(projectDir);
                            if (updateSuccess)
                            {
                                UpdateUI("项目更新成功", 100);
                                Logger.Log("Git更新成功");
                                this.Invoke(() =>
                                {
                                    MessageBox.Show("项目已更新到最新版本", "更新成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                });

                                // 更新恢复按钮状态并退出方法
                                this.Invoke(RestoreButtons);
                                return;
                            }

                            // Git更新失败，转为ZIP方式更新
                            UpdateUI("Git更新失败，尝试ZIP下载方式...", 30);
                            Logger.Log("Git更新失败，切换到ZIP下载方式");
                        }
                    }

                    // Git不可用或不是Git仓库，使用ZIP更新
                    UpdateUI("准备使用ZIP下载方式更新...", 25);
                    Logger.Log("使用ZIP下载方式更新项目...");

                    // 使用ZIP文件方式更新
                    bool zipUpdateSuccess = await UpdateFromGitHubZipAsync(projectDir);
                    if (zipUpdateSuccess)
                    {
                        UpdateUI("项目更新成功", 100);
                        Logger.Log("ZIP更新成功");
                        this.Invoke(() =>
                        {
                            MessageBox.Show("项目已通过ZIP下载更新到最新版本", "更新成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        });
                    }
                    else
                    {
                        UpdateUI("项目更新失败", 0);
                        Logger.Log("ZIP更新失败");
                        this.Invoke(() =>
                        {
                            MessageBox.Show("项目更新失败，请检查网络连接或尝试清理后重新部署", "更新失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        });
                    }
                }
                catch (Exception ex)
                {
                    UpdateUI("更新时发生错误", 0);
                    Logger.Log($"更新项目时出错: {ex.Message}");
                    this.Invoke(() =>
                    {
                        MessageBox.Show($"更新项目时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    });
                }
                finally
                {
                    // 恢复按钮状态
                    this.Invoke(RestoreButtons);
                }
            });
        }

        private void RestoreButtons()
        {
            button1.Enabled = true;
            button2.Enabled = true;
            button3.Enabled = true;
            btnCleanup.Enabled = true;
        }

        private async Task<bool> UpdateFromGitHubZipAsync(string projectDir)
        {
            try
            {
                UpdateUI("准备从GitHub下载最新版本...", 30);

                // 从comboBox中获取当前分支
                string branch = "main"; // 默认分支
                this.Invoke(() =>
                {
                    if (comboBox1.SelectedItem != null)
                    {
                        branch = comboBox1.SelectedItem.ToString();
                    }
                });

                // 构建GitHub ZIP下载URL (对于私有仓库可能需要身份验证)
                string repoOwner = "1Eliver";
                string repoName = "KouriChat-dev";
                string zipUrl = $"https://github.com/{repoOwner}/{repoName}/archive/refs/heads/{branch}.zip";

                UpdateUI("正在下载最新代码...", 40);
                Logger.Log($"从 {zipUrl} 下载最新代码");

                // 创建临时目录
                string tempDir = Path.Combine(Path.GetTempPath(), $"KouriChat_Update_{Guid.NewGuid()}");
                Directory.CreateDirectory(tempDir);
                string zipPath = Path.Combine(tempDir, "update.zip");

                // 下载ZIP文件
                using (var httpClient = new HttpClient())
                {
                    // 增加超时时间，GitHub有时候下载较慢
                    httpClient.Timeout = TimeSpan.FromMinutes(5);

                    using (var response = await httpClient.GetAsync(zipUrl, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();

                        // 流式下载，避免内存压力
                        using (var fileStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await response.Content.CopyToAsync(fileStream);
                        }
                    }
                }

                Logger.Log("ZIP文件下载完成，准备解压");
                UpdateUI("下载完成，正在解压文件...", 60);

                // 解压ZIP文件到临时目录
                string extractPath = Path.Combine(tempDir, "extracted");
                Directory.CreateDirectory(extractPath);

                System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, extractPath, true);

                // 在解压后的目录中查找实际的项目文件夹
                // GitHub ZIP通常将内容放在一个以"repoName-branch"命名的子文件夹中
                string[] extractedDirs = Directory.GetDirectories(extractPath);
                if (extractedDirs.Length == 0)
                {
                    Logger.Log("解压后找不到有效的项目目录");
                    return false;
                }

                string extractedProjectDir = extractedDirs[0]; // 通常只有一个目录
                Logger.Log($"找到解压后的项目目录: {extractedProjectDir}");

                // 询问用户如何处理本地修改
                DialogResult handleLocalChanges = DialogResult.Yes;
                this.Invoke(() =>
                {
                    handleLocalChanges = MessageBox.Show(
                        "更新可能会覆盖您本地的修改。\n\n是否要保留本地已修改的文件？\n\n" +
                        "· 选择\"是\"将保留本地已修改的文件\n" +
                        "· 选择\"否\"将使用远程文件覆盖所有本地修改",
                        "处理本地修改",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);
                });

                UpdateUI("正在应用更新...", 80);

                // 备份当前项目配置文件
                string configBackupDir = Path.Combine(tempDir, "config_backup");
                Directory.CreateDirectory(configBackupDir);

                // 如果用户选择保留本地修改，创建本地修改文件的备份
                if (handleLocalChanges == DialogResult.Yes)
                {
                    Logger.Log("备份本地配置文件");

                    // 要保留的文件列表（通常是配置文件、日志、数据文件等）
                    string[] filesToPreserve = {
                        "run_config_web.py",
                        "run_web.py",
                        "config.json",
                        "app.db",
                        "*.log"
                    };

                    foreach (string filePattern in filesToPreserve)
                    {
                        foreach (string filePath in Directory.GetFiles(projectDir, filePattern, SearchOption.AllDirectories))
                        {
                            string relativePath = filePath.Substring(projectDir.Length + 1);
                            string backupPath = Path.Combine(configBackupDir, relativePath);

                            // 确保目标目录存在
                            Directory.CreateDirectory(Path.GetDirectoryName(backupPath));

                            File.Copy(filePath, backupPath, true);
                            Logger.Log($"已备份文件: {relativePath}");
                        }
                    }
                }

                // 复制新文件到项目目录
                Logger.Log("正在复制新文件到项目目录");
                CopyDirectory(extractedProjectDir, projectDir, handleLocalChanges == DialogResult.Yes ? configBackupDir : null);

                // 清理临时文件
                try
                {
                    Directory.Delete(tempDir, true);
                }
                catch (Exception ex)
                {
                    Logger.Log($"清理临时文件时出错（非致命）: {ex.Message}");
                }

                Logger.Log("ZIP更新完成");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"从ZIP更新时出错: {ex.Message}");
                return false;
            }
        }

        private void CopyDirectory(string sourceDir, string targetDir, string preserveFromDir = null)
        {
            // 确保目标目录存在
            Directory.CreateDirectory(targetDir);

            // 复制所有文件
            foreach (string filePath in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
            {
                string relativePath = filePath.Substring(sourceDir.Length + 1);
                string destPath = Path.Combine(targetDir, relativePath);

                // 确保目标目录存在
                Directory.CreateDirectory(Path.GetDirectoryName(destPath));

                // 检查是否需要保留此文件
                bool shouldPreserve = false;
                if (preserveFromDir != null)
                {
                    string preservePath = Path.Combine(preserveFromDir, relativePath);
                    if (File.Exists(preservePath))
                    {
                        // 这个文件在备份中存在，使用备份版本
                        File.Copy(preservePath, destPath, true);
                        Logger.Log($"保留了本地修改: {relativePath}");
                        shouldPreserve = true;
                    }
                }

                if (!shouldPreserve)
                {
                    // 这个文件不需要保留，使用新版本
                    File.Copy(filePath, destPath, true);
                }
            }
        }

        private async Task<bool> CheckIsGitRepositoryAsync(string directory)
        {
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "rev-parse --is-inside-work-tree",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = directory
                };

                using (var process = new Process { StartInfo = processStartInfo })
                {
                    process.Start();
                    string output = await process.StandardOutput.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    return process.ExitCode == 0 && output.Trim() == "true";
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"检查Git仓库时出错: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> UpdateRepositoryAsync(string directory)
        {
            try
            {
                UpdateUI("正在检查远程变更...", 50);

                // 先执行git fetch获取远程更新
                var fetchInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "fetch",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = directory
                };

                using (var fetchProcess = new Process { StartInfo = fetchInfo })
                {
                    fetchProcess.Start();
                    await fetchProcess.WaitForExitAsync();

                    if (fetchProcess.ExitCode != 0)
                    {
                        Logger.Log("获取远程仓库信息失败");
                        return false;
                    }
                }

                UpdateUI("正在应用更新...", 75);

                // 然后执行git pull更新本地代码
                var pullInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "pull",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = directory
                };

                using (var pullProcess = new Process { StartInfo = pullInfo })
                {
                    pullProcess.Start();
                    string output = await pullProcess.StandardOutput.ReadToEndAsync();
                    string error = await pullProcess.StandardError.ReadToEndAsync();
                    await pullProcess.WaitForExitAsync();

                    Logger.Log($"Git pull输出: {output}");
                    if (!string.IsNullOrEmpty(error))
                        Logger.Log($"Git pull错误: {error}");

                    return pullProcess.ExitCode == 0;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"更新仓库时出错: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> DeployFromGitHubZipAsync(string projectDir, string branch)
        {
            try
            {
                UpdateUI("准备从GitHub下载最新版本...", 30);

                // 构建GitHub ZIP下载URL
                string repoOwner = "1Eliver";
                string repoName = "KouriChat-dev";
                string zipUrl = $"https://github.com/{repoOwner}/{repoName}/archive/refs/heads/{branch}.zip";

                UpdateUI("正在下载项目代码...", 40);
                Logger.Log($"从 {zipUrl} 下载项目代码");

                // 创建临时目录
                string tempDir = Path.Combine(Path.GetTempPath(), $"KouriChat_Deploy_{Guid.NewGuid()}");
                Directory.CreateDirectory(tempDir);
                string zipPath = Path.Combine(tempDir, "deploy.zip");

                // 下载ZIP文件
                using (var httpClient = new HttpClient())
                {
                    // 增加超时时间，GitHub有时候下载较慢
                    httpClient.Timeout = TimeSpan.FromMinutes(5);

                    using (var response = await httpClient.GetAsync(zipUrl, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();

                        // 流式下载，避免内存压力
                        using (var fileStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await response.Content.CopyToAsync(fileStream);
                        }
                    }
                }

                Logger.Log("ZIP文件下载完成，准备解压");
                UpdateUI("下载完成，正在解压文件...", 60);

                // 解压ZIP文件到临时目录
                string extractPath = Path.Combine(tempDir, "extracted");
                Directory.CreateDirectory(extractPath);

                System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, extractPath, true);

                // 在解压后的目录中查找实际的项目文件夹
                string[] extractedDirs = Directory.GetDirectories(extractPath);
                if (extractedDirs.Length == 0)
                {
                    Logger.Log("解压后找不到有效的项目目录");
                    return false;
                }

                string extractedProjectDir = extractedDirs[0]; // 通常只有一个目录
                Logger.Log($"找到解压后的项目目录: {extractedProjectDir}");

                // 确保目标目录存在
                Directory.CreateDirectory(projectDir);

                // 复制所有文件到项目目录
                UpdateUI("正在复制项目文件...", 80);
                CopyDirectory(extractedProjectDir, projectDir);

                // 清理临时文件
                try
                {
                    Directory.Delete(tempDir, true);
                }
                catch (Exception ex)
                {
                    Logger.Log($"清理临时文件时出错（非致命）: {ex.Message}");
                }

                Logger.Log("ZIP部署完成");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"从ZIP部署时出错: {ex.Message}");
                return false;
            }
        }
    }
}

