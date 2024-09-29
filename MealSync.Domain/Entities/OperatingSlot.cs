﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealSync.Domain.Entities;

[Table("operating_slot")]
public class OperatingSlot : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long ShopId { get; set; }

    public int StartTime { get; set; }

    public int EndTime { get; set; }

    public virtual Shop Shop { get; set; }

    public virtual ICollection<ProductOperatingSlot> ProductOperatingSlots { get; set; } = new List<ProductOperatingSlot>();
}