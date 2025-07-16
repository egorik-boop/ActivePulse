using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ActivePulse.Entities;

public partial class ProductsInOrder
{
    [Key]
    public int ProductId { get; set; }

    [Key]
    public int OrderId { get; set; }

    public int Count { get; set; }

    [ForeignKey("OrderId")]
    public virtual Order Order { get; set; } = null!;

    [ForeignKey("ProductId")]
    public virtual Product Product { get; set; } = null!;
}