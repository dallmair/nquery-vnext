using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class CastExpressionSyntax : ExpressionSyntax
    {
        private readonly SyntaxToken _castKeyword;
        private readonly SyntaxToken _leftParenthesisToken;
        private readonly ExpressionSyntax _expression;
        private readonly SyntaxToken _asKeyword;
        private readonly SyntaxToken _typeName;
        private readonly SyntaxToken _rightParenthesisToken;

        public CastExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken castKeyword, SyntaxToken leftParenthesisToken, ExpressionSyntax expression, SyntaxToken asKeyword, SyntaxToken typeName, SyntaxToken rightParenthesisToken)
            : base(syntaxTree)
        {
            _castKeyword = castKeyword.WithParent(this);
            _leftParenthesisToken = leftParenthesisToken.WithParent(this);
            _expression = expression;
            _asKeyword = asKeyword.WithParent(this);
            _typeName = typeName.WithParent(this);
            _rightParenthesisToken = rightParenthesisToken.WithParent(this);
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CastExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _castKeyword;
            yield return _leftParenthesisToken;
            yield return _expression;
            yield return _asKeyword;
            yield return _typeName;
            yield return _rightParenthesisToken;
        }

        public SyntaxToken CastKeyword
        {
            get { return _castKeyword; }
        }

        public SyntaxToken LeftParenthesisToken
        {
            get { return _leftParenthesisToken; }
        }

        public ExpressionSyntax Expression
        {
            get { return _expression; }
        }

        public SyntaxToken AsKeyword
        {
            get { return _asKeyword; }
        }

        public SyntaxToken TypeName
        {
            get { return _typeName; }
        }

        public SyntaxToken RightParenthesisToken
        {
            get { return _rightParenthesisToken; }
        }
    }
}