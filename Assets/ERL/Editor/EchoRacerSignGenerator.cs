
using UnityEditor;
using UnityEngine;
using TMPro;

namespace EchoRacer
{
    public class EchoRacerSignGenerator : EditorWindow
    {
        [MenuItem("Echo Racer/Generate Sign Prefab")]
        public static void GenerateSign()
        {
            // Create root object
            GameObject signRoot = new GameObject("EchoRacerSign");
            signRoot.transform.position = Vector3.zero;

            // Add the view component
            EchoRacerSignView view = signRoot.AddComponent<EchoRacerSignView>();

            // Create materials
            Material frameMaterial = new Material(Shader.Find("Standard"));
            frameMaterial.color = HexToColor("222222");
            AssetDatabase.CreateAsset(frameMaterial, "Assets/ERL/Materials/SignFrameMaterial.mat");

            Material panelMaterial = new Material(Shader.Find("Standard"));
            panelMaterial.color = HexToColor("111111");
            panelMaterial.EnableKeyword("_EMISSION");
            panelMaterial.SetColor("_EmissionColor", HexToColor("111111"));
            AssetDatabase.CreateAsset(panelMaterial, "Assets/ERL/Materials/SignPanelMaterial.mat");

            Material lightMaterial = new Material(Shader.Find("Standard"));
            lightMaterial.color = HexToColor("FFFFEE");
            lightMaterial.EnableKeyword("_EMISSION");
            lightMaterial.SetColor("_EmissionColor", HexToColor("FFFFEE") * 2.0f);
            AssetDatabase.CreateAsset(lightMaterial, "Assets/ERL/Materials/SignLightMaterial.mat");
            
            // Create outer frame
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "OuterFrame";
            frame.transform.SetParent(signRoot.transform);
            frame.transform.localPosition = new Vector3(0, 2.5f, 0);
            frame.transform.localScale = new Vector3(4.2f, 5.2f, 0.3f);
            frame.GetComponent<Renderer>().material = frameMaterial;

            GameObject frameInterior = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frameInterior.name = "OuterFrame_Interior";
            frameInterior.transform.SetParent(frame.transform);
            frameInterior.transform.localPosition = new Vector3(0, 0, -0.4f);
            frameInterior.transform.localScale = new Vector3(0.9f, 0.95f, 1);
            frameInterior.GetComponent<Renderer>().material = panelMaterial;


            // Create vertical lights
            for (int i = 0; i < 2; i++)
            {
                GameObject light = GameObject.CreatePrimitive(PrimitiveType.Cube);
                light.name = "VerticalLight_" + i;
                light.transform.SetParent(signRoot.transform);
                light.transform.localPosition = new Vector3(i == 0 ? -1.9f : 1.9f, 2.5f, -0.2f);
                light.transform.localScale = new Vector3(0.1f, 4.8f, 0.1f);
                light.GetComponent<Renderer>().material = lightMaterial;
            }


            // Create panels and text
            view.platformTexts = new TextMeshProUGUI[4];
            view.raceNameTexts = new TextMeshProUGUI[4];
            view.racerCountTexts = new TextMeshProUGUI[4];
            view.statusTexts = new TextMeshProUGUI[4];

            Color[] raceNameColors = new Color[]
            {
                HexToColor("ADD8E6"), // Coastal Run
                HexToColor("FFFF00"), // Desert Rally
                HexToColor("800080"), // Urban Circuit
                HexToColor("FFA500")  // Nitro Arena
            };

            for (int i = 0; i < 4; i++)
            {
                GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                panel.name = "RaceInfoPanel_" + i;
                panel.transform.SetParent(signRoot.transform);
                panel.transform.localPosition = new Vector3(0, (i * 1.2f) + 0.6f, -0.2f);
                panel.transform.localScale = new Vector3(3.8f, 1.1f, 0.1f);
                panel.GetComponent<Renderer>().material = panelMaterial;

                // Platform Text
                view.platformTexts[i] = CreateText("PlatformText_" + i, "PLATFORM " + (i + 1), new Vector3(-1.5f, 0.4f, -0.1f), 0.2f, panel.transform);
                
                // Race Name Text
                view.raceNameTexts[i] = CreateText("RaceNameText_" + i, "RACE NAME", new Vector3(-0.5f, 0.1f, -0.1f), 0.4f, panel.transform);
                view.raceNameTexts[i].color = raceNameColors[i];

                // Racer Count Text
                view.racerCountTexts[i] = CreateText("RacerCountText_" + i, "0/0", new Vector3(1.5f, 0.1f, -0.1f), 0.3f, panel.transform);

                // Status Text
                view.statusTexts[i] = CreateText("StatusText_" + i, "STATUS", new Vector3(-0.5f, -0.3f, -0.1f), 0.2f, panel.transform);
            }
            
            // Save as prefab
            string prefabPath = "Assets/ERL/Prefabs/EchoRacerSign.prefab";
            PrefabUtility.SaveAsPrefabAsset(signRoot, prefabPath);
            

        }

        private static TextMeshProUGUI CreateText(string name, string text, Vector3 position, float fontSize, Transform parent)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent);
            textObj.transform.localPosition = position;
            
            TextMeshProUGUI textMesh = textObj.AddComponent<TextMeshProUGUI>();
            textMesh.text = text;
            textMesh.fontSize = fontSize;
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.rectTransform.sizeDelta = new Vector2(2, 0.5f);

            return textMesh;
        }

        private static Color HexToColor(string hex)
        {
            Color color = new Color();
            ColorUtility.TryParseHtmlString("#" + hex, out color);
            return color;
        }
    }
}
