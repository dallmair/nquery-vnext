using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Utilities;

namespace NQueryViewer.EditorIntegration
{
    internal sealed class NQueryContentTypeProvider
    {
#pragma warning disable 649
        [Export]
        [BaseDefinition("Code")]
        [Name("NQuery")]
        public ContentTypeDefinition NQueryContentTypeDefinition;
#pragma warning restore 649
    }
}