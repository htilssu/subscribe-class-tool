using System.Windows;
using System.Windows.Input;

namespace ClassRegisterApp;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
internal partial class Authenticator
{
    private readonly CodeService _codeService;
    private User? _user;

    public Authenticator()
    {
        InitializeComponent();
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Icon = ImageHelper.GetEmbeddedImage("huflit-logo.ico");
        AuthImage.Source = Icon;

        _codeService = new CodeService();
    }


    private async void LogButton_OnClick(object sender, RoutedEventArgs e)
    {
        var code = await _codeService.CheckCodeAsync(CodeTextBox.Text);

        if (code == null)
        {
            MessageBox.Show("Code sai!!", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        _user = new User(UserNameTextBox.Text, PasswordTextBox.Password);
        var main = new Main(_user, code);
        main.Show();
        Close();
    }

    private void OnEnterLogin(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) LogButton_OnClick(sender, e);
    }
}