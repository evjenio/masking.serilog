# Masking.Serilog 🎭
Masking sensitive information during logging to Serilog by hiding individual properties.

[![Build status](https://ci.appveyor.com/api/projects/status/a68pglg77ixl8qoq?svg=true)](https://ci.appveyor.com/project/evjenio/masking-serilog)

Install from NuGet:

```powershell
Install-Package Masking.Serilog
```

Mark properties to ignore:

```csharp
Log.Logger = new LoggerConfiguration()
    .Destructure.ByMaskingProperties("Email", "Password")
    // Other logger configurationg
    .CreateLogger()
```

or

```csharp
Log.Logger = new LoggerConfiguration()
    .Destructure.ByMaskingProperties(opts =>
    {
        opts.PropertyNames.Add("Hash");
        opts.PropertyNames.Add("Token");
        opts.Mask = "*removed*";
    })
    // Other logger configurationg
    .CreateLogger()
```

When types are destructured, named properties will be covered up with mask:

```csharp
Log.Information("Logged on {@User}", new User { Username = "sudo", Password = "SuperAdmin" });

// Prints `Logged on User { Username: "sudo", Password: "******" }`
```

