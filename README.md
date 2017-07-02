# Serilog.Sinks.Literate [![Build status](https://ci.appveyor.com/api/projects/status/nrj4s6rbgtf4210m?svg=true)](https://ci.appveyor.com/project/serilog/serilog-sinks-literate) [![NuGet Version](http://img.shields.io/nuget/v/Serilog.Sinks.Literate.svg?style=flat)](https://www.nuget.org/packages/Serilog.Sinks.Literate/) [![Documentation](https://img.shields.io/badge/docs-wiki-yellow.svg)](https://github.com/serilog/serilog/wiki) [![Join the chat at https://gitter.im/serilog/serilog](https://img.shields.io/gitter/room/serilog/serilog.svg)](https://gitter.im/serilog/serilog)

An alternative colored console sink for Serilog that uses a [literate programming](http://en.wikipedia.org/wiki/Literate_programming)-inspired presentation to showcase the structure/type of event data.  This is in contrast with the [ColoredConsole](https://github.com/serilog/serilog-sinks-coloredconsole) sink that uses color predominantly to emphasise an event's level.

![Screenshot](https://raw.githubusercontent.com/serilog/serilog-sinks-literate/dev/assets/Screenshot.png)

### This package is being retired

The features of this sink have now been merged into the default Serilog console sink. We recommend using [the console sink](https://github.com/serilog/serilog-sinks-console) instead.

### Getting started

Install the [Serilog.Sinks.Literate](https://nuget.org/packages/serilog.sinks.literate) package from NuGet:

```powershell
Install-Package Serilog.Sinks.Literate
```

To configure the sink in C# code, call `WriteTo.LiterateConsole()` during logger configuration:

```csharp
var log = new LoggerConfiguration()
    .WriteTo.LiterateConsole()
    .CreateLogger();
    
Log.Information("This will be written to the literate console");
```

### XML `<appSettings>` configuration

To use the literate console sink with the [Serilog.Settings.AppSettings](https://github.com/serilog/serilog-settings-appsettings) package, first install that package if you haven't already done so:

```powershell
Install-Package Serilog.Settings.AppSettings
```

Instead of configuring the logger in code, call `ReadFrom.AppSettings()`:

```csharp
var log = new LoggerConfiguration()
    .ReadFrom.AppSettings()
    .CreateLogger();
```

In your application's `App.config` or `Web.config` file, specify the literate sink assembly and required path format under the `<appSettings>` node:

```xml
<configuration>
  <appSettings>
    <add key="serilog:using" value="Serilog.Sinks.Literate" />
    <add key="serilog:write-to:LiterateConsole" />
  </appSettings>
</configuration>
```


### JSON `appsettings.json` configuration

To use the rolling file sink with _Microsoft.Extensions.Configuration_, for example with ASP.NET Core or .NET Core, use the [Serilog.Settings.Configuration](https://github.com/serilog/serilog-settings-configuration) package. First install that package if you have not already done so:

```powershell
Install-Package Serilog.Settings.Configuration
```

Instead of configuring the literate console directly in code, call `ReadFrom.Configuration()`:

```csharp
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();
```

In your `appsettings.json` file, under the `Serilog` node, :

```json
{
  "Serilog": {
    "WriteTo": [
      { "Name": "LiterateConsole" }
    ]
  }
}
```

_Copyright &copy; 2016 Serilog Contributors - Provided under the [Apache License, Version 2.0](http://apache.org/licenses/LICENSE-2.0.html)._


