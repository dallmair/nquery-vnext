﻿using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Syntax;

using Xunit;

namespace NQuery.UnitTests.Binding
{
    public sealed partial class IntrinsicOperatorTests
    {
        [Fact]
        public void IntrinsicOperator_UnarySignaturesAreCorrect()
        {
            var issues = new List<string>();

            foreach (var testCase in UnaryTestCases)
            {
                var opText = testCase.Op;
                var argument = GetValue(testCase.Argument);
                var source = string.Format("{0} {1}", opText, argument);
                var syntaxTree = SyntaxTree.ParseExpression(source);
                var syntaxTreeSource = syntaxTree.Root.ToString();
                if (syntaxTreeSource != source)
                    Assert.True(false, string.Format("Source should have been {0} but is {1}", syntaxTreeSource, source));

                var expression = (UnaryExpressionSyntax)syntaxTree.Root.Root;
                var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
                var semanticModel = compilation.GetSemanticModel();

                var argumentType = GetExpressionTypeString(semanticModel.GetExpressionType(expression.Expression));
                if (testCase.Argument != argumentType)
                    Assert.True(false, string.Format("Left should be of type '{0}' but has type '{1}", testCase.Argument, argumentType));

                var diagnostic = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).SingleOrDefault();
                var expressionType = semanticModel.GetExpressionType(expression);
                var result = diagnostic == null
                                 ? GetExpressionTypeString(expressionType)
                                 : GetErrorString(diagnostic.DiagnosticId);

                if (result != testCase.ExpectedResult)
                {
                    var issue = string.Format("Expression {0} should have evaluated to '{1}' but was '{2}'", source, testCase.ExpectedResult, result);
                    issues.Add(issue);
                }
            }

            if (issues.Count > 0)
            {
                issues.Insert(0, string.Format("{0} errors:", issues.Count));
                var issueText = string.Join(Environment.NewLine, issues);
                Assert.True(false, issueText);
            }
        }

        [Fact]
        public void IntrinsicOperator_BinarySignaturesAreCorrect()
        {
            var issues = new List<string>();

            foreach(var testCase in BinaryTestCases)
            {
                var opText = testCase.Op;
                var left = GetValue(testCase.Left);
                var right = GetValue(testCase.Right);
                var source = string.Format("{0} {1} {2}", left, opText, right);
                var syntaxTree = SyntaxTree.ParseExpression(source);
                var syntaxTreeSource = syntaxTree.Root.ToString();
                if (syntaxTreeSource != source)
                    Assert.True(false, string.Format("Source should have been {0} but is {1}", syntaxTreeSource, source));

                var expression = (BinaryExpressionSyntax) syntaxTree.Root.Root;
                var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
                var semanticModel = compilation.GetSemanticModel();

                var leftType = GetExpressionTypeString(semanticModel.GetExpressionType(expression.Left));
                if (testCase.Left != leftType)
                    Assert.True(false, string.Format("Left should be of type '{0}' but has type '{1}", testCase.Left, leftType));

                var rightType = GetExpressionTypeString(semanticModel.GetExpressionType(expression.Right));
                if (testCase.Right != rightType)
                    Assert.True(false, string.Format("Right should be of type '{0}' but has type '{1}", testCase.Right, rightType));

                var diagnostic = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).SingleOrDefault();
                var expressionType = semanticModel.GetExpressionType(expression);
                var result = diagnostic == null
                                 ? GetExpressionTypeString(expressionType)
                                 : GetErrorString(diagnostic.DiagnosticId);

                if (result != testCase.ExpectedResult)
                {
                    var issue = string.Format("Expression {0} should have evaluated to '{1}' but was '{2}'", source, testCase.ExpectedResult, result);
                    issues.Add(issue);
                }
            }

            if (issues.Count > 0)
            {
                issues.Insert(0, string.Format("{0} errors:", issues.Count));
                var issueText = string.Join(Environment.NewLine, issues);
                Assert.True(false, issueText);
            }
        }

        private static string GetExpressionTypeString(Type type)
        {
            if (type == typeof(byte))
                return "byte";

            if (type == typeof(sbyte))
                return "sbyte";

            if (type == typeof(char))
                return "char";

            if (type == typeof(short))
                return "short";

            if (type == typeof(ushort))
                return "ushort";

            if (type == typeof(int))
                return "int";

            if (type == typeof(uint))
                return "uint";

            if (type == typeof(long))
                return "long";

            if (type == typeof(ulong))
                return "ulong";

            if (type == typeof(float))
                return "float";

            if (type == typeof(double))
                return "double";

            if (type == typeof(decimal))
                return "decimal";

            if (type == typeof(bool))
                return "bool";

            if (type == typeof(string))
                return "string";

            if (type == typeof(object))
                return "object";

            throw new ArgumentOutOfRangeException("type");
        }

        private static string GetErrorString(DiagnosticId diagnosticId)
        {
            switch (diagnosticId)
            {
                case DiagnosticId.CannotApplyUnaryOperator:
                case DiagnosticId.CannotApplyBinaryOperator:
                    return "#inapplicable";
                case DiagnosticId.AmbiguousUnaryOperator:
                case DiagnosticId.AmbiguousBinaryOperator:
                    return "#ambiguous";
                default:
                    throw new ArgumentOutOfRangeException("diagnosticId");
            }
        }

        private static string GetValue(string type)
        {
            switch (type)
            {
                case "byte":
                    return "CAST(1 AS byte)";

                case "sbyte":
                    return "CAST(1 AS sbyte)";

                case "char":
                    return "CAST(65 AS char)";

                case "short":
                    return "CAST(1 AS short)";

                case "ushort":
                    return "CAST(1 AS ushort)";

                case "int":
                    return "1";

                case "uint":
                    return "CAST(1 AS uint)";

                case "long":
                    return "CAST(1 AS long)";

                case "ulong":
                    return "CAST(1 AS ulong)";

                case "float":
                    return "CAST(1.0 AS single)";

                case "double":
                    return "1.0";

                case "decimal":
                    return "CAST(1.0 AS decimal)";

                case "bool":
                    return "false";

                case "string":
                    return "'s'";

                case "object":
                    return "CAST(null AS object)";

                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }
    }
}