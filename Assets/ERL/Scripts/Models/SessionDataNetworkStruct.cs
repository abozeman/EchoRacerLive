using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.CryptoKartz.Scripts.Models
{
    public struct SessionDataNetworkStruct : INetworkStruct
    {
        public NetworkString<_16> Name;
        public NetworkString<_16> Trackid;
        public int Level;
        public int PlayerCount;
        public int MaxPlayers;
    }
}
