# How to Prepare a Publishing Profile

In order to deploy applications to a development board (e.g. Raspberry Pi, HummingBoard, etc.), you need to setup a Publishing Profile.  Below provides a few steps to quickly get started wihin your projects.

1. Once you have an application setup in Visual Studio, right-click the project in the Solution Explorer and select Publish...

2. In the Pick a publish target dialog, select Folder, choose a folder to publish your application files and click Create Profile.  The example below shows a path of C:\PublishedApps.

3. A default profile, FolderProfile.pubxml, will now be created and added to your project.  You can view it in the Solution Explorer under your project > Properties > PublishProfiles folder.

4. It is a good practice to rename your profile to something you can relate with your project.  In the Publish Window, click the Actions dropdown and select Rename Profile.  A Rename Profile dialog prompts you to rename your profile.  Click Save after renaming.

5. You can configure the profile's settings by clicking Configure... in the Publish Window.  A Profile Settings dialog prompt includes a few options.  
  
    **Notes**  
    * Currently, the Target Runtime doesn't offer a selection for Linux ARM.  You will need to manually open the profile's XML and change the RuntimeIdentifier element to **linux-arm** shown below:

        ```
        <RuntimeIdentifier>linux-arm</RuntimeIdentifier>
        ```  
  
    * Deployment Mode Options:
        * **Framework Dependent** - App relies on the presence of a shared system-wide version of .NET Core on the target system.  This will create a smaller size package.
        * **Self-contained** - .NET Core and required libraries will be bundled with your application creating a larger size package.
  
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

## References
* [Have Your Pi and Eat It Too: .NET Core 2.1 on Raspberry Pi](https://channel9.msdn.com/Events/dotnetConf/2018/S314)
* [Visual Studio publish profiles for ASP.NET Core app deployment](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/visual-studio-publish-profiles?view=aspnetcore-2.2)
* [Deploy an app to a local folder using Visual Studio](https://docs.microsoft.com/en-us/visualstudio/deployment/quickstart-deploy-to-local-folder?view=vs-2017)
* [Deploy .NET Core apps with Visual Studio](https://docs.microsoft.com/en-us/dotnet/core/deploying/deploy-with-vs?tabs=vs156)
* [How to: Edit Deployment Settings in Publish Profile (.pubxml) Files and the .wpp.targets File in Visual Studio Web Projects](https://go.microsoft.com/fwlink/?LinkID=208121)
* [.NET Core application deployment](https://docs.microsoft.com/en-us/dotnet/core/deploying/)



  




