using OIMS.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OIMS.Domain.Entities
{
    [Table("order_status_history")]
    public class OrderStatusHistory
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("order_id")]
        public int OrderId { get; set; }

        [Required]
        [Column("status")]
        public OrderStatus Status { get; set; }

        [Column("changed_by")]
        public int? ChangedBy { get; set; }

        [Column("changed_at")]
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        [ForeignKey("ChangedBy")]
        public virtual User? ChangedByUser { get; set; }
    }
}