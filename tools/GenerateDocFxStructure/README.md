# DocFx file structure generator

In order to reuse the README for each binding and reference them from the source comments (triple slash), the files need to be moved into a specific directory.

This directory cannot include the images and other medias, they need to be moved in another folder.

The links to those media files need to be adjusted as well.

On top, the links on the internal structure need to be adjusted as well from relative to absolute.

## Usage

The tool can be used like this:

`GenerateDocFxStructure -d includesDirectory -m mediaDirectory -s sourceDirectory -r repoUrl`

- `includesDirectory`: the includes destination directory. ex: `c:\iot-page-docfx\includes`
- `mediaDirectory`: the media destination directory. ex: `c:\iot-page-docfx\media`
- `sourceDirectory`: the source directory. ex: `c:\iot\src\devices`. It is important to point on the devices directory where all the markdown files will be extracted and analyzed.
- `repoUrl`: the repository UR, by default: `https://github.com/dotnet/iot/tree/main/src/devices`

**Important**:

- The links on media will be transformed from `something.jpg` to `~/media/Binding/something.jpg` assuming the previous example with the media directory. Binding is the name of the binding, more precisely the name of the directory containing the Markdown file.
- The links on the repository will be transformed into absolute URL. For example `./sample/program.cs` into `https://github.com/dotnet/iot/tree/main/src/devices/Binding/sample/program.cs` assuming the previous repository and as for the media, Binding is the name of the directory containing the Markdown
- If the Markdown is in a series of directories, they will all be replaced the proper way.

The tool can be used with `-v` option to show verbose.

In case of a broken relative link, the link and the line number in the file will be displayed. This doesn't stop the execution.

In case of any error, the program will return with the code 1.
