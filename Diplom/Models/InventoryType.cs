using System;
using System.Collections.Generic;

namespace Diplom.Models;

public partial class InventoryType
{
    public int InventoryTypeId { get; set; }

    public string InventoryTypeTitle { get; set; } = null!;

    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
}
