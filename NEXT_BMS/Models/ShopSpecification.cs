using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NEXT_BMS.Models;

public partial class ShopSpecification
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    public int ShopRequestId { get; set; }

    public int UseTypeId { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    [ForeignKey("ShopRequestId")]
    [InverseProperty("ShopSpecifications")]
    public virtual ShopRequest ShopRequest { get; set; }

    [ForeignKey("UseTypeId")]
    [InverseProperty("ShopSpecifications")]
    public virtual UseType UseType { get; set; }
}
