using Avalonia.Controls;
using Avalonia.Interactivity;
using Diplom.Models;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Linq;

namespace Diplom;

public partial class ResponsiblePersonEditWindow : Window
{
    private ResponsiblePerson? _currentResponsible;

    public ResponsiblePersonEditWindow()
    {
        InitializeComponent();
        Title = "Добавление ответственного лица";
    }

    public ResponsiblePersonEditWindow(ResponsiblePerson responsible)
    {
        InitializeComponent();
        _currentResponsible = responsible;
        LoadResponsibleData();
        Title = "Редактирование ответственного лица";
    }

    private void LoadResponsibleData()
    {
        if (_currentResponsible != null)
        {
            LastNameTextBox.Text = _currentResponsible.LastName;
            FirstNameTextBox.Text = _currentResponsible.FirstName;
            MiddleNameTextBox.Text = _currentResponsible.MiddleName ?? "";
            PositionTextBox.Text = _currentResponsible.Position;
            PhoneTextBox.Text = _currentResponsible.Phone ?? "";
            EmailTextBox.Text = _currentResponsible.Email ?? "";
        }
    }

    private async void SaveButton_Click(object? sender, RoutedEventArgs e)
    {
        // Валидация
        if (string.IsNullOrWhiteSpace(LastNameTextBox.Text))
        {
            StatusTextBlock.Text = "Фамилия обязательна для заполнения";
            return;
        }

        if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text))
        {
            StatusTextBlock.Text = "Имя обязательно для заполнения";
            return;
        }

        if (string.IsNullOrWhiteSpace(PositionTextBox.Text))
        {
            StatusTextBlock.Text = "Должность обязательна для заполнения";
            return;
        }

        // Валидация email (если указан)
        if (!string.IsNullOrWhiteSpace(EmailTextBox.Text))
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(EmailTextBox.Text);
                if (addr.Address != EmailTextBox.Text)
                {
                    StatusTextBlock.Text = "Некорректный формат email";
                    return;
                }
            }
            catch
            {
                StatusTextBlock.Text = "Некорректный формат email";
                return;
            }
        }

        try
        {
            using (var context = new KsiptDbContext())
            {
                if (_currentResponsible == null)
                {
                    // Создание нового ответственного лица
                    var newResponsible = new ResponsiblePerson
                    {
                        LastName = LastNameTextBox.Text.Trim(),
                        FirstName = FirstNameTextBox.Text.Trim(),
                        MiddleName = string.IsNullOrWhiteSpace(MiddleNameTextBox.Text) ? null : MiddleNameTextBox.Text.Trim(),
                        Position = PositionTextBox.Text.Trim(),
                        Phone = string.IsNullOrWhiteSpace(PhoneTextBox.Text) ? null : PhoneTextBox.Text.Trim(),
                        Email = string.IsNullOrWhiteSpace(EmailTextBox.Text) ? null : EmailTextBox.Text.Trim(),
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    context.ResponsiblePersons.Add(newResponsible);
                }
                else
                {
                    // Обновление существующего ответственного лица
                    var responsibleToUpdate = await context.ResponsiblePersons.FindAsync(_currentResponsible.Id);

                    if (responsibleToUpdate != null)
                    {
                        responsibleToUpdate.LastName = LastNameTextBox.Text.Trim();
                        responsibleToUpdate.FirstName = FirstNameTextBox.Text.Trim();
                        responsibleToUpdate.MiddleName = string.IsNullOrWhiteSpace(MiddleNameTextBox.Text) ? null : MiddleNameTextBox.Text.Trim();
                        responsibleToUpdate.Position = PositionTextBox.Text.Trim();
                        responsibleToUpdate.Phone = string.IsNullOrWhiteSpace(PhoneTextBox.Text) ? null : PhoneTextBox.Text.Trim();
                        responsibleToUpdate.Email = string.IsNullOrWhiteSpace(EmailTextBox.Text) ? null : EmailTextBox.Text.Trim();
                        responsibleToUpdate.UpdatedAt = DateTime.Now;
                    }
                }

                await context.SaveChangesAsync();

                // Показываем сообщение об успехе
                var successBox = MessageBoxManager.GetMessageBoxStandard(
                    "Успех",
                    _currentResponsible == null ? "Ответственное лицо успешно добавлено" : "Ответственное лицо успешно обновлено",
                    ButtonEnum.Ok);
                await successBox.ShowAsync();

                Close();
            }
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = $"Ошибка при сохранении: {ex.Message}";
        }
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}