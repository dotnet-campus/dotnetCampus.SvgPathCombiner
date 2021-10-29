using Walterlv.MiniLanguages.Visitors;

namespace Walterlv.MiniLanguages.Syntax
{
    internal sealed class HSyntax : PathSyntax
    {
        protected internal override void Parse(ref CommandContext context, ref int parsingIndex, IPathSyntaxVisitor visitor)
        {
            var originalText = context.OriginalText;
            var x = ReadDouble(originalText, ref parsingIndex);
            context.Length = parsingIndex - context.StartIndex;
            visitor.H(in context, x);
        }
    }
}
