using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ClassRegisterApp;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class Authenticator
{
    private readonly CodeService _codeService;
    private User? _user;

    public Authenticator()
    {
        InitializeComponent();
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Icon = new BitmapImage(new Uri(@"D:\projects\C# WPF\ClassRegisterApp\Images\huflit-logo.ico"));
        _codeService = new CodeService();
    }


    private async void LogButton_OnClick(object sender, RoutedEventArgs e)
    {
        var status = await _codeService.CheckCode(CodeTextBox.Text);

        if (!status)
        {
            MessageBox.Show("Code sai!!", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        _user = new User(UserNameTextBox.Text, PasswordTextBox.Password);
        var main = new Main(_user);
        main.Show();
        Close();
    }
}