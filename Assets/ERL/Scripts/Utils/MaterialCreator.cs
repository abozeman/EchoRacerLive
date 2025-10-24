// 10/16/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;
using UnityEditor;

public class MaterialCreator : MonoBehaviour
{
    [ContextMenu("Create Base and Frame Materials")]
    public void CreateMaterials()
    {
        //// Load the EchoRacerSignConcept texture
        //Texture2D signTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ERL/Textures/EchoRacerSignConcept.jpg");

        //if (signTexture == null)
        //{
        //    Debug.LogError("EchoRacerSignConcept texture not found at the specified path.");
        //    return;
        //}

        //// Create the base material
        //Material baseMaterial = new Material(Shader.Find("Standard"));
        //baseMaterial.name = "SignBaseMaterial";
        //baseMaterial.mainTexture = signTexture;
        //baseMaterial.color = Color.white; // Adjust color if needed

        //// Save the base material
        //AssetDatabase.CreateAsset(baseMaterial, "Assets/ERL/Materials/SignBaseMaterial.mat");

        //// Create the frame material
        //Material frameMaterial = new Material(Shader.Find("Standard"));
        //frameMaterial.name = "SignFrameMaterial";
        //frameMaterial.color = Color.gray; // Adjust color to match the frame design

        //// Save the frame material
        //AssetDatabase.CreateAsset(frameMaterial, "Assets/ERL/Materials/SignFrameMaterial.mat");

        //// Save all assets
        //AssetDatabase.SaveAssets();

        //Debug.Log("Base and frame materials created and saved in Assets/ERL/Materials/");
    }
}