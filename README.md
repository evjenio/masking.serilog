# Masking.Serilog 🎭
Masking sensitive information during logging to Serilog by hiding individual properties.

![.NET](https://github.com/evjenio/masking.serilog/workflows/.NET/badge.svg) [![NuGet version](https://badge.fury.io/nu/Masking.Serilog.svg)](https://badge.fury.io/nu/Masking.Serilog)

Install from NuGet:

```powershell
Install-Package Masking.Serilog
```

Mark properties to mask:

```csharp
Log.Logger = new LoggerConfiguration()
    .Destructure.ByMaskingProperties("Password", "Token")
    .CreateLogger()
```

or

```csharp
Log.Logger = new LoggerConfiguration()
    .Destructure.ByMaskingProperties(opts =>
    {
        opts.PropertyNames.Add("Password");
        opts.PropertyNames.Add("Token");
        opts.Mask = "******";
    })
    .CreateLogger()
```

When types are destructured, listed properties will be covered up with mask:

```csharp
Log.Information("Logged on {@User}", new User { Username = "sudo", Password = "SuperAdmin" });

// Prints `Logged on User { Username: "sudo", Password: "******" }`
```

You can ignore masking for given namespaces by including them within the Masking Options configuration, as shown in the example below. 
This is especially helpful when dealing with complex objects which often results in performance issues.

```csharp
Log.Logger = new LoggerConfiguration()
    .Destructure.ByMaskingProperties(opts =>
    {
        opts.PropertyNames.Add("Password");
        opts.PropertyNames.Add("Token");
        opts.Mask = "******";
        opts.IgnoredNamespaces.Add("System.Net.Http");
    })
    .CreateLogger()
```

Please note that this is an explicit whitelist implementation, this helps to avoid mistakes resulting in exposure of sensitive data.


