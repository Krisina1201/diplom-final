using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diplom
{
    public class Dialog
    {
        public void ShowErrorDialog(string title, string message, Window thiss)
        {
            var dialog = new Window
            {
                Title = title,
                Content = new TextBlock { Text = message },
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            dialog.ShowDialog(thiss);
        }
    }
}
