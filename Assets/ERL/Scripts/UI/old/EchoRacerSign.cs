
using System.Collections.Generic;
using UnityEngine;

namespace ERL.UI
{
    public class EchoRacerSign : MonoBehaviour
    {
        public List<RaceInfo> raceInfos;
    }

    [System.Serializable]
    public class RaceInfo
    {
        public string platform;
        public string raceName;
        public int currentRacers;
        public int maxRacers;
        public string status;
        public Color titleColor;
    }
}
