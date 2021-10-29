using Walterlv.MiniLanguages.Visitors;

namespace Walterlv.MiniLanguages.Syntax
{
    internal sealed class FSyntax : PathSyntax
    {
        protected internal override void Parse(ref CommandContext context, ref int parsingIndex, IPathSyntaxVisitor visitor)
        {
            var originalText = context.OriginalText;
            var isEvenOddOtherwiseNonzero = ReadBoolean(originalText, ref parsingIndex);
            context.Length = parsingIndex - context.StartIndex;
            visitor.F(in context, isEvenOddOtherwiseNonzero);
        }
    }
}
