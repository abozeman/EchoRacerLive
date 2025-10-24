
using TMPro;
using UnityEngine;

namespace EchoRacer
{
    public class EchoRacerSignView : MonoBehaviour
    {
        public TextMeshProUGUI[] platformTexts;
        public TextMeshProUGUI[] raceNameTexts;
        public TextMeshProUGUI[] racerCountTexts;
        public TextMeshProUGUI[] statusTexts;

        public void UpdateSign(EchoRacerSignInfo signInfo)
        {
            Debug.Log("UpdateSign called");
            if (signInfo == null)
            {
                Debug.LogError("signInfo is null");
                return;
            }

            if (signInfo.raceInfoList == null)
            {
                Debug.LogError("signInfo.raceInfoList is null");
                return;
            }
            
            Debug.Log($"signInfo.raceInfoList.Count: {signInfo.raceInfoList.Count}");

            if (platformTexts == null || platformTexts.Length == 0)
            {
                Debug.LogError("platformTexts is null or empty");
                return;
            }

            for (int i = 0; i < signInfo.raceInfoList.Count; i++)
            {
                if (i < platformTexts.Length && platformTexts[i] != null)
                {
                    platformTexts[i].text = signInfo.raceInfoList[i].platform;
                    Debug.Log($"Setting platform text for index {i} to: {signInfo.raceInfoList[i].platform}");
                }

                if (i < raceNameTexts.Length && raceNameTexts[i] != null)
                {
                    raceNameTexts[i].text = signInfo.raceInfoList[i].raceName;
                }

                if (i < racerCountTexts.Length && racerCountTexts[i] != null)
                {
                    racerCountTexts[i].text = $"{signInfo.raceInfoList[i].currentRacers}/{signInfo.raceInfoList[i].maxRacers}";
                }

                if (i < statusTexts.Length && statusTexts[i] != null)
                {
                    statusTexts[i].text = signInfo.raceInfoList[i].status;
                }
            }
        }
    }
}
