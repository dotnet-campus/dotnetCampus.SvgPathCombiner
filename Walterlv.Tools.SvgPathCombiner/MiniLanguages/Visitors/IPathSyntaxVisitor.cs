using System.Windows;

namespace Walterlv.MiniLanguages.Visitors
{
    public interface IPathSyntaxVisitor
    {
        void Start(in CommandContext context);
        void F(in CommandContext context, bool isEvenOddOtherwiseNonzero);
        void M(in CommandContext context, Point startPoint);
        void L(in CommandContext context, Point endPoint);
        void H(in CommandContext context, double x);
        void V(in CommandContext context, double y);
        void C(in CommandContext context, Point controlPoint1, Point controlPoint2, Point endPoint);
        void Q(in CommandContext context, Point controlPoint, Point endPoint);
        void S(in CommandContext context, Point controlPoint2, Point endPoint);
        void T(in CommandContext context, Point endPoint);
        void A(in CommandContext context, Size size, double rotationAngle, bool isLargeArc, bool isClockwise, Point endPoint);
        void Z(in CommandContext context);
        void Complete();
    }
}
