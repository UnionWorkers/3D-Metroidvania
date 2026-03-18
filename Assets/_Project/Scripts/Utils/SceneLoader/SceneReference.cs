using UnityEngine;
using Utils.SceneLoader;

namespace SceneLoaderUtil
{
    [System.Serializable]
    public class SceneReference : MonoBehaviour
    {
        [SerializeField] private SceneData sceneData = new(-1, "");
        public SceneData SceneData { get => sceneData; set { sceneData = value; } }

        public int GetBuildIndex()
        {
            return sceneData.SceneIndex;
        }

        public string GetName()
        {
            return sceneData.SceneName;
        }

    }

}