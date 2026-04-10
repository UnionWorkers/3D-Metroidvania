using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class MaterialData
{
    public string PropertyName;
    public ShaderPropertyType PropertyType;
    public Color Color;
    public float Float;
    public int Int;
    public Texture2D Texture;
    public Vector4 Vector;

    public MaterialData(string inPropertyName, ShaderPropertyType inPropertyType)
    {
        PropertyName = inPropertyName;
        PropertyType = inPropertyType;
    }
}

public class MaterialToolEditor : EditorWindow
{
    private Shader currentShader;
    private Shader newShader;
    private Material newMaterial;
    private string materialName = "";
    private string defaultPath = "Assets/_Project/";
    private string savePath = "";
    private string finishedPath = "";

    private List<MaterialData> materialData = new();

    [MenuItem("Tools/Material Maker")]
    public static void ShowWindow()
    {
        GetWindow(typeof(MaterialToolEditor));
    }

    private void OnGUI()
    {
        GUILayout.Label("Material Maker", EditorStyles.boldLabel);

        newShader = EditorGUILayout.ObjectField("Shader For Material", newShader, typeof(Shader), false) as Shader;

        if (newShader != null)
        {
            if (newShader != currentShader)
            {
                materialData.Clear();
                currentShader = newShader;
            }

            materialName = EditorGUILayout.TextField("Material Name", materialName);

            EditorGUILayout.Space(10);
            defaultPath = EditorGUILayout.TextField("DefaultPath", defaultPath);
            savePath = EditorGUILayout.TextField("Save Path", savePath);



            if (savePath.Length > 0 && savePath[savePath.Length - 1] != '/')
            {
                finishedPath = defaultPath + savePath + '/';
            }
            else
            {
                finishedPath = defaultPath + savePath;
            }

            bool bathExist = Directory.Exists(finishedPath);

            if (bathExist)
            {
                GUILayout.Label($"Path Exists ✅");
            }
            else
            {
                GUILayout.Label($"Cant find path ❌");
            }
            GUILayout.Label($"Current Path: {finishedPath + materialName + ".mat"}");

            if (materialName != "" && bathExist)
            {

                EditorGUILayout.Space(20);
                GUILayout.Label("Material variables", EditorStyles.boldLabel);

                int propertyCount = currentShader.GetPropertyCount();
                // Populate materialData if its empty 
                if (materialData.Count <= 0)
                {
                    for (int i = 0; i < propertyCount; i++)
                    {
                        string propertyName = currentShader.GetPropertyName(i);
                        if (propertyName == "_QueueOffset")
                        {
                            break;
                        }

                        ShaderPropertyType propertyType = currentShader.GetPropertyType(i);

                        MaterialData newData = new(propertyName, propertyType);

                        // get default values
                        switch (propertyType)
                        {
                            case ShaderPropertyType.Color:
                                newData.Color = currentShader.GetPropertyDefaultVectorValue(i);
                                break;
                            case ShaderPropertyType.Float:
                                newData.Float = currentShader.GetPropertyDefaultFloatValue(i);
                                break;
                            case ShaderPropertyType.Int:
                                newData.Int = currentShader.GetPropertyDefaultIntValue(i);
                                break;
                            case ShaderPropertyType.Vector:
                                newData.Vector = currentShader.GetPropertyDefaultVectorValue(i);
                                break;
                        }

                        switch (propertyType)
                        {
                            case ShaderPropertyType.Color:
                                newData.Color = EditorGUILayout.ColorField($"{newData.PropertyName}", newData.Color);
                                break;
                            case ShaderPropertyType.Float:
                                newData.Float = EditorGUILayout.FloatField($"{newData.PropertyName}", newData.Float);
                                break;
                            case ShaderPropertyType.Int:
                                newData.Int = EditorGUILayout.IntField($"{newData.PropertyName}", newData.Int);
                                break;
                            case ShaderPropertyType.Texture:
                                newData.Texture = EditorGUILayout.ObjectField($"{newData.PropertyName}", newData.Texture, typeof(Texture2D), false) as Texture2D;
                                break;
                            case ShaderPropertyType.Vector:
                                newData.Vector = EditorGUILayout.Vector4Field($"{newData.PropertyName}", newData.Vector);
                                break;
                        }

                        materialData.Add(newData);
                    }
                }
                else
                {
                    for (int i = 0; i < propertyCount; i++)
                    {
                        string propertyName = currentShader.GetPropertyName(i);
                        if (propertyName == "_QueueOffset")
                        {
                            break;
                        }

                        ShaderPropertyType propertyType = currentShader.GetPropertyType(i);

                        MaterialData newData = materialData[i];

                        // make values editable in view
                        switch (propertyType)
                        {
                            case ShaderPropertyType.Color:
                                newData.Color = EditorGUILayout.ColorField($"{newData.PropertyName}", newData.Color);
                                break;
                            case ShaderPropertyType.Float:
                                newData.Float = EditorGUILayout.FloatField($"{newData.PropertyName}", newData.Float);
                                break;
                            case ShaderPropertyType.Int:
                                newData.Int = EditorGUILayout.IntField($"{newData.PropertyName}", newData.Int);
                                break;
                            case ShaderPropertyType.Texture:
                                newData.Texture = EditorGUILayout.ObjectField($"{newData.PropertyName}", newData.Texture, typeof(Texture2D), false) as Texture2D;
                                break;
                            case ShaderPropertyType.Vector:
                                newData.Vector = EditorGUILayout.Vector4Field($"{newData.PropertyName}", newData.Vector);
                                break;
                        }
                    }
                }

                bool canMakeMaterial = true;

                // Check if values are valid 
                newMaterial = new Material(currentShader);
                for (int i = 0; i < materialData.Count; i++)
                {
                    switch (materialData[i].PropertyType)
                    {
                        case ShaderPropertyType.Color:
                            if (materialData[i].Color == null) { canMakeMaterial = false; }
                            break;
                        case ShaderPropertyType.Texture:
                            if (materialData[i].Texture == null) { canMakeMaterial = false; }
                            break;
                        case ShaderPropertyType.Vector:
                            if (materialData[i].Vector == null) { canMakeMaterial = false; }
                            break;
                    }
                }

                EditorGUILayout.Space(20);

                if (canMakeMaterial)
                {
                    GUILayout.Label($"Material can be created", EditorStyles.boldLabel);
                }
                else
                {
                    GUILayout.Label($"Somethings wrong, cant make material, check Color, Texture or Vector", EditorStyles.boldLabel);
                }

                //Try to make material
                if (GUILayout.Button("Make Material") && canMakeMaterial)
                {
                    newMaterial = new Material(currentShader);
                    for (int i = 0; i < materialData.Count; i++)
                    {
                        switch (materialData[i].PropertyType)
                        {
                            case ShaderPropertyType.Color:
                                if (materialData[i].Color == null)
                                {
                                    canMakeMaterial = false;
                                }
                                newMaterial.SetColor(materialData[i].PropertyName, materialData[i].Color);
                                break;
                            case ShaderPropertyType.Float:
                                newMaterial.SetFloat(materialData[i].PropertyName, materialData[i].Float);

                                break;
                            case ShaderPropertyType.Int:
                                newMaterial.SetInt(materialData[i].PropertyName, materialData[i].Int);

                                break;
                            case ShaderPropertyType.Texture:
                                if (materialData[i].Texture == null)
                                {
                                    canMakeMaterial = false;
                                }
                                newMaterial.SetTexture(materialData[i].PropertyName, materialData[i].Texture);

                                break;
                            case ShaderPropertyType.Vector:
                                if (materialData[i].Vector == null)
                                {
                                    canMakeMaterial = false;
                                }
                                newMaterial.SetVector(materialData[i].PropertyName, materialData[i].Vector);
                                break;
                        }
                    }

                    // Make material
                    string materialPath = finishedPath + materialName + ".mat";
                    materialPath = AssetDatabase.GenerateUniqueAssetPath(materialPath);
                    AssetDatabase.CreateAsset(newMaterial, materialPath);

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    EditorGUIUtility.PingObject(newMaterial);

                }
            }
        }

    }
}
