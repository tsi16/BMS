using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NEXT_BMS.Models;

public partial class RoomRentalPaymentDetail
{
    [Key]
    public int Id { get; set; }

    public int RoomRentalPaymentId { get; set; }

    public int MonthId { get; set; }

    public int YearId { get; set; }

    public double TotalAmount { get; set; }

    public int AccceptedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime Date { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    [ForeignKey("AccceptedBy")]
    [InverseProperty("RoomRentalPaymentDetails")]
    public virtual User AccceptedByNavigation { get; set; }

    [ForeignKey("MonthId")]
    [InverseProperty("RoomRentalPaymentDetails")]
    public virtual Month Month { get; set; }

    [ForeignKey("RoomRentalPaymentId")]
    [InverseProperty("RoomRentalPaymentDetails")]
    public virtual RoomRentalPayment RoomRentalPayment { get; set; }

    [ForeignKey("YearId")]
    [InverseProperty("RoomRentalPaymentDetails")]
    public virtual Year Year { get; set; }
}
