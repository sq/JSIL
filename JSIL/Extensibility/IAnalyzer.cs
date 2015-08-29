using System.Collections.Generic;
using JSIL.Ast;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL.Compiler.Extensibility {
    public interface IAnalyzer {
        void SetConfiguration (IDictionary<string, object> analyzerSettings);

        void Analyze (AssemblyTranslator translator, AssemblyDefinition[] assemblies, TypeInfoProvider typeInfoProvider);
        bool ShouldSkipMember (AssemblyTranslator translator, MemberReference member);

        IEnumerable<IFunctionTransformer> FunctionTransformers { get; }
        string SettingsKey { get; }
    }

    // Analyzers can implement this interface to apply custom transformations to function
    public interface IFunctionTransformer {
        // This happens before the transform pipeline, during initial IL -> JSNode translation
        JSExpression MaybeReplaceMethodCall (
            MethodReference caller,
            MethodReference method, MethodInfo methodInfo, 
            JSExpression thisExpression, JSExpression[] arguments, 
            TypeReference resultType, bool explicitThis
        );

        // Add any custom transforms here
        void InitializeTransformPipeline (AssemblyTranslator translator, FunctionTransformPipeline transformPipeline);
    }
}