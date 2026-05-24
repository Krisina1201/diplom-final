using Avalonia.Controls;
using QRCoder;
using System.IO;
using Avalonia.Media.Imaging;
using System;
using Tmds.DBus.Protocol;
using Avalonia.Media;
using Avalonia;
using MsBox.Avalonia;

namespace Diplom;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

    }

    private async void EntranceClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        string password = "000";

        string enterStr = passwordBox.Text;

        if (string.IsNullOrEmpty(enterStr))
        {
            var message = MessageBoxManager.GetMessageBoxStandard("Уведомление",
                      "Поле должно быть заполнено", MsBox.Avalonia.Enums.ButtonEnum.Ok,
                      MsBox.Avalonia.Enums.Icon.Error);
            await message.ShowAsync();
            return;
        } else if (enterStr != password)
        {
            var message = MessageBoxManager.GetMessageBoxStandard("Уведомление",
                      "Не верный пароль", MsBox.Avalonia.Enums.ButtonEnum.Ok,
                      MsBox.Avalonia.Enums.Icon.Error);
            await message.ShowAsync();
            return;
        } else
        {
            QrWindow qrWindow = new QrWindow();
            qrWindow.Show();
            this.Close();
        }
    }

}