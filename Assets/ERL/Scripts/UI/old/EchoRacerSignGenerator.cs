
using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace ERL.UI
{
    public class EchoRacerSignGenerator : MonoBehaviour
    {
        void Start()
        {
            CreateSign();
        }

        void CreateSign()
        {
            // Create parent GameObject
            GameObject signObject = new GameObject("EchoRacerSign");
            EchoRacerSign signData = signObject.AddComponent<EchoRacerSign>();

            // Define colors from the image
            Color outerStructureColor = new Color(35 / 255f, 35 / 255f, 37 / 255f);
            Color innerStructureColor = new Color(24 / 255f, 25 / 255f, 28 / 255f);
            Color textColor = new Color(201 / 255f, 201 / 255f, 201 / 255f);
            Color nitroArenaColor = new Color(255 / 255f, 165 / 255f, 0 / 255f); // Orange
            Color urbanCircuitColor = new Color(138 / 255f, 43 / 255f, 226 / 255f); // BlueViolet
            Color desertRallyColor = new Color(255 / 255f, 215 / 255f, 0 / 255f);   // Gold
            Color coastalRunColor = new Color(0 / 255f, 191 / 255f, 255 / 255f);   // DeepSkyBlue

            // Create materials
            Material outerMaterial = new Material(Shader.Find("Standard"));
            outerMaterial.color = outerStructureColor;
            Material innerMaterial = new Material(Shader.Find("Standard"));
            innerMaterial.color = innerStructureColor;

            // Create outer structure
            GameObject outerStructure = GameObject.CreatePrimitive(PrimitiveType.Cube);
            outerStructure.name = "OuterStructure";
            outerStructure.transform.SetParent(signObject.transform);
            outerStructure.transform.localPosition = Vector3.zero;
            outerStructure.transform.localScale = new Vector3(5, 7, 0.2f);
            outerStructure.GetComponent<Renderer>().material = outerMaterial;

            // Create inner structure
            GameObject innerStructure = GameObject.CreatePrimitive(PrimitiveType.Cube);
            innerStructure.name = "InnerStructure";
            innerStructure.transform.SetParent(signObject.transform);
            innerStructure.transform.localPosition = new Vector3(0, 0, 0.1f);
            innerStructure.transform.localScale = new Vector3(4.8f, 6.8f, 0.2f);
            innerStructure.GetComponent<Renderer>().material = innerMaterial;

            // Race Information
            signData.raceInfos = new List<RaceInfo>
            {
                new RaceInfo { platform = "PLATFORM 4", raceName = "NITRO ARENA", currentRacers = 6, maxRacers = 10, status = "SKILL CHALLENGE", titleColor = nitroArenaColor },
                new RaceInfo { platform = "PLATFORM 3", raceName = "URBAN CIRCUIT", currentRacers = 8, maxRacers = 8, status = "IN RACE - LAP 2/3", titleColor = urbanCircuitColor },
                new RaceInfo { platform = "PLATFORM 2", raceName = "DESERT RALLY", currentRacers = 3, maxRacers = 5, status = "TIME TRIAL", titleColor = desertRallyColor },
                new RaceInfo { platform = "PLATFORM 1", raceName = "COASTAL RUN", currentRacers = 4, maxRacers = 8, status = "PRACTICE", titleColor = coastalRunColor }
            };

            float yPos = 2.5f;
            for (int i = 0; i < signData.raceInfos.Count; i++)
            {
                RaceInfo info = signData.raceInfos[i];

                // Create platform container
                GameObject platformObject = new GameObject("Platform" + (4 - i));
                platformObject.transform.SetParent(signObject.transform);
                platformObject.transform.localPosition = new Vector3(0, yPos, 0.2f);

                // Create text elements
                CreateTextElement(platformObject, "PlatformText", info.platform, new Vector3(0, 0.6f, 0), 0.2f, textColor);
                CreateTextElement(platformObject, "RaceNameText", info.raceName, new Vector3(0, 0.3f, 0), 0.4f, info.titleColor);
                CreateTextElement(platformObject, "RacersText", "RACERS: " + info.currentRacers + "/" + info.maxRacers, new Vector3(-0.5f, -0.1f, 0), 0.2f, textColor, TextAlignmentOptions.Left);
                CreateTextElement(platformObject, "StatusText", "STATUS: " + info.status, new Vector3(-0.5f, -0.4f, 0), 0.2f, textColor, TextAlignmentOptions.Left);

                yPos -= 1.7f;
            }
        }

        void CreateTextElement(GameObject parent, string name, string text, Vector3 position, float fontSize, Color color, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent.transform);
            TextMeshProUGUI textMesh = textObject.AddComponent<TextMeshProUGUI>();
            textMesh.text = text;
            textMesh.fontSize = fontSize;
            textMesh.color = color;
            textMesh.alignment = alignment;
            RectTransform rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.localPosition = position;
            rectTransform.sizeDelta = new Vector2(4, 1);
        }
    }
}
