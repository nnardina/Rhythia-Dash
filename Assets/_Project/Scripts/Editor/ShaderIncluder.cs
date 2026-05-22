using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

[InitializeOnLoad]
public class ShaderIncluder
{
    static ShaderIncluder()
    {
        EnsureShaderIncluded();
    }

    static void EnsureShaderIncluded()
    {
        var shader = Shader.Find("Custom/MotionBlur");
        if (shader == null)
        {
            Debug.LogWarning("[ShaderIncluder] Custom/MotionBlur shader not found");
            return;
        }

        var graphicsSettings = GraphicsSettings.GetGraphicsSettings();
        var serializedObject = new SerializedObject(graphicsSettings);
        var arrayProp = serializedObject.FindProperty("m_AlwaysIncludedShaders");

        bool alreadyIncluded = false;
        for (int i = 0; i < arrayProp.arraySize; i++)
        {
            var shaderProp = arrayProp.GetArrayElementAtIndex(i);
            if (shaderProp.objectReferenceValue == shader)
            {
                alreadyIncluded = true;
                break;
            }
        }

        if (!alreadyIncluded)
        {
            int arrayIndex = arrayProp.arraySize;
            arrayProp.InsertArrayElementAtIndex(arrayIndex);
            var newShaderProp = arrayProp.GetArrayElementAtIndex(arrayIndex);
            newShaderProp.objectReferenceValue = shader;
            serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            Debug.Log("[ShaderIncluder] Added Custom/MotionBlur to Always Included Shaders");
        }
    }
}
