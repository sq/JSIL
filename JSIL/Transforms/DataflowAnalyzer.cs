using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSIL.Ast;
using JSIL.Internal;

namespace JSIL.Transforms {
    public class DataflowAnalyzer : JSAstVisitor {
        protected FunctionDataflow State;

        public FunctionDataflow Analyze (JSFunctionExpression function) {
            var result = new FunctionDataflow(function);

            Visit(function);

            return result;
        }

        public void VisitNode (JSFunctionExpression fn) {
            // Do not analyze nested function expressions
            if (Stack.OfType<JSFunctionExpression>().Skip(1).FirstOrDefault() != null)
                return;
        }
    }

    public class FunctionDataflow {
        public struct Access {
            public readonly int NodeIndex;
            public readonly JSVariable Source;
            public readonly bool IsControlFlow;

            public Access (int nodeIndex, JSVariable source, bool isControlFlow) {
                NodeIndex = nodeIndex;
                Source = source;
                IsControlFlow = isControlFlow;
            }
        }

        public struct Assignment {
            public readonly int NodeIndex;
            public readonly JSVariable Target;
            public readonly JSExpression Value;

            public Assignment (int nodeIndex, JSVariable target, JSExpression value) {
                NodeIndex = nodeIndex;
                Target = target;
                Value = value;
            }
        }

        public struct Conversion {
            public readonly int NodeIndex;
            public readonly JSVariable Source;
            public readonly JSType TargetType;

            public Conversion (int nodeIndex, JSVariable source, JSType targetType) {
                NodeIndex = nodeIndex;
                Source = source;
                TargetType = targetType;
            }
        }

        public struct Copy {
            public readonly int NodeIndex;
            public readonly JSVariable Target;
            public readonly JSVariable Source;

            public Copy (int nodeIndex, JSVariable target, JSVariable source) {
                NodeIndex = nodeIndex;
                Target = target;
                Source = source;
            }
        }

        public readonly JSFunctionExpression Function;
        public readonly List<Access> Accesses = new List<Access>();
        public readonly List<Assignment> Assignments = new List<Assignment>();
        public readonly List<Conversion> Conversions = new List<Conversion>();
        public readonly List<Copy> Copies = new List<Copy>();

        public FunctionDataflow (JSFunctionExpression function) {
            Function = function;
        }
    }
}
