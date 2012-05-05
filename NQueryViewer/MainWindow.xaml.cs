﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

using NQuery.Language;

using NQueryViewer.Helpers;

namespace NQueryViewer
{
    [Export(typeof(IMainWindowProvider))]
    internal sealed partial class MainWindow : IMainWindowProvider, IPartImportsSatisfiedNotification
    {
        private IWpfTextViewHost _textViewHost;

        [Import]
        public TextViewFactory TextViewFactory { get; set; }

        public Window Window
        {
            get { return this; }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        public void OnImportsSatisfied()
        {
            _textViewHost = TextViewFactory.CreateTextViewHost();
            _editorHost.Content = _textViewHost.HostControl;

            _textViewHost.TextView.Caret.PositionChanged += CaretOnPositionChanged;
            _textViewHost.TextView.TextBuffer.PostChanged += TextBufferOnPostChanged;

            UpdateTree();
        }

        private static IEnumerable<NodeViewModel> ToViewModel(string source)
        {
            //return Lex(source);
            //return ParseExpression(source);
            return ParseQuery(source);
        }

        private static IEnumerable<NodeViewModel> Lex(string source)
        {
            var lexer = new Lexer(source);
            var tokens = new List<SyntaxToken>();

            SyntaxToken token;
            do
            {
                token = lexer.Lex();
                tokens.Add(token);
            } while (token.Kind != SyntaxKind.EndOfFileToken);

            return ToViewModel(tokens);
        }

        private static IEnumerable<NodeViewModel> ParseExpression(string source)
        {
            var syntaxTree = SyntaxTree.ParseExpression(source);
            return new[] { ToViewModel(syntaxTree.Root) };
        }

        private static IEnumerable<NodeViewModel> ParseQuery(string source)
        {
            var syntaxTree = SyntaxTree.ParseQuery(source);
            return new[] { ToViewModel(syntaxTree.Root) };
        }

        private static IEnumerable<NodeViewModel> ToViewModel(IEnumerable<SyntaxToken> root)
        {
            return root.Select(ToViewModel).ToList();
        }

        private static NodeViewModel ToViewModel(SyntaxNodeOrToken nodeOrToken)
        {
            return nodeOrToken.IsNode
                       ? ToViewModel(nodeOrToken.AsNode())
                       : ToViewModel(nodeOrToken.AsToken());
        }

        private static NodeViewModel ToViewModel(SyntaxNode node)
        {
            var children = new List<NodeViewModel>();

            foreach (var child in node.GetChildren())
            {
                if (child.IsToken)
                    children.AddRange(child.AsToken().LeadingTrivia.Select(ToViewModel));

                children.Add(ToViewModel(child));

                if (child.IsToken)
                    children.AddRange(child.AsToken().TrailingTrivia.Select(ToViewModel));
            }

            return new NodeViewModel(node, children);
        }

        private static NodeViewModel ToViewModel(SyntaxToken token)
        {
            return new NodeViewModel(token, new List<NodeViewModel>());
        }

        private static NodeViewModel ToViewModel(SyntaxTrivia trivia)
        {
            return new NodeViewModel(trivia, new List<NodeViewModel>());
        }

        private enum NodeViewModelKind
        {
            Node,
            Token,
            Trivia
        }

        private sealed class NodeViewModel
        {
            public NodeViewModel(SyntaxToken token, IList<NodeViewModel> children)
            {
                Data = token;
                NodeType = NodeViewModelKind.Token;
                Kind = token.Kind;
                ContextualKind = token.ContextualKind;
                Span = token.Span;
                FullSpan = token.FullSpan;
                IsMissing = token.IsMissing;
                UpdateChildren(children);
            }

            public NodeViewModel(SyntaxTrivia data, IList<NodeViewModel> children)
            {
                Data = data;
                NodeType = NodeViewModelKind.Trivia;
                Kind = data.Kind;
                ContextualKind = SyntaxKind.BadToken;
                Span = data.Span;
                FullSpan = data.Span;
                IsMissing = false;
                UpdateChildren(children);
            }

            public NodeViewModel(SyntaxNode data, IList<NodeViewModel> children)
            {
                Data = data;
                NodeType = NodeViewModelKind.Node;
                Kind = data.Kind;
                ContextualKind = SyntaxKind.BadToken;
                Span = data.Span;
                FullSpan = data.FullSpan;
                IsMissing = false;
                UpdateChildren(children);
            }

            private void UpdateChildren(IList<NodeViewModel> children)
            {
                Children = new ReadOnlyCollection<NodeViewModel>(children);

                foreach (var nodeViewModel in children)
                    nodeViewModel.Parent = this;
            }

            public NodeViewModel Parent { get; private set; }

            public object Data { get; private set; }

            public NodeViewModelKind NodeType { get; private set; }

            public SyntaxKind Kind { get; private set; }

            public SyntaxKind ContextualKind { get; private set; }

            public TextSpan Span { get; private set; }

            public TextSpan FullSpan { get; private set; }

            public bool IsMissing { get; set; }

            public ReadOnlyCollection<NodeViewModel> Children { get; private set; }

            public override string ToString()
            {
                return ContextualKind == SyntaxKind.BadToken
                           ? Kind.ToString()
                           : string.Format("{0} ({1})", Kind, ContextualKind);
            }
        }

        private void UpdateTree()
        {
            var source = _textViewHost.TextView.TextSnapshot.GetText();
            _treeView.ItemsSource = ToViewModel(source);
        }

        private void UpdateTreeExpansion()
        {
            var position = _textViewHost.TextView.Caret.Position.BufferPosition.Position;
            var roots = _treeView.ItemsSource.OfType<NodeViewModel>();
            var node = FindViewModelNode(roots, position) ?? FindViewModelNode(roots, position - 1);
            if (node != null)
                _treeView.SelectNode(node, n => n.Parent, true);
        }

        private NodeViewModel FindViewModelNode(IEnumerable<NodeViewModel> roots, int position)
        {
            var nonTrivia = from r in roots
                            where r.NodeType != NodeViewModelKind.Trivia
                            select r;

            foreach (var nodeViewModel in nonTrivia)
            {
                if (nodeViewModel.Span.Contains(position))
                {
                    return nodeViewModel.Children.Any()
                               ? FindViewModelNode(nodeViewModel.Children, position)
                               : nodeViewModel;
                }
            }

            return null;
        }

        private void UpdateSelectedText()
        {
            var viewModel = _treeView.SelectedItem as NodeViewModel;
            if (viewModel == null)
                return;

            var span = viewModel.Span;

            var snapshot = _textViewHost.TextView.TextBuffer.CurrentSnapshot;
            var snapshotSpan = new SnapshotSpan(snapshot, span.Start, span.Length);
            _textViewHost.TextView.Selection.Select(snapshotSpan, false);
        }

        private void CaretOnPositionChanged(object sender, CaretPositionChangedEventArgs caretPositionChangedEventArgs)
        {
            if (_textViewHost.HostControl.IsKeyboardFocusWithin)
                UpdateTreeExpansion();
        }

        private void TextBufferOnPostChanged(object sender, EventArgs eventArgs)
        {
            UpdateTree();
            if (_textViewHost.HostControl.IsKeyboardFocusWithin)
                UpdateTreeExpansion();
        }

        private void TreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_treeView.IsKeyboardFocusWithin)
                UpdateSelectedText();
        }
    }
}
