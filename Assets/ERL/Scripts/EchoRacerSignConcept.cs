// 10/16/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;
using TMPro;

public class SignInserter : MonoBehaviour
{
    public Texture2D signTexture; // Assign EchoRacerSignConcept texture here
    public Material baseMaterial; // Assign a material for the base
    public Material frameMaterial; // Assign a material for the frame
    public string initialText = "Your Text Here"; // Default text for the sign

    [ContextMenu("Insert Sign Into Scene")]
    public void InsertSignIntoScene()
    {
        // Create the parent GameObject
        GameObject sign = new GameObject("EchoRacerSign");

        // Create the base of the sign
        GameObject baseObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseObject.name = "SignBase";
        baseObject.transform.SetParent(sign.transform);
        baseObject.transform.localScale = new Vector3(2f, 1f, 0.1f); // Adjust size as needed
        baseObject.transform.localPosition = Vector3.zero;

        if (baseMaterial != null)
        {
            baseObject.GetComponent<Renderer>().material = baseMaterial;
        }

        // Create the frame of the sign
        GameObject frameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frameObject.name = "SignFrame";
        frameObject.transform.SetParent(sign.transform);
        frameObject.transform.localScale = new Vector3(2.2f, 1.2f, 0.1f); // Slightly larger than the base
        frameObject.transform.localPosition = new Vector3(0, 0, -0.05f); // Offset slightly behind the base

        if (frameMaterial != null)
        {
            frameObject.GetComponent<Renderer>().material = frameMaterial;
        }

        // Create the text
        GameObject textObject = new GameObject("SignText");
        textObject.transform.SetParent(sign.transform);
        textObject.transform.localPosition = new Vector3(0, 0, 0.06f); // Slightly in front of the base

        TextMeshPro textMeshPro = textObject.AddComponent<TextMeshPro>();
        textMeshPro.text = initialText;
        textMeshPro.fontSize = 5;
        textMeshPro.alignment = TextAlignmentOptions.Center;
        textMeshPro.rectTransform.sizeDelta = new Vector2(2f, 1f); // Match the size of the base

        // Apply the texture to the base material if provided
        if (signTexture != null && baseMaterial != null)
        {
            baseMaterial.mainTexture = signTexture;
        }

        // Place the sign in the scene
        sign.transform.position = Vector3.zero; // Adjust position as needed
    }
}