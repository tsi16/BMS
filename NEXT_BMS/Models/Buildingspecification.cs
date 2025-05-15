using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NEXT_BMS.Models;

public partial class Buildingspecification
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    public int UseTypeId { get; set; }

    public int NumberOfFloor { get; set; }

    public int CityId { get; set; }

    public int LocationId { get; set; }

    public int BuildingRequestId { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }
}
