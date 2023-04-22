using AndroidDeviceHelper.View.TasksPage.TaskPage;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndroidDeviceHelper.View.MainWindowPage.TaskPage
{
    public class ProcessIO
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="programFile">Path of program need being executed.</param>
        /// <param name="args">Program arguments</param>
        /// <param name="timeLimit">Set if user wants to limit execution time. If execution time approach timeLimit, process will be killed and 
        /// "ProcessTimeOut" will occur in output <seealso cref="ShellOutputEventArgs.Error"/> array at first element.</param>
        /// <returns><seealso cref="ShellOutputEventArgs"/> contains output and errors information.</returns>
        public static async Task<ShellOutputEventArgs> ExecuteCommand(string programFile, string args, TimeSpan? timeLimit = null)
        {
            //if(handler == null) throw new ArgumentNullException(nameof(handler));

            ConcurrentBag<string> outputQueue = new ConcurrentBag<string>();
            ConcurrentBag<string> errorQueue = new ConcurrentBag<string>();
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo()
            {
                FileName = programFile,
                Arguments = args,
                RedirectStandardInput = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardErrorEncoding = Encoding.UTF8,
                StandardInputEncoding = Encoding.UTF8,
                StandardOutputEncoding = Encoding.UTF8,
            };
            try
            {
                process.Start();
                bool isFinished = false;
                bool timeOut = false;
                if(timeLimit.HasValue)
                {
                    Task.Delay(timeLimit.Value).ContinueWith(t =>
                    {
                        if (!isFinished && !process.HasExited)
                        {
                            timeOut = true;
                            process.Kill();
                        }
                    });
                }
                process.OutputDataReceived += (sender, e) =>
                {
                    string data = e.Data;
                    if (e.Data != null)
                    {
                        outputQueue.Add(data);
                    }
                };
                process.ErrorDataReceived += (sender, e) =>
                {
                    string data = e.Data;
                    if (e.Data != null)
                    {
                        errorQueue.Add(data);
                    }
                };
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                await process.WaitForExitAsync();
                isFinished = true;
                if (timeOut)
                {
                    return new ShellOutputEventArgs(outputQueue.ToArray(), new string[] {"ProcessTimeOut"}, "", args);
                }
            }
            catch (Exception ex)
            {
                while(ex != null)
                {
                    errorQueue.Add((string)ex.Message);
                    ex = ex.InnerException;
                }
            }
            finally
            {
                process.Dispose();
            }
            return new ShellOutputEventArgs(outputQueue.ToArray(), errorQueue.ToArray(), "", args);
        }
    }
}
