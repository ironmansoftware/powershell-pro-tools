using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace PowerShellTools.Common.Controls
{
    /// <summary>
    /// Provides attached properties for overlaying text on controls.
    /// This is useful for displaying watermark text to prompt the user for input.
    /// </summary>
    public sealed class Watermark : DependencyObject
    {
        #region Attached Properties

        #region Text Property

        /// <summary>
        /// The text to be overlain on the attached control.
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached(
            "Text", typeof(String), typeof(Watermark),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnPropertyChanged));

        /// <summary>
        /// Gets the watermark text.
        /// </summary>
        /// <param name="element">The control the property is attached to.</param>
        /// <returns>The watermark text.</returns>
        public static string GetText(FrameworkElement element)
        {
            return (string)element.GetValue(TextProperty);
        }

        /// <summary>
        /// Sets the watermark text.
        /// </summary>
        /// <param name="element">The control the property is attached to.</param>
        /// <param name="value">The text to be used as the watermark.</param>
        public static void SetText(FrameworkElement element, string value)
        {
            element.SetValue(TextProperty, value);
        }

        #endregion

        #region Foreground Property

        /// <summary>
        /// The brush used for the watermark text (defaults to SystemColors.GrayTextBrush).
        /// </summary>
        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.RegisterAttached(
            "Foreground", typeof(Brush), typeof(Watermark),
            new FrameworkPropertyMetadata(SystemColors.GrayTextBrush, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Gets the brush of the watermark text.
        /// </summary>
        /// <param name="element">The control the property is attached to.</param>
        /// <returns>The watermark text brush.</returns>
        public static Brush GetForeground(FrameworkElement element)
        {
            return (Brush)element.GetValue(ForegroundProperty);
        }

        /// <summary>
        /// Sets the brush of the watermark text.
        /// </summary>
        /// <param name="element">The control the property is attached to.</param>
        /// <param name="value">The brush to be used for the watermark text.</param>
        public static void SetForeground(FrameworkElement element, Brush value)
        {
            element.SetValue(ForegroundProperty, value);
        }

        #endregion

        #region Font Family Property

        /// <summary>
        /// The font family for the watermark text (defaults to the default value for TextElement).
        /// </summary>
        public static readonly DependencyProperty FontFamilyProperty = DependencyProperty.RegisterAttached(
            "FontFamily", typeof(FontFamily), typeof(Watermark),
            new FrameworkPropertyMetadata(TextElement.FontFamilyProperty.DefaultMetadata.DefaultValue, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Gets the font family for the watermark text.
        /// </summary>
        /// <param name="element">The control the property is attached to.</param>
        /// <returns>The watermark text font family.</returns>
        public static FontFamily GetFontFamily(FrameworkElement element)
        {
            return (FontFamily)element.GetValue(FontFamilyProperty);
        }

        /// <summary>
        /// Sets the font family for the watermark text.
        /// </summary>
        /// <param name="element">The control the property is attached to.</param>
        /// <param name="value">The font family to be used for the watermark text.</param>
        public static void SetFontFamily(FrameworkElement element, FontFamily value)
        {
            element.SetValue(FontFamilyProperty, value);
        }

        #endregion

        #region Font Stretch Property

        /// <summary>
        /// The font stretch for the watermark text.
        /// </summary>
        public static readonly DependencyProperty FontStretchProperty = DependencyProperty.RegisterAttached(
            "FontStretch", typeof(FontStretch), typeof(Watermark),
            new FrameworkPropertyMetadata(FontStretches.Normal, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Gets the font stretch for the watermark text.
        /// </summary>
        /// <param name="element">The control the property is attached to.</param>
        /// <returns>The watermark text font stretch.</returns>
        public static FontStretch GetFontStretch(FrameworkElement element)
        {
            return (FontStretch)element.GetValue(FontStretchProperty);
        }

        /// <summary>
        /// Sets the font stretch for the watermark text.
        /// </summary>
        /// <param name="element">The control the property is attached to.</param>
        /// <param name="value">The font stretch to be used for the watermark text.</param>
        public static void SetFontStretch(FrameworkElement element, FontStretch value)
        {
            element.SetValue(FontStretchProperty, value);
        }

        #endregion

        #region Font Style Property

        /// <summary>
        /// The font style for the watermark text.
        /// </summary>
        public static readonly DependencyProperty FontStyleProperty = DependencyProperty.RegisterAttached(
            "FontStyle", typeof(FontStyle), typeof(Watermark),
            new FrameworkPropertyMetadata(FontStyles.Normal, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Gets the font style for the watermark text.
        /// </summary>
        /// <param name="element">The control the property is attached to.</param>
        /// <returns>The watermark font style.</returns>
        public static FontStyle GetFontStyle(FrameworkElement element)
        {
            return (FontStyle)element.GetValue(FontStyleProperty);
        }

        /// <summary>
        /// Sets the font style for the watermark text.
        /// </summary>
        /// <param name="element">The control the property is attached to.</param>
        /// <param name="value">The font style to be used for the watermark text.</param>
        public static void SetFontStyle(FrameworkElement element, FontStyle value)
        {
            element.SetValue(FontStyleProperty, value);
        }

        #endregion

        #region Font Size Property

        /// <summary>
        /// The font size for the watermark text (defaults to the default value for TextElement).
        /// </summary>
        public static readonly DependencyProperty FontSizeProperty = DependencyProperty.RegisterAttached(
            "FontSize", typeof(double), typeof(Watermark),
            new FrameworkPropertyMetadata(TextElement.FontSizeProperty.DefaultMetadata.DefaultValue, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Gets the font size for the watermark text.
        /// </summary>
        /// <param name="element">The control the property is attached to.</param>
        /// <returns>The watermark text font size.</returns>
        public static double GetFontSize(FrameworkElement element)
        {
            return (double)element.GetValue(FontSizeProperty);
        }

        /// <summary>
        /// Sets the font size for the watermark text.
        /// </summary>
        /// <param name="element">The control the property is attached to.</param>
        /// <param name="value">The font size to be used for the watermark text.</param>
        public static void SetFontSize(FrameworkElement element, double value)
        {
            element.SetValue(FontSizeProperty, value);
        }

        #endregion

        #region Font Weight Property

        /// <summary>
        /// The font weight for the watermark text.
        /// </summary>
        public static readonly DependencyProperty FontWeightProperty = DependencyProperty.RegisterAttached(
            "FontWeight", typeof(FontWeight), typeof(Watermark),
            new FrameworkPropertyMetadata(FontWeights.Normal, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Gets the font weight for the watermark text.
        /// </summary>
        /// <param name="element">The control the property is attached to.</param>
        /// <returns>The watermark text font weight.</returns>
        public static FontWeight GetFontWeight(FrameworkElement element)
        {
            return (FontWeight)element.GetValue(FontWeightProperty);
        }

        /// <summary>
        /// Sets the font weight for the watermark text.
        /// </summary>
        /// <param name="element">The control the property is attached to.</param>
        /// <param name="value">The font weight to be used for the watermark text.</param>
        public static void SetFontWeight(FrameworkElement element, FontWeight value)
        {
            element.SetValue(FontWeightProperty, value);
        }

        #endregion

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the watermark text being set.
        /// When the watermark text is set on an element, this method hooks up a watermark adorner to the element.
        /// </summary>
        /// <param name="d">The object whose watermark text is being set (should be a <see cref="FrameworkElement"/>)</param>
        /// <param name="e">Event arguments</param>
        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)d;
            element.GotFocus += HandleHideWatermarkEvent;
            element.LostFocus += HandleShowWatermarkEvent;
            element.Loaded += HandleShowWatermarkEvent;

            if (element is Control || element is TextBlock)
            {
                // Set the watermark font family to the VS environment font family if it has not been set.
                if (DependencyPropertyHelper.GetValueSource(element, FontFamilyProperty).BaseValueSource == BaseValueSource.Default)
                {
                    element.SetBinding(FontFamilyProperty, new Binding("FontFamily") { Source = element, Mode = BindingMode.OneWay });
                }

                // Set the watermark font size to the VS environment font size if it has not been set.
                if (DependencyPropertyHelper.GetValueSource(element, FontSizeProperty).BaseValueSource == BaseValueSource.Default)
                {
                    element.SetBinding(FontSizeProperty, new Binding("FontSize") { Source = element, Mode = BindingMode.OneWay });
                }
            }

            if (element is TextBoxBase)
            {
                ((TextBoxBase)element).TextChanged += HandleShowWatermarkEvent;
            }

            if (element is Selector)
            {
                ((Selector)element).SelectionChanged += HandleShowWatermarkEvent;
            }
        }

        /// <summary>
        /// Handles events in which a watermark should be hidden.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private static void HandleHideWatermarkEvent(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            HideWatermark(element);
        }

        /// <summary>
        /// Handles events in which a watermark's visibility should change.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private static void HandleShowWatermarkEvent(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            if (ShouldShowWatermark(element))
            {
                ShowWatermark(element);
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Makes the watermark adorner visible.
        /// </summary>
        /// <param name="element">Element with the watermark</param>
        private static void ShowWatermark(FrameworkElement element)
        {
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(element);
            if (adornerLayer != null)
            {
                // Check for existing adorners.
                var adorners = adornerLayer.GetAdorners(element);
                if (adorners != null)
                {
                    // This assures only one watermark adorner is present.
                    var adorner = adorners.OfType<WatermarkAdorner>().SingleOrDefault();
                    if (adorner != null)
                    {
                        adorner.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    adornerLayer.Add(new WatermarkAdorner(element));
                }
            }
        }

        /// <summary>
        /// Makes the watermark adorner invisible.
        /// </summary>
        /// <param name="element">The element with the watermark</param>
        private static void HideWatermark(FrameworkElement element)
        {
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(element);

            if (adornerLayer != null)
            {
                // Search for existing adorners.
                Adorner[] adorners = adornerLayer.GetAdorners(element);
                if (adorners != null)
                {
                    // We only expect one watermark adorner to be present.
                    var adorner = adorners.OfType<WatermarkAdorner>().SingleOrDefault();
                    if (adorner != null)
                    {
                        adornerLayer.Remove(adorner);
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether a watermark should be shown base on the current state of the specified element.
        /// </summary>
        /// <param name="element">A <see cref="ComboBox"/> object</param>
        /// <returns>True if a watermark should be shown, false if otherwise</returns>
        private static bool ShouldShowWatermark(ComboBox element)
        {
            // A watermark should not be shown if an item has been selected.
            if (element.SelectedItem != null)
            {
                return false;
            }

            // A watermark should not be shown if an editable ComboBox has text.
            if (element.IsEditable)
            {
                return element.Text == String.Empty;
            }

            return true;
        }

        /// <summary>
        /// Determines whether a watermark should be shown base on the current state of the specified element.
        /// </summary>
        /// <param name="element">A <see cref="ItemsControl"/> object</param>
        /// <returns>True if a watermark should be shown, false if otherwise</returns>
        private static bool ShouldShowWatermark(ItemsControl element)
        {
            return element.Items.Count == 0;
        }

        /// <summary>
        /// Determines whether a watermark should be shown base on the current state of the specified element.
        /// </summary>
        /// <param name="element">A <see cref="TextBlock"/> object</param>
        /// <returns>True if a watermark should be shown, false if otherwise</returns>
        private static bool ShouldShowWatermark(TextBlock element)
        {
            return element.Text == String.Empty;
        }

        /// <summary>
        /// Determines whether a watermark should be shown base on the current state of the specified element.
        /// </summary>
        /// <param name="element">A <see cref="TextBox"/> object</param>
        /// <returns>True if a watermark should be shown, false if otherwise</returns>
        private static bool ShouldShowWatermark(TextBox element)
        {
            if (element.IsFocused)
            {
                return false;
            }
            else
            {
                return element.Text == String.Empty;
            }
        }

        /// Determines whether a watermark should be shown base on the current state of the specified element.
        /// </summary>
        /// <param name="element">A <see cref="PasswordBox"/> object</param>
        /// <returns>True if a watermark should be shown, false if otherwise</returns>
        private static bool ShouldShowWatermark(PasswordBox element)
        {
            return element.SecurePassword.IsEmptyOrNull();
        }

        /// <summary>
        /// Determines whether a watermark should be shown base on the current state of the specified element.
        /// </summary>
        /// <param name="element">A <see cref="FrameworkElement"/> object</param>
        /// <returns>True if a watermark should be shown, false if otherwise</returns>
        private static bool ShouldShowWatermark(FrameworkElement element)
        {
            if (element is ComboBox)
            {
                return ShouldShowWatermark((ComboBox)element);
            }
            else if (element is TextBlock)
            {
                return ShouldShowWatermark((TextBlock)element);
            }
            else if (element is TextBox)
            {
                return ShouldShowWatermark((TextBox)element);
            }
            else if (element is ItemsControl)
            {
                return ShouldShowWatermark((ItemsControl)element);
            }
            else if (element is PasswordBox)
            {
                return ShouldShowWatermark((PasswordBox)element);
            }
            else
            {
                return false;
            }
        }

        #endregion
    }
}
