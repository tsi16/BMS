using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NEXT_BMS.Models;

public partial class BuildingEmployee
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string FullName { get; set; }

    [Required]
    [StringLength(50)]
    public string PhoneNumber { get; set; }

    public int BuildingId { get; set; }

    public int? UserId { get; set; }

    public int EmployeeTypeId { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    [ForeignKey("BuildingId")]
    [InverseProperty("BuildingEmployees")]
    public virtual Building Building { get; set; }

    [ForeignKey("EmployeeTypeId")]
    [InverseProperty("BuildingEmployees")]
    public virtual EmployeeType EmployeeType { get; set; }

    [InverseProperty("BuildingEmployee")]
    public virtual ICollection<MaintenanceRequestAllocation> MaintenanceRequestAllocations { get; set; } = new List<MaintenanceRequestAllocation>();

    [ForeignKey("UserId")]
    [InverseProperty("BuildingEmployees")]
    public virtual User User { get; set; }
}
