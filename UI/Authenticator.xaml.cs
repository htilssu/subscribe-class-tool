using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ClassRegisterApp.Core;
using ClassRegisterApp.Infrastructure;

namespace ClassRegisterApp.UI;

public partial class Authenticator
{
    private readonly CodeService _codeService;
    bool _isLogging = false;

    public Authenticator()
    {
        InitializeComponent();
        Icon = ImageHelper.GetEmbeddedImage("huflit-logo.ico");
        AuthImage.Source = Icon;

        _codeService = new CodeService();
        CheckSavedCode();
    }

    private async void CheckSavedCode()
    {
        var savedCode = await CodeService.LoadSavedCodeAsync();
        if (savedCode != null)
        {
            if (savedCode.ExpiredAt < DateTime.Now)
            {
                MessageBox.Show("Code của bạn đã hết hạn. Vui lòng nhập code mới!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            var main = new Main(savedCode);
            main.Show();
            Close();
        }
    }

    private async void LoginBtn_OnClick(object sender, RoutedEventArgs e)
    {
        if (_isLogging) return; // Prevent spam login
        _isLogging = true;
        var code = await CodeService.VerifyCodeAsync(CodeTextBox.Text);
        _isLogging = false;
        if (!code.IsOk)
        {
            MessageBox.Show("Code sai!!", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (code.Result!.ExpiredAt < DateTime.Now)
        {
            MessageBox.Show("Code của bạn đã hết hạn. Vui lòng nhập code mới!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var main = new Main(code.Result!);
        main.Show();
        Close();
    }

    private void OnEnterLogin(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) LoginBtn_OnClick(sender, e);
    }

    private void SwitchLoginBtn_OnClick(object sender, RoutedEventArgs e)
    {
        AccountPanel.Visibility = AccountPanel.IsVisible ? Visibility.Collapsed : Visibility.Visible;
    }
}
