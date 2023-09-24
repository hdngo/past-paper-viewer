using AutoUpdaterDotNET;
using CefSharp;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Xceed.Wpf.Toolkit;
using MessageBox = System.Windows.MessageBox;

namespace PastPaperViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Initialization and declaration
        public ItemCollection ns_List;
        public List<ListViewItem> ns_SelectedItem;
        public ItemCollection qs_List;
        public List<ListViewItem> qs_SelectedItem;
        public List<string> files = new List<string>();
        public List<string> searchResults = new List<string>();
        public bool animationGoingOn = false;
        public static SolidColorBrush accentColor;
        public static SolidColorBrush boldColor;

        public MainWindow()
        {
            InitializeComponent();
            CheckForSavedSettings();

            AutoUpdater.HttpUserAgent = "PPViewerAutoUpdate";
            AutoUpdater.Start("https://rebrand.ly/pp-viewer-update");

            ToolTipService.ShowDurationProperty.OverrideMetadata(
                typeof(DependencyObject), new FrameworkPropertyMetadata(Int32.MaxValue));
        }

        private void RememberSettings(object sender, EventArgs e)
            => Properties.Settings.Default.Save();
        private void CheckForSavedSettings()
        {
            if (Properties.Settings.Default.DoubleLayout)
            {
                MyGridSplitter.Visibility = Visibility.Collapsed;
                Grid.SetColumnSpan(QuestionPaperCard, 3);
                Grid.SetColumnSpan(MarkSchemeCard, 3);
                Grid.SetColumn(MarkSchemeCard, 0);
                MarkSchemeCard.Margin = new Thickness(20);
                QuestionPaperCard.Margin = new Thickness(20);
                SwapButton.Visibility = Visibility.Visible;
                LayoutIcon.Kind = PackIconKind.Paper;
            }

            if (Properties.Settings.Default.DarkMode)
            {
                BigWindow.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#474749");
                MyGridSplitter.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#89898A");
                RightPanel.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#474749");
                ListOfFiles.Foreground = Brushes.White;
                ListOfFilesSearch.Foreground = Brushes.White;
                DarkModeIcon.Kind = PackIconKind.WeatherNight;
                QuestionPaperCard.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#89898A");
                MarkSchemeCard.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#89898A");
            }

            accentColor = FindResource("AccentColor") as SolidColorBrush;
            accentColor.Color = (Color)ColorConverter.ConvertFromString(Properties.Settings.Default.AccentColor);
            boldColor = FindResource("FadeOutColor") as SolidColorBrush;
            boldColor.Color = (Color)ColorConverter.ConvertFromString(Properties.Settings.Default.BoldColor);

            BrightColorCheck(accentColor);
        }

        // Effects
        private void EnterMouseOption(object sender, MouseEventArgs e)
        {
            Storyboard myStoryboard = (Storyboard)TryFindResource("OptionsOpen");
            myStoryboard.Completed += new EventHandler(CompletedWhenDone);
            if (!animationGoingOn)
            {
                animationGoingOn = true;
                myStoryboard.Begin(OptionsPanel);
            }
        }
        private void LeaveMouseOption(object sender, MouseEventArgs e)
        {
            Storyboard myStoryboard = (Storyboard)TryFindResource("OptionsClose");
            myStoryboard.Completed += new EventHandler(CompletedWhenDone);
            if (!animationGoingOn)
            {
                animationGoingOn = true;
                myStoryboard.Begin(OptionsPanel);
            }
        }
        private void CompletedWhenDone(object sender, EventArgs e)

        {
            animationGoingOn = false;
        }

        private void OnMouseButton(object sender, MouseEventArgs e) => (sender as Button).Opacity = 1;
        private void LeaveMouseButton(object sender, MouseEventArgs e) => (sender as Button).Opacity = 0.5;

        private void Media_Ended(object sender, EventArgs e)
        {
            (sender as MediaElement).Position = TimeSpan.Zero;
            //(sender as MediaElement).Play();
        }

        // Options
        private void ChangeSearch(object sender, RoutedEventArgs e)
        {
            var accentColor = FindResource("AccentColor") as SolidColorBrush;
            var boldColor = FindResource("FadeOutColor") as SolidColorBrush;

            if ((sender as Button).Name == "FirstMenu")
            {
                FirstMenu.BorderBrush = accentColor;
                FirstMenuBorder.Background = accentColor;
                SecondMenu.BorderBrush = boldColor;
                SecondMenuBorder.Background = boldColor;

                NameSearch.Visibility = Visibility.Visible;
                QuestionSearchGrid.Visibility = Visibility.Collapsed;
                YearSearch.Visibility = Visibility.Collapsed;
                Year.Visibility = Visibility.Visible;
                SessionSearch.Visibility = Visibility.Collapsed;
                Session.Visibility = Visibility.Visible;
                ListOfFiles.Visibility = Visibility.Visible;
                ListOfFilesSearch.Visibility = Visibility.Collapsed;
                SearchType.Visibility = Visibility.Collapsed;
                RandomButton.Visibility = Visibility.Visible;
                ShuffleIcon.Visibility = Visibility.Visible;
                FilterSearchText.Visibility = Visibility.Collapsed;
            }
            else
            {
                SecondMenu.BorderBrush = accentColor;
                SecondMenuBorder.Background = accentColor;
                FirstMenu.BorderBrush = boldColor;
                FirstMenuBorder.Background = boldColor;

                QuestionSearchGrid.Visibility = Visibility.Visible;
                NameSearch.Visibility = Visibility.Collapsed;
                YearSearch.Visibility = Visibility.Visible;
                Year.Visibility = Visibility.Collapsed;
                SessionSearch.Visibility = Visibility.Visible;
                Session.Visibility = Visibility.Collapsed;
                ListOfFiles.Visibility = Visibility.Collapsed;
                ListOfFilesSearch.Visibility = Visibility.Visible;
                SearchType.Visibility = Visibility.Visible;
                RandomButton.Visibility = Visibility.Collapsed;
                ShuffleIcon.Visibility = Visibility.Collapsed;
                FilterSearchText.Visibility = Visibility.Visible;

                // OptionsGrid.RowDefinitions.RemoveAt(3);
            }
        }
        private void ChangeButton(object sender, RoutedEventArgs e)
        {
            FakeOpenMenuButton.Visibility = Visibility.Collapsed;

            if ((sender as Button).Name == "OpenMenuButton")
            {
                OpenMenuButton.Visibility = Visibility.Hidden;
                CloseMenuButton.Visibility = Visibility.Visible;

                //ButtonGrid.Margin = new Thickness(0,0,0,0);
            }
            else
            {
                CloseMenuButton.Visibility = Visibility.Hidden;
                OpenMenuButton.Visibility = Visibility.Visible;

                //ButtonGrid.Margin = new Thickness(0, 0, 30, 0);
            }
        }
        private void ChangeLayout(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.DoubleLayout)
            {
                MyGridSplitter.Visibility = Visibility.Collapsed;
                Grid.SetColumnSpan(QuestionPaperCard, 3);
                Grid.SetColumnSpan(MarkSchemeCard, 3);
                Grid.SetColumn(MarkSchemeCard, 0);
                MarkSchemeCard.Margin = new Thickness(20);
                QuestionPaperCard.Margin = new Thickness(20);
                SwapButton.Visibility = Visibility.Visible;
                LayoutIcon.Kind = PackIconKind.Paper;
                Properties.Settings.Default.DoubleLayout = true;
            }
            else
            {
                MyGridSplitter.Visibility = Visibility.Visible;
                Grid.SetColumnSpan(QuestionPaperCard, 1);
                Grid.SetColumnSpan(MarkSchemeCard, 1);
                Grid.SetColumn(MarkSchemeCard, 2);
                MarkSchemeCard.Margin = new Thickness(10);
                QuestionPaperCard.Margin = new Thickness(10);
                SwapButton.Visibility = Visibility.Collapsed;
                LayoutIcon.Kind = PackIconKind.BoxShadow;
                Properties.Settings.Default.DoubleLayout = false;
            }

            Properties.Settings.Default.Save();
        }
        private void MoveMarkScheme(object sender, RoutedEventArgs e)
        {
            if (Panel.GetZIndex(QuestionPaperCard) == 2)
            {
                Panel.SetZIndex(QuestionPaperCard, 1);
                Panel.SetZIndex(MarkSchemeCard, 2);
            }
            else
            {
                Panel.SetZIndex(MarkSchemeCard, 1);
                Panel.SetZIndex(QuestionPaperCard, 2);
            }
        }
        private void ShowInfo(object sender, RoutedEventArgs e)
        {
            var infoWindow = new Information
            {
                Owner = this
            };
            infoWindow.Show();
        }
        private void ChangeDynamicColor(object sender, RoutedEventArgs e)
        {
            accentColor.Color = (Color)(sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor;

            int red = accentColor.Color.R * 2 / 4;
            int green = accentColor.Color.G * 2 / 4;
            int blue = accentColor.Color.B * 2 / 4;

            boldColor.Color = Color.FromRgb(Convert.ToByte(red), Convert.ToByte(green), Convert.ToByte(blue));

            Properties.Settings.Default.AccentColor = "#" + accentColor.Color.ToString().Substring(3);
            Properties.Settings.Default.BoldColor = "#" + boldColor.Color.ToString().Substring(3);

            BrightColorCheck(accentColor);
        }
        private void ColorButtonClick(object sender, RoutedEventArgs e)
        {
            CustomColorPicker.IsOpen = true;
        }
        private void ResetSettings(object sender, RoutedEventArgs e)
        {
            BrightnessSlider.Value = 0;

            Properties.Settings.Default.DoubleLayout = true;
            ChangeLayout(null, null);

            Properties.Settings.Default.DarkMode = true;
            ToggleNightMode(null, null);

            Properties.Settings.Default.Reset();
            CheckForSavedSettings();
            RememberSettings(null, null);
        }

        private void ToggleNightMode(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.DarkMode)
            {
                // Light to dark
                BigWindow.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#474749");
                MyGridSplitter.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#89898A");
                RightPanel.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#474749");
                ListOfFiles.Foreground = Brushes.White;
                ListOfFilesSearch.Foreground = Brushes.White;
                DarkModeIcon.Kind = PackIconKind.WeatherNight;
                QuestionPaperCard.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#89898A");
                MarkSchemeCard.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#89898A");

                Properties.Settings.Default.DarkMode = true;
            }
            else
            {
                // Dark to light
                BigWindow.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#E1E2E3");
                MyGridSplitter.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#D2D2D4");
                RightPanel.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#F7F7F8");
                ListOfFiles.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#DD000000");
                ListOfFilesSearch.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#DD000000");
                DarkModeIcon.Kind = PackIconKind.WeatherPartlyCloudy;
                QuestionPaperCard.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#D2D2D4");
                MarkSchemeCard.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#D2D2D4");

                Properties.Settings.Default.DarkMode = false;
            }

            Properties.Settings.Default.Save();
        }
        private void BrightColorCheck(SolidColorBrush colorBrush)
        {
            var c = colorBrush.Color;
            int brightness = (int)(c.R * 0.30 + c.G * 0.59 + c.B * 0.11);
            if (brightness < 160 && c.R != 255 && c.G != 255 || (c.B == 255 && c.R != 255 && c.G != 255))
            {
                // Light text
                SubjectChosen.Foreground = Brushes.White;
                Year.Foreground = Brushes.White;
                Session.Foreground = Brushes.White;
                NameSearch.Foreground = Brushes.White;
                QuestionSearch.Foreground = Brushes.White;
                ProgressText.Foreground = Brushes.White;
                YearSearch.Foreground = Brushes.White;
                SessionSearch.Foreground = Brushes.White;
                FilterSearchText.Foreground = Brushes.White;
                SearchType.Foreground = Brushes.White;
                PaperIcon.Foreground = Brushes.White;
                MagnifyIcon.Foreground = Brushes.White;
                ViewModeButton.Foreground = Brushes.White;
                NightModeButton.Foreground = Brushes.White;
                InfoButton.Foreground = Brushes.White;
                ColorPicker.Foreground = Brushes.White;
                ResetButton.Foreground = Brushes.White;
                BrightnessSlider.Foreground = Brushes.White;
                SwapButton.Foreground = Brushes.White;
            }
            else
            {
                // Dark text
                SubjectChosen.Foreground = Brushes.Black;
                Year.Foreground = Brushes.Black;
                Session.Foreground = Brushes.Black;
                NameSearch.Foreground = Brushes.Black;
                QuestionSearch.Foreground = Brushes.Black;
                ProgressText.Foreground = Brushes.Black;
                YearSearch.Foreground = Brushes.Black;
                SessionSearch.Foreground = Brushes.Black;
                FilterSearchText.Foreground = Brushes.Black;
                SearchType.Foreground = Brushes.Black;
                PaperIcon.Foreground = Brushes.Black;
                MagnifyIcon.Foreground = Brushes.Black;
                ViewModeButton.Foreground = Brushes.Black;
                NightModeButton.Foreground = Brushes.Black;
                InfoButton.Foreground = Brushes.Black;
                ColorPicker.Foreground = Brushes.Black;
                ResetButton.Foreground = Brushes.Black;
                BrightnessSlider.Foreground = Brushes.Black;
                SwapButton.Foreground = Brushes.Black;
            }
        }

        // Subject Choosing
        private void SubjectChanged(object sender, RoutedEventArgs e)
        {
            // Enable resources if they are not
            if (!Year.IsEnabled)
            {
                Year.IsEnabled = true;
                Session.IsEnabled = true;
                NameSearch.IsEnabled = true;
                QuestionSearch.IsEnabled = true;
                QuestionSearchButton.IsEnabled = true;
                YearSearch.IsEnabled = true;
                SessionSearch.IsEnabled = true;
                SearchType.IsEnabled = true;
                RandomButton.IsEnabled = true;
                ShuffleIcon.IsEnabled = true;
                FilterSearchText.IsEnabled = true;
            }

            // Reset and clear
            Year.SelectedIndex = 0;
            Session.SelectedIndex = 0;
            YearSearch.SelectedIndex = 0;
            SessionSearch.SelectedIndex = 0;
            SearchType.SelectedIndex = 0;
            NameSearch.Clear();

            // Get question paper files with subject code
            var subjectCode = GetSubjectCode();
            if (!Directory.Exists(@".\PastPapers\")) Directory.CreateDirectory(@".\PastPapers\");
            files = Directory.GetFiles(@".\PastPapers\", "*.pdf", SearchOption.AllDirectories).Where(x => x.Contains(subjectCode)).ToList();
            if (files.Count == 0)
            {
                PromptDownload(subjectCode);
            }

            // Display if exists
            ClearAndAddItems(files);
        }

        // Select Files
        private void ViewFiles(object sender, SelectionChangedEventArgs e)
        {
            if (ListOfFiles.SelectedItem != null)
            {
                var questionPaper = files.Where(x => x.Contains(ListOfFiles.SelectedValue.ToString().Replace("__", "_"))).FirstOrDefault();
                var markScheme = files.Where(x => x.Contains(ListOfFiles.SelectedValue.ToString().Replace("__", "_").Replace("qp", "ms"))).FirstOrDefault();

                QuestionPaperWindow.MenuHandler = new CustomMenuHandler();
                MarkSchemeWindow.MenuHandler = new CustomMenuHandler();

                QuestionPaperWindow.Address = $"{Directory.GetCurrentDirectory()}\\{questionPaper.Substring(1)}";
                MarkSchemeWindow.Address = $"{Directory.GetCurrentDirectory()}\\{markScheme.Substring(1)}";
            }
        }

        private void UpdateFiles(object sender, SelectionChangedEventArgs e) => UpdateFiles();
        private void UpdateFilesViaText(object sender, TextChangedEventArgs e) => UpdateFiles();

        private void UpdateFiles()
        {
            if (Year.SelectedItem != null && Session.SelectedItem != null)
            {
                // Get year and session
                string selectedYearCode = (Year.SelectedItem as ComboBoxItem).Content.ToString() != "All" ? (Year.SelectedItem as ComboBoxItem).Content.ToString().Substring(2, 2) + "_" : "_";
                string selectedSessionCode = (Session.SelectedItem as ComboBoxItem).Content.ToString() switch
                {
                    "March" => "_m",
                    "May/June" => "_s",
                    "Oct/Nov" => "_w",
                    _ => "_"
                };

                ClearAndAddItems(files.Where(x => x.Contains(selectedYearCode) && x.Contains(selectedSessionCode) && x.Contains(NameSearch.Text)).ToList());
            }
        }

        private void ClearAndAddItems(List<string> filesList)
        {
            ListOfFiles.Items.Clear();
            foreach (var file in filesList.Where(x => x.Contains("qp")))
            {
                ListOfFiles.Items.Add(file.Split('\\').Last().Replace("_", "__"));
            }
        }

        // Download if no files found
        private async void PromptDownload(string subjectCode)
        {
            string sMessageBoxText = "It seems that you don't have these past papers, would you like to download them?";
            string sCaption = "No past papers found";

            MessageBoxButton btnMessageBox = MessageBoxButton.YesNo;
            MessageBoxImage icnMessageBox = MessageBoxImage.Warning;

            MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);

            if (rsltMessageBox.Equals(MessageBoxResult.Yes))
            {
                string downloadLink = String.Empty;
                if (subjectCode == "9231") downloadLink = "https://dl.dropbox.com/s/uff2qv1dmw0g8gc/FurtherMaths.zip?dl=1";
                else if (subjectCode == "9608") downloadLink = "https://dl.dropbox.com/s/uttjnk64y7l7jyl/ComputerScience.zip?dl=1";
                else if (subjectCode == "9609") downloadLink = "https://dl.dropbox.com/s/cr7lylyhk08uzx4/Business.zip?dl=1";
                else if (subjectCode == "9700") downloadLink = "https://dl.dropbox.com/s/bnyarae91j6hawg/Biology.zip?dl=1";
                else if (subjectCode == "9701") downloadLink = "https://dl.dropbox.com/s/q62c0phopyo9fod/Chemistry.zip?dl=1";
                else if (subjectCode == "9702") downloadLink = "https://dl.dropbox.com/s/z2sdxzyvp76bski/Physics.zip?dl=1";
                else if (subjectCode == "9708") downloadLink = "https://dl.dropbox.com/s/ii2w9jocm24cqaz/Economics.zip?dl=1";
                else if (subjectCode == "9709") downloadLink = "https://dl.dropbox.com/s/14r7d520o68h9lb/Maths.zip?dl=1";

                await Task.Run(() => DownloadFile(downloadLink));
            }
        }

        private void DownloadFile(string downloadLink)
        {
            ToggleSearchButton();
            SetProgressText("Downloading...");
            using WebClient wc = new WebClient();
            SetProgressValue(0, 100);
            wc.DownloadProgressChanged += ChangedDownloadProgress;
            wc.DownloadFileCompleted += ExtractFile();
            wc.DownloadFileAsync(new Uri(downloadLink), $"{System.IO.Path.GetTempPath()}\\pp.zip");
        }

        private void ChangedDownloadProgress(object sender, DownloadProgressChangedEventArgs e) => SetProgressValue(e.ProgressPercentage);
        private AsyncCompletedEventHandler ExtractFile()
        {
            return new AsyncCompletedEventHandler((sender, e) =>
            {
                SetProgressText("Extracting...");

                using (ZipArchive archive = ZipFile.OpenRead($"{System.IO.Path.GetTempPath()}\\pp.zip")) archive.ExtractToDirectory(".\\PastPapers\\");

                File.Delete($"{System.IO.Path.GetTempPath()}\\pp.zip");

                SetProgressText("Finished!");

                var subjectCode = GetSubjectCode();
                files = Directory.GetFiles(@".\PastPapers\", "*.pdf", SearchOption.AllDirectories).Where(x => x.Contains(subjectCode)).ToList();
                foreach (var file in files.Where(x => x.Contains("qp")))
                {
                    ListOfFiles.Dispatcher.Invoke(() => ListOfFiles.Items.Add(file.Split('\\').Last().Replace("_", "__")));
                }

                MessageBox.Show("Download complete!");
                ToggleSearchButton();
            });
        }

        private string GetSubjectCode() => SubjectChosen.Dispatcher.Invoke(() => (SubjectChosen.SelectedItem as ComboBoxItem).Content.ToString().Substring(0, 4));
        private void SetProgressText(string text) => ProgressText.Dispatcher.Invoke(() => ProgressText.Text = text);
        private void SetProgressValue(int value, int maxValue = 0)
        {
            if (maxValue != 0) MyProgressBar.Dispatcher.Invoke(() => MyProgressBar.Maximum = maxValue);
            MyProgressBar.Dispatcher.Invoke(() => MyProgressBar.Value = value);
        }

        // Searching 
        private async void StartSearch(object sender, RoutedEventArgs e)
        {
            ToggleSearchButton();
            ListOfFilesSearch.Dispatcher.Invoke(() => ListOfFilesSearch.Items.Clear());
            await Task.Run(() => LongProcedure());

            // Search completed, send message to user
            if (ListOfFilesSearch.Items.Count == 0)
                MessageBox.Show("Search returned no results.");
            else
                MessageBox.Show($"Search completed! Found {ListOfFilesSearch.Items.Count} results!");

            searchResults.Clear();
            for (int i = 0; i < ListOfFilesSearch.Items.Count; i++) searchResults.Add(ListOfFilesSearch.Items[i].ToString());
        }

        private void LongProcedure()
        {
            var criteria = ObtainNecessaryData();

            // Get necessary data and filter
            var filteredFilesList = files.Where(x => x.Contains(criteria[0]) && x.Contains(criteria[1]) && x.Contains(criteria[2]));
            if (filteredFilesList.Count() == 0)
            {
                MessageBox.Show("No file to search... Maybe your subject does not have the papers in that year/session?");
                ToggleSearchButton();
                return;
            }

            if (String.IsNullOrWhiteSpace(criteria[3])) {
                ToggleSearchButton();
                return;
            }
            else criteria[3] = criteria[3].Trim().ToLower().Replace("\n", "").Replace(" ", "");
            var termsIncludeOr = new List<string>();
            var termsIncludeAnd = new List<string>();
            var termsExclude = new List<string>();

            if (!(criteria[3].Contains('{') || criteria[3].Contains('[') || criteria[3].Contains('(')))
                termsIncludeOr.Add(criteria[3]);
            else
            {
                if (criteria[3].Split('{').Count() != 0)
                    termsExclude = GetTermsBySeparator(criteria[3], '{', '}');
                if (criteria[3].Split('{').Count() != 0)
                    termsIncludeAnd = GetTermsBySeparator(criteria[3], '(', ')');
                if (criteria[3].Split('[').Count() != 0)
                    termsIncludeOr = GetTermsBySeparator(criteria[3], '[', ']');
            }

            // Enable checks based on terms found
            bool orCheck = termsIncludeOr.Count != 0;
            bool andCheck = termsIncludeAnd.Count != 0;
            bool excludeCheck = termsExclude.Count != 0;

            // Commence search
            SetProgressValue(0, filteredFilesList.Count());

            for (var i = 0; i < filteredFilesList.Count(); i++)
            {
                SetProgressValue(i + 1);
                SetProgressText($"Searched {i + 1} / {filteredFilesList.Count()}");

                var searchPass = PastPaperSearch(filteredFilesList.ElementAt(i), termsIncludeOr, termsIncludeAnd, termsExclude, orCheck, andCheck, excludeCheck);

                if (searchPass.Key)
                {
                    ListOfFilesSearch.Dispatcher.Invoke(()
                        => ListOfFilesSearch.Items.Add($"{filteredFilesList.ElementAt(i).Split('\\').Last().Replace("_", "__")}{(searchPass.Value != -1 ? $" - Page {searchPass.Value}" : "")}"));
                }
            }

            ToggleSearchButton();
        }

        private List<string> ObtainNecessaryData()
        {
            return new List<string> {
                    GetYearSearchCode(),
                    GetSessionSearchCode(),
                    GetSearchType(),
                    GetSearchTerm().ToLower()
                };
        }
        private KeyValuePair<bool, int> PastPaperSearch(string filePath, List<string> termsIncludeOr, List<string> termsIncludeAnd, List<string> termsToExclude, bool orCheck, bool andCheck, bool excludeCheck)
        {
            using PdfReader pdfReader = new PdfReader(filePath);
            using var pdfDoc = new PdfDocument(pdfReader);

            var tempList = new List<string>();
            foreach (var term in termsIncludeAnd) tempList.Add(term);
            var repList = new List<string>();
            foreach (var term in termsIncludeAnd) repList.Add(term);

            for (int page = 2; page <= pdfDoc.GetNumberOfPages(); page++)
            {
                try
                {
                    // Read page, replace out common misreds
                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                    string currentPageText = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(page), strategy).Trim().ToLower().Replace("\n", "")
                        .Replace(" ", "")
                        .Replace("ﬁ", "fi")
                        .Replace("ĳ", "ij")
                        .Replace("œ", "oe")
                        .Replace("ﬀ", "ff")
                        .Replace("ﬂ", "fl")
                        .Replace("ﬅ", "ft")
                        .Replace("ﬃ", "ffi")
                        .Replace("ﬄ", "ffl")
                        .Replace("ﬆ", "st");

                    // Check for excluding words
                        if (excludeCheck)
                    {
                        foreach (var term in termsToExclude)
                            if (currentPageText.Contains(term)) return new KeyValuePair<bool, int>(false, page);

                        if (!orCheck && !andCheck) return new KeyValuePair<bool, int>(true, -1);
                    }

                    // Check for or words
                    if (orCheck)
                    {
                        foreach (var term in termsIncludeOr)
                            if (currentPageText.Contains(term)) return new KeyValuePair<bool, int>(true, page);
                    }

                    // Check for and words
                    if (andCheck && termsIncludeAnd.Count() != 0)
                    {
                        foreach (var term in tempList)
                            if (currentPageText.Contains(term))
                                try
                                {
                                    repList.Remove(term);
                                }
                                catch { }

                        if (andCheck && repList.Count == 0) return new KeyValuePair<bool, int>(true, page);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    MessageBox.Show(e.StackTrace);
                    MessageBox.Show(filePath);
                    continue;
                }
            }
            
            return new KeyValuePair<bool, int>(false, -1);
        }


        private List<string> GetTermsBySeparator(string searchTerm, char separator1, char separator2)
        {
            var terms = new List<string>();
            var splitByBracket = searchTerm.Split(separator1);

            foreach (string term in splitByBracket.Skip(1).Take(splitByBracket.Count()))
                terms.Add(term.Substring(0, term.IndexOf(separator2)));

            return terms;
        }
        private string GetSearchType() => SearchType.Dispatcher.Invoke(() =>
        {
            return SearchType.SelectedIndex == 0 ? "qp" : "ms";
        });
        private string GetSearchTerm() => YearSearch.Dispatcher.Invoke(() =>
        {
            return QuestionSearch.Text;
        });
        private string GetYearSearchCode() => YearSearch.Dispatcher.Invoke(() =>
        {
            return (YearSearch.SelectedItem as ComboBoxItem).Content.ToString() != "All" ? (YearSearch.SelectedItem as ComboBoxItem).Content.ToString().Substring(2, 2) + "_" : "_";
        });
        private string GetSessionSearchCode() => SessionSearch.Dispatcher.Invoke(() =>
        {
            return (SessionSearch.SelectedItem as ComboBoxItem).Content.ToString() switch
            {
                "March" => "_m",
                "May/June" => "_s",
                "Oct/Nov" => "_w",
                _ => "_"
            };
        });
        private void ToggleSearchButton() => QuestionSearchButton.Dispatcher.Invoke(() =>
        {
            QuestionSearchButton.IsEnabled = QuestionSearchButton.IsEnabled ? false : true;
        });

        private void ViewFilesSearch(object sender, SelectionChangedEventArgs e)
        {
            if (ListOfFilesSearch.SelectedItem != null)
            {
                var selectedValue = ListOfFilesSearch.SelectedValue.ToString().Replace("__", "_");
                string questionPaper;
                string markScheme;

                if (selectedValue.Contains("-"))
                {
                    var splitValue = selectedValue.Split('-');

                    if (selectedValue.Contains("qp"))
                    {
                        questionPaper = files.Where(x => x.Contains(splitValue.First().Trim())).FirstOrDefault();
                        markScheme = files.Where(x => x.Contains(splitValue.First().Trim().Replace("qp", "ms"))).FirstOrDefault();

                        QuestionPaperWindow.Address = $"{Directory.GetCurrentDirectory()}\\{questionPaper.Substring(1)}#page={splitValue.Last().Substring(6)}";
                        MarkSchemeWindow.Address = $"{Directory.GetCurrentDirectory()}\\{markScheme.Substring(1)}";
                    }
                    else
                    {
                        markScheme = files.Where(x => x.Contains(splitValue.First().Trim())).FirstOrDefault();
                        questionPaper = files.Where(x => x.Contains(splitValue.First().Trim().Replace("ms", "qp"))).FirstOrDefault();

                        QuestionPaperWindow.Address = $"{Directory.GetCurrentDirectory()}\\{questionPaper.Substring(1)}";
                        MarkSchemeWindow.Address = $"{Directory.GetCurrentDirectory()}\\{markScheme.Substring(1)}#page={splitValue.Last().Substring(6)}";
                    }
                }
                else
                {
                    if (selectedValue.Contains("qp"))
                    {
                        questionPaper = files.Where(x => x.Contains(selectedValue)).FirstOrDefault();
                        markScheme = files.Where(x => x.Contains(selectedValue.Replace("qp", "ms"))).FirstOrDefault();
                    }
                    else
                    {
                        markScheme = files.Where(x => x.Contains(selectedValue)).FirstOrDefault();
                        questionPaper = files.Where(x => x.Contains(selectedValue.Replace("ms", "qp"))).FirstOrDefault();
                    }

                    QuestionPaperWindow.Address = $"{Directory.GetCurrentDirectory()}\\{questionPaper.Substring(1)}";
                    MarkSchemeWindow.Address = $"{Directory.GetCurrentDirectory()}\\{markScheme.Substring(1)}";
                }

            }
        }

        private void UpdateFilesViaTextSearch(object sender, TextChangedEventArgs e) 
        {
            try
            {
                ListOfFilesSearch.Items.Clear();

                foreach (var item in searchResults)
                {
                    if (item.Contains(FilterSearchText.Text.Replace("_", "__"))) ListOfFilesSearch.Items.Add(item);
                }
            }
            catch (Exception err) { MessageBox.Show(err.Message + "\n============" + err.StackTrace); }
        }

        private void OnEnter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                StartSearch(null, null);
        }

        // Viewer + search related
        private void OpenRandomPaper(object sender, RoutedEventArgs e)
        {
            if (ListOfFiles.Items.Count != 0)
            {
                var rnd = new Random().Next(0, ListOfFiles.Items.Count);
                ListOfFiles.SelectedIndex = rnd;
            }
        }

        // Debugging and miscs
    }
}
