using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using ClassRegisterApp.Models;
using ClassRegisterApp.Services;

namespace ClassRegisterApp.Pages;

internal partial class Main : Window
{
    public enum SubscribeType
    {
        KH,
        NKH
    }

    private readonly HuflitPortal _huflitPortal = new();
    private bool _isLogged;
    private SubscribeType _subscribeType = SubscribeType.KH;

    public Main(Code code)
    {
        _huflitPortal = new HuflitPortal
        {
            Delay = code.Delay * 1000,
            SubscribeType = _subscribeType
        };
        InitializeComponent();
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Icon = ImageHelper.GetEmbeddedImage("huflit-logo.ico");
        KH.IsChecked = true;
    }

    public Main()
    {
    }


    private async void BtnRun_OnClick(object sender, RoutedEventArgs e)
    {
        var listClass = new List<string>();
        if (IsRichTextBoxEmpty(RtbClassList))
        {
            ListBoxState.Items.Add("Danh sách lớp trống");
            return;
        }

        if (!_isLogged)
        {
            ListBoxState.Items.Add("Chưa đăng nhập fen ưi");
            return;
        }


        var textRange = new TextRange(RtbClassList.Document.ContentStart, RtbClassList.Document.ContentEnd);

        if (textRange.Text is not (null or ""))
            listClass.AddRange(
                from lboxInfoItem in textRange.Text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                where lboxInfoItem is not null
                select lboxInfoItem);
        await _huflitPortal.ConnectToDkmh();
        try
        {
            _huflitPortal.RunOptimized(listClass, ListBoxState);
        }
        catch (Exception)
        {
            MessageBox.Show("Có lỗi hãy thử lại", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }


    private async void BtnCheckLogin_OnClick(object sender, RoutedEventArgs e)
    {
        _huflitPortal.SetCookie(TbCookie.Text.Replace(Environment.NewLine, ""));
        var res = await _huflitPortal.CheckCookie();
        ListBoxState.Items.Add(res != null ? "Cookie hợp lệ" : "Cookie không hợp lệ");
        if (res != null)
        {
            ListBoxState.Items.Add(res.UserName);
            _isLogged = true;
        }
    }

    private void KH_OnChecked(object sender, RoutedEventArgs e)
    {
        _subscribeType = SubscribeType.KH;
    }

    private void NKH_OnChecked(object sender, RoutedEventArgs e)
    {
        _subscribeType = SubscribeType.NKH;
    }

    private bool IsRichTextBoxEmpty(RichTextBox rtb)
    {
        var textRange = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
        return string.IsNullOrWhiteSpace(textRange.Text);
    }
}