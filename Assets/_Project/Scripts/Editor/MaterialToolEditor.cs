using System.Collections.Generic;
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
    private Shader shader;
    private Material newMaterial;
    private string materialName;
    private List<MaterialData> materialData = new();

    [MenuItem("Tools/Material Maker")]
    public static void ShowWindow()
    {
        GetWindow(typeof(MaterialToolEditor));
    }

    private void OnGUI()
    {
        GUILayout.Label("Material Maker", EditorStyles.boldLabel);

        shader = EditorGUILayout.ObjectField("Shader For Material", shader, typeof(Shader), false) as Shader;

        if (shader != null)
        {
            materialName = EditorGUILayout.TextField("Material Name", materialName);

            EditorGUILayout.Space(20);
            GUILayout.Label("Material variables", EditorStyles.boldLabel);

            int propertyCount = shader.GetPropertyCount();

            for (int i = 0; i < propertyCount; i++)
            {
                string propertyName = shader.GetPropertyName(i);
                if (propertyName == "_QueueOffset")
                {
                    break;
                }

                ShaderPropertyType propertyType = shader.GetPropertyType(i);

                MaterialData newData = new(propertyName, propertyType);

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

            if (GUILayout.Button("Make Material"))
            {
                newMaterial = new Material(shader);
                bool canMakeMaterial = true;
                for (int i = 0; i < materialData.Count; i++)
                {
                    Debug.Log(materialData[i]);
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
                
                if (canMakeMaterial)
                {
                    Debug.Log("can make");
                }
                else
                {
                    Debug.Log("can not make");

                }

            }

            // Cleanup 
            materialData.Clear();
        }
    }

}