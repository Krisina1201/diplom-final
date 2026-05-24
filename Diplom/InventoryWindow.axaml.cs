using Avalonia.Controls;
using Avalonia.Interactivity;
using Diplom.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace Diplom;

public partial class InventoryWindow : Window
{
    private int? _classroomFilterId;
    private int? _typeFilterId;

    public InventoryWindow()
    {
        InitializeComponent();
        LoadFilters();
        LoadData();
    }

    public InventoryWindow(int? classroomId) : this()
    {
        _classroomFilterId = classroomId;
        if (classroomId.HasValue)
        {
            // Устанавливаем фильтр в комбобокс после загрузки данных
            this.Loaded += (s, e) => {
                if (ClassroomFilterComboBox.ItemsSource != null)
                {
                    var items = ClassroomFilterComboBox.ItemsSource.Cast<dynamic>().ToList();
                    var selected = items.FirstOrDefault(i => i.Id == classroomId.Value);
                    ClassroomFilterComboBox.SelectedItem = selected;
                }
            };
        }
    }

    private void LoadFilters()
    {
        using (var context = new KsiptDbContext())
        {
            // Загружаем аудитории для фильтра
            var classrooms = context.Classrooms
                .Where(c => c.IsActive == true)
                .OrderBy(c => c.RoomNumber)
                .Select(c => new { c.Id, DisplayName = $"{c.RoomNumber} - {c.RoomName}" })
                .ToList();

            classrooms.Insert(0, new { Id = 0, DisplayName = "Все аудитории" });
            ClassroomFilterComboBox.ItemsSource = classrooms;
            ClassroomFilterComboBox.DisplayMemberBinding = new Avalonia.Data.Binding("DisplayName");

            // Загружаем типы для фильтра
            var types = context.InventoryTypes
                .OrderBy(t => t.InventoryTypeTitle)
                .Select(t => new { t.InventoryTypeId, t.InventoryTypeTitle })
                .ToList();

            types.Insert(0, new { InventoryTypeId = 0, InventoryTypeTitle = "Все типы" });
            TypeFilterComboBox.ItemsSource = types;
            TypeFilterComboBox.DisplayMemberBinding = new Avalonia.Data.Binding("InventoryTypeTitle");
        }
    }

    private void LoadData()
    {
        using (var context = new KsiptDbContext())
        {
            var query = context.Inventories
                .Include(i => i.Classroom)
                .Include(i => i.ItemTypeNavigation)
                .AsQueryable();

            // Применяем фильтры
            if (_classroomFilterId.HasValue && _classroomFilterId.Value > 0)
            {
                query = query.Where(i => i.ClassroomId == _classroomFilterId.Value);
            }

            if (_typeFilterId.HasValue && _typeFilterId.Value > 0)
            {
                query = query.Where(i => i.ItemType == _typeFilterId.Value);
            }

            var list = query
                .OrderBy(i => i.Classroom.RoomNumber)
                .ThenBy(i => i.ItemName)
                .ToList();

            InventoryListBox.ItemsSource = list;
        }
    }

    private void ClassroomFilter_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ClassroomFilterComboBox.SelectedItem != null)
        {
            var selected = ClassroomFilterComboBox.SelectedItem;
            var id = (int)selected.GetType().GetProperty("Id").GetValue(selected);
            _classroomFilterId = id == 0 ? null : id;
            LoadData();
        }
    }

    private void TypeFilter_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (TypeFilterComboBox.SelectedItem != null)
        {
            var selected = TypeFilterComboBox.SelectedItem;
            var id = (int)selected.GetType().GetProperty("InventoryTypeId").GetValue(selected);
            _typeFilterId = id == 0 ? null : id;
            LoadData();
        }
    }

    private async void AddInventory_Click(object? sender, RoutedEventArgs e)
    {
        var dialog = new InventoryEditWindow();
        dialog.Closed += (s, args) => LoadData();
        await dialog.ShowDialog(this);
    }

    private async void EditInventory_Click(object? sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var inventory = button?.CommandParameter as Inventory;

        if (inventory != null)
        {
            var dialog = new InventoryEditWindow(inventory);
            dialog.Closed += (s, args) => LoadData();
            await dialog.ShowDialog(this);
        }
    }

    private async void DeleteInventory_Click(object? sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var inventory = button?.CommandParameter as Inventory;

        if (inventory != null)
        {
            var messageBox = MessageBoxManager.GetMessageBoxStandard(
                "Подтверждение удаления",
                $"Вы действительно хотите удалить инвентарь '{inventory.ItemName}'?",
                ButtonEnum.YesNo);

            var result = await messageBox.ShowAsync();

            if (result == ButtonResult.Yes)
            {
                try
                {
                    using (var context = new KsiptDbContext())
                    {
                        var inventoryToDelete = await context.Inventories.FindAsync(inventory.Id);
                        if (inventoryToDelete != null)
                        {
                            context.Inventories.Remove(inventoryToDelete);
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
        var classroomWindow = new QrWindow();
        classroomWindow.Show();
        this.Close();
    }
}