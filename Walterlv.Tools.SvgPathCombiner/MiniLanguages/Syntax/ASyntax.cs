using Walterlv.MiniLanguages.Visitors;

namespace Walterlv.MiniLanguages.Syntax
{
    internal sealed class ASyntax : PathSyntax
    {
        protected internal override void Parse(ref CommandContext context, ref int parsingIndex, IPathSyntaxVisitor visitor)
        {
            var originalText = context.OriginalText;
            var size = ReadSize(originalText, ref parsingIndex);
            var rotationAngle = ReadDouble(originalText, ref parsingIndex);
            var isLargeArc = ReadBoolean(originalText, ref parsingIndex);
            var isClockwise = ReadBoolean(originalText, ref parsingIndex);
            var endPoint = ReadPoint(originalText, ref parsingIndex);
            context.Length = parsingIndex - context.StartIndex;
            visitor.A(in context, size, rotationAngle, isLargeArc, isClockwise, endPoint);
        }
    }
}
