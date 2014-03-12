﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace NQuery.Authoring.CodeActions
{
    public abstract class CodeIssueProvider<T> : ICodeIssueProvider
        where T : SyntaxNode
    {
        public IEnumerable<CodeIssue> GetIssues(SemanticModel semanticModel)
        {
            var syntaxTree = semanticModel.SyntaxTree;
            var nodes = syntaxTree.Root.DescendantNodesAndSelf().OfType<T>();
            return nodes.SelectMany(node => GetIssues(semanticModel, node));
        }

        protected abstract IEnumerable<CodeIssue> GetIssues(SemanticModel semanticModel, T node);
    }
}