using SilentAuction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SilentAuction.Classes
{
    // results of an auction for a specific user
    public class UserResults
    {
        public ApplicationUser User { get; set; }

        /// <summary>
        /// Lots whih this user has won
        /// </summary>
        public Dictionary<int, Lot> Lots { get; set; }

        public UserResults()
        {
            Lots = new Dictionary<int, Lot>();
        }

        /// <summary>
        /// Total amount which this user has bid on winning items
        /// </summary>
        /// <returns></returns>
        public decimal TotalExposure()
        {
            decimal amount = 0;
            foreach(KeyValuePair<int, Lot> kvp in Lots)
            {
                Lot lot = kvp.Value;
                amount += lot.HighestBid.Amount;
            }

            return amount;
        }
    }
}