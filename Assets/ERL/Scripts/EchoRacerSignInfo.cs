
using System.Collections.Generic;

namespace EchoRacer
{
    public class EchoRacerSignInfo
    {
        public List<RaceInfo> raceInfoList;
    }

    [System.Serializable]
    public class RaceInfo
    {
        public string platform;
        public string raceName;
        public int currentRacers;
        public int maxRacers;
        public string status;
    }
}
