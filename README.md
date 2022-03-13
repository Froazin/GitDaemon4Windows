# GitDaemon4Windows
A simple .Net Core Worker to operate Git daemon on Windows as a windows service.

_**This project uses [Serilog](https://github.com/serilog/serilog) under and Apache-2.0 license.** (see Serilog/LICENSE for details.)_

#### Notes:
* Requires Git for Windows to be installed.
* Provided PowerShell install script for this must be in the same folder as GitDaemon4Windows.exe.
* Must have **admin** on a machine to install.
* [Requires .Net Core 6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
* Logs are sent to "_Windows\System32\LogFiles\GitDaemon4Windows\GitDaemon4Windows.log_"

#### Package Dependencies
* [Microsoft.Extensions.Hosting.WindowsServices](https://www.nuget.org/packages/Microsoft.Extensions.Hosting.WindowsServices/6.0.0?_src=template)
* [Serilog.AspNetCore](https://www.nuget.org/packages/Serilog.AspNetCore/5.0.0?_src=template)
* [Serilog.Sinks.File](https://www.nuget.org/packages/Serilog.Sinks.File/5.0.0?_src=template)
