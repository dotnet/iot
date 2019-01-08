# Device Binding Template

The Device Binding Template provides an easy way to create device binding projects.  This template is based on the [.NET Templates](https://github.com/dotnet/templating).

## How to Install
Currently, there is no NuGet package available so the template folder must be stored locally.  Use the following command to install the template where `PATH` is the directory path the template was stored.

```console
dotnet new -i C:\[PATH]\iot\tools\templates\DeviceBindingTemplate\dotnet_new_device-binding_csharp
```

## How to Uninstall
The template can be uninstalled by using the following command where `PATH` is the directory path the template was stored.

```console
dotnet new -u C:\[PATH]\iot\tools\templates\DeviceBindingTemplate\dotnet_new_device-binding_csharp
```

## Example Device Binding
Use the following command to create a device binding project.

  * `PATH` is the recommended directory path where other device bindings are located in the repo structure.
  * The output parameter (-o) is not required when working in the current directory expected to create the binding project.
  
```console
dotnet new device-binding -n Foo -o C:\[PATH]\iot\src\devices\Foo
```
The following device binding structure will be created after running the command.

```
iot/
  src/
    devices/
      Foo/
        Foo.csproj
        Foo.cs
        README.md
        samples/
          Foo.Sample.csproj
          Foo.Sample.cs
        tests/   <--  Tests are optional
          Foo_Not_Required.Tests.csproj
          Foo.Tests.cs
```

## Tests Project
There is an optional test project created when the device binding project is created.  The tests folder can be deleted if not used.