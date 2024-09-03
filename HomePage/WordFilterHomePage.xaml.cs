using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;
using System.Windows;
using Gma.System.MouseKeyHook;
using System.Windows.Forms;

namespace ExplicitWordMonitor.HomePage
{
    public partial class WordFilterHomePage : Page
    {
        private IKeyboardMouseEvents _globalHook;
        private Dictionary<IntPtr, string> _inputByWindow = new Dictionary<IntPtr, string>();
        private List<string> DefaultWords = new List<string>();
        private List<string> UserAddedWords = new List<string>();
        public string _password = "1234";
        private IntPtr _lastWindowHandle = IntPtr.Zero;

        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }
        public WordFilterHomePage()
        {
            InitializeComponent();
            LoadDefaultWords();
            LoadUserAddedWords(); 
            _globalHook = Hook.GlobalEvents();
            _globalHook.KeyPress += OnKeyPress;
            UpdateComboBox();
        }


        private void LoadDefaultWords()
        {
            try
            {
                string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "DefaultWords.txt");
                if (File.Exists(filePath))
                {
                    var words = File.ReadAllLines(filePath);

                    // Filtrer les mots vides ou les lignes contenant uniquement des espaces
                    DefaultWords.AddRange(words.Where(word => !string.IsNullOrWhiteSpace(word)));
                }
                else
                {
                    System.Windows.MessageBox.Show("Le fichier de mots par défaut est introuvable.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur lors du chargement des mots par défaut : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void LoadUserAddedWords()
        {
            string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "UserAddedWords.txt");

            try
            {
                if (File.Exists(filePath))
                {
                    var words = File.ReadAllLines(filePath);
                    UserAddedWords.AddRange(words.Where(word => !string.IsNullOrWhiteSpace(word)));
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur lors du chargement des mots ajoutés par l'utilisateur : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void btnAddWord_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtNewWord.Text) && !UserAddedWords.Contains(txtNewWord.Text))
            {
                UserAddedWords.Add(txtNewWord.Text);
                System.Windows.MessageBox.Show("Word added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                txtNewWord.Clear();
                UpdateComboBox(); // Mettre à jour le ComboBox

                // Enregistrer le mot dans le fichier
                SaveUserAddedWords();
            }
            else
            {
                System.Windows.MessageBox.Show("Please enter a unique, non-empty word.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveUserAddedWords()
        {
            string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "UserAddedWords.txt");

            try
            {
                // Écrire tous les mots dans le fichier texte
                File.WriteAllLines(filePath, UserAddedWords);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur lors de l'enregistrement des mots ajoutés : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void UpdateComboBox()
        {
            cmbExplicitWords.ItemsSource = null; // Réinitialiser la source pour la mise à jour
            cmbExplicitWords.ItemsSource = UserAddedWords;
        }

        private void btnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            if (txtOldPassword.Password == Password) // Utiliser la propriété Password pour comparer l'ancien mot de passe
            {
                if (!string.IsNullOrWhiteSpace(txtNewPassword.Password))
                {
                    Password = txtNewPassword.Password; // Utiliser la propriété Password pour mettre à jour le mot de passe
                    System.Windows.MessageBox.Show("Password changed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtOldPassword.Clear();
                    txtNewPassword.Clear();
                }
                else
                {
                    System.Windows.MessageBox.Show("New password cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Old password is incorrect.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            var currentWindowHandle = GetForegroundWindow();

            // Vérifier si l'utilisateur a changé de fenêtre
            if (currentWindowHandle != _lastWindowHandle)
            {
                _lastWindowHandle = currentWindowHandle;
                _inputByWindow[currentWindowHandle] = ""; // Réinitialiser la séquence de la nouvelle fenêtre
            }

            if (!_inputByWindow.ContainsKey(currentWindowHandle))
            {
                _inputByWindow[currentWindowHandle] = "";
            }

            _inputByWindow[currentWindowHandle] += e.KeyChar;

            foreach (var word in DefaultWords)
            {
                if (_inputByWindow[currentWindowHandle].EndsWith(word, StringComparison.OrdinalIgnoreCase))
                {
                    CloseActiveApplication();
                    _inputByWindow[currentWindowHandle] = "";
                    break;
                }
            }

            foreach (var word in UserAddedWords)
            {
                if (_inputByWindow[currentWindowHandle].EndsWith(word, StringComparison.OrdinalIgnoreCase))
                {
                    CloseActiveApplication();
                    _inputByWindow[currentWindowHandle] = "";
                    break;
                }
            }

            if (_inputByWindow[currentWindowHandle].Length > 100)
            {
                _inputByWindow[currentWindowHandle] = _inputByWindow[currentWindowHandle].Substring(_inputByWindow[currentWindowHandle].Length - 10);
            }
        }



        private void CloseActiveApplication()
        {
            var activeWindowHandle = GetForegroundWindow();
            GetWindowThreadProcessId(activeWindowHandle, out uint processID);

            var process = Process.GetProcessById((int)processID);

            if (process.ProcessName.Equals("ExplicitWordMonitor", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            process.Kill();
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
    }
}
