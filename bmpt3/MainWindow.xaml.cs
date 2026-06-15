using bmpt3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace bmpt3 
{ 
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Button firstButton = null;
        private readonly Random random = new Random();

        public MainWindow()
        {
            InitializeComponent();
            ShuffleCaptcha();
        }

        private void ShuffleCaptcha()
        {
            int[] parts = { 1, 2, 3, 4 };
            parts = parts.OrderBy(x => random.Next()).ToArray();

            SetCaptchaPart(CaptchaButton1, parts[0]);
            SetCaptchaPart(CaptchaButton2, parts[1]);
            SetCaptchaPart(CaptchaButton3, parts[2]);
            SetCaptchaPart(CaptchaButton4, parts[3]);

            firstButton = null;
        }

        private void SetCaptchaPart(Button button, int partNumber)
        {
            Image image = new Image();

            image.Source = new BitmapImage(
                new Uri($"pack://application:,,,/image/part{partNumber}.png")
            );

            image.Stretch = Stretch.Fill;

            button.Content = image;
            button.Tag = partNumber;
        }

        private void CaptchaButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;

            if (firstButton == null)
            {
                firstButton = clickedButton;
                firstButton.BorderThickness = new Thickness(4);
                return;
            }

            if (firstButton == clickedButton)
            {
                firstButton.BorderThickness = new Thickness(1);
                firstButton = null;
                return;
            }

            object tempContent = firstButton.Content;
            object tempTag = firstButton.Tag;

            firstButton.Content = clickedButton.Content;
            firstButton.Tag = clickedButton.Tag;

            clickedButton.Content = tempContent;
            clickedButton.Tag = tempTag;

            firstButton.BorderThickness = new Thickness(1);
            firstButton = null;
        }

        private bool IsCaptchaCorrect()
        {
            return Convert.ToInt32(CaptchaButton1.Tag) == 1 &&
                   Convert.ToInt32(CaptchaButton2.Tag) == 2 &&
                   Convert.ToInt32(CaptchaButton3.Tag) == 3 &&
                   Convert.ToInt32(CaptchaButton4.Tag) == 4;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Заполните логин и пароль");
                return;
            }

            UserInfo user = Database.GetUserByLogin(login);

            if (user == null)
            {
                MessageBox.Show("Вы ввели неверный логин или пароль. Пожалуйста проверьте ещё раз введенные данные");
                return;
            }

            if (user.IsBlocked)
            {
                MessageBox.Show("Вы заблокированы. Обратитесь к администратору");
                return;
            }

            if (!IsCaptchaCorrect())
            {
                Database.AddFailedAttempt(user.UserId);
                MessageBox.Show("Капча собрана неверно");
                ShuffleCaptcha();
                return;
            }

            if (!Database.CheckPassword(login, password))
            {
                Database.AddFailedAttempt(user.UserId);
                MessageBox.Show("Вы ввели неверный логин или пароль. Пожалуйста проверьте ещё раз введенные данные");
                ShuffleCaptcha();
                return;
            }

            Database.ResetFailedAttempts(user.UserId);

            MessageBox.Show("Вы успешно авторизовались");

            if (user.Role == "Администратор")
            {
                AdminWindow adminWindow = new AdminWindow();
                adminWindow.Show();
            }
            else
            {
                UserWindow userWindow = new UserWindow();
                userWindow.Show();
            }

            Close();
        }
    }
}
