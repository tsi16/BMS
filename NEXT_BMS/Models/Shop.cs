using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NEXT_BMS.Models;

public partial class Shop
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    public int UserId { get; set; }

    public int BusinessAreaId { get; set; }

    [Required]
    [StringLength(100)]
    public string Description { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedDate { get; set; }

    [StringLength(250)]
    public string ImagUrl { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    [ForeignKey("BusinessAreaId")]
    [InverseProperty("Shops")]
    public virtual BusinessArea BusinessArea { get; set; }

    [InverseProperty("Shop")]
    public virtual ICollection<ShopImage> ShopImages { get; set; } = new List<ShopImage>();

    [InverseProperty("Shop")]
    public virtual ICollection<ShopItem> ShopItems { get; set; } = new List<ShopItem>();

    [InverseProperty("Shop")]
    public virtual ICollection<ShopLocation> ShopLocations { get; set; } = new List<ShopLocation>();

    [ForeignKey("UserId")]
    [InverseProperty("Shops")]
    public virtual User User { get; set; }
}
