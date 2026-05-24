using Avalonia.Controls;
using Avalonia.Interactivity;
using Diplom.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace Diplom;

public partial class ResponsiblePersonWindow : Window
{
    public ResponsiblePersonWindow()
    {
        InitializeComponent();
        LoadData();
    }


    private void LoadData()
    {
        using (var context = new KsiptDbContext())
        {
            var list = context.ResponsiblePersons
                .Include(r => r.Classrooms)
                .OrderBy(r => r.LastName)
                .ThenBy(r => r.FirstName)
                .ToList();

            ResponsibleListBox.ItemsSource = list;
        }
    }

    private async void AddResponsible_Click(object? sender, RoutedEventArgs e)
    {
        var dialog = new ResponsiblePersonEditWindow();
        dialog.Closed += (s, args) => LoadData();
        await dialog.ShowDialog(this);
    }

    private async void EditResponsible_Click(object? sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var responsible = button?.CommandParameter as ResponsiblePerson;

        if (responsible != null)
        {
            var dialog = new ResponsiblePersonEditWindow(responsible);
            dialog.Closed += (s, args) => LoadData();
            await dialog.ShowDialog(this);
        }
    }

    private async void DeleteResponsible_Click(object? sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var responsible = button?.CommandParameter as ResponsiblePerson;

        if (responsible != null)
        {
            // Проверяем, есть ли связанные аудитории
            if (responsible.Classrooms != null && responsible.Classrooms.Count > 0)
            {
                var warningBox = MessageBoxManager.GetMessageBoxStandard(
                    "Невозможно удалить",
                    $"Это ответственное лицо закреплено за {responsible.Classrooms.Count} аудиторией(ями). Сначала переназначьте ответственных в аудиториях.",
                    ButtonEnum.Ok);
                await warningBox.ShowAsync();
                return;
            }

            var messageBox = MessageBoxManager.GetMessageBoxStandard(
                "Подтверждение удаления",
                $"Вы действительно хотите удалить ответственное лицо {responsible.LastName} {responsible.FirstName}?",
                ButtonEnum.YesNo);

            var result = await messageBox.ShowAsync();

            if (result == ButtonResult.Yes)
            {
                try
                {
                    using (var context = new KsiptDbContext())
                    {
                        var responsibleToDelete = await context.ResponsiblePersons.FindAsync(responsible.Id);
                        if (responsibleToDelete != null)
                        {
                            context.ResponsiblePersons.Remove(responsibleToDelete);
                            await context.SaveChangesAsync();
                            LoadData();
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    var errorBox = MessageBoxManager.GetMessageBoxStandard(
                        "Ошибка",
                        $"Произошла ошибка при удалении: {ex.Message}",
                        ButtonEnum.Ok);
                    await errorBox.ShowAsync();
                }
            }
        }
    }

    private void BackButton_Click(object? sender, RoutedEventArgs e)
    {
        var qrWindow = new QrWindow();
        qrWindow.Show();
        this.Close();
    }
}