using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

namespace NQuery.Text
{
    public abstract class SourceText
    {
        private SourceTextContainer _container;

        protected SourceText()
            : this(null)
        {
        }

        protected SourceText(SourceTextContainer container)
        {
            _container = container;
        }

        public static SourceText From(string text)
        {
            return new StringText(text);
        }

        public SourceTextContainer Container
        {
            get
            {
                if (_container == null)
                {
                    var container = new StaticSourceTextContainer(this);
                    Interlocked.CompareExchange(ref _container, container, null);
                }
                return _container;
            }
        }

        public TextLine GetLineFromPosition(int position)
        {
            if (position < 0 || position > Length)
                throw new ArgumentOutOfRangeException("position");

            var lineNumber = GetLineNumberFromPosition(position);
            return Lines[lineNumber];
        }

        public abstract int GetLineNumberFromPosition(int position);

        public TextLocation GetTextLocation(int position)
        {
            var line = GetLineFromPosition(position);
            var lineNumber = line.LineNumber;
            var column = position - line.Span.Start;
            return new TextLocation(lineNumber, column);
        }

        public int GetPosition(TextLocation location)
        {
            var textLine = Lines[location.Line];
            return textLine.Span.Start + location.Column;
        }

        public abstract string GetText(TextSpan textSpan);

        public string GetText(int position, int length)
        {
            return GetText(new TextSpan(position, length));
        }

        public string GetText(int position)
        {
            var remaining = Length - position;
            return GetText(position, remaining);
        }

        public string GetText()
        {
            return GetText(0, Length);
        }

        public SourceText WithChanges(params TextChange[] changes)
        {
            if (changes == null || changes.Length == 0)
                return this;

            return WithChanges((IEnumerable<TextChange>) changes);
        }

        public SourceText WithChanges(IEnumerable<TextChange> changes)
        {
            var persistedChanges = changes.ToImmutableArray();

            var sb = new StringBuilder(GetText());
            foreach (var textChange in persistedChanges)
            {
                sb.Remove(textChange.Span.Start, textChange.Span.Length);
                sb.Insert(textChange.Span.Start, textChange.NewText);
            }

            var newText = From(sb.ToString());

            return new ChangedSourceText(this, newText, persistedChanges);
        }

        public IEnumerable<TextChange> GetChanges(SourceText oldText)
        {
            if (oldText == null)
                throw new ArgumentNullException("oldText");

            if (oldText == this)
                return Enumerable.Empty<TextChange>();

            var rootFound = false;
            var candidate = this;
            var path = new Stack<ChangedSourceText>();

            while (candidate != null && !rootFound)
            {
                var changed = candidate as ChangedSourceText;
                if (changed == null)
                {
                    candidate = null;
                }
                else
                {
                    if (changed.OldText == oldText)
                        rootFound = true;

                    path.Push(changed);
                    candidate = changed.OldText;
                }
            }

            if (!rootFound)
            {
                var oldSpan = new TextSpan(0, oldText.Length);
                var newText = GetText();
                var textChange = new TextChange(oldSpan, newText);
                return ImmutableArray.Create(textChange);
            }
            
            var changes = new List<TextChange>();
            while (path.Count > 0)
            {
                var c = path.Pop();
                changes.AddRange(c.Changes);
            }

            return changes.ToImmutableArray();
        }

        public SourceText Replace(TextSpan span, string newText)
        {
            return WithChanges(new TextChange(span, newText));
        }

        public SourceText Replace(int start, int length, string newText)
        {
            var span = new TextSpan(start, length);
            return Replace(span, newText);
        }

        public abstract char this[int index] { get; }

        public abstract int Length { get; }

        public abstract TextLineCollection Lines { get; }
    }
}