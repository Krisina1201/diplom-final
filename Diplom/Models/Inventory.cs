using System;
using System.Collections.Generic;

namespace Diplom.Models;

public partial class Inventory
{
    public int Id { get; set; }

    public int ClassroomId { get; set; }

    public string ItemName { get; set; } = null!;

    public int ItemType { get; set; }

    public string? Manufacturer { get; set; }

    public string? Model { get; set; }

    public int? InventoryNumber { get; set; }

    public string? ConditionDescription { get; set; }

    public DateOnly? PurchaseDate { get; set; }

    public DateOnly? WarrantyUntil { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Classroom Classroom { get; set; } = null!;

    public virtual ICollection<InventoryHistory> InventoryHistories { get; set; } = new List<InventoryHistory>();

    public virtual InventoryType ItemTypeNavigation { get; set; } = null!;
}
