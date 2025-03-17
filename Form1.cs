using KouriChat_Downloader.Utils.PythonTools;
using System.Diagnostics;
using KouriChat_Downloader.Utils;

namespace KouriChat_Downloader
{
    public partial class Form1 : Form
    {
        private PythonEnvironmentManager manager = new PythonEnvironmentManager();
        private TextBox textBox2;
        private Button btnViewLog;
        private bool _pythonInstalled = false;
        private bool _repoCloned = false;
        private bool _dependenciesInstalled = false;
        private string _pythonPath = string.Empty;

        public Form1()
        {
            InitializeComponent();

            // 创建查看日志按钮
            btnViewLog = new Button
            {
                Text = "查看日志",
                Dock = DockStyle.Bottom,
                Height = 30
            };
            btnViewLog.Click += btnViewLog_Click;
            this.Controls.Add(btnViewLog);

            // 添加日志显示框
            textBox2 = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Bottom,
                Height = 150,
                Visible = false // 初始隐藏
            };
            this.Controls.Add(textBox2);

            // 初始化日志系统
            Logger.Initialize(textBox2);
            Logger.Log("程序启动...");

            // 启动时检测已安装的Python和项目状态
            Task.Run(() => CheckExistingComponents());

            // 添加表单关闭事件
            this.FormClosing += Form1_FormClosing;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 添加清理功能按钮
            Button btnCleanup = new Button
            {
                Text = "清理项目",
                Dock = DockStyle.Bottom,
                Height = 30
            };
            btnCleanup.Click += (s, ev) => CleanupProject();
            this.Controls.Add(btnCleanup);

            Load_ComboBox();
        }

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
            foreach (Control ctrl in Controls)
            {
                if (ctrl is Button btn && btn.Text == "清理项目")
                {
                    btn.Enabled = false;
                    break;
                }
            }

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
                        foreach (Control ctrl in Controls)
                        {
                            if (ctrl is Button btn && btn.Text == "清理项目")
                            {
                                btn.Enabled = true;
                                break;
                            }
                        }
                    });
                }
                catch (Exception ex)
                {
                    // 处理错误
                    this.Invoke(() =>
                    {
                        button1.Enabled = true;
                        button2.Enabled = true;
                        foreach (Control ctrl in Controls)
                        {
                            if (ctrl is Button btn && btn.Text == "清理项目")
                            {
                                btn.Enabled = true;
                                break;
                            }
                        }

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
            foreach (Control ctrl in Controls)
            {
                if (ctrl is Button btn && btn.Text == "清理项目")
                {
                    btn.Enabled = false;
                    break;
                }
            }

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
                            if (!await CheckAndInstallDependenciesAsync())
                            {
                                return;
                            }

                            UpdateUI("正在克隆项目文件", 50);
                            bool cloneSuccess = await manager.CloneRepositoryAsync("https://github.com/1Eliver/KouriChat-dev.git", branch, projectDir);
                            if (!cloneSuccess)
                            {
                                UpdateUI("克隆项目失败，请检查网络连接或Git配置", 50);
                                throw new Exception("克隆项目失败，请检查网络连接或Git配置");
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
                            foreach (Control ctrl in Controls)
                            {
                                if (ctrl is Button btn && btn.Text == "清理项目")
                                {
                                    btn.Enabled = true;
                                    break;
                                }
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        // 出错时也要启用按钮
                        this.Invoke(() =>
                        {
                            button1.Enabled = true;
                            button2.Enabled = true;
                            foreach (Control ctrl in Controls)
                            {
                                if (ctrl is Button btn && btn.Text == "清理项目")
                                {
                                    btn.Enabled = true;
                                    break;
                                }
                            }

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
                foreach (Control ctrl in Controls)
                {
                    if (ctrl is Button btn && btn.Text == "清理项目")
                    {
                        btn.Enabled = true;
                        break;
                    }
                }

                MessageBox.Show($"启动任务失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<bool> CheckAndInstallDependenciesAsync()
        {
            // 检查git是否已安装
            bool gitInstalled = await CheckGitInstalledAsync();
            if (!gitInstalled)
            {
                UpdateUI("未检测到Git，请先安装Git并确保添加到PATH环境变量", 25);
                MessageBox.Show("未检测到Git。请先安装Git并将其添加到系统PATH中，然后重试。\n可以从 https://git-scm.com/downloads 下载Git。",
                               "缺少Git", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private async Task<bool> CheckGitInstalledAsync()
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

                    // 记录Git版本，验证是否正常工作
                    Console.WriteLine($"Git版本: {output}");
                    return process.ExitCode == 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Git检测异常: {ex.Message}");
                // 尝试查找Git的几个常见安装位置
                string[] commonPaths = {
                    @"C:\Program Files\Git\cmd\git.exe",
                    @"C:\Program Files (x86)\Git\cmd\git.exe"
                };

                foreach (var path in commonPaths)
                {
                    if (File.Exists(path))
                    {
                        Console.WriteLine($"找到Git安装路径: {path}，但未添加到环境变量");
                    }
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

        private void btnViewLog_Click(object sender, EventArgs e)
        {
            // 显示或隐藏日志区域
            textBox2.Visible = !textBox2.Visible;
            btnViewLog.Text = textBox2.Visible ? "隐藏日志" : "查看日志";

            // 调整窗体大小
            if (textBox2.Visible)
            {
                this.Height += textBox2.Height;
            }
            else
            {
                this.Height -= textBox2.Height;
            }
        }

        private void CheckExistingComponents()
        {
            try
            {
                string baseDir = Application.StartupPath;
                string pythonDir = Path.Combine(baseDir, ".python");
                string projectDir = Path.Combine(baseDir, "KouriChat");

                // 检测Python
                if (manager.TryDetectExistingPython(pythonDir, out string pythonPath))
                {
                    _pythonInstalled = true;
                    _pythonPath = pythonPath;
                    // 仅记录日志，不更新UI
                    Logger.Log($"已检测到Python环境: {_pythonPath}");
                }

                // 检测仓库
                if (Directory.Exists(projectDir) &&
                    Directory.GetFiles(projectDir, "*", SearchOption.AllDirectories).Length > 0)
                {
                    _repoCloned = true;
                    // 仅记录日志，不更新UI
                    Logger.Log("已检测到克隆的项目");

                    // 检测依赖项
                    string requirementsPath = Path.Combine(projectDir, "requirements.txt");
                    if (File.Exists(requirementsPath))
                    {
                        // 检查是否已安装依赖 (简单检测几个常见模块文件)
                        string sitePackagesDir = Path.Combine(pythonDir, "Lib", "site-packages");
                        if (Directory.Exists(sitePackagesDir))
                        {
                            string[] dependencyIndicators = { "flask", "requests", "werkzeug" };
                            bool allFound = true;
                            foreach (var dep in dependencyIndicators)
                            {
                                if (!Directory.Exists(Path.Combine(sitePackagesDir, dep)))
                                {
                                    allFound = false;
                                    break;
                                }
                            }

                            if (allFound)
                            {
                                _dependenciesInstalled = true;
                                // 仅记录日志，不更新UI
                                Logger.Log("依赖已安装");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"检测已有组件时出错: {ex.Message}");
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
            foreach (Control ctrl in Controls)
            {
                if (ctrl is Button btn && btn.Text == "清理项目")
                {
                    btn.Enabled = false;
                    break;
                }
            }

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
                foreach (Control ctrl in Controls)
                {
                    if (ctrl is Button btn && btn.Text == "清理项目")
                    {
                        btn.Enabled = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                // 出错时也要启用按钮
                button1.Enabled = false;
                button2.Enabled = false;
                foreach (Control ctrl in Controls)
                {
                    if (ctrl is Button btn && btn.Text == "清理项目")
                    {
                        btn.Enabled = true;
                        break;
                    }
                }

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
            foreach (Control ctrl in Controls)
            {
                if (ctrl is Button btn && btn.Text == "清理项目")
                {
                    btn.Enabled = false;
                    break;
                }
            }

            // 异步执行更新操作
            Task.Run(async () =>
            {
                try
                {
                    // 检查Git是否安装
                    if (!await CheckGitInstalledAsync())
                    {
                        this.Invoke(() =>
                        {
                            MessageBox.Show("未检测到Git。请先安装Git并将其添加到系统PATH中，然后重试。\n可以从 https://git-scm.com/downloads 下载Git。",
                                           "缺少Git", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        });
                        return;
                    }

                    UpdateUI("正在更新项目...", 25);
                    Logger.Log("开始从远程仓库更新项目...");

                    // 检查是否是Git仓库
                    bool isGitRepo = await CheckIsGitRepositoryAsync(projectDir);
                    if (!isGitRepo)
                    {
                        UpdateUI("当前项目不是Git仓库，无法更新", 0);
                        Logger.Log("当前项目不是Git仓库，无法更新");
                        this.Invoke(() =>
                        {
                            MessageBox.Show("当前项目不是有效的Git仓库，无法更新。\n请尝试清理项目后重新部署。",
                                           "更新失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        });
                        return;
                    }

                    // 执行git pull命令更新代码
                    bool updateSuccess = await UpdateRepositoryAsync(projectDir);
                    if (updateSuccess)
                    {
                        UpdateUI("项目更新成功", 100);
                        Logger.Log("项目更新成功");
                        this.Invoke(() =>
                        {
                            MessageBox.Show("项目已更新到最新版本", "更新成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        });
                    }
                    else
                    {
                        UpdateUI("项目更新失败", 0);
                        Logger.Log("项目更新失败");
                        this.Invoke(() =>
                        {
                            MessageBox.Show("项目更新失败，可能存在冲突或网络问题", "更新失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    this.Invoke(() =>
                    {
                        button1.Enabled = true;
                        button2.Enabled = true;
                        button3.Enabled = true;
                        foreach (Control ctrl in Controls)
                        {
                            if (ctrl is Button btn && btn.Text == "清理项目")
                            {
                                btn.Enabled = true;
                                break;
                            }
                        }
                    });
                }
            });
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
    }
}

