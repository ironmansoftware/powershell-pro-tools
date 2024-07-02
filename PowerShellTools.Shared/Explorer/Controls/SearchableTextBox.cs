using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace PowerShellTools.Explorer
{
    public class SearchableTextBox : TextBox
    {
        public static readonly DependencyProperty HighlightBackgroundProperty =
            DependencyProperty.Register("HighlightBackground", typeof(Brush), typeof(SearchableTextBox),
            new UIPropertyMetadata(Brushes.Yellow, UpdateControlCallBack));

        public static readonly DependencyProperty HighlightForegroundProperty =
            DependencyProperty.Register("HighlightForeground", typeof(Brush), typeof(SearchableTextBox),
            new UIPropertyMetadata(Brushes.Black, UpdateControlCallBack));

        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register("SearchText", typeof(string), typeof(SearchableTextBox),
            new UIPropertyMetadata(string.Empty, UpdateControlCallBack));

        public static readonly DependencyProperty SearchResultCountProperty =
            DependencyProperty.Register("SearchResultCount", typeof(int), typeof(SearchableTextBox),
            new UIPropertyMetadata(0)); 

        static SearchableTextBox()
        {
            //Type ownerType = typeof(SearchableTextBox);
            //DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
        }

        public Brush HighlightBackground
        {
            get { return (Brush)GetValue(HighlightBackgroundProperty); }
            set { SetValue(HighlightBackgroundProperty, value); }
        }

        public Brush HighlightForeground
        {
            get { return (Brush)GetValue(HighlightForegroundProperty); }
            set { SetValue(HighlightForegroundProperty, value); }
        }

        public string SearchText
        {
            get { return (string)GetValue(SearchTextProperty); }
            set { SetValue(SearchTextProperty, value); }
        }

        public int SearchResultCount
        {
            get { return (int)GetValue(SearchResultCountProperty); }
            private set { SetValue(SearchResultCountProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            this.TextChanged += OnSearchableTextBoxTextChanged;
            base.OnApplyTemplate();
        }

        private void OnSearchableTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            SearchableTextBox control = sender as SearchableTextBox;
            if (control != null)
            {
                control.InvalidateVisual();
            }
        }

        private static void UpdateControlCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SearchableTextBox control = d as SearchableTextBox;
            if (control != null)
            {
                control.InvalidateVisual();
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            string searchstring = this.SearchText.ToLowerInvariant();
            string compareText = this.Text.ToLowerInvariant();
            string displayText = this.Text;
            int resultCount = 0;

            TextBlock displayTextBlock = this.Template.FindName("PART_TEXT", this) as TextBlock;

            if (displayTextBlock == null && 
                string.IsNullOrWhiteSpace(this.Text) && 
                string.IsNullOrWhiteSpace(this.SearchText))
            {
                this.SearchResultCount = resultCount;
                base.OnRender(drawingContext);

                return;
            }

            displayTextBlock.Inlines.Clear();

            Run run = null;
            while (!string.IsNullOrEmpty(searchstring) && compareText.IndexOf(searchstring) >= 0)
            {
                int position = compareText.IndexOf(searchstring);
                run = GenerateRun(displayText.Substring(0, position), false);

                if (run != null)
                {
                    displayTextBlock.Inlines.Add(run);
                }

                run = GenerateRun(displayText.Substring(position, searchstring.Length), true);

                if (run != null)
                {
                    displayTextBlock.Inlines.Add(run);
                    resultCount++;
                }

                compareText = compareText.Substring(position + searchstring.Length);
                displayText = displayText.Substring(position + searchstring.Length);
            }

            run = GenerateRun(displayText, false);

            if (run != null)
            {
                displayTextBlock.Inlines.Add(run);
            }

            this.SearchResultCount = resultCount;
            base.OnRender(drawingContext);
        }

        private Run GenerateRun(string searchedString, bool isHighlight)
        {
            if (!string.IsNullOrEmpty(searchedString))
            {
                Run run = new Run(searchedString)
                {
                    Background = isHighlight ? this.HighlightBackground : this.Background,
                    Foreground = isHighlight ? this.HighlightForeground : this.Foreground,
                };
                return run;
            }
            return null;
        } 
    }
}
