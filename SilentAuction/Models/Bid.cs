using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SilentAuction.Models
{
    public enum BidStatus
    {
        /// <summary>Bid was successful and is the currently active bid</summary>
        Open = 0,
        
        /// <summary>Bid was once open but a higher bid has since been made</summary>
        Superceeded = 1,

        /// <summary>Bid was too low at the time it was made. It was never open.</summary>
        TooLow = 2,
        
        /// <summary>User deleted the bid</summary>
        Deleted = 3         //TODO allow users to delete their previous bid
    }

    public class Bid
    {
        [Key]
        [Required]
        public int BidId { get; set; }

        [DataType(DataType.Currency)]
        [Required]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        [Required]
        public BidStatus Status { get; set; }

        [ForeignKey("User")]
        [Required]
        public string UserId { get; set; }

        [Required]
        public virtual ApplicationUser User { get; set; }

        [ForeignKey("Lot")]
        public int LotId { get; set; }

        [Required]
        public virtual Lot Lot { get; set; }

    }
}