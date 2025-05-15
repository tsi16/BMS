using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NEXT_BMS.Models;

public partial class PasswordResetCode
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(15)]
    public string Code { get; set; }

    [Required]
    [StringLength(50)]
    public string Email { get; set; }

    public int UserId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime SentDate { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public bool IsVerifayed { get; set; }
}
