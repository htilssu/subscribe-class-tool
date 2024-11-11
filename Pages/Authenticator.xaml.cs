using System.Windows;
using System.Windows.Input;
using ClassRegisterApp.Services;

namespace ClassRegisterApp.Pages;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
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
    }


    private async void LoginBtn_OnClick(object sender, RoutedEventArgs e)
    {
        if (_isLogging) return; // Prevent multiple login
        _isLogging = true;
        var code = await _codeService.CheckCodeAsync(CodeTextBox.Text);
        _isLogging = false;
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
