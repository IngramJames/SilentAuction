using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using SilentAuction.Models;

namespace SilentAuction.Models
{
	
	public class LotImageFile : IComparable
	{
		[Key, Column(Order = 0)]
		public int LotId { get; set; }

		[Key, Column(Order = 1)]
		public int ImageFileId { get; set; }

		public virtual Lot Lot { get; set; }
		public virtual ImageFile ImageFile { get; set; }

		public int Order { get; set; }

        public int CompareTo(object other)
        {
            LotImageFile o = (LotImageFile)other;
            return Order - o.Order;
        }

        public static List<LotImageFile> SortByOrder(IEnumerable<LotImageFile> items)
        {
            List<LotImageFile> sorted = new List<LotImageFile>();
            foreach (LotImageFile imageFile in items)
            {
                sorted.Add(imageFile);
            }

            sorted.Sort(delegate (LotImageFile a, LotImageFile b)
            {
                return a.Order- b.Order;
            });

            return sorted;

        }

    }

}