using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace JSIL.Tests
{
    public class CommonCodeProvider : CodeDomProvider
    {
        private Func<ICodeCompiler> _compilerCreator;

        public CommonCodeProvider(Func<ICodeCompiler> compilerCreator)
        {
            _compilerCreator = compilerCreator;
        }

        public override ICodeGenerator CreateGenerator()
        {
            return null;
        }

        public override ICodeCompiler CreateCompiler()
        {
            return _compilerCreator();
        }
    }

    public abstract class CommandLineCodeCompiler : ICodeCompiler
    {
        public CompilerResults CompileAssemblyFromDom(CompilerParameters options, CodeCompileUnit compilationUnit)
        {
            throw new System.NotImplementedException();
        }

        public CompilerResults CompileAssemblyFromFile(CompilerParameters options, string fileName)
        {
            throw new System.NotImplementedException();
        }

        public CompilerResults CompileAssemblyFromSource(CompilerParameters options, string source)
        {
            throw new System.NotImplementedException();
        }

        public CompilerResults CompileAssemblyFromDomBatch(CompilerParameters options,
                                                           CodeCompileUnit[] compilationUnits)
        {
            throw new System.NotImplementedException();
        }

        public CompilerResults CompileAssemblyFromFileBatch(CompilerParameters options, string[] fileNames)
        {
            if (options == null)
                throw new ArgumentNullException("options");
            if (fileNames == null)
                throw new ArgumentNullException("fileNames");

            var results = new CompilerResults(options.TempFiles);

            bool outputFileWasCreated = false;
            if (string.IsNullOrEmpty(options.OutputAssembly))
            {
                string fileExtension = options.GenerateExecutable ? "exe" : "dll";
                options.OutputAssembly = results.TempFiles.AddExtension(fileExtension, !options.GenerateInMemory);
                new FileStream(options.OutputAssembly, FileMode.Create, FileAccess.ReadWrite).Close();
                outputFileWasCreated = true;
            }

            var ilasmProcess = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    FileName = GetCompilerPath(),
                    Arguments = GetArguments(fileNames, options.OutputAssembly, options.GenerateExecutable),
                    CreateNoWindow = true
                }
            };
            ilasmProcess.Start();
            var output = ilasmProcess.StandardOutput.ReadToEnd();
            ilasmProcess.WaitForExit();
            results.Output.Add(output);

            if (ilasmProcess.ExitCode != 0)
            {
                if (outputFileWasCreated)
                {
                    File.Delete(options.OutputAssembly);
                }

                results.Errors.Add(new CompilerError { ErrorText = "Compilation failed: \r\n" + output });
                return results;
            }

            results.CompiledAssembly = Assembly.LoadFrom(options.OutputAssembly);
            return results;
        }

        public CompilerResults CompileAssemblyFromSourceBatch(CompilerParameters options, string[] sources)
        {
            throw new NotImplementedException();
        }

        protected abstract string GetCompilerPath();

        protected abstract string GetArguments(string[] filenemas, string outpuAssemblyName, bool isExecutable);
    }

    public class CILCodeProvider : CommonCodeProvider
    {
        public CILCodeProvider()
            : base(() => new CILCodeCompiler())
        {
        }
    }

    public class CILCodeCompiler : CommandLineCodeCompiler
    {
        protected override string GetCompilerPath()
        {
            return "../Upstream/ILAsm/ilasm.exe";
        }

        protected override string GetArguments(string[] filenemas, string outpuAssemblyName, bool isExecutable)
        {
            string arguments =
                (isExecutable ? "/exe " : "/dll ") +
                "/output:\"" + outpuAssemblyName + "\" "
                + string.Join(" ", filenemas);
            return arguments;
        }
    }

    public class CPPCodeProvider : CommonCodeProvider
    {
        public CPPCodeProvider()
            : base(() => new CPPCodeCompiler())
        {
        }
    }

    public class CPPCodeCompiler : CommandLineCodeCompiler
    {
        protected override string GetCompilerPath()
        {
            for (int i = 14; i >= 10; i--)
            {
                var vsPath = Environment.GetEnvironmentVariable(string.Format("VS{0}0COMNTOOLS", i));
                if (string.IsNullOrEmpty(vsPath))
                {
                    continue;
                }

                var clPath = Path.Combine(vsPath, @"..\..\VC\bin\cl.exe");
                if (!File.Exists(clPath))
                {
                    continue;
                }
                return clPath;
            }

            throw new Exception("C++/CLI compiler not found");
        }

        protected override string GetArguments(string[] filenemas, string outpuAssemblyName, bool isExecutable)
        {
            string arguments =
                string.Format(
                "/clr:pure /Zl{1} {0} /link /NODEFAULTLIB{2} /OUT:{3}",
                string.Join(" ", filenemas),
                !isExecutable ? " /LD" : string.Empty,
                isExecutable ? " /ENTRY:\"Program::Main\" /SUBSYSTEM:CONSOLE" : string.Empty,
                "\"" + outpuAssemblyName + "\"");

            return arguments;
        }
    }
}