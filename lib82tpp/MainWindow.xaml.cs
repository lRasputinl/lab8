using Microsoft.Win32;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace lib82tpp
{
    public partial class MainWindow : Window
    {
        private bool _isModified = false;

        public MainWindow()
        {
            InitializeComponent();
            CommandBindings.Add(new CommandBinding(ApplicationCommands.New, New_Click));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, Open_Click));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, Save_Click));
            MainTextBox.TextChanged += MainTextBox_TextChanged;
            UpdateLocalization();
        }
        private void MainTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _isModified = true;
        }


        private void New_Click(object sender, RoutedEventArgs e)
        {
            MainTextBox.Document.Blocks.Clear();
            _isModified = false;
        }


        private void Open_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "RTF files (*.rtf)|*.rtf" };
            if (dlg.ShowDialog() == true)
            {
                using var fs = new FileStream(dlg.FileName, FileMode.Open);
                new TextRange(MainTextBox.Document.ContentStart, MainTextBox.Document.ContentEnd).Load(fs, DataFormats.Rtf);
            }
            _isModified = false;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog { Filter = "RTF files (*.rtf)|*.rtf" };
            if (dlg.ShowDialog() == true)
            {
                using var fs = new FileStream(dlg.FileName, FileMode.Create);
                new TextRange(MainTextBox.Document.ContentStart, MainTextBox.Document.ContentEnd).Save(fs, DataFormats.Rtf);
            }
            _isModified = false;
        }

        private void Exit_Click(object sender, RoutedEventArgs e) => Close();

        private void BoldButton_Click(object sender, RoutedEventArgs e)
            => EditingCommands.ToggleBold.Execute(null, MainTextBox);

        private void ItalicButton_Click(object sender, RoutedEventArgs e)
            => EditingCommands.ToggleItalic.Execute(null, MainTextBox);

        private void UnderlineButton_Click(object sender, RoutedEventArgs e)
            => EditingCommands.ToggleUnderline.Execute(null, MainTextBox);

        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton button && button.Tag is string color)
            {
                var brush = (SolidColorBrush)new BrushConverter().ConvertFromString(color);
                var selection = new TextRange(MainTextBox.Selection.Start, MainTextBox.Selection.End);
                if (!selection.IsEmpty)
                {
                    selection.ApplyPropertyValue(TextElement.ForegroundProperty, brush);
                }
            }
        }

        private void ClearFormatting_Click(object sender, RoutedEventArgs e)
        {
            var selection = MainTextBox.Selection;
            if (!selection.IsEmpty)
            {
                selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
                selection.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Normal);
                selection.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
                selection.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
            }
        }

        private void MainTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var caret = MainTextBox.Selection.Start;
            var range = new TextRange(caret, caret);

            object weight = range.GetPropertyValue(TextElement.FontWeightProperty);
            BoldButton.IsChecked = (weight != DependencyProperty.UnsetValue) && weight.Equals(FontWeights.Bold);

            object style = range.GetPropertyValue(TextElement.FontStyleProperty);
            ItalicButton.IsChecked = (style != DependencyProperty.UnsetValue) && style.Equals(FontStyles.Italic);

            object decorations = range.GetPropertyValue(Inline.TextDecorationsProperty);
            UnderlineButton.IsChecked = decorations != DependencyProperty.UnsetValue &&
                                        decorations is TextDecorationCollection decs &&
                                        decs.Contains(TextDecorations.Underline[0]);

            object foreground = range.GetPropertyValue(TextElement.ForegroundProperty);
            if (foreground is SolidColorBrush brush)
                UpdateColorButtons(brush.Color);
        }


        private void UpdateColorButtons(Color color)
        {
            RedButton.IsChecked = color == Colors.Red;
            BlueButton.IsChecked = color == Colors.Blue;
            GreenButton.IsChecked = color == Colors.Green;
            BlackButton.IsChecked = color == Colors.Black;
        }

        private void Language_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && item.Tag is string cultureName)
            {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureName);
                Thread.CurrentThread.CurrentCulture = new CultureInfo(cultureName);
                UpdateLocalization();
            }
        }

        private void UpdateLocalization()
        {
            Title = Properties.Resources.WindowTitle;
            FileMenu.Header = Properties.Resources.FileMenu;
            NewMenuItem.Header = Properties.Resources.New;
            OpenMenuItem.Header = Properties.Resources.Open;
            SaveMenuItem.Header = Properties.Resources.Save;
            ExitMenuItem.Header = Properties.Resources.Exit;
            LanguageMenu.Header = Properties.Resources.Language;
        }
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (_isModified)
            {
                var result = MessageBox.Show(
                    "Документ содержит несохранённые изменения. Закрыть без сохранения?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
            }

            base.OnClosing(e);
        }

    }

}
