using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RGBKit.Core;
using MQTTConnect;

namespace MQTTConnect
{
    public class Program
    {
        [DllImport("kernel32")]
        private static extern void AllocConsole();
        public static void Main(string[] args)
        {
            var mutex = new Mutex(true, "MQTTConnect", out var result);

            if (!result)
            {
                return;
            }

            if (Debugger.IsAttached || args.Length > 0 && args[0] == "--console")
            {
                AllocConsole();
            }

            CreateHostBuilder(args).Build().Run();
        }
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var logFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MQTTConnect\\logs");

            if (!Directory.Exists(logFolder))
                Directory.CreateDirectory(logFolder);
            Worker.API_Init();            

            return Host.CreateDefaultBuilder(args)
                .UseWindowsService();
        }
    }
}
