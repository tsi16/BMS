﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NEXT_BMS.Models;

public partial class RentalAgreementTermination
{
    [Key]
    public int Id { get; set; }

    public int RoomRentalId { get; set; }

    [Required]
    [StringLength(50)]
    public string Reason { get; set; }

    public int DocumentId { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    [ForeignKey("DocumentId")]
    [InverseProperty("RentalAgreementTerminations")]
    public virtual Documente Document { get; set; }

    [InverseProperty("RentalTermination")]
    public virtual ICollection<RentalTerminationApproval> RentalTerminationApprovals { get; set; } = new List<RentalTerminationApproval>();

    [ForeignKey("RoomRentalId")]
    [InverseProperty("RentalAgreementTerminations")]
    public virtual RoomRental RoomRental { get; set; }
}
