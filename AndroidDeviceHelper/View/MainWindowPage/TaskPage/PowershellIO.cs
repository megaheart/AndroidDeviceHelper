using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
//using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AndroidDeviceHelper.View.TasksPage.TaskPage
{
    public delegate void ShellOutputEventHandler(ShellOutputEventArgs e);
    public class ShellOutputEventArgs
    {
        public ShellOutputEventArgs(string[] output, string[] error, string currentDirectory, string command)
        {
            Output = output;
            Error = error;
            CurrentDirectory = currentDirectory;
            Command = command;
        }
        public string CurrentDirectory { get; private set; }
        public string[] Output { get; private set; }
        public string[] Error { get; private set; }
        public string Command { get; private set; }
        public void PrintToConsole()
        {
            Console.WriteLine($"{CurrentDirectory}> {Command}");
            foreach (var line in Output)
                Console.WriteLine(line);
            foreach (var line in Error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(line);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
    //public class PowershellIOCommand
    //{
    //    public PowershellIOCommand() { }
    //    public string Command { get; set; }
    //    public PowershellOutputEventHandler OutputEventHanlder { get; set; }
    //}
    /// <summary>
    /// Create and run powershell process with finite number of commands
    /// </summary>
    public class PowershellIO /*: IDisposable, IAsyncDisposable*/
    {
        private static readonly ProcessStartInfo info = new ProcessStartInfo()
        {
            FileName = "powershell.exe",
            //Arguments = "'C:\\Users\\linh2\\OneDrive\\Tài liệu\\Portable Apps\\platform-tools\\'",
            RedirectStandardInput = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardErrorEncoding = Encoding.UTF8,
            StandardInputEncoding = Encoding.UTF8,
            StandardOutputEncoding = Encoding.UTF8,
        };
        private ConcurrentQueue<string> outputQueue;
        private ConcurrentQueue<bool> isErrorQueue;
        //private List<PowershellIOCommand> commands;
        private List<string> commands = new List<string>(4);
        private List<ShellOutputEventHandler?> handlers = new List<ShellOutputEventHandler?>(4);
        public PowershellIO()
        {
            outputQueue = new ConcurrentQueue<string>();
            isErrorQueue = new ConcurrentQueue<bool>();
            //commands = new List<PowershellIOCommand>();
        }
        //public async Task WaitForExitAsync()
        //{
        //    await p.WaitForExitAsync();
        //}
        //public bool WaitForInputIdle()
        //{
        //    return p.WaitForInputIdle();
        //}
        //public StreamWriter Input
        //{
        //    get => p.StandardInput;
        //}
        /// <summary>
        /// Add a command to the last of command queue which will be executed in powershell process
        /// </summary>
        /// <param name="command">a powershell command line</param>
        /// <param name="handler">return current directory, output, error in event args, is handled after powershell process completed.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">throw if <paramref name="command"/> is null or empty or whitespace.</exception>
        public PowershellIO AddCommand(string command, ShellOutputEventHandler? handler)
        {
            if (string.IsNullOrEmpty(command) || string.IsNullOrWhiteSpace(command))
                throw new ArgumentNullException(nameof(command), $"{nameof(command)} mustn't be null or empty or whitespace.");
            commands.Add(command.Trim());
            handlers.Add(handler);
            return this;
        }
        private void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            string data = e.Data;
            outputQueue.Enqueue(data?.Trim() ?? "");
            isErrorQueue.Enqueue(false);
        }
        private void ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            string data = e.Data;
            outputQueue.Enqueue(data?.Trim() ?? "");
            isErrorQueue.Enqueue(true);
        }
        /// <summary>
        /// Create a powershell process and run all commands which have just been added. After powershell process completed, 
        /// handle all <see cref="ShellOutputEventHandler"/>
        /// </summary>
        public async Task ExecuteAsync()
        {
            if(commands.Count > 0 && commands.Last() != "exit") { commands.Add("exit"); }
            //handlers.Add(null);
            var p = new Process();
            p.StartInfo = info;
            p.Start();
            p.OutputDataReceived += OutputDataReceived;
            p.ErrorDataReceived += ErrorDataReceived;
            p.BeginErrorReadLine();
            p.BeginOutputReadLine();
            using (var input = p.StandardInput)
            {
                if (input.BaseStream.CanWrite)
                {
                    foreach (var command in commands)
                    {
                        input.WriteLine(command);
                    }
                }
            }
            await p.WaitForExitAsync();
            _HandleOutput();
            p.Dispose();
        }
        /// <summary>
        /// Clear all commands and <see cref="ShellOutputEventHandler"/> which were added before
        /// </summary>
        public void Reset()
        {
            commands.Clear();
            handlers.Clear();
            isErrorQueue.Clear();
            outputQueue.Clear();
        }
        /// <summary>
        /// Check if output line is PS Command
        /// </summary>
        public bool isCommand(string line, string _)
        {
            //if (line.Length < command.Length)
            //{
            //    return false;
            //}
            //for (int i = line.Length - 1, j = command.Length - 1; j > -1; j--, i--)
            //{
            //    if (line[i] != command[j])
            //    {
            //        return false;
            //    }
            //}
            //return true;
            return line.Length > 2 && line[0] == 'P' && line[1] == 'S';
        }
        /// <summary>
        /// Handle <see cref="ShellOutputEventHandler"/> which have just been added
        /// </summary>
        private void _HandleOutput()
        {
            int i = 0;
            List<string> outputLines = new List<string>();
            List<string> errorLines = new List<string>();
            var curDir = "";
            while (i != 1 && outputQueue.TryDequeue(out var output))
            {
                isErrorQueue.TryDequeue(out var _);
                if (isCommand(output, commands[i]))
                {
                    curDir = output.Substring(3, output.IndexOf('>') - 3);
                    i++;
                }
            }
            while (i != commands.Count && outputQueue.TryDequeue(out var output))
            {
                isErrorQueue.TryDequeue(out var isError);
                if (isCommand(output, commands[i]))
                {
                    handlers[i - 1]?.Invoke(new ShellOutputEventArgs(outputLines.ToArray(), errorLines.ToArray(), curDir, commands[i - 1]));
                    outputLines.Clear();
                    errorLines.Clear();
                    curDir = output.Substring(3, output.IndexOf('>') - 3);
                    i++;
                }
                else
                {
                    if (isError)
                    {
                        errorLines.Add(output);
                    }
                    else
                    {
                        outputLines.Add(output);
                    }
                }

            }

        }
        //public void Dispose()
        //{
        //    p.WaitForExit();
        //    p.Dispose();
        //}

        //public async ValueTask DisposeAsync()
        //{
        //    await p.WaitForExitAsync();
        //    p.Dispose();
        //}
    }
}
