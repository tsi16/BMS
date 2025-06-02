using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NEXT_BMS.Models;

public partial class ShopImage
{
    [Key]
    public int Id { get; set; }

    public int ShopId { get; set; }

    [Required]
    [StringLength(250)]
    public string ImageUrl { get; set; }

    [Required]
    [StringLength(150)]
    public string Description { get; set; }

    public bool? IsProfile { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    [ForeignKey("ShopId")]
    [InverseProperty("ShopImages")]
    public virtual Shop Shop { get; set; }
}
