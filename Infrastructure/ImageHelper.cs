using System.Reflection;
using System.Windows.Media.Imaging;

namespace ClassRegisterApp.Infrastructure;

internal static class ImageHelper
{
    public static BitmapImage GetEmbeddedImage(string imageName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"{assembly.GetName().Name}.Image.{imageName}";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.StreamSource = stream;
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.EndInit();

        return bitmap;
    }
}
