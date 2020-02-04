using System;
using System.Collections;
using System.Collections.Generic;
using Rocket.Chat.Net.Models;

namespace RockChat.UWP.Common
{
    public class RoomModelComparer : IComparer, IComparer<RoomModel>
    {
        public int Compare(RoomModel x, RoomModel y)
        {
            return CompareCore(x, y);
        }

        public int Compare(object x, object y)
        {
            if (!(x is RoomModel xmodel) || !(y is RoomModel ymodel))
            {
                return 0;
            }
            return CompareCore(xmodel, ymodel);
        }

        private int CompareCore(RoomModel x, RoomModel y)
        {
            if (ReferenceEquals(x, y))
            {
                return 0;
            }


            return DateTime.Compare(x.RoomsResult.UpdatedAt.ToDateTime(), y.RoomsResult.UpdatedAt.ToDateTime());
        }

    }
}