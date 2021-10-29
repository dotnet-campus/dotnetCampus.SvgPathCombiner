using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;

using Walterlv.MiniLanguages.Visitors;

namespace Walterlv.MiniLanguages.Syntax
{
    internal abstract class PathSyntax
    {
        protected internal abstract void Parse(ref CommandContext context, ref int parsingIndex, IPathSyntaxVisitor visitor);

        protected static bool ReadBoolean(string originalText, ref int parsingIndex)
        {
            var value = ReadDouble(originalText, ref parsingIndex);
            // 取值只有 1 和 0，我们用 0.5 来将其区分。
            return value >= 0.5;
        }

        protected static Point ReadPoint(string originalText, ref int parsingIndex)
        {
            var x = ReadDouble(originalText, ref parsingIndex);
            var y = ReadDouble(originalText, ref parsingIndex);
            return new Point(x, y);
        }

        protected static Size ReadSize(string originalText, ref int parsingIndex)
        {
            var x = ReadDouble(originalText, ref parsingIndex);
            var y = ReadDouble(originalText, ref parsingIndex);
            return new Size(x, y);
        }

        protected static double ReadDouble(string originalText, ref int parsingIndex)
        {
            var numberLength = 0;
            for (var i = parsingIndex; i < originalText.Length; i++)
            {
                var shouldParse = false;
                var c = originalText[i];
                if (IsNumberChar(c))
                {
                    // 遇到数字，长度增加。
                    numberLength++;
                }
                else if (numberLength is 0)
                {
                    // 开头遇到非数字，跳过。
                    parsingIndex++;
                }
                else
                {
                    shouldParse = true;
                }

                var isEnd = i == originalText.Length - 1;
                if (shouldParse || isEnd)
                {
                    // 非开头遇到非数字（或者已抵达文本末尾），解析数字。
                    var number = double.TryParse(
#if NETCOREAPP
                            originalText.AsSpan(parsingIndex, numberLength),
#else
                            originalText.Substring(parsingIndex, numberLength),
#endif
                            NumberStyles.Float, CultureInfo.InvariantCulture, out var result)
                        ? result
                        : 0;
                    // 正常是遇到了后面的非数字才结束解析的，而那时 index 已经往后移了一位了。
                    // 但现在解析到数字时遇到了字符串结尾，index 还没有往后移；所以我们需要补偿一个 1，以便让解析正常结束，否则陷入无限循环。
                    parsingIndex = (!shouldParse && isEnd) ? i + 1 : i;
                    return number;
                }
            }
            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsNumberChar(char c)
        {
            return char.IsDigit(c) || c is '.' or '+' or '-' or 'E';
        }
    }
}
