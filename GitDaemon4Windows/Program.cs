using GitDaemon4Windows;
using Serilog;

string getLogPath()
{
    string dllpath = System.Reflection.Assembly.GetExecutingAssembly().Location.ToString();
    string dirpath = dllpath.Substring(0, dllpath.LastIndexOf("\\") + 1);
    return dirpath + "LogFiles\\GetDaemon4Windows\\GitDaemon4Windows.log";
}

string pth = getLogPath();

IHost host = Host.CreateDefaultBuilder(args)
    .UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .WriteTo.File(pth))
    .UseWindowsService()
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();