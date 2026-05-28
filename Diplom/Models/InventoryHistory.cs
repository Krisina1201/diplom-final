using System;
using System.Collections.Generic;

namespace Diplom.Models;

public partial class InventoryHistory
{
    public int InventoryHistoryId { get; set; }

    public int InventoryId { get; set; }

    public int InitialClassroomId { get; set; }

    public int FinalClassroom { get; set; }

    public DateTime DateOfTransfer { get; set; }

    public int ResponsiblePersonsId { get; set; }

    public virtual Classroom FinalClassroomNavigation { get; set; } = null!;

    public virtual Classroom InitialClassroom { get; set; } = null!;

    public virtual Inventory Inventory { get; set; } = null!;

    public virtual ResponsiblePerson ResponsiblePersons { get; set; } = null!;
}
