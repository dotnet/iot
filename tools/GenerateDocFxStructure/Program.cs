// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using CommandLine;
using GenerateDocFxStructure.Helpers;

namespace GenerateDocFxStructure
{
    internal class Program
    {
        private static readonly string[] _mediaExtentions = new string[] { ".png", ".jpg", ".mpg", ".mpeg", ".mov", ".mp4", ".bmp", ".gif", ".svg" };
        private static CommandlineOptions? _options;
        private static int _returnvalue;
        private static MessageHelper? _message;
        private static Regex _rxContent = new Regex(@"(\[{1}.*\]{1}\({1}.*\){1}?)", RegexOptions.Compiled);

        private static List<string> allFiles = new List<string>();

        private static int Main(string[] args)
        {
            Console.WriteLine("DocFX markdown structure preparation");
            try
            {
                Parser.Default.ParseArguments<CommandlineOptions>(args)
                                   .WithParsed<CommandlineOptions>(RunLogic)
                                   .WithNotParsed(HandleErrors);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Parsing arguments threw an exception with message `{ex.Message}`");
                _returnvalue = 1;
            }

            Console.WriteLine($"Exit with return code {_returnvalue}");

            return _returnvalue;
        }

        private static void RunLogic(CommandlineOptions o)
        {
            _message = new MessageHelper(o);
            if (string.IsNullOrEmpty(o.DestinationFolder))
            {
                _message.Error($"Destination folder must be specified.");
                _returnvalue = 1;
                return;
            }

            if (string.IsNullOrEmpty(o.SourceFolder))
            {
                _message.Error($"Source folder must be specified.");
                _returnvalue = 1;
                return;
            }

            if (string.IsNullOrEmpty(o.MediaFolder))
            {
                _message.Error($"Media folder must be specified.");
                _returnvalue = 1;
                return;
            }

            o.DestinationFolder = Path.GetFullPath(o.DestinationFolder);
            o.MediaFolder = Path.GetFullPath(o.MediaFolder);
            o.SourceFolder = Path.GetFullPath(o.SourceFolder);
            _options = o;

            _message.Verbose($"Source folder     : {_options.SourceFolder}");
            _message.Verbose($"Destination folder: {_options.DestinationFolder}");
            _message.Verbose($"Media folder      : {_options.MediaFolder}");
            _message.Verbose($"Verbose           : {_options.Verbose}");

            if (!Directory.Exists(_options.SourceFolder))
            {
                _message.Error($"ERROR: Documentation folder '{_options.SourceFolder}' doesn't exist.");
                _returnvalue = 1;
                return;
            }

            // We will start to get all the files
            foreach (string file in Directory.EnumerateFiles(_options.SourceFolder, "*.*", SearchOption.AllDirectories))
            {
                allFiles.Add(file);
            }

            // we start at the root to generate the items
            DirectoryInfo rootDir = new DirectoryInfo(_options.SourceFolder);
            WalkDirectoryTree(rootDir);
        }

        /// <summary>
        /// Main function going through all the folders, files and subfolders.
        /// </summary>
        /// <param name="folder">The folder to search.</param>
        private static void WalkDirectoryTree(DirectoryInfo folder)
        {
            _message!.Verbose($"Processing folder {folder.FullName}");

            // process MD files in this folder
            ProcessFiles(folder);

            // process other sub folders
            DirectoryInfo[] subDirs = folder.GetDirectories();
            foreach (DirectoryInfo dirInfo in subDirs)
            {
                WalkDirectoryTree(dirInfo);
            }
        }

        /// <summary>
        /// Get the list of the files in the current directory.
        /// </summary>
        /// <param name="folder">The folder to search.</param>
        private static void ProcessFiles(DirectoryInfo folder)
        {
            _message!.Verbose($"Process {folder.FullName} for files.");

            List<FileInfo> files = folder.GetFiles("*.md").OrderBy(f => f.Name).ToList();
            if (files.Count == 0)
            {
                _message.Verbose($"  No MD files found in {folder.FullName}.");
                return;
            }

            foreach (FileInfo fi in files)
            {
                _message.Verbose($"Processing {fi.FullName}");
                string content = File.ReadAllText(fi.FullName);

                // first see if there are links in this file
                if (_rxContent.Matches(content).Any())
                {
                    _message.Verbose($"  Links detected.");

                    // it has references, so check in detail
                    ProcessFile(folder, fi.FullName);
                }
                else
                {
                    // Create the destination file
                    var relativeDestinationFilePath = fi.FullName.Substring(_options!.SourceFolder.Length + 1);
                    var newFileDestination = Path.Combine(_options.DestinationFolder, relativeDestinationFilePath);
                    // Ensure the path exists and is created
                    var dir = Path.GetDirectoryName(newFileDestination);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir!);
                    }

                    File.Copy(fi.FullName, newFileDestination);
                }

                _message.Verbose($"{fi.FullName} processed.");
            }
        }

        /// <summary>
        /// Process given file in give folder. Check all references.
        /// </summary>
        /// <param name="folder">Folder where file live.</param>
        /// <param name="filepath">Complete path of the file to check.</param>
        private static void ProcessFile(DirectoryInfo folder, string filepath)
        {
            if ((_options == null) || (_message == null))
            {
                throw new InvalidOperationException($"Options or message can't be null, something went wrong");
            }

            string[] lines = File.ReadAllLines(filepath);
            // Create the destination file
            var relativeDestinationFilePath = filepath.Substring(_options.SourceFolder.Length + 1);
            var newFileDestination = Path.Combine(_options.DestinationFolder, relativeDestinationFilePath);
            // Ensure the path exists and is created
            var dir = Path.GetDirectoryName(newFileDestination);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir!);
            }

            StreamWriter destinationFile = File.CreateText(newFileDestination);

            // get matches per line to be able to reference a line in a file.
            int linenr = 1;
            foreach (string line in lines)
            {
                string lineToWrite = line;
                MatchCollection matches = _rxContent.Matches(line);
                if (matches.Any())
                {
                    // process all matches
                    foreach (Match match in matches)
                    {
                        // get just the reference
                        int start = match.Value.IndexOf("](") + 2;
                        string relative = match.Value.Substring(start);
                        int end = relative.IndexOf(")");
                        relative = relative.Substring(0, end);
                        string initialReletive = relative;

                        // relative string contain not only URL, but also "title", get rid of it
                        int positionOfLinkTitle = relative.IndexOf('\"');
                        if (positionOfLinkTitle > 0)
                        {
                            relative = relative.Substring(0, relative.IndexOf('\"')).Trim();
                        }

                        // strip in-doc references using a #
                        if (relative.Contains("#"))
                        {
                            relative = relative.Substring(0, relative.IndexOf("#"));
                        }

                        // decode possible HTML encoding
                        relative = HttpUtility.UrlDecode(relative);

                        // check link if not to a URL, in-doc link or e-mail address
                        if (!relative.StartsWith("http:") &&
                            !relative.StartsWith("https:") &&
                            !relative.StartsWith("#") &&
                            !relative.Contains("@") &&
                            !string.IsNullOrEmpty(Path.GetExtension(relative)) &&
                            !string.IsNullOrWhiteSpace(relative))
                        {
                            // check validity of the link
                            string absolute = Path.GetFullPath(relative, folder.FullName);

                            // check that paths are relative
                            if (Path.IsPathFullyQualified(relative))
                            {
                                // link is full path - not allowed
                                _message!.Output($"{filepath} {linenr}:{match.Index}");
                                _message.Error($"Full path '{relative}' used. Use relative path.");
                                _returnvalue = 1;
                            }

                            // don't need to check if reference is to a directory
                            if (!Directory.Exists(absolute))
                            {
                                // check if we have file in allFiles list or if it exists on disc
                                if (!allFiles.Contains(absolute.ToLowerInvariant()) && !File.Exists(absolute))
                                {
                                    // ERROR: link to non existing file
                                    _message!.Output($"{filepath} {linenr}:{match.Index}");
                                    _message.Error($"Not found: {relative}");

                                    // mark error in returnvalue of the tool
                                    _returnvalue = 1;
                                }
                                else
                                {
                                    // This is where we'll have to adjust the file reference
                                    bool needAbsoluteLink = true;
                                    string newRelative = string.Empty;
                                    var extention = Path.GetExtension(absolute).ToLowerInvariant();
                                    if (_mediaExtentions.Contains(extention))
                                    {
                                        // Case 1: it's a link on a media
                                        // Copy the file to the right destination
                                        var relativeMediaPath = absolute.Substring(_options.SourceFolder.Length + 1);
                                        var nameOfMediaFolder = Path.GetFileName(_options.MediaFolder);
                                        var newMediaDestination = Path.Combine(_options.MediaFolder, relativeMediaPath);
                                        var dirMedia = Path.GetDirectoryName(newMediaDestination);
                                        if (!Directory.Exists(dirMedia))
                                        {
                                            Directory.CreateDirectory(dirMedia!);
                                        }

                                        if (!File.Exists(newMediaDestination))
                                        {
                                            File.Copy(absolute, newMediaDestination);
                                        }

                                        // Adjust the link: ~/ + nameOfMediaFolder + / + relativeMediaPath
                                        // For web links, it has to be a /
                                        newRelative = $"~/{nameOfMediaFolder}/{relativeMediaPath.Replace('\\', '/')}";
                                        needAbsoluteLink = false;
                                    }
                                    else if (extention == ".md")
                                    {
                                        // Case 2: it's a Markdown file in what's been moved
                                        // we will do a relative link on it
                                        // if not an absolute link on the repo
                                        if ((_options.SourceFolder.Length + 1) >= absolute.Length)
                                        {
                                            needAbsoluteLink = true;
                                        }
                                        else
                                        {
                                            var relativeIncludesPath = absolute.Substring(_options.SourceFolder.Length + 1);
                                            var nameOfIncludesFolder = Path.GetFileName(_options.DestinationFolder);
                                            newRelative = $"~/{nameOfIncludesFolder}/{relativeIncludesPath.Replace('\\', '/')}";
                                            needAbsoluteLink = false;
                                        }
                                    }

                                    if (needAbsoluteLink)
                                    {
                                        // Case 2: it's a link on a directory in the repo or a file
                                        // Adjust the link
                                        var relativeLinkPath = absolute.Substring(_options.SourceFolder.Length + 1).Replace('\\', '/');
                                        newRelative = $"{_options.Repo}/{relativeLinkPath}";
                                    }

                                    _message.Verbose($"  Replacing {initialReletive} by {newRelative}");
                                    lineToWrite = line.Replace(initialReletive, newRelative);
                                }
                            }
                        }
                    }
                }

                destinationFile.WriteLine(lineToWrite);
                linenr++;
            }

            destinationFile.Flush();
            destinationFile.Close();
        }

        /// <summary>
        /// On parameter errors, we set the returnvalue to 1 to indicated an error.
        /// </summary>
        /// <param name="errors">List or errors (ignored).</param>
        private static void HandleErrors(IEnumerable<Error> errors)
        {
            _returnvalue = 1;
        }
    }
}
