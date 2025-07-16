using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ActivePulse.Entities;

public partial class Order
{
    [Key]
    public int OrderId { get; set; }

    public int CustomerId { get; set; }

    public int EmployeeId { get; set; }

    public int StoreId { get; set; }

    public decimal Price { get; set; }

    public int PaymentMethod { get; set; }

    public int OrderStatus { get; set; }

    public DateOnly OrderDate { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();

    public virtual Employee Employee { get; set; } = null!;

    public virtual OrderStatus OrderStatusNavigation { get; set; } = null!;

    public virtual PaymentMethod PaymentMethodNavigation { get; set; } = null!;

    public virtual Store Store { get; set; } = null!;

    public virtual ICollection<ProductsInOrder> ProductsInOrders { get; set; } = new List<ProductsInOrder>();
}
