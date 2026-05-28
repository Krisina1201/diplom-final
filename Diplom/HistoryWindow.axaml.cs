using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Avalonia.Interactivity;
using Diplom.Models;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.Measure;
using SkiaSharp;

namespace Diplom;

public partial class HistoryWindow : Window
{
    private readonly KsiptDbContext _context;
    private ObservableCollection<MovementRecord> _movements = new();
    private ObservableCollection<ISeries> _series;

    public class MovementRecord
    {
        public int Id { get; set; }
        public int? InventoryNumber { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string InventoryType { get; set; } = string.Empty;
        public string FromClassroom { get; set; } = string.Empty;
        public string ToClassroom { get; set; } = string.Empty;
        public string MovementDate { get; set; } = string.Empty;
        public string ResponsiblePerson { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public HistoryWindow()
    {
        InitializeComponent();
        DataContext = this;
        _context = new KsiptDbContext();

        // Инициализация графика
        _series = new ObservableCollection<ISeries>();

        Loaded += async (s, e) => await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        await LoadFilters();
        await LoadMovementsHistory();
    }

    private async Task LoadFilters()
    {
        try
        {
            // Загрузка кабинетов
            var classrooms = await _context.Classrooms
                .Where(c => c.IsActive == true)
                .OrderBy(c => c.RoomNumber)
                .ToListAsync();

            ClassroomFilterComboBox.Items.Clear();
            ClassroomFilterComboBox.Items.Add(new ComboBoxItem { Content = "Все кабинеты", Tag = null });

            foreach (var classroom in classrooms)
            {
                ClassroomFilterComboBox.Items.Add(new ComboBoxItem
                {
                    Content = $"{classroom.RoomNumber} - {classroom.RoomName}",
                    Tag = classroom.Id
                });
            }
            ClassroomFilterComboBox.SelectedIndex = 0;

            // Загрузка типов оборудования
            var inventoryTypes = await _context.InventoryTypes
                .OrderBy(t => t.InventoryTypeTitle)
                .ToListAsync();

            InventoryTypeFilterComboBox.Items.Clear();
            InventoryTypeFilterComboBox.Items.Add(new ComboBoxItem { Content = "Все типы", Tag = null });

            foreach (var type in inventoryTypes)
            {
                InventoryTypeFilterComboBox.Items.Add(new ComboBoxItem
                {
                    Content = type.InventoryTypeTitle,
                    Tag = type.InventoryTypeId
                });
            }
            InventoryTypeFilterComboBox.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            await ShowMessage($"Ошибка загрузки фильтров: {ex.Message}");
        }
    }

    private async Task LoadMovementsHistory()
    {
        try
        {
            var query = _context.InventoryHistories
                .Include(h => h.Inventory)
                    .ThenInclude(i => i != null ? i.ItemTypeNavigation : null)
                .Include(h => h.InitialClassroom)
                .Include(h => h.FinalClassroomNavigation)
                .Include(h => h.ResponsiblePersons)
                .OrderByDescending(h => h.DateOfTransfer)
                .AsQueryable();

            query = ApplyFilters(query);
            var movements = await query.Take(500).ToListAsync();

            System.Diagnostics.Debug.WriteLine($"Найдено записей: {movements.Count}");

            _movements.Clear();

            if (movements.Count == 0)
            {
                MovementsDataGrid.ItemsSource = null;
                await ShowMessage("Нет данных для отображения. Проверьте фильтры или наличие записей в базе.");
                return;
            }

            foreach (var h in movements)
            {
                var record = new MovementRecord
                {
                    Id = h.InventoryHistoryId,
                    InventoryNumber = h.Inventory?.InventoryNumber,
                    ItemName = h.Inventory?.ItemName ?? "Неизвестно",
                    InventoryType = h.Inventory?.ItemTypeNavigation?.InventoryTypeTitle ?? "Неизвестно",
                    FromClassroom = h.InitialClassroom != null
                        ? $"{h.InitialClassroom.RoomNumber} - {h.InitialClassroom.RoomName}"
                        : "Не указан",
                    ToClassroom = h.FinalClassroomNavigation != null
                        ? $"{h.FinalClassroomNavigation.RoomNumber} - {h.FinalClassroomNavigation.RoomName}"
                        : "Не указан",
                    MovementDate = h.DateOfTransfer.ToString("dd.MM.yyyy HH:mm"),
                    ResponsiblePerson = h.ResponsiblePersons != null
                        ? $"{h.ResponsiblePersons.LastName} {h.ResponsiblePersons.FirstName} {h.ResponsiblePersons.MiddleName}".Trim()
                        : "Не указан",
                    Status = "Завершено"
                };
                _movements.Add(record);
            }

            MovementsDataGrid.ItemsSource = _movements;
            await UpdateStatistics(movements);
        }
        catch (Exception ex)
        {
            await ShowMessage($"Ошибка загрузки истории: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Ошибка: {ex.ToString()}");
        }
    }

    private IQueryable<InventoryHistory> ApplyFilters(IQueryable<InventoryHistory> query)
    {
        // Фильтр по периоду
        if (PeriodComboBox.SelectedItem is ComboBoxItem periodItem &&
            periodItem.Content?.ToString() != "За всё время")
        {
            var days = periodItem.Content?.ToString() switch
            {
                "Последние 30 дней" => 30,
                "Последние 90 дней" => 90,
                "Последний год" => 365,
                _ => 0
            };

            if (days > 0)
            {
                var cutoffDate = DateTime.Now.AddDays(-days);
                query = query.Where(h => h.DateOfTransfer >= cutoffDate);
            }
        }

        // Фильтр по кабинету
        if (ClassroomFilterComboBox.SelectedItem is ComboBoxItem classroomItem &&
            classroomItem.Tag != null)
        {
            var classroomId = (int)classroomItem.Tag;
            query = query.Where(h => h.FinalClassroom == classroomId ||
                                      h.InitialClassroomId == classroomId);
        }

        // Фильтр по типу оборудования
        if (InventoryTypeFilterComboBox.SelectedItem is ComboBoxItem typeItem &&
            typeItem.Tag != null)
        {
            var typeId = (int)typeItem.Tag;
            query = query.Where(h => h.Inventory != null && h.Inventory.ItemType == typeId);
        }

        // Поиск
        if (!string.IsNullOrWhiteSpace(SearchTextBox?.Text))
        {
            var searchText = SearchTextBox.Text.ToLower();
            query = query.Where(h => h.Inventory != null &&
                (h.Inventory.InventoryNumber.ToString().Contains(searchText) ||
                 h.Inventory.ItemName.ToLower().Contains(searchText)));
        }

        return query;
    }

    private async Task UpdateStatistics(List<InventoryHistory> movements)
    {
        try
        {
            if (TotalMovementsText != null)
                TotalMovementsText.Text = movements.Count.ToString();

            var uniqueEquipment = movements.Select(m => m.InventoryId).Distinct().Count();
            if (UniqueEquipmentText != null)
                UniqueEquipmentText.Text = uniqueEquipment.ToString();

            var allClassrooms = movements.SelectMany(m => new[] { m.InitialClassroomId, m.FinalClassroom })
                .Distinct()
                .Count();
            if (UniqueClassroomsText != null)
                UniqueClassroomsText.Text = allClassrooms.ToString();

            var mostActive = movements
                .GroupBy(m => m.FinalClassroom)
                .Select(g => new { ClassroomId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefault();

            if (mostActive != null && mostActive.ClassroomId > 0 && MostActiveClassroomText != null)
            {
                var classroom = await _context.Classrooms
                    .FirstOrDefaultAsync(c => c.Id == mostActive.ClassroomId);
                MostActiveClassroomText.Text = classroom != null ? $"{classroom.RoomNumber}" : "—";
            }
            else if (MostActiveClassroomText != null)
            {
                MostActiveClassroomText.Text = "—";
            }

            await UpdateChart(movements);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка обновления статистики: {ex.Message}");
        }
    }

    private async Task UpdateChart(List<InventoryHistory> movements)
    {
        try
        {
            var monthlyStats = movements
                .GroupBy(m => new { m.DateOfTransfer.Year, m.DateOfTransfer.Month })
                .Select(g => new
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                    Count = g.Count()
                })
                .OrderBy(m => m.Month)
                .ToList();

            if (monthlyStats.Any() && MovementsChart != null)
            {
                _series.Clear();

                var values = monthlyStats.Select(m => (double)m.Count).ToArray();
                var labels = monthlyStats.Select(m => m.Month.ToString("MMM yyyy")).ToArray(); // Названия месяцев

                // Создаем колонки с данными
                var columnSeries = new ColumnSeries<double>
                {
                    Values = values,
                    Name = "Количество перемещений",
                    Fill = new SolidColorPaint(SKColors.SteelBlue)
                };

                _series.Add(columnSeries);

                // Настраиваем ось X с названиями месяцев
                MovementsChart.XAxes = new[]
                {
                new Axis
                {
                    Name = "Месяцы",
                    Labels = labels, // Вот здесь передаем названия месяцев
                    LabelsRotation = 45, // Поворачиваем подписи на 45 градусов, чтобы не налезали
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray)
                }
            };

                // Настраиваем ось Y
                MovementsChart.YAxes = new[]
                {
                new Axis
                {
                    Name = "Количество перемещений",
                    Labeler = (value) => value.ToString("0"),
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray)
                }
            };

                MovementsChart.Series = _series;

                System.Diagnostics.Debug.WriteLine($"График обновлен: {values.Length} месяцев");
                foreach (var label in labels)
                {
                    System.Diagnostics.Debug.WriteLine($"Месяц: {label}");
                }
            }
            else if (MovementsChart != null)
            {
                _series.Clear();
                MovementsChart.Series = _series;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки графика: {ex.Message}");
        }
    }

    // Обработчики событий
    private async void ApplyFilters_Click(object? sender, RoutedEventArgs e)
    {
        await LoadMovementsHistory();
    }

    private async void ResetFilters_Click(object? sender, RoutedEventArgs e)
    {
        if (PeriodComboBox != null)
            PeriodComboBox.SelectedIndex = 0;
        if (ClassroomFilterComboBox != null)
            ClassroomFilterComboBox.SelectedIndex = 0;
        if (InventoryTypeFilterComboBox != null)
            InventoryTypeFilterComboBox.SelectedIndex = 0;
        if (SearchTextBox != null)
            SearchTextBox.Text = "";

        await LoadMovementsHistory();
    }

    private async void SearchTextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        await LoadMovementsHistory();
    }

    private async void PeriodComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        await LoadMovementsHistory();
    }

    private async void ClassroomFilter_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        await LoadMovementsHistory();
    }

    private async void InventoryTypeFilter_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        await LoadMovementsHistory();
    }

    private async void ExportToExcel_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (_movements == null || !_movements.Any())
            {
                await ShowMessage("Нет данных для экспорта");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("Инвентарный номер;Оборудование;Тип;Из кабинета;В кабинет;Дата перемещения;Ответственный;Статус");

            foreach (var item in _movements)
            {
                sb.AppendLine($"{item.InventoryNumber};{item.ItemName};{item.InventoryType};{item.FromClassroom};{item.ToClassroom};{item.MovementDate};{item.ResponsiblePerson};{item.Status}");
            }

            var saveDialog = new SaveFileDialog
            {
                Title = "Сохранить отчет",
                DefaultExtension = "csv",
                Filters = new List<FileDialogFilter>
                {
                    new() { Name = "CSV файлы", Extensions = { "csv" } }
                }
            };

            var result = await saveDialog.ShowAsync(this);

            if (!string.IsNullOrEmpty(result))
            {
                await File.WriteAllTextAsync(result, sb.ToString(), Encoding.UTF8);
                await ShowMessage($"Отчет сохранен в файл: {result}");
            }
        }
        catch (Exception ex)
        {
            await ShowMessage($"Ошибка при экспорте: {ex.Message}");
        }
    }

    private void BackButton_Click(object? sender, RoutedEventArgs e)
    {
        QrWindow qrWindow = new QrWindow();
        qrWindow.Show();
        Close();
    }

    private async Task ShowMessage(string message)
    {
        var dialog = new Window
        {
            Title = "Информация",
            Width = 350,
            Height = 180,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            Content = new StackPanel
            {
                Margin = new Avalonia.Thickness(20),
                Spacing = 15,
                Children =
                {
                    new TextBlock
                    {
                        Text = message,
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                        TextAlignment = Avalonia.Media.TextAlignment.Center
                    },
                    new Button
                    {
                        Content = "OK",
                        Width = 80,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                    }
                }
            }
        };

        var button = (Button)((StackPanel)dialog.Content).Children[1];
        button.Click += (s, args) => dialog.Close();

        await dialog.ShowDialog(this);
    }
}