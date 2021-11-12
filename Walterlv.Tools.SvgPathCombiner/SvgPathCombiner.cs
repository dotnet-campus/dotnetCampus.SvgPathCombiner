using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

using Walterlv.MiniLanguages;

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

        public static string Combine(DrawingGroup drawingGroup)
        {
            string? currentGeometry = null;
            foreach (var drawing in drawingGroup.Children.OfType<Drawing>())
            {
                if (drawing is DrawingGroup dg)
                {
                    var path1 = currentGeometry;
                    var path2 = Combine(dg);
                    currentGeometry = RecursiveCombine(path1, path2);
                }
                else if (drawing is GeometryDrawing gd)
                {
                    if (gd.Brush is null)
                    {
                        // 不可见图形。
                    }
                    else if (gd.Brush is SolidColorBrush scb && scb.Color.A == 0)
                    {
                        // 不可见图形。
                    }
                    else if (gd.Geometry is PathGeometry pg)
                    {
                        var path1 = currentGeometry;
                        var path2 = pg.ToString(CultureInfo.InvariantCulture);
                        currentGeometry = RecursiveCombine(path1, path2);
                    }
                }
            }
            if (currentGeometry is null)
            {
                return "";
            }
            else
            {
                var visitor = new PathTransformVisitor(drawingGroup.Transform);
                MiniLanguageParser.Visit(currentGeometry, visitor);
                return visitor.ToString();
            }
        }

        private static string RecursiveCombine(string? path1, string path2)
        {
            if (path1 is null)
            {
                return path2;
            }
            else
            {
                var mIndex = path2.IndexOf('M');
                if (mIndex > 0)
                {
                    path2 = path2[mIndex..];
                }

                return path1 + path2;
            }
        }
    }
}
