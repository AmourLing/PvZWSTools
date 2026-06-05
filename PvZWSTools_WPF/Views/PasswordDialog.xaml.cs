using System.Windows;
using System.Windows.Input;
using PvZWSTools_WPF.Helpers;

namespace PvZWSTools_WPF.Views
{
    public partial class PasswordDialog:Window
    {
        public bool IsPasswordCorrect { get; private set; }
        private int _attemptCount = 0;

        public PasswordDialog()
        {
            InitializeComponent();
            _ = PasswordBox.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            _attemptCount++;
            string password = PasswordBox.Password;

            if(Lock.VerifyPassword(password))
            {
                IsPasswordCorrect = true;
                DialogResult = true;
                Close();
            }
            else
            {
                if(_attemptCount >= 3)
                {
                    ErrorText.Text = "密码错误已达3次，程序将退出";
                    DialogResult = false;
                    Close();
                }
                else
                {
                    ErrorText.Text = $"密码错误，还剩 {3 - _attemptCount} 次机会";
                    PasswordBox.Clear();
                    _ = PasswordBox.Focus();
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            IsPasswordCorrect = false;
            DialogResult = false;
            Close();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                OkButton_Click(this, new RoutedEventArgs());
            }
            base.OnKeyDown(e);
        }
    }
}
