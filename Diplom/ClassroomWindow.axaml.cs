using Avalonia.Controls;
using Avalonia.Interactivity;
using Diplom.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace Diplom;

public partial class ClassroomWindow : Window
{
    public ClassroomWindow()
    {
        InitializeComponent();
        LoadData();
    }

    private void LoadData()
    {
        using (var context = new KsiptDbContext())
        {
            var list = context.Classrooms
                .Include(e => e.Responsible)
                .ToList();

            var selectSort = sortByTitle.SelectedIndex;

            switch (selectSort)
            {
                case 0: list = list.OrderByDescending(e => int.Parse(e.RoomNumber)).ToList(); break;
                case 1: list = list.OrderBy(e => int.Parse(e.RoomNumber)).ToList(); break;
            }

            ClassroomListBox.ItemsSource = list;
        }
    }

    private async void AddClassroom_Click(object? sender, RoutedEventArgs e)
    {
        var dialog = new ClassroomEditWindow();
        dialog.Closed += (s, args) => LoadData(); // Перезагружаем данные после закрытия окна
        await dialog.ShowDialog(this);
    }

    private async void EditClassroom_Click(object? sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var classroom = button?.CommandParameter as Classroom;

        if (classroom != null)
        {
            var dialog = new ClassroomEditWindow(classroom);
            dialog.Closed += (s, args) => LoadData(); // Перезагружаем данные после закрытия окна
            await dialog.ShowDialog(this);
        }
    }

    private async void DeleteClassroom_Click(object? sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var classroom = button?.CommandParameter as Classroom;

        if (classroom != null)
        {
            var messageBox = MessageBoxManager.GetMessageBoxStandard(
                "Подтверждение удаления",
                $"Вы действительно хотите удалить аудиторию {classroom.RoomNumber}?",
                ButtonEnum.YesNo);

            var result = await messageBox.ShowAsync();

            if (result == ButtonResult.Yes)
            {
                try
                {
                    using (var context = new KsiptDbContext())
                    {
                        // Проверяем, есть ли связанный инвентарь
                        var hasInventory = context.Inventories.Any(i => i.ClassroomId == classroom.Id);

                        if (hasInventory)
                        {
                            var warningBox = MessageBoxManager.GetMessageBoxStandard(
                                "Невозможно удалить",
                                "В этой аудитории есть инвентарь. Сначала удалите весь инвентарь из аудитории.",
                                ButtonEnum.Ok);
                            await warningBox.ShowAsync();
                            return;
                        }

                        var classroomToDelete = await context.Classrooms.FindAsync(classroom.Id);
                        if (classroomToDelete != null)
                        {
                            context.Classrooms.Remove(classroomToDelete);
                            await context.SaveChangesAsync();

                            LoadData(); // Перезагружаем список
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

    private void ComboBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        LoadData();
    }
}