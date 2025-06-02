using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NEXT_BMS.Models;

public partial class OwnerUser
{
    [Key]
    public int Id { get; set; }

    public int OwnerId { get; set; }

    public int UserId { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    [InverseProperty("OwnerUser")]
    public virtual ICollection<BuildingRequest> BuildingRequests { get; set; } = new List<BuildingRequest>();

    [ForeignKey("OwnerId")]
    [InverseProperty("OwnerUsers")]
    public virtual Owner Owner { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("OwnerUsers")]
    public virtual User User { get; set; }
}
