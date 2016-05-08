using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilentAuction.Models
{
    public class Payment
    {
        public int ID { get; set; }

        [ForeignKey("User")]
        [Required]
        public string UserID { get; set; }

        [Required]
        public virtual ApplicationUser User { get; set; }


        [ForeignKey("LoggedByUser")]
        [Required]
        public string LoggedBy { get; set; }

        [Required]
        public virtual ApplicationUser LoggedByUser { get; set; }


        [Required]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Datetime { get; set; }
    }
}
