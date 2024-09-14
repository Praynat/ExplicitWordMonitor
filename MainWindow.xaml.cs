using ExplicitWordMonitor.HomePage;
using ExplicitWordMonitor.ProxyFilter;
using System.Windows;
using System.Windows.Controls;

namespace ExplicitWordMonitor
{
    public partial class MainWindow : Window
    {
        private WordFilterHomePage _homePage;
        private WebFilterProxy webFilterProxy;

        private bool isClosing = false;

        public MainWindow()
        {
            InitializeComponent();
            _homePage = new WordFilterHomePage();
            mainFrame.Navigate(_homePage);
            this.Closing += MainWindow_Closing;

            this.Closing += MainWindow_Closing;

            // Start the proxy server
            List<string> allBadWords = _homePage.GetAllBadWords();
            webFilterProxy = new WebFilterProxy(allBadWords);
            webFilterProxy.Start();

            // Subscribe to the event to update bad words list
            _homePage.BadWordsListUpdated += UpdateBadWordsList;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (isClosing)
            {
                return;
            }

            if (!AskForPassword())
            {
                e.Cancel = true;
            }
            else
            {
                isClosing = true;

                // Stop the proxy server
                webFilterProxy.Stop();
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
        private void UpdateBadWordsList(List<string> newBadWords)
        {
            webFilterProxy.UpdateBadWords(newBadWords);
        }
    }
}
