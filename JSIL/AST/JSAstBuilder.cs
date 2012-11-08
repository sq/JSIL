using System;
using JSIL.Ast;
using Mono.Cecil;
using JSIL.Internal;

namespace JSIL.Ast {
    public class JSAstBuilder {
        private JSExpression _expression;
        
        private JSAstBuilder(JSExpression expression)
        {
            _expression = expression;
        }

        public JSExpression GetExpression()
        {
            return _expression;
        }

        public JSAstBuilder FakeMethod(string name, TypeReference returnType, TypeReference[] parameterTypes, MethodTypeFactory methodTypeFactory)
        {
            return Dot(new JSFakeMethod(name, returnType, parameterTypes, methodTypeFactory));
        }

        public static JSAstBuilder StringIdentifier(string name, TypeReference type = null)
        {
            return new JSAstBuilder(new JSStringIdentifier(name, type));
        }

        public JSAstBuilder Dot(JSIdentifier identifier)
        {
            return new JSAstBuilder(new JSDotExpression(_expression, identifier));
        }

        public JSAstBuilder Dot(string identifier, TypeReference type = null)
        {
            return Dot(new JSStringIdentifier(identifier, type));
        }
    }
}
