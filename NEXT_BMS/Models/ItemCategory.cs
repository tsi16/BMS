﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NEXT_BMS.Models;

public partial class ItemCategory
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    public int UserId { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    [InverseProperty("ItemCategory")]
    public virtual ICollection<Item> Items { get; set; } = new List<Item>();

    [ForeignKey("UserId")]
    [InverseProperty("ItemCategories")]
    public virtual User User { get; set; }
}
