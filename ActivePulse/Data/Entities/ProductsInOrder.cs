using System.ComponentModel.DataAnnotations;

namespace ActivePulse.Entities;

public partial class ProductsInOrder
{
    [Key]
    public int ProductId { get; set; }

    [Key]
    public int OrderId { get; set; }

    public int Count { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}