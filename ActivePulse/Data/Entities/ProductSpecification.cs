using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ActivePulse.Entities;

public class ProductSpecification
{
    [Key]
    public int ProductId { get; set; }
    public string Color { get; set; }
    public string Material { get; set; }
    public decimal? Weight { get; set; }
    public string Size { get; set; }
    public string Gender { get; set; }

    [ForeignKey("ProductId")]
    public Product Product { get; set; }
}
