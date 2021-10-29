using System.Windows;
using System.Windows.Media;

using Walterlv.MiniLanguages.Visitors;

namespace Walterlv.Tools
{
    internal class PathTransformVisitor : PathSyntaxVisitor
    {
        private readonly Transform _transform;
        private PathGeometry _path = new();
        private PathFigure _figure = null!;

        public PathTransformVisitor(Transform? transform)
        {
            _transform = transform ?? Transform.Identity;
        }

        public override void Start()
        {
        }

        public override void F(bool isEvenOddOtherwiseNonzero)
        {
            _path.FillRule = isEvenOddOtherwiseNonzero ? FillRule.EvenOdd : FillRule.Nonzero;
        }

        public override void M(Point startPoint)
        {
            _figure = new PathFigure
            {
                StartPoint = _transform.Transform(startPoint)
            };
            _path.Figures.Add(_figure);
        }

        public override void L(Point lastPoint, Point endPoint)
        {
            _figure.Segments.Add(new LineSegment(
                _transform.Transform(endPoint),
                false));
        }

        public override void C(Point lastPoint, Point controlPoint1, Point controlPoint2, Point endPoint)
        {
            _figure.Segments.Add(new BezierSegment(
                _transform.Transform(controlPoint1),
                _transform.Transform(controlPoint2),
                _transform.Transform(endPoint),
                false));
        }

        public override void Q(Point lastPoint, Point controlPoint, Point endPoint)
        {
            _figure.Segments.Add(new QuadraticBezierSegment(
                _transform.Transform(controlPoint),
                _transform.Transform(endPoint),
                false));
        }

        public override void A(Point lastPoint, Size size, double rotationAngle, bool isLargeArc, bool isClockwise, Point endPoint)
        {
            var scale = GetScaleSize(_transform.Value);
            _figure.Segments.Add(new ArcSegment(
                _transform.Transform(endPoint),
                new Size(size.Width * scale.Width, size.Height * scale.Height),
                rotationAngle,
                isLargeArc,
                isClockwise ? SweepDirection.Clockwise : SweepDirection.Counterclockwise,
                false));
        }

        public override void Z()
        {
            _figure.IsClosed = true;
        }

        public override void Complete()
        {
        }

        private static Size GetScaleSize(Matrix transform)
        {
            // 计算旋转分量。
            var unitVector = new Vector(1, 0);
            var vector = transform.Transform(unitVector);
            //如果图片旋转了，那么得到的值和图片显示的不同，会被计算旋转后的值，参见 Element 旋转。
            //所以需要把图片还原
            //还原的方法是计算获得的角度，也就是和单位分量角度，由角度可以得到旋转度。
            //用转换旋转之前旋转角度反过来就是得到原来图片的值
            var angle = Vector.AngleBetween(unitVector, vector);
            transform.Rotate(-angle);
            // 综合缩放。
            var rect = new Rect(new Size(1, 1));
            rect.Transform(transform);

            return rect.Size;
        }

        public override string ToString()
        {
            return _path.ToString();
        }
    }
}
