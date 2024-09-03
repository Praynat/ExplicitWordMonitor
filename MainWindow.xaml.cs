using ExplicitWordMonitor.HomePage;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace ExplicitWordMonitor
{
    public partial class MainWindow : Window
    {
        private WordFilterHomePage _homePage;
        private string dontShowFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dontShowAgain.txt");

        public MainWindow()
        {
            InitializeComponent();
            _homePage = new WordFilterHomePage();
            mainFrame.Navigate(_homePage);
            this.Closing += MainWindow_Closing;

            bool dontShow = DontShowAgain();
            MessageBox.Show(dontShow.ToString());
            // Hide the popup if the user has chosen "Don't show again"
            if (dontShow)
            {
                infoPopup.IsOpen = false;
            }
        }

        private bool DontShowAgain()
        {
            return File.Exists(dontShowFilePath) && File.ReadAllText(dontShowFilePath) == "true";
        }

        private void ClosePopup_Click(object sender, RoutedEventArgs e)
        {
            // Hide the popup
            infoPopup.IsOpen = false;

            // Save the "Don't show again" preference if checked
            if (chkDontShowAgain.IsChecked == true)
            {
                File.WriteAllText(dontShowFilePath, "true");
                MessageBox.Show("You won't see this popup again.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!AskForPassword())
            {
                e.Cancel = true;
            }
        }

        private bool AskForPassword()
        {
            Window passwordWindow = new Window
            {
                Title = "Enter Password",
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };

            StackPanel panel = new StackPanel { Margin = new Thickness(10) };
            PasswordBox passwordBox = new PasswordBox { Width = 200 };
            Button submitButton = new Button { Content = "Submit", Width = 100, Margin = new Thickness(0, 10, 0, 0) };

            panel.Children.Add(new Label { Content = "Please enter the password to close the application:" });
            panel.Children.Add(passwordBox);
            panel.Children.Add(submitButton);

            passwordWindow.Content = panel;
            bool isPasswordCorrect = false;

            submitButton.Click += (sender, e) =>
            {
                if (passwordBox.Password == _homePage.Password)
                {
                    isPasswordCorrect = true;
                    passwordWindow.Close();
                }
                else
                {
                    MessageBox.Show("Incorrect password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    passwordBox.Clear();
                }
            };

            passwordWindow.ShowDialog();

            return isPasswordCorrect;
        }
    }
}
