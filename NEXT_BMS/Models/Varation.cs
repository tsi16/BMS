using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NEXT_BMS.Models;

public partial class Varation
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    public int VarationTypeId { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    [InverseProperty("Varation")]
    public virtual ICollection<ItemEntryVaration> ItemEntryVarations { get; set; } = new List<ItemEntryVaration>();

    [ForeignKey("VarationTypeId")]
    [InverseProperty("Varations")]
    public virtual VarationType VarationType { get; set; }
}
