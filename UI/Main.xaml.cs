using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using ClassRegisterApp.Models;
using ClassRegisterApp.Core;
using ClassRegisterApp.Infrastructure;

namespace ClassRegisterApp.UI;

public partial class Main 
{
    public enum SubscribeType
    {
        /// <summary>
        /// Trong kế hoạch
        /// </summary>
        KH,
        /// <summary>
        /// Ngoài kế hoạch
        /// </summary>
        NKH
    }

    private readonly HuflitPortal _huflitPortal;


    /// <summary>
    ///     If true, login with portal cookie, else login with student id and password
    /// </summary>
    private bool _loginType = true;

    private SubscribeType _subscribeType = SubscribeType.KH;

    public Main(Code code)
    {
        _huflitPortal = new HuflitPortal
        {
            SubscribeType = _subscribeType
        };
        InitializeComponent();
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Icon = ImageHelper.GetEmbeddedImage("huflit-logo.ico");
    }

    public Main()
    {
        _huflitPortal = new();
    }


    private async void BtnRun_OnClick(object sender, RoutedEventArgs e)
    {
        var listClass = new List<string>();
        if (IsRichTextBoxEmpty(RtbClassList))
        {
            ListBoxState.Items.Add("Danh sách lớp trống");
            return;
        }

        var textRange = new TextRange(RtbClassList.Document.ContentStart, RtbClassList.Document.ContentEnd);

        if (textRange.Text is not (null or ""))
            listClass.AddRange(
                from lboxInfoItem in textRange.Text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                where lboxInfoItem is not null
                select lboxInfoItem);

        try
        {
            if (!_loginType)
            {
                var dicAuth = HuflitPortal.ParseCookie(TbCookie.Text.Replace(Environment.NewLine, ""));
                _huflitPortal.UserDkmh = new UserService.UserDKMH(dicAuth["User"], dicAuth["UserPW"]);
                await _huflitPortal.ConnectToDkmh();
            }
            else { await _huflitPortal.RegisterCookieToServer(); }

            _huflitPortal.RunOptimized(listClass, ListBoxState);
        } catch (Exception)
        {
            MessageBox.Show("Có lỗi hãy thử lại", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }


    private async void BtnCheckLogin_OnClick(object sender, RoutedEventArgs e)
    {
        if (!_loginType) return;
        var dicAuth = HuflitPortal.ParseCookie("ASP.NET_SessionId="+TbCookie.Text.Replace(Environment.NewLine, ""));
        _huflitPortal.SetCookie(dicAuth);
        var user = await _huflitPortal.CheckCookie();
        ListBoxState.Items.Add(user != null ? "Cookie hợp lệ" : "Cookie không hợp lệ");
        if (user == null) return;
        ListBoxState.Items.Add(user.Fullname);
    }


    private void KH_OnChecked(object sender, RoutedEventArgs e)
    {
        _subscribeType = SubscribeType.KH;
    }

    private void NKH_OnChecked(object sender, RoutedEventArgs e)
    {
        _subscribeType = SubscribeType.NKH;
    }

    private static bool IsRichTextBoxEmpty(RichTextBox rtb)
    {
        var textRange = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
        return string.IsNullOrWhiteSpace(textRange.Text);
    }

    private void PortalCookie_OnChecked(object sender, RoutedEventArgs e)
    {
        _loginType = true;
        if (CheckLoginBtn != null) { CheckLoginBtn.Visibility = Visibility.Visible; }
    }

    private void PW_OnChecked(object sender, RoutedEventArgs e)
    {
        _loginType = false;
        CheckLoginBtn.Visibility = Visibility.Hidden;
    }

    private void ResetButton_OnClick(object sender, RoutedEventArgs e)
    {
        _huflitPortal.IsRegisterCookie = false;
        _huflitPortal.ClearIsRegistered();
    }
}
