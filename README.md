# Serilog.Sinks.Literate [![Build status](https://ci.appveyor.com/api/projects/status/nrj4s6rbgtf4210m/branch/master?svg=true)](https://ci.appveyor.com/project/serilog/serilog-sinks-literate/branch/master)

**Package** - [Serilog.Sinks.Literate](http://nuget.org/packages/serilog.sinks.literate) | **Platforms** - .NET 4.5+

An alternative colored console sink for Serilog that uses a [literate programming](http://en.wikipedia.org/wiki/Literate_programming)-inspired presentation to showcase the structure/type of event data.

![Screenshot](https://raw.githubusercontent.com/serilog/serilog-sinks-literate/master/assets/Screenshot.png)

This is in contrast with the `ColoredConsole` sink that uses color predominantly to emphasise an event's level.

### Enabling the sink

To use the literate console sink, first install the NuGet package:

```powershell
Install-Package Serilog.Sinks.Literate
```

Then add the sink to your logger configuration:

```csharp
var log = new LoggerConfiguration()
    .WriteTo.LiterateConsole()
    .CreateLogger();
```

Or in XML [app-settings format](https://github.com/serilog/serilog/wiki/AppSettings), making sure the assembly is deployed alongside your app:

```xml
<appSettings>
  <add key="serilog:using" value="Serilog.Sinks.Literate" />
  <add key="serilog:write-to:LiterateConsole" value="" />
</appSettings>
```
