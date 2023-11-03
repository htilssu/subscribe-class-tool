using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Documents;

namespace ClassRegisterApp;

/// <summary>
/// </summary>
public partial class Main : Window
{
    private readonly HuflitPortal? _huflitPortal;

    /// <summary>
    /// </summary>
    /// <param name="user"></param>
    public Main(User user)
    {
        _huflitPortal = new HuflitPortal(user.UserName, user.Password);
        Login();
        InitializeComponent();
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        HiLabel.Content = $"Hi {_huflitPortal.UserName}";
    }

    private async void Login()
    {
        var res = await _huflitPortal!.Login();
        LboxInfo.Items.Add(res == HttpStatusCode.OK ? "Đăng nhập thành công" : "Đăng nhập thất bại");
        BtnLogout.Visibility = Visibility.Visible;
    }

    private async void BtnRun_OnClick(object sender, RoutedEventArgs e)
    {
        var listClass = new List<string>();
        var textRange = new TextRange(RtbClassList.Document.ContentStart, RtbClassList.Document.ContentEnd);
        if (textRange.Text is not (null or ""))
            listClass.AddRange(from lboxInfoItem in textRange.Text.Split("\n")
                where lboxInfoItem.Replace("\r", "") != ""
                select lboxInfoItem.Replace("\r", ""));
        await _huflitPortal?.ConnectToDKMH()!;
        await _huflitPortal.Run(listClass, LboxInfo);
    }
}