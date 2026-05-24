using System;
using System.Collections.Generic;

namespace Diplom.Models;

public partial class Classroom
{
    public int Id { get; set; }

    public string RoomNumber { get; set; } = null!;

    public string RoomName { get; set; } = null!;

    public string? Description { get; set; }

    public int? Capacity { get; set; }

    public int? ResponsibleId { get; set; }

    public string? QrCodeData { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

    public virtual ResponsiblePerson? Responsible { get; set; }
}
