using System;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace ClassRegisterApp;

internal class ImageHelper
{
    public static BitmapImage GetEmbeddedImage(string imageName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"{assembly.GetName().Name}.Images.{imageName}";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            throw new Exception($"Resource '{resourceName}' not found.");

        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.StreamSource = stream;
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.EndInit();

        return bitmap;
    }
}