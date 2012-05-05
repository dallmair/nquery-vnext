using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace NQueryViewer.EditorIntegration
{
    [Export(typeof(IClassifierProvider))]
    [ContentType("NQuery")]
    [Name("NQueryClassifier")]
    internal sealed class NQueryClassifierProvider : IClassifierProvider
    {
        [Import]
        public IStandardClassificationService ClassificationService { get; set; }

        [Import]
        public INQuerySyntaxTreeManagerService InQuerySyntaxTreeManagerService { get; set; }

        public IClassifier GetClassifier(ITextBuffer textBuffer)
        {
            var syntaxTreeManager = InQuerySyntaxTreeManagerService.GetCSharpSyntaxTreeManager(textBuffer);
            return new NQueryClassifier(ClassificationService, textBuffer, syntaxTreeManager);
        }
    }
}