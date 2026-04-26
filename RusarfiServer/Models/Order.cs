using System.ComponentModel.DataAnnotations;

namespace RusarfiServer.Models;

public sealed class Order
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Total { get; set; }

    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = "Pendiente";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}