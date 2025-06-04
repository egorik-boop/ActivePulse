using ActivePulse.Entities;
using System.ComponentModel.DataAnnotations;

public partial class CardData
{
    [Key]
    public int CardDataId { get; set; }  // Новый первичный ключ

    public int OrderId { get; set; }
    public string CardNumber { get; set; }
    public string CardOwner { get; set; }
    public DateOnly CardPeriod { get; set; }
    public string Cvv { get; set; }

    public virtual Order Order { get; set; }
}