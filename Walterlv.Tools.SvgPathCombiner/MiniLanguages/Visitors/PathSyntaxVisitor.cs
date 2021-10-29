using System.Runtime.CompilerServices;
using System.Windows;

namespace Walterlv.MiniLanguages.Visitors
{
    public abstract class PathSyntaxVisitor : IPathSyntaxVisitor
    {
        private char _lastCommand;
        private Point _lastPoint;
        private Point _lastControlPoint;

        void IPathSyntaxVisitor.Start(in CommandContext context)
        {
            Start();
        }

        void IPathSyntaxVisitor.F(in CommandContext context, bool isEvenOddOtherwiseNonzero)
        {
            _lastCommand = 'F';
            F(isEvenOddOtherwiseNonzero);
        }

        void IPathSyntaxVisitor.M(in CommandContext context, Point startPoint)
        {
            _lastCommand = 'M';
            M(startPoint);
            _lastPoint = startPoint;
        }

        void IPathSyntaxVisitor.L(in CommandContext context, Point endPoint)
        {
            _lastCommand = 'L';
            endPoint = MakeAbsoluteEndPoint(context.Command, in endPoint);
            _lastCommand = 'L';
            L(_lastPoint, endPoint);
            _lastPoint = endPoint;
        }

        void IPathSyntaxVisitor.H(in CommandContext context, double x)
        {
            var endPoint = new Point(x + (char.IsUpper(context.Command) ? 0 : _lastPoint.X), _lastPoint.Y);
            _lastCommand = 'L';
            L(_lastPoint, endPoint);
            _lastPoint = endPoint;
        }

        void IPathSyntaxVisitor.V(in CommandContext context, double y)
        {
            var endPoint = new Point(_lastPoint.X, y + (char.IsUpper(context.Command) ? 0 : _lastPoint.Y));
            _lastCommand = 'L';
            L(_lastPoint, endPoint);
            _lastPoint = endPoint;
        }

        void IPathSyntaxVisitor.C(in CommandContext context, Point controlPoint1, Point controlPoint2, Point endPoint)
        {
            controlPoint1 = MakeAbsoluteEndPoint(context.Command, in controlPoint1);
            controlPoint2 = MakeAbsoluteEndPoint(context.Command, in controlPoint2);
            endPoint = MakeAbsoluteEndPoint(context.Command, in endPoint);
            _lastCommand = 'C';
            _lastControlPoint = controlPoint2;
            C(_lastPoint, controlPoint1, controlPoint2, endPoint);
            _lastPoint = endPoint;
        }

        void IPathSyntaxVisitor.Q(in CommandContext context, Point controlPoint, Point endPoint)
        {
            controlPoint = MakeAbsoluteEndPoint(context.Command, in controlPoint);
            endPoint = MakeAbsoluteEndPoint(context.Command, in endPoint);
            _lastCommand = 'Q';
            _lastControlPoint = controlPoint;
            Q(_lastPoint, controlPoint, endPoint);
            _lastPoint = endPoint;
        }

        void IPathSyntaxVisitor.S(in CommandContext context, Point controlPoint2, Point endPoint)
        {
            controlPoint2 = MakeAbsoluteEndPoint(context.Command, in controlPoint2);
            endPoint = MakeAbsoluteEndPoint(context.Command, in endPoint);
            var controlPoint1 = _lastCommand is 'C'
                ? Reflect()
                : _lastPoint;
            _lastCommand = 'C';
            _lastControlPoint = controlPoint2;
            C(_lastPoint, controlPoint1, controlPoint2, endPoint);
            _lastPoint = endPoint;
        }

        void IPathSyntaxVisitor.T(in CommandContext context, Point endPoint)
        {
            endPoint = MakeAbsoluteEndPoint(context.Command, in endPoint);
            var controlPoint = _lastCommand is 'Q'
                ? Reflect()
                : _lastPoint;
            _lastCommand = 'Q';
            _lastControlPoint = controlPoint;
            Q(_lastPoint, controlPoint, endPoint);
            _lastPoint = endPoint;
        }

        void IPathSyntaxVisitor.A(in CommandContext context, Size size, double rotationAngle, bool isLargeArc, bool isClockwise, Point endPoint)
        {
            _lastCommand = 'A';
            endPoint = MakeAbsoluteEndPoint(context.Command, in endPoint);
            A(_lastPoint, size, rotationAngle, isLargeArc, isClockwise, endPoint);
            _lastPoint = endPoint;
        }

        void IPathSyntaxVisitor.Z(in CommandContext context)
        {
            _lastCommand = 'Z';
            Z();
        }

        void IPathSyntaxVisitor.Complete()
        {
            Complete();
        }

        public virtual void Start()
        {
        }

        public virtual void F(bool isEvenOddOtherwiseNonzero)
        {
        }

        public virtual void M(Point startPoint)
        {
        }

        public virtual void L(Point lastPoint, Point endPoint)
        {
        }

        public virtual void C(Point lastPoint, Point controlPoint1, Point controlPoint2, Point endPoint)
        {
        }

        public virtual void Q(Point lastPoint, Point controlPoint, Point endPoint)
        {
        }

        public virtual void A(Point lastPoint, Size size, double rotationAngle, bool isLargeArc, bool isClockwise, Point endPoint)
        {
        }

        public virtual void Z()
        {
        }

        public virtual void Complete()
        {
        }

        /// <summary>
        /// 将上一个控制点根据上一个点的位置进行对称，以获得平滑的贝塞尔曲线。
        /// </summary>
        /// <returns>平滑的贝塞尔曲线控制点。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Point Reflect()
        {
            return new Point(2 * _lastPoint.X - _lastControlPoint.X, 2 * _lastPoint.Y - _lastControlPoint.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Point MakeAbsoluteEndPoint(in char command, in Point endPoint)
        {
            return char.IsUpper(command) ? endPoint : _lastPoint + new Vector(endPoint.X, endPoint.Y);
        }
    }
}
