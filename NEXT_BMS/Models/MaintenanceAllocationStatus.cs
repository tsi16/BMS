using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NEXT_BMS.Models;

public partial class MaintenanceAllocationStatus
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(30)]
    public string Name { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    [InverseProperty("AllocationStatus")]
    public virtual ICollection<MaintenanceRequestAllocation> MaintenanceRequestAllocations { get; set; } = new List<MaintenanceRequestAllocation>();
}
