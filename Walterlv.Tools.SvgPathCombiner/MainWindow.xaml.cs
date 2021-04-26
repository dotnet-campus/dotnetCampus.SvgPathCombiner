using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Indentation;
using ICSharpCode.AvalonEdit.Search;

using SharpVectors.Converters;
using SharpVectors.Dom.Svg;
using SharpVectors.Renderers.Wpf;

namespace Walterlv.Tools
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            InitializeTextEditor(BeforeTextEditor);
        }

        private void BeforeBorder_DragOver(object sender, DragEventArgs e)
        {
            if (!TryGetSvgFileFromDataObject(e.Data, out _))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        private void BeforeBorder_Drop(object sender, DragEventArgs e)
        {
            if (TryGetSvgFileFromDataObject(e.Data, out var svgFile))
            {
                var text = File.ReadAllText(svgFile.FullName);

                var rawReader = new FileSvgReader(new WpfDrawingSettings())
                {
                    SaveXaml = false,
                };
                var reader = new FileSvgReader(new WpfDrawingSettings
                {
                    TextAsGeometry = true,
                    IncludeRuntime = false,
                    IgnoreRootViewbox = true,
                    EnsureViewboxSize = false,
                    EnsureViewboxPosition = false,
                    CultureInfo = CultureInfo.InvariantCulture,
                    OptimizePath = true,
                })
                {
                    SaveXaml = true,
                };
                var drawing = rawReader.Read(new StringReader(text));
                reader.Read(new StringReader(text));
                var xamlBuilder = new StringBuilder();
                reader.Save(new StringWriter(xamlBuilder));
                var xaml = Regex.Replace(xamlBuilder.ToString(), @" x:Name="".+?""", "");

                BeforeDrawingImage.Drawing = drawing;
                BeforeTextEditor.Text = xaml;

                if (SvgPathCombiner.TryGuessSvgDrawingViewbox(drawing, out var viewbox))
                {
                    BeforePathBorder.Width = viewbox.Width;
                    BeforePathBorder.Height = viewbox.Height;
                    AfterPathBorder.Width = viewbox.Width;
                    AfterPathBorder.Height = viewbox.Height;
                }
                else
                {
                    BeforePathBorder.Width = double.NaN;
                    BeforePathBorder.Height = double.NaN;
                    AfterPathBorder.Width = double.NaN;
                    AfterPathBorder.Height = double.NaN;
                }

                var pathData = SvgPathCombiner.Combine(drawing);
                AfterPath.Data = pathData;
                AfterTextEditor.Text = pathData.ToString(CultureInfo.InvariantCulture);
                AfterPathBorder.InvalidateProperty(Border.BorderBrushProperty);
            }
        }

        private void AfterTextEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AfterTextEditor.IsKeyboardFocusWithin)
            {
                try
                {
                    var pathData = Geometry.Parse(AfterTextEditor.Text);
                    AfterPath.Data = pathData;
                    AfterPathBorder.InvalidateProperty(Border.BorderBrushProperty);
                }
                catch (Exception ex)
                {
                    AfterPath.Data = null;
                    AfterPathBorder.SetCurrentValue(Border.BorderBrushProperty, Brushes.Red);
                }
            }
        }

        private bool TryGetSvgFileFromDataObject(IDataObject dataObject, [NotNullWhen(true)] out FileInfo? svgFile)
        {
            var fileDrop = dataObject.GetData(DataFormats.FileDrop);
            if (fileDrop is string[] fileArray
                && fileArray.Length > 0
                && string.Equals(Path.GetExtension(fileArray[0]), ".svg", StringComparison.OrdinalIgnoreCase))
            {
                svgFile = new FileInfo(fileArray[0]);
                return true;
            }
            svgFile = null;
            return false;
        }

        private void InitializeTextEditor(TextEditor textEditor)
        {
            textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("XML");
            TextEditorOptions options = textEditor.Options;
            if (options != null)
            {
                options.AllowScrollBelowDocument = true;
                options.EnableHyperlinks = true;
                options.EnableEmailHyperlinks = true;
                options.EnableVirtualSpace = true;
                options.HighlightCurrentLine = true;
                options.ShowSpaces = true;
                options.ShowTabs = true;
                options.ShowEndOfLine = true;
            }

            var foldingManager = FoldingManager.Install(textEditor.TextArea);
            var foldingStrategy = new XmlFoldingStrategy();
            textEditor.TextArea.IndentationStrategy = new DefaultIndentationStrategy();
            foldingStrategy.UpdateFoldings(foldingManager, textEditor.Document);
        }
    }
}
