using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ArduinoCsCompiler;
using Iot.Device.Arduino;
using Xunit;
using SystemException = ArduinoCsCompiler.SystemException;

namespace Iot.Device.Arduino.Tests
{
    /// <summary>
    /// This class is used to copy some information to the Arduino code, so that it stays in sync with the C# part
    /// </summary>
    public class WriteRuntimeCoreData
    {
        /// <summary>
        /// Returns the path where the runtime sources are
        /// </summary>
        /// <returns></returns>
        private string GetRuntimePath()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.Combine(path, @"Arduino\ExtendedConfigurableFirmata");
        }

        [Fact]
        public void WriteNativeMethodDefinitions()
        {
            // WriteEnumHeaderFile<NativeMethod>();
            // Collect all methods that have an ArduinoImplementation attribute attached to them.
            // Must scan all functions, including internals.
            Assembly[] typesWhereToLook = new Assembly[]
            {
                Assembly.GetAssembly(typeof(MicroCompiler))!,
                Assembly.GetAssembly(typeof(ArduinoBoard))!
            };

            string[] specials = new string[]
            {
                "ByReferenceCtor",
                "ByReferenceValue",
            };

            Dictionary<string, int> entries = new();

            foreach (var s in specials)
            {
                entries.Add(s, s.GetHashCode());
            }

            // The loop below will throw on duplicate entries. This is expected.
            foreach (var a in typesWhereToLook)
            {
                foreach (var type in a.GetTypes())
                {
                    foreach (var method in type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                    {
                        var attribs = method.GetCustomAttributes(typeof(ArduinoImplementationAttribute)).Cast<ArduinoImplementationAttribute>();
                        var attrib = attribs.FirstOrDefault();
                        if (attrib != null && attrib.MethodNumber != 0)
                        {
                            TryAddEntry(entries, attrib);
                        }
                    }

                    foreach (var method in type.GetConstructors(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                    {
                        var attribs = method.GetCustomAttributes(typeof(ArduinoImplementationAttribute)).Cast<ArduinoImplementationAttribute>();
                        var attrib = attribs.FirstOrDefault();
                        if (attrib != null && attrib.MethodNumber != 0)
                        {
                            TryAddEntry(entries, attrib);
                        }
                    }
                }
            }

            var hashCodes = entries.Select(x => x.Value).ToList();
            if (hashCodes.Distinct().Count() != hashCodes.Count())
            {
                // Let's hope it doesn't happen, otherwise we would have to extend this code to give a better error message.
                throw new InvalidOperationException("Duplicate method key found");
            }

            var list = entries.OrderBy(x => x.Key).Select(y => (y.Key, y.Value));
            WriteNativeMethodList(list);
        }

        private void TryAddEntry(Dictionary<string, int> entries, ArduinoImplementationAttribute attrib)
        {
            if (!entries.ContainsKey(attrib.Name))
            {
                entries.Add(attrib.Name, attrib.MethodNumber);
            }
            else if (entries[attrib.Name] == attrib.MethodNumber)
            {
                // Nothing to do
            }
            else
            {
                throw new InvalidOperationException($"Method {attrib.Name} was already declared with a different hash code");
            }
        }

        private void WriteNativeMethodList(IEnumerable<(string Key, int Value)> entries)
        {
            string name = "NativeMethod";
            string header = FormattableString.Invariant($@"
#pragma once

enum class {name}
{{
    None = 0,
");
            string outputFile = Path.Combine(GetRuntimePath(), name + ".h");
            TextWriter w = new StreamWriter(outputFile, false, Encoding.ASCII);
            w.Write(header);
            foreach (var e in entries)
            {
                w.WriteLine(FormattableString.Invariant($"    {e.Key} = {e.Value},"));
            }

            w.WriteLine("};"); // Tail
            w.Close();
        }

        [Fact]
        public void WriteSystemExceptions()
        {
            WriteEnumHeaderFile<SystemException>();
        }

        [Fact]
        public void WriteMethodFlags()
        {
            WriteEnumHeaderFile<MethodFlags>();
        }

        [Fact]
        public void WriteKnownTypeTokens()
        {
            WriteEnumHeaderFile<KnownTypeTokens>();
        }

        [Fact]
        public void WriteExceptionClauseTypes()
        {
            WriteEnumHeaderFile<ExceptionHandlingClauseOptions>();
        }

        [Fact]
        public void WriteExecutorCommands()
        {
            string name = nameof(ExecutorCommand);
            string header = FormattableString.Invariant($@"
#pragma once

enum class {name}
{{
");
            string outputFile = Path.Combine(GetRuntimePath(), name + ".h");
            TextWriter w = new StreamWriter(outputFile, false, Encoding.ASCII);
            w.Write(header);
            foreach (var e in Enum.GetValues(typeof(ExecutorCommand)))
            {
                w.WriteLine(FormattableString.Invariant($"    {e.ToString()} = {(byte)e},"));
            }

            w.WriteLine("};"); // Tail
            w.Close();
        }

        [Fact]
        public void WriteVariableKind()
        {
            string name = nameof(VariableKind);
            string header = FormattableString.Invariant($@"
#pragma once

enum class {name}
{{
");
            string outputFile = Path.Combine(GetRuntimePath(), name + ".h");
            TextWriter w = new StreamWriter(outputFile, false, Encoding.ASCII);
            w.Write(header);
            foreach (var e in Enum.GetValues(typeof(VariableKind)))
            {
                w.WriteLine(FormattableString.Invariant($"    {e.ToString()} = {(byte)e},"));
            }

            w.WriteLine("};"); // Tail
            w.Close();
        }

        private void WriteEnumHeaderFile<T>()
            where T : Enum
        {
            string name = typeof(T).Name;
            string header = FormattableString.Invariant($@"
#pragma once

enum class {name}
{{
");
            string outputFile = Path.Combine(GetRuntimePath(), name + ".h");
            // Must use ascii encoding, because GCC fails to recognize the UTF-8-BOM header
            // sometimes. Not sure why it works sometimes only.
            TextWriter w = new StreamWriter(outputFile, false, Encoding.ASCII);
            w.Write(header);
            foreach (var e in Enum.GetValues(typeof(T)))
            {
                w.WriteLine(FormattableString.Invariant($"    {e.ToString()} = {(int)e},"));
            }

            w.WriteLine("};"); // Tail
            w.Close();
        }
    }
}
