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
            return RecursiveCombine(drawingGroup, null);
        }

        private static StreamGeometry RecursiveCombine(DrawingGroup drawingGroup, StreamGeometry? currentGeometry)
        {
            foreach (var drawing in drawingGroup.Children.OfType<Drawing>())
            {
                if (drawing is DrawingGroup dg)
                {
                    currentGeometry = RecursiveCombine(dg, currentGeometry);
                }
                else if (drawing is GeometryDrawing gd)
                {
                    if (gd.Geometry is PathGeometry pg)
                    {
                        currentGeometry = RecursiveCombine(pg, currentGeometry);
                    }
                }
            }
            return currentGeometry!;
        }

        private static StreamGeometry RecursiveCombine(PathGeometry geometry1, StreamGeometry? geometry2)
        {
            if (geometry2 is null)
            {
                var path = geometry1.ToString(CultureInfo.InvariantCulture);
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
                    path2 = path2[mIndex..];
                }

                var result = (StreamGeometry)Geometry.Parse(path1 + path2);
                return result;
            }
        }
    }
}
