using Avalonia.Controls;
using QRCoder;
using System.IO;
using Avalonia.Media.Imaging;
using System;
using Tmds.DBus.Protocol;
using System.Collections.Generic;
using MsBox.Avalonia;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
namespace Diplom;

public partial class QrWindow : Window
{
    public QrWindow()
    {
        InitializeComponent();
        DataContext = this;

        List<string> classroom = ["1.11", "1.12", "1.121.12", "1.16", "1.18", "1.19", "1.191.31", "1.26", "1.29", "1.31", "1.32", "2.16", "2.18", "2.20", "2.32", "2.46", "2.47", "2.48", "2.49", "2.8", "3.16", "3.22", "3.224.2", "3.29", "3.313.32", "3.323.31", "3.33", "3.35", "3.40", "3.43", "3.50", "3.53", "3.532.20", "3.533.40", "3.533.53", "3.534.2", "4.12", "4.2", "4.23.40", "4.24.3", "4.24.5", "4.3", "4.31.12", "4.32.20", "4.33.22", "4.33.40", "4.34.2", "4.34.3", "4.34.5", "4.5", "4.6", "4.8", "4.9", "Спортивный зал № 1", "Спортивный зал № 1Спортивный зал № 1", "акт.зал"];
        ClassroomComboBox.ItemsSource = classroom;
    }

    public string selecCab;

    public Bitmap imageToSave;

    public Dialog dialog = new Dialog();

    // Делаем свойство публичным для привязки
    //public Bitmap? QrCodeImage;

    public void GenerateQrCode(string text)
    {
        try
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);

            var qrCodeBytes = qrCode.GetGraphic(20);
            using var stream = new MemoryStream(qrCodeBytes);

            imageToSave = new Bitmap(stream);

            QrCodeImage.Source = imageToSave;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка генерации QR: {ex.Message}");
            dialog.ShowErrorDialog("ОШибка", "Произошла ошибка при генерации qr кода, попробуйте позже", this);
        }
    }

    private async void Generate_qr_click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Генерируем QR-код при нажатии кнопки
        if (string.IsNullOrEmpty(selecCab))
        {
            var mes = MessageBoxManager.GetMessageBoxStandard("Уведомление", "Выберите кабинет для которого хотите сгенерировать qr код", MsBox.Avalonia.Enums.ButtonEnum.Ok);
            await mes.ShowAsync();
        } else
        {
            string path = "http://83.166.246.113/pages/schedule.html?room=" + $"{selecCab}";
            GenerateQrCode(path);
            qrPath.Text = path;
        }
            

    }


    private void ClassroomClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ClassroomWindow classroomWindow = new ClassroomWindow();
        classroomWindow.Show();
        this.Close();
    }
    
    private void PeopleClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ResponsiblePersonWindow responsiblePersonWindow = new();
        responsiblePersonWindow.Show();
        this.Close();
    }
    
    private void InventoryClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        InventoryWindow inventoryWindow = new InventoryWindow();
        inventoryWindow.Show();
        this.Close();
    }

    private void ComboBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        var selectedItem = ClassroomComboBox.SelectedItem;
        selecCab = selectedItem.ToString();
    }

    private async void Button_Click_1(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(selecCab) || imageToSave == null)
        {
            var mes = MessageBoxManager.GetMessageBoxStandard("Уведомление", "Сначала сгенерируйте qr код, выбрав кабинет, после этого вы сможете его сохранить", MsBox.Avalonia.Enums.ButtonEnum.Ok);
            await mes.ShowAsync();
        } else
        {
            string fileName = $"image_{DateTime.Now:yyyy-MM-dd}" + $"_{selecCab}" + ".png";

            var topLevel = TopLevel.GetTopLevel(this); // или передайте ссылку на Window

            // Настройки диалога выбора папки
            var folderPickerOptions = new FolderPickerOpenOptions
            {
                Title = "Выберите папку для сохранения",
                AllowMultiple = false
            };

            // Открываем диалог
            var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(folderPickerOptions);

            // Проверяем, выбрал ли пользователь папку
            if (folders.Count > 0)
            {
                var selectedFolder = folders[0]; // IStorageFolder

                // Получаем полный путь к файлу
                string fullPath = Path.Combine(selectedFolder.Path.LocalPath, fileName);

                // Сохраняем изображение
                imageToSave.Save(fullPath);

                var mes = MessageBoxManager.GetMessageBoxStandard("Уведомление", $"Изображение успешно сохранено, со следующем путем: \n{fullPath}", MsBox.Avalonia.Enums.ButtonEnum.Ok);
                await mes.ShowAsync();
            }

            

        }
    }
}