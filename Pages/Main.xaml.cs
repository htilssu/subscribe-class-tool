using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using ClassRegisterApp.Models;
using ClassRegisterApp.Services;

namespace ClassRegisterApp.Pages;

internal partial class Main : Window
{
    private readonly HuflitPortal _huflitPortal = new();


    public Main(Code code)
    {
        _huflitPortal = new HuflitPortal
        {
            Delay = code.Delay * 1000
        };
        InitializeComponent();
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Icon = ImageHelper.GetEmbeddedImage("huflit-logo.ico");
    }

    public Main()
    {
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
        if (res != null) ListBoxState.Items.Add(res.UserName);
    }
}