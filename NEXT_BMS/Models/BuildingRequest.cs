using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NEXT_BMS.Models;

public partial class BuildingRequest
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [Required]
    [StringLength(200)]
    public string Description { get; set; }

    [Required]
    [StringLength(20)]
    public string Amount { get; set; }

    public int LocationId { get; set; }

    public int BuildingTypeId { get; set; }

    public int RequestStatusId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime RequestedDate { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    [ForeignKey("BuildingTypeId")]
    [InverseProperty("BuildingRequests")]
    public virtual BuildingType BuildingType { get; set; }

    [ForeignKey("LocationId")]
    [InverseProperty("BuildingRequests")]
    public virtual Location Location { get; set; }

    [ForeignKey("RequestStatusId")]
    [InverseProperty("BuildingRequests")]
    public virtual BuildingRequestStatus RequestStatus { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("BuildingRequests")]
    public virtual User User { get; set; }
}
