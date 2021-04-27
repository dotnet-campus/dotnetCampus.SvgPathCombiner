using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Walterlv.Tools
{
    public class SvgPathCombiner
    {
        public static bool TryGuessSvgDrawingViewbox(DrawingGroup dg, out Rect viewbox)
        {
            var current = dg;
            for (var i = 0; i < 2; i++)
            {
                if (current.ClipGeometry is RectangleGeometry rectangleGeometry && rectangleGeometry.Rect is { } rect)
                {
                    viewbox = rect;
                    return true;
                }
                if(current.Children.FirstOrDefault() is DrawingGroup d)
                {
                    current = d;
                }
                else
                {
                    break;
                }
            }
            viewbox = default;
            return false;
        }

        public static StreamGeometry Combine(DrawingGroup drawingGroup)
        {
            return RecursiveCombine(drawingGroup, null, Matrix.Identity);
        }

        private static StreamGeometry RecursiveCombine(DrawingGroup drawingGroup, StreamGeometry? currentGeometry, Matrix currentTransform)
        {
            foreach (var drawing in drawingGroup.Children.OfType<Drawing>())
            {
                var transform = currentTransform;
                if (drawing is DrawingGroup dg)
                {
                    var dgTransform = dg.Transform?.Value ?? Matrix.Identity;
                    transform = currentTransform * dgTransform;
                    currentGeometry = RecursiveCombine(dg, currentGeometry, transform);
                }
                else if (drawing is GeometryDrawing gd)
                {
                    if (gd.Geometry is PathGeometry pg)
                    {
                        var geometry = Transform(pg, currentTransform);
                        currentGeometry = RecursiveCombine(currentGeometry, geometry);
                    }
                }
            }
            return currentGeometry!;
        }

        private static StreamGeometry RecursiveCombine(StreamGeometry? geometry1, PathGeometry geometry2)
        {
            if (geometry1 is null)
            {
                var path = geometry2.ToString(CultureInfo.InvariantCulture);
                var result = (StreamGeometry)Geometry.Parse(path);
                return result;
            }
            else
            {
                var path1 = geometry1.ToString(CultureInfo.InvariantCulture);
                var path2 = geometry2.ToString(CultureInfo.InvariantCulture);

                var mIndex = path2.IndexOf('M');
                if (mIndex > 0)
                {
                    // 严格来说，不能去掉前面的 F0（EvenOdd）和 F1（NonZero）合并方式；但如果合并方式不同，也无法合并呀……
                    path2 = path2[mIndex..];
                }

                var result = (StreamGeometry)Geometry.Parse(path1 + path2);
                return result;
            }
        }

        private static PathGeometry Transform(PathGeometry pathGeometry, Matrix transform)
        {
            if (transform == Matrix.Identity)
            {
                return pathGeometry;
            }
            return Geometry.Combine(pathGeometry, pathGeometry, GeometryCombineMode.Intersect, new MatrixTransform(transform));
        }
    }
}
