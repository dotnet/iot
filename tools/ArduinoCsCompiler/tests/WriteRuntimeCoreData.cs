﻿using System;
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
            WriteEnumHeaderFile<NativeMethod>();
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