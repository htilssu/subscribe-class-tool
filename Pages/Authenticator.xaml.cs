using System.Windows;
using System.Windows.Input;
using ClassRegisterApp.Pages;
using ClassRegisterApp.Services;

namespace ClassRegisterApp;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
internal partial class Authenticator
{
    private readonly CodeService _codeService;

    public Authenticator()
    {
        InitializeComponent();
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Icon = ImageHelper.GetEmbeddedImage("huflit-logo.ico");
        AuthImage.Source = Icon;

        _codeService = new CodeService();
    }


    private async void LoginBtn_OnClick(object sender, RoutedEventArgs e)
    {
        var code = await _codeService.CheckCodeAsync(CodeTextBox.Text);

        if (code == null)
        {
            MessageBox.Show("Code sai!!", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var main = new Main(code);
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