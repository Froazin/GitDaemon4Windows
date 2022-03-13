using System.Diagnostics;
using System.Text.RegularExpressions;

namespace GitDaemon4Windows
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private ProcessStartInfo _startInfo => new ProcessStartInfo("git.exe", "daemon --export-all --port=9418");

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        // During start up
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting up worker.");

            // Check for git installation and start git daemon
            if (_isGit())
            {
                _logger.LogInformation("Found Git in PATH.");

                _startInfo.CreateNoWindow = true;
                _startInfo.UseShellExecute = false;

                // Stop existing git daemon processes
                foreach (Process proc in Process.GetProcessesByName("git-daemon"))
                {
                    proc.Kill();
                }
            }
            else
            {
                _logger.LogCritical("Could not find Git in PATH.");
                throw new Exception("Git not found...");
            }

            return base.StartAsync(cancellationToken);
        }

        // During exit
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Shutting down Git daemon.");
            foreach (Process proc in Process.GetProcessesByName("git-daemon"))
            {
                proc.Kill();
            }
            _logger.LogInformation("Worker stopping.");
            return base.StopAsync(cancellationToken);
        }

        // During runtime
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int retries = 3;
            int rcount = 0;
            bool isFirstStart = true;

            while (!stoppingToken.IsCancellationRequested)
            {
                // if git daemon stops, attempt to restart it
                if (!Process.GetProcessesByName("git-daemon").Any())
                {
                    if (rcount > retries)
                    {
                        _logger.LogCritical("Could not start Git daemon.");
                        throw new Exception("Unable to start Git daemon.");
                    }

                    if (!isFirstStart)
                    {
                        _logger.LogError("Git daemon has stopped running.");
                    }
                    else
                    {
                        isFirstStart = false;
                    }

                    Process.Start(_startInfo);
                    rcount++;
                    _logger.LogInformation("Attempting to start Git daemon.");
                }
                else if (rcount > 0)
                {
                    _logger.LogInformation("Git daemon started successfully.");
                    rcount = 0;
                }

                if (Process.GetProcessesByName("git-daemon").Length > 1)
                {
                    _logger.LogWarning("Git daemon seems to have developed a twin. Restarting...");
                    isFirstStart = true;
                    foreach (Process proc in Process.GetProcessesByName("git-daemon"))
                    {
                        proc.Kill();
                    }
                }

                await Task.Delay(5 * 1000, stoppingToken);
            }
        }

        public bool _isGit()
        {
            // Check if Git is in PATH
            var envpath = Environment.GetEnvironmentVariable("path");

            if (envpath == null || !Regex.IsMatch(envpath.ToString(), @"(?:\\[Gg]it\\cmd)"))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}