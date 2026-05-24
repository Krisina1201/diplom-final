using Avalonia.Controls;
using Avalonia.Interactivity;
using Diplom.Models;
using Microsoft.EntityFrameworkCore;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System.Linq;

namespace Diplom;

public partial class ClassroomEditWindow : Window
{
    private Classroom? _currentClassroom;
    private ResponsiblePerson? _selectedResponsible;

    public ClassroomEditWindow()
    {
        InitializeComponent();
        LoadResponsiblePersons();
        Title = "Добавление аудитории";
    }

    public ClassroomEditWindow(Classroom classroom)
    {
        InitializeComponent();
        _currentClassroom = classroom;
        LoadResponsiblePersons();
        LoadClassroomData();
        Title = "Редактирование аудитории";
    }

    //private void LoadResponsiblePersons()
    //{
    //    using (var context = new KsiptDbContext())
    //    {
    //        var responsiblePersons = context.ResponsiblePersons.ToList();
    //        ResponsibleComboBox.ItemsSource = responsiblePersons;

    //        if (responsiblePersons.Any() && _currentClassroom?.ResponsibleId != null)
    //        {
    //            var selected = responsiblePersons.FirstOrDefault(r => r.Id == _currentClassroom.ResponsibleId);
    //            ResponsibleComboBox.SelectedItem = selected;
    //        }
    //    }
    //}

    private void LoadResponsiblePersons()
    {
        using (var context = new KsiptDbContext())
        {
            var responsiblePersons = context.ResponsiblePersons
                .OrderBy(r => r.LastName)
                .ThenBy(r => r.FirstName)
                .ToList();

            ResponsibleComboBox.ItemsSource = responsiblePersons;
            ResponsibleComboBox.DisplayMemberBinding = new Avalonia.Data.Binding("FullName");

            if (_currentClassroom != null && _currentClassroom.ResponsibleId.HasValue)
            {
                var selected = responsiblePersons.FirstOrDefault(r => r.Id == _currentClassroom.ResponsibleId.Value);
                ResponsibleComboBox.SelectedItem = selected;
            }
        }
    }
    private void LoadClassroomData()
    {
        if (_currentClassroom != null)
        {
            RoomNumberTextBox.Text = _currentClassroom.RoomNumber;
            RoomNameTextBox.Text = _currentClassroom.RoomName;
            DescriptionTextBox.Text = _currentClassroom.Description ?? "";
            CapacityTextBox.Text = _currentClassroom.Capacity?.ToString() ?? "";
            IsActiveCheckBox.IsChecked = _currentClassroom.IsActive ?? true;
        }
    }

    private async void SaveButton_Click(object? sender, RoutedEventArgs e)
    {
        // Валидация
        if (string.IsNullOrWhiteSpace(RoomNumberTextBox.Text))
        {
            StatusTextBlock.Text = "Номер аудитории обязателен для заполнения";
            return;
        }

        if (string.IsNullOrWhiteSpace(RoomNameTextBox.Text))
        {
            StatusTextBlock.Text = "Название аудитории обязательно для заполнения";
            return;
        }

        try
        {
            using (var context = new KsiptDbContext())
            {
                // Проверка уникальности номера аудитории
                bool isNumberUnique = !await context.Classrooms
                    .AnyAsync(c => c.RoomNumber == RoomNumberTextBox.Text &&
                                   (_currentClassroom == null || c.Id != _currentClassroom.Id));

                if (!isNumberUnique)
                {
                    StatusTextBlock.Text = "Аудитория с таким номером уже существует";
                    return;
                }

                Classroom classroomToSave;

                if (_currentClassroom == null)
                {
                    // Создание новой аудитории
                    classroomToSave = new Classroom
                    {
                        RoomNumber = RoomNumberTextBox.Text,
                        RoomName = RoomNameTextBox.Text,
                        Description = string.IsNullOrWhiteSpace(DescriptionTextBox.Text) ? null : DescriptionTextBox.Text,
                        IsActive = IsActiveCheckBox.IsChecked ?? true,
                        CreatedAt = System.DateTime.Now,
                        UpdatedAt = System.DateTime.Now
                    };

                    // Парсим вместимость
                    if (int.TryParse(CapacityTextBox.Text, out int capacity))
                    {
                        classroomToSave.Capacity = capacity;
                    }

                    // Устанавливаем ответственное лицо
                    var selectedItem = ResponsibleComboBox.SelectedItem;
                    if (selectedItem != null)
                    {
                        var responsibleId = (int)selectedItem.GetType().GetProperty("Id").GetValue(selectedItem);
                        classroomToSave.ResponsibleId = responsibleId;
                    }

                    context.Classrooms.Add(classroomToSave);
                }
                else
                {
                    // Обновление существующей аудитории
                    classroomToSave = await context.Classrooms.FindAsync(_currentClassroom.Id);

                    if (classroomToSave != null)
                    {
                        classroomToSave.RoomNumber = RoomNumberTextBox.Text;
                        classroomToSave.RoomName = RoomNameTextBox.Text;
                        classroomToSave.Description = string.IsNullOrWhiteSpace(DescriptionTextBox.Text) ? null : DescriptionTextBox.Text;
                        classroomToSave.IsActive = IsActiveCheckBox.IsChecked ?? true;
                        classroomToSave.UpdatedAt = System.DateTime.Now;

                        // Парсим вместимость
                        if (int.TryParse(CapacityTextBox.Text, out int capacity))
                        {
                            classroomToSave.Capacity = capacity;
                        }
                        else
                        {
                            classroomToSave.Capacity = null;
                        }

                        // Устанавливаем ответственное лицо
                        var selectedItem = ResponsibleComboBox.SelectedItem;
                        if (selectedItem != null)
                        {
                            var responsibleId = (int)selectedItem.GetType().GetProperty("Id").GetValue(selectedItem);
                            classroomToSave.ResponsibleId = responsibleId;
                        }
                        else
                        {
                            classroomToSave.ResponsibleId = null;
                        }
                    }
                }

                await context.SaveChangesAsync();

                // Показываем сообщение об успехе
                var successBox = MessageBoxManager.GetMessageBoxStandard(
                    "Успех",
                    _currentClassroom == null ? "Аудитория успешно добавлена" : "Аудитория успешно обновлена",
                    ButtonEnum.Ok);
                await successBox.ShowAsync();

                Close();
            }
        }
        catch (System.Exception ex)
        {
            StatusTextBlock.Text = $"Ошибка при сохранении: {ex.Message}";
        }
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}