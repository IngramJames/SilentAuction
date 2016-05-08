using Common.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;

namespace SilentAuction.Models
{
	public class Lot
	{
		public int LotId { get; set; }

        [Required]
		[DisplayNameLocalized(typeof(SilentAuction.Resources.TextStrings), "LotName")]
		[StringLengthLocalized(100, 3, typeof(SilentAuction.Resources.TextStrings), "ValidationErrorMinLength")]
		[DataType(DataType.Text)]
		public string Name { get; set; }

		[DisplayNameLocalized(typeof(SilentAuction.Resources.TextStrings), "LotDescription")]
		[StringLengthLocalized(500, 0, typeof(SilentAuction.Resources.TextStrings), "ValidationErrorMinLength")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

		[DisplayNameLocalized(typeof(SilentAuction.Resources.TextStrings), "LotReserve")]
		public decimal Reserve { get; set; }

		[DisplayNameLocalized(typeof(SilentAuction.Resources.TextStrings), "LotSold")]
		public bool Sold { get; set; }

        [ForeignKey("Auction")]
        public int? AuctionId { get; set; }

        public virtual Auction Auction { get; set; }
		public virtual ICollection<LotImageFile> ImageFiles { get; set; }

        public virtual ICollection<Bid> Bids { get; set; }
        public virtual Bid HighestBid { get; set; }


        // The order that this lot appears in the Auction display
        public int? AuctionOrder { get; set; }

        /// <summary>
        /// Field exists only to be used as a lock. Before updating a Lot, this field must be NULL and then be set to the username of the user performing the action.
        /// It must then be checked to verify that it is the expected username.
        /// </summary>
        public string LockedByUser { get; set; }

        public DateTime? LockedAt { get; set; }

        [NotMapped]
        public ImageFile MainImage
        {
            get
            {
                // TODO: make more efficient.
                foreach(LotImageFile lif in ImageFiles)
                {
                    if(lif.Order==0)
                    {
                        return lif.ImageFile;
                    }
                }

                // Dummy image file pointing to the "no image" graphic
                ImageFile noImage = new ImageFile();
                noImage.LocalRelativePath = "/Images/NoImage.png";
                return noImage;
            }
        }

        public Lot() { }		// required by Entity Model

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name"></param>
		/// <param name="description"></param>
		/// <param name="reserve"></param>
		public Lot(string name, string description, decimal reserve)
		{
			Name = name;
			Description = description;
			Reserve = reserve;
		}

        public static List<Lot> SortLotsByAuctionOrder(IEnumerable<Lot> items)
        {
            List<Lot> sorted = new List<Lot>();
            foreach(Lot lot in items)
            {
                sorted.Add(lot);
            }

            sorted.Sort(delegate (Lot a, Lot b)
            {
                if (a.AuctionOrder == null)
                {
                    if (b.AuctionOrder == null)
                    {
                        // both null: the same.
                        return 0;
                    }
                    // a is null; b is not. a is lower.
                    return -1;
                }
 
                // b is null: it is lower
                if (b.AuctionOrder == null)
                {
                    return 1;
                }

                return (int)a.AuctionOrder - (int)b.AuctionOrder;
            });

            return sorted;
        }
	}
}