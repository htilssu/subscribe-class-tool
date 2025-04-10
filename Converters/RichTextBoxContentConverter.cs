using System;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace ClassRegisterApp.Converters
{
    public class RichTextBoxContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Collections.ICollection blocks && blocks.Count > 0)
            {
                // Kiểm tra nếu có bất kỳ block nào có nội dung
                return blocks.Cast<Block>().Any(block => 
                {
                    if (block is Paragraph paragraph)
                    {
                        var inlineText = new TextRange(paragraph.ContentStart, paragraph.ContentEnd).Text;
                        return !string.IsNullOrWhiteSpace(inlineText);
                    }
                    return true;
                });
            }
            
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}