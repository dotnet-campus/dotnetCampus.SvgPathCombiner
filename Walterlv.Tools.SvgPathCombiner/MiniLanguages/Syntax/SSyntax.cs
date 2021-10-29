using Walterlv.MiniLanguages.Visitors;

namespace Walterlv.MiniLanguages.Syntax
{
    internal sealed class SSyntax : PathSyntax
    {
        protected internal override void Parse(ref CommandContext context, ref int parsingIndex, IPathSyntaxVisitor visitor)
        {
            var originalText = context.OriginalText;
            var controlPoint2 = ReadPoint(originalText, ref parsingIndex);
            var endPoint = ReadPoint(originalText, ref parsingIndex);
            context.Length = parsingIndex - context.StartIndex;
            visitor.S(in context, controlPoint2, endPoint);
        }
    }
}
