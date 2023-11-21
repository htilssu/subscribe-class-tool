using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Documents;

namespace ClassRegisterApp;

/// <summary>
/// </summary>
internal partial class Main : Window
{
    private readonly HuflitPortal _huflitPortal;

    /// <summary>
    /// </summary>
    /// <param name="user"></param>
    /// <param name="code"></param>
    public Main(User user, Code code)
    {
        _huflitPortal = new HuflitPortal(user.UserName, user.Password)
        {
            Delay = code.Delay * 1000
        };
        Login();
        InitializeComponent();
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Icon = ImageHelper.GetEmbeddedImage("huflit-logo.ico");
        HiLabel.Content = $"Hi {_huflitPortal.UserName.ToUpper()}";
    }

    public Main()
    {
        _huflitPortal = new HuflitPortal("22DH114528", "Shuu2004")
        {
            Delay = 0
        };
        Login();
        InitializeComponent();
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Icon = ImageHelper.GetEmbeddedImage("huflit-logo.ico");
        HiLabel.Content = $"Hi {_huflitPortal.UserName.ToUpper()}";
    }

    private async void Login()
    {
        var res = await _huflitPortal.Login();
        LboxInfo.Items.Add(res == HttpStatusCode.OK ? "Đăng nhập thành công" : "Đăng nhập thất bại");
        BtnLogout.Visibility = Visibility.Visible;
    }

    private async void BtnRun_OnClick(object sender, RoutedEventArgs e)
    {
        var listClass = new List<string>();
        if (RtbClassList.Document.ContentEnd == RtbClassList.Document.ContentStart) return;

        var textRange = new TextRange(RtbClassList.Document.ContentStart, RtbClassList.Document.ContentEnd);

        if (textRange.Text is not (null or ""))
            listClass.AddRange(
                from lboxInfoItem in textRange.Text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                where lboxInfoItem is not null
                select lboxInfoItem);
        await _huflitPortal.ConnectToDkmh();
        try
        {
            await _huflitPortal.RunOptimized(listClass, LboxInfo);
        }
        catch (Exception exception)
        {
            MessageBox.Show("Có lỗi hãy thử lại", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnLogout_OnClick(object sender, RoutedEventArgs e)
    {
        var loginForm = new Authenticator();
        loginForm.Show();
        Close();
    }
}