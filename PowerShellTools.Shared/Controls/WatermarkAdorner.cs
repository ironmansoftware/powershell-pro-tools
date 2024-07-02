using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace PowerShellTools.Common.Controls
{
    /// <summary>
    /// An adorner for rendering watermark text over a control.
    /// </summary>
    public sealed class WatermarkAdorner : Adorner
    {
        private VisualCollection _visuals;
        private ContentPresenter _contentPresenter;

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="adornedElement">The control being adorned.</param>
        public WatermarkAdorner(FrameworkElement adornedElement)
            : base(adornedElement)
        {
            _visuals = new VisualCollection(this);

            // Setup the text box, which implements the watermark text.
            TextBox adorningTextBox = new TextBox()
            {
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(),
                IsHitTestVisible = false,
                Focusable = false,
                Text = Watermark.GetText(adornedElement),
                Foreground = Watermark.GetForeground(adornedElement),
                FontFamily = Watermark.GetFontFamily(adornedElement),
                FontSize = Watermark.GetFontSize(adornedElement),
                FontStretch = Watermark.GetFontStretch(adornedElement),
                FontStyle = Watermark.GetFontStyle(adornedElement),
                FontWeight = Watermark.GetFontWeight(adornedElement),
            };

            // Setup bindings to allow the watermark text to respond to changes in the adorned element.
            adorningTextBox.SetBinding(FrameworkElement.WidthProperty,
                new Binding("ActualWidth") { Source = adornedElement, Mode = BindingMode.OneWay });
            adorningTextBox.SetBinding(FrameworkElement.HeightProperty,
                new Binding("ActualHeight") { Source = adornedElement, Mode = BindingMode.OneWay });

            // Setup Control bindings to give the watermark more responsiveness to changes in the adorned element.
            if (adornedElement is Control)
            {
                adorningTextBox.SetBinding(Control.VerticalContentAlignmentProperty,
                    new Binding("VerticalContentAlignment") { Source = adornedElement });
                adorningTextBox.SetBinding(Control.PaddingProperty,
                    new Binding("Padding") { Source = adornedElement });
            }

            // Setup the content presenter that houses the control displaying the watermark text.
            _contentPresenter = new ContentPresenter()
            {
                IsHitTestVisible = false,
                Focusable = false,
                Content = adorningTextBox
            };
            _visuals.Add(_contentPresenter);

            // Setup the adorner.
            this.IsHitTestVisible = false;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Already documented in base class.")]
        protected override int VisualChildrenCount
        {
            get
            {
                return _visuals.Count;
            }
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Already documented in base class.")]
        protected override Visual GetVisualChild(int index)
        {
            return _visuals[index];
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Already documented in base class.")]
        protected override Size MeasureOverride(Size constraint)
        {
            _contentPresenter.Measure(constraint);
            return _contentPresenter.DesiredSize;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Already documented in base class.")]
        protected override Size ArrangeOverride(Size finalSize)
        {
            _contentPresenter.Arrange(new Rect(new Point(), finalSize));
            return new Size(_contentPresenter.ActualWidth, _contentPresenter.ActualHeight);
        }
    }
}
