using SilentAuction.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace SilentAuction.Classes
{
    public class KeyedCollectionLots : KeyedCollection<int, Lot>
    {
        protected override int GetKeyForItem(Lot lot)
        {
            return lot.LotId;
        }
    }
}