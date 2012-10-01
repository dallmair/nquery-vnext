using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.Language.UnitTests
{
    [TestClass]
    public class CoalesceExpressionTests
    {
        [TestMethod]
        public void Coalesce_DetectsConversionIssues()
        {
            var syntaxTree = SyntaxTree.ParseExpression("COALESCE(1, '2', 3.0)");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.CannotConvert, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void Coalesce_AppliesConversion()
        {
            var syntaxTree = SyntaxTree.ParseExpression("COALESCE(1, 3.0)");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToArray();

            var type = semanticModel.GetExpressionType((ExpressionSyntax) syntaxTree.Root.Root);

            Assert.AreEqual(0, diagnostics.Length);
            Assert.AreEqual(typeof(double), type);
        }

        [TestMethod]
        public void Coalesce_DetectsTooFewArguments_WhenNoArgumentIsProvided()
        {
            var syntaxTree = SyntaxTree.ParseExpression("COALESCE()");
            var diagnostics = syntaxTree.GetDiagnostics().ToArray();
            Assert.AreEqual(3, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.TokenExpected, diagnostics[0].DiagnosticId);                        
            Assert.AreEqual(DiagnosticId.TokenExpected, diagnostics[1].DiagnosticId);                        
            Assert.AreEqual(DiagnosticId.TokenExpected, diagnostics[2].DiagnosticId);                        
        }

        [TestMethod]
        public void Coalesce_DetectsTooFewArguments_WhenSingleArgumentIsProvided()
        {
            var syntaxTree = SyntaxTree.ParseExpression("COALESCE(1)");
            var diagnostics = syntaxTree.GetDiagnostics().ToArray();
            Assert.AreEqual(2, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.TokenExpected, diagnostics[0].DiagnosticId);
            Assert.AreEqual(DiagnosticId.TokenExpected, diagnostics[1].DiagnosticId);
        }
    }
}