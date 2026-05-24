using Avalonia.Controls;
using Avalonia.Interactivity;
using Diplom.Models;
using Microsoft.EntityFrameworkCore;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Linq;

namespace Diplom;

public partial class InventoryEditWindow : Window
{
    private Inventory? _currentInventory;

    public InventoryEditWindow()
    {
        InitializeComponent();
        LoadComboBoxes();
        Title = "Добавление инвентаря";
    }

    public InventoryEditWindow(Inventory inventory)
    {
        InitializeComponent();
        _currentInventory = inventory;
        LoadComboBoxes();
        LoadInventoryData();
        Title = "Редактирование инвентаря";
    }

    private void LoadComboBoxes()
    {
        using (var context = new KsiptDbContext())
        {
            // Загружаем типы инвентаря
            var types = context.InventoryTypes.Select(e => e.InventoryTypeTitle).ToList();
            ItemTypeComboBox.ItemsSource = types;

            // Загружаем активные аудитории
            var classrooms = context.Classrooms
                .Where(c => c.IsActive == true)
                .Select(c => c.RoomNumber)
                .ToList();

            ClassroomComboBox.ItemsSource = classrooms;

            // Устанавливаем выбранные значения, если редактируем
            if (_currentInventory != null)
            {
                ItemTypeComboBox.SelectedItem = _currentInventory.ItemTypeNavigation.InventoryTypeTitle;

                var classRoomNumber = context.Classrooms.FirstOrDefault(e => e.Id == _currentInventory.ClassroomId)!.RoomNumber;

                ClassroomComboBox.SelectedItem = classRoomNumber;
            }
        }
    }

    private void LoadInventoryData()
    {
        if (_currentInventory != null)
        {
            ItemNameTextBox.Text = _currentInventory.ItemName;
            ManufacturerTextBox.Text = _currentInventory.Manufacturer ?? "";
            ModelTextBox.Text = _currentInventory.Model ?? "";
            InventoryNumberTextBox.Text = _currentInventory.InventoryNumber?.ToString() ?? "0";
            ConditionDescriptionTextBox.Text = _currentInventory.ConditionDescription ?? "";
            NotesTextBox.Text = _currentInventory.Notes ?? "";

            // Устанавливаем даты, если они есть
            if (_currentInventory.PurchaseDate.HasValue)
            {
                PurchaseDatePicker.SelectedDate = _currentInventory.PurchaseDate.Value.ToDateTime(new TimeOnly(0, 0));
            }

            if (_currentInventory.WarrantyUntil.HasValue)
            {
                WarrantyUntilPicker.SelectedDate = _currentInventory.WarrantyUntil.Value.ToDateTime(new TimeOnly(0, 0));
            }
        }
    }
    public class ClassroomItem
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = "";
        public string RoomName { get; set; } = "";
        public string DisplayName => $"{RoomNumber} - {RoomName}";
    }

    private async void SaveButton_Click(object? sender, RoutedEventArgs e)
    {
        // Валидация
        if (string.IsNullOrWhiteSpace(ItemNameTextBox.Text))
        {
            StatusTextBlock.Text = "Наименование предмета обязательно для заполнения";
            return;
        }

        if (ItemTypeComboBox.SelectedItem == null)
        {
            StatusTextBlock.Text = "Выберите тип инвентаря";
            return;
        }

        if (ClassroomComboBox.SelectedItem == null)
        {
            StatusTextBlock.Text = "Выберите аудиторию";
            return;
        }

        try
        {
            using (var context = new KsiptDbContext())
            {
                var selectedType = (InventoryType)ItemTypeComboBox.SelectedItem;
                var selectedClassroom = (ClassroomItem)ClassroomComboBox.SelectedItem;

                // Проверка уникальности инвентарного номера (если указан)
                if (!string.IsNullOrWhiteSpace(InventoryNumberTextBox.Text) &&
                    int.TryParse(InventoryNumberTextBox.Text, out int inventoryNumber))
                {
                    bool isNumberUnique = !await context.Inventories
                        .AnyAsync(i => i.InventoryNumber == inventoryNumber &&
                                       (_currentInventory == null || i.Id != _currentInventory.Id));

                    if (!isNumberUnique)
                    {
                        StatusTextBlock.Text = "Инвентарный номер уже существует";
                        return;
                    }
                }

                if (_currentInventory == null)
                {
                    // Создание нового инвентаря
                    var newInventory = new Inventory
                    {
                        ItemName = ItemNameTextBox.Text,
                        ItemType = selectedType.InventoryTypeId,
                        ClassroomId = selectedClassroom.Id,
                        Manufacturer = string.IsNullOrWhiteSpace(ManufacturerTextBox.Text) ? null : ManufacturerTextBox.Text,
                        Model = string.IsNullOrWhiteSpace(ModelTextBox.Text) ? null : ModelTextBox.Text,
                        ConditionDescription = string.IsNullOrWhiteSpace(ConditionDescriptionTextBox.Text) ? null : ConditionDescriptionTextBox.Text,
                        Notes = string.IsNullOrWhiteSpace(NotesTextBox.Text) ? null : NotesTextBox.Text,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    // Парсим инвентарный номер
                    if (int.TryParse(InventoryNumberTextBox.Text, out int invNum))
                    {
                        newInventory.InventoryNumber = invNum;
                    }

                    // Парсим даты
                    if (PurchaseDatePicker.SelectedDate.HasValue)
                    {
                        newInventory.PurchaseDate = DateOnly.FromDateTime(PurchaseDatePicker.SelectedDate.Value.DateTime);
                    }

                    if (WarrantyUntilPicker.SelectedDate.HasValue)
                    {
                        newInventory.WarrantyUntil = DateOnly.FromDateTime(WarrantyUntilPicker.SelectedDate.Value.DateTime);
                    }

                    context.Inventories.Add(newInventory);
                }
                else
                {
                    // Обновление существующего инвентаря
                    var inventoryToUpdate = await context.Inventories.FindAsync(_currentInventory.Id);

                    if (inventoryToUpdate != null)
                    {
                        inventoryToUpdate.ItemName = ItemNameTextBox.Text;
                        inventoryToUpdate.ItemType = selectedType.InventoryTypeId;
                        inventoryToUpdate.ClassroomId = selectedClassroom.Id;
                        inventoryToUpdate.Manufacturer = string.IsNullOrWhiteSpace(ManufacturerTextBox.Text) ? null : ManufacturerTextBox.Text;
                        inventoryToUpdate.Model = string.IsNullOrWhiteSpace(ModelTextBox.Text) ? null : ModelTextBox.Text;
                        inventoryToUpdate.ConditionDescription = string.IsNullOrWhiteSpace(ConditionDescriptionTextBox.Text) ? null : ConditionDescriptionTextBox.Text;
                        inventoryToUpdate.Notes = string.IsNullOrWhiteSpace(NotesTextBox.Text) ? null : NotesTextBox.Text;
                        inventoryToUpdate.UpdatedAt = DateTime.Now;

                        // Парсим инвентарный номер
                        if (int.TryParse(InventoryNumberTextBox.Text, out int invNum))
                        {
                            inventoryToUpdate.InventoryNumber = invNum;
                        }
                        else
                        {
                            inventoryToUpdate.InventoryNumber = null;
                        }

                        // Парсим даты
                        if (PurchaseDatePicker.SelectedDate.HasValue)
                        {
                            inventoryToUpdate.PurchaseDate = DateOnly.FromDateTime(PurchaseDatePicker.SelectedDate.Value.DateTime);
                        }
                        else
                        {
                            inventoryToUpdate.PurchaseDate = null;
                        }

                        if (WarrantyUntilPicker.SelectedDate.HasValue)
                        {
                            inventoryToUpdate.WarrantyUntil = DateOnly.FromDateTime(WarrantyUntilPicker.SelectedDate.Value.DateTime);
                        }
                        else
                        {
                            inventoryToUpdate.WarrantyUntil = null;
                        }
                    }
                }

                await context.SaveChangesAsync();

                // Показываем сообщение об успехе
                var successBox = MessageBoxManager.GetMessageBoxStandard(
                    "Успех",
                    _currentInventory == null ? "Инвентарь успешно добавлен" : "Инвентарь успешно обновлен",
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