using Common.Attributes;
using SilentAuction.Common;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SilentAuction.Models
{
    public enum AuctionStatus
    {
        Pending=0,
        Running=1,
        Closed=2,
        Deleted=3,
        Paused=4
    }

	public class Auction
	{
        [Key]
		public int AuctionId { get; set; }

		[Required]
        [DisplayNameLocalized(typeof(SilentAuction.Resources.TextStrings), "AuctionName")]
        [StringLengthLocalized(100, 3, typeof(SilentAuction.Resources.TextStrings), "ValidationErrorMinLength")]
        [DataType(DataType.Text)]
        public string Name { get; set; }


        [DisplayNameLocalized(typeof(SilentAuction.Resources.TextStrings), "AuctionDescription")]
        [StringLengthLocalized(1000, 0, typeof(SilentAuction.Resources.TextStrings), "ValidationErrorMinLength")]
		[DataType(DataType.Text)]
		public string Description { get; set; }

        [Required]
        [DisplayNameLocalized(typeof(SilentAuction.Resources.TextStrings), "ReserveUse")]
        public bool UseReserves { get; set; }

        [Required]
        public AuctionStatus Status { get; set; }

        public virtual ImageFile Image { get; set; }
        public virtual ICollection<Lot> Lots { get; set; }


        public Dictionary<int, Lot> LotsByKey()
        {
            Dictionary<int, Lot> result = Lots.ToDictionary(l=> l.LotId);
            return result;
        }

        public SortedList<int, Lot> LotsSorted()
        {
            SortedList<int, Lot> result = new SortedList<int, Lot>();
            foreach(Lot lot in Lots)
            {
                result.Add((int)lot.AuctionOrder, lot);
            }

            return result;
        }


        public string StatusIcon(int height)
        {
            const string statusIconPath = "/Images/StatusIcons/";
            string iconPath = string.Empty;
             
            switch(Status)
            {
                case AuctionStatus.Closed:
                    iconPath = statusIconPath + "Closed.png";
                    break;
                case AuctionStatus.Running:
                    iconPath = statusIconPath + "Running.png";
                    break;
                case AuctionStatus.Pending:
                    iconPath = statusIconPath + "Pending.png";
                    break;

                default:
                    iconPath = "/Images/NoImage.png";
                    break;
            }

            return SilentAuction.Common.Utils.GetThumbnail(iconPath, height);
        }

        public string StatusText
        {
            get
            {
                switch(Status)
                {
                    case AuctionStatus.Closed:
                        return Resources.TextStrings.AuctionStatusClosed;
                    case AuctionStatus.Deleted:
                        return Resources.TextStrings.AuctionStatusDeleted;
                    case AuctionStatus.Pending:
                        return Resources.TextStrings.AuctionStatusPending;
                    case AuctionStatus.Running:
                        return Resources.TextStrings.AuctionStatusRunning;
                    case AuctionStatus.Paused:
                        return Resources.TextStrings.AuctionStatusPaused;
                    default:
                        return "String not found.";
                }
            }
        }

        /// <summary>
        /// Returns the WINNING bid for a lot. Bid must have met the reserve if reserves are active.
        /// If no winning bid has been made, returns NULL.
        /// </summary>
        /// <param name="lot"></param>
        /// <returns></returns>
        public Bid GetWinningBidForLot(Lot lot)
        {
            if (lot.HighestBid == null)
            {
                // no bids at all for this item.
                return null;
            }

            if (UseReserves)
            {
                if (lot.HighestBid.Amount < lot.Reserve)
                {
                    // nobody reached the reserve
                    return null;
                }
            }

            return lot.HighestBid;
        }
    }
}