// 10/16/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;

namespace EchoRacer
{
    public class EchoRacerSignInfoTester : MonoBehaviour
    {
        public EchoRacerSignView signView;

        void Start()
        {
            // Create a sample EchoRacerSignInfo object
            EchoRacerSignInfo signInfo = new EchoRacerSignInfo
            {
                raceInfoList = new System.Collections.Generic.List<RaceInfo>
                {
                    new RaceInfo
                    {
                        platform = "PC",
                        raceName = "Speedway Challenge",
                        currentRacers = 8,
                        maxRacers = 12,
                        status = "Ongoing"
                    },
                    new RaceInfo
                    {
                        platform = "Console",
                        raceName = "Mountain Dash",
                        currentRacers = 5,
                        maxRacers = 10,
                        status = "Waiting"
                    },
                    new RaceInfo
                    {
                        platform = "Mobile",
                        raceName = "City Sprint",
                        currentRacers = 10,
                        maxRacers = 10,
                        status = "Full"
                    },
                    new RaceInfo
                    {
                        platform = "Please",
                        raceName = "City Please",
                        currentRacers = 1,
                        maxRacers = 10,
                        status = "Waitimg"
                    }
                }
            };

            // Test the UpdateSign method
            if (signView != null)
            {
                signView.UpdateSign(signInfo);
            }
            else
            {
                Debug.LogError("SignView is not assigned in the Inspector.");
            }
        }
    }
}