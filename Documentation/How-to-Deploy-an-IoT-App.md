# How to Deploy an IoT App

This provides information how to prepare a Publish Profile and deploy an application to a development board.

### Using Visual Studio

1. Once you have an application setup in Visual Studio, right-click the project in the Solution Explorer and select Publish...

2. In the Pick a publish target dialog, select Folder, choose a folder to publish your application files and click Create Profile.  The example below shows a path of C:\PublishedApps.

3. A default profile, FolderProfile.pubxml, will now be created and added to your project.  You can view it in the Solution Explorer under your project > Properties > PublishProfiles folder.

4. It is a good practice to rename your profile to something you can relate with your project.  In the Publish Window, click the Actions dropdown and select Rename Profile.  A Rename Profile dialog prompts you to rename your profile.  Click Save after renaming.

5. You can configure the profile's settings by clicking Configure... in the Publish Window.  A Profile Settings dialog prompt includes a few options.  
  
    **Notes**  
    * Prior to Visual Studio 2019, the Target Runtime doesn't offer a selection for Linux ARM or Windows ARM.  When using an older version of Visual Studio, you will need to manually open the profile's XML and change the RuntimeIdentifier element to **linux-arm** or **win-arm** shown below:

        ```
        <RuntimeIdentifier>linux-arm</RuntimeIdentifier>
        or..
        <RuntimeIdentifier>win-arm</RuntimeIdentifier>
        or both..
        <RuntimeIdentifiers>linux-arm;win-arm</RuntimeIdentifiers>
        ```  
  
    * Deployment Mode Options:
        * **Framework Dependent** - App relies on the presence of a shared system-wide version of .NET Core on the target system.  This will create a smaller size package.
        * **Self-contained** - .NET Core and required libraries will be bundled with your application creating a larger size package.  IoT devices are usually constrained by memory resources.  There is a useful NuGet package available that helps trim unused files.  Reference [Microsoft.Packaging.Tools.Trimming](https://www.nuget.org/packages/Microsoft.Packaging.Tools.Trimming/) for more information.

  
     * While there are more options available that can be modified, this only shows the basics.  It is recommended to read more in the provided **References** section below.

6. You can view the contents by double-clicking the profile.  The contents should look similar to the XML below after your modifications.

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="4.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <PublishProtocol>FileSystem</PublishProtocol>
    <Configuration>Release</Configuration>
    <Platform>Any CPU</Platform>
    <TargetFramework>netcoreapp2.1</TargetFramework>
    <PublishDir>C:\PublishedApps</PublishDir>
    <RuntimeIdentifier>linux-arm</RuntimeIdentifier>
    <SelfContained>true</SelfContained>
    <_IsPortable>false</_IsPortable>
  </PropertyGroup>
</Project>
```

7. You can now publish your application by clicking Publish in the Publish Window.  All folders/files should now be packaged in the specified publish folder.

8. Your application is now ready for deployment to the target device.

### Using .NET Core CLI

1. Once you have an application setup, navigate to the directory where the project is located and run the `dotnet publish` command.  Below shows a few common options and example.

    * -c defines the build configuration (Debug or Release).
    * -o specifies the output path where application will be packaged.
    * -r publishes the application for given runtime (e.g. linux-arm, win-arm, etc.) being targeted.

      ```
      dotnet publish -c Release -o C:\DeviceApiTester -r linux-arm
      ```

2. Your application is now ready for deployment to the target device.

## How to Copy an IoT App to Target Device

The application can be deployed to a target device once the Publish Profile and related files have been packaged.  There are various tools for transferring files depending on the platform being used.  Below are a few popular tools available.

* [PuTTy](https://www.putty.org/)
* PSCP (provided with PuTTy install) 
* [FileZilla](https://filezilla-project.org/)

## References
* [Have Your Pi and Eat It Too: .NET Core 2.1 on Raspberry Pi](https://channel9.msdn.com/Events/dotnetConf/2018/S314)
* [Visual Studio publish profiles for ASP.NET Core app deployment](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/visual-studio-publish-profiles?view=aspnetcore-2.2)
* [Deploy an app to a local folder using Visual Studio](https://docs.microsoft.com/en-us/visualstudio/deployment/quickstart-deploy-to-local-folder?view=vs-2017)
* [Deploy .NET Core apps with Visual Studio](https://docs.microsoft.com/en-us/dotnet/core/deploying/deploy-with-vs?tabs=vs156)
* [How to: Edit Deployment Settings in Publish Profile (.pubxml) Files and the .wpp.targets File in Visual Studio Web Projects](https://go.microsoft.com/fwlink/?LinkID=208121)  
* [.NET Core application deployment](https://docs.microsoft.com/en-us/dotnet/core/deploying/)
* [dotnet publish](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-publish?tabs=netcore21)
* [.NET Core RID Catalog](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog)