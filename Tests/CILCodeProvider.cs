using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace JSIL.Tests
{
    public class CILCodeProvider : CodeDomProvider
    {
        public override ICodeGenerator CreateGenerator()
        {
            return null;
        }

        public override ICodeCompiler CreateCompiler()
        {
            return new CILCodeCompiler();
        }
    }

    public class CILCodeCompiler : ICodeCompiler
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

            string arguments =
                (options.GenerateExecutable ? "/exe " : "/dll ") +
                "/output:\"" + options.OutputAssembly + "\" "
                + string.Join(" ", fileNames);

            var ilasmProcess = new Process
                {
                    StartInfo =
                        {
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            FileName = "../Upstream/ILAsm/ilasm.exe",
                            Arguments = arguments,
                            WindowStyle = ProcessWindowStyle.Hidden
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

                results.Errors.Add(new CompilerError { ErrorText = "CIL compilation failed" });
                return results;
            }

            byte[] rawAssembly = File.ReadAllBytes(options.OutputAssembly);
            results.CompiledAssembly = Assembly.Load(rawAssembly);
            return results;
        }

        public CompilerResults CompileAssemblyFromSourceBatch(CompilerParameters options, string[] sources)
        {
            throw new NotImplementedException();
        }
    }
}