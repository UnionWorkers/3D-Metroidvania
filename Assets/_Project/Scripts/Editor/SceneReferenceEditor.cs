using SceneLoaderUtil;
using UnityEditor;
using Utils.SceneLoader;

namespace Utils.SceneLoaderEditor
{
    [CustomEditor(typeof(SceneReference), true)]
    public class SceneReferenceEditor : Editor
    {
        int selected = 0;
        SceneData[] scenes = new SceneData[0];
        string[] options = new string[0];

        private void OnEnable()
        {
            int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;

            scenes = new SceneData[sceneCount];
            options = new string[sceneCount];

            for (int i = 0; i < sceneCount; i++)
            {
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i));
                scenes[i] = new(i, sceneName);
                options[i] = sceneName;
            }
        }

        public override void OnInspectorGUI()
        {
            SceneReference sceneReference = target as SceneReference;

            EditorGUI.BeginChangeCheck();

            base.OnInspectorGUI();

            for (int i = 0; i < options.Length; i++)
            {
                if (options[i] == sceneReference.SceneData.SceneName)
                {
                    selected = i;
                }
            }

            selected = EditorGUILayout.Popup("Scene List", selected, options);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(sceneReference, "Scene data changed");
                sceneReference.SceneData = scenes[selected];
            }

        }
    }

}