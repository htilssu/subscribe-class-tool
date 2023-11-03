using System.Windows;
using Microsoft.Extensions.Hosting;

namespace ClassRegisterApp;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private readonly IHost? _appHost;

    public App()
    {
        _appHost = Host.CreateDefaultBuilder().ConfigureServices(collection => { }).Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _appHost!.StartAsync();
        base.OnStartup(e);
    }
}