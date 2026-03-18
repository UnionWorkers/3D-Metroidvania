using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Utils.SceneLoader
{
    [System.Serializable]
    public struct SceneData
    {
        public int SceneIndex;
        public string SceneName;

        public SceneData(int inSceneIndex = -1, string inSceneName = "")
        {
            SceneIndex = inSceneIndex;
            SceneName = inSceneName;
        }

    }

    public class SceneLoader
    {
        private int sceneInBuildCount = -1;
        private SceneData oldDataScene;
        private SceneData currentDataScene;
        List<SceneData> availableScenes = new();

        public Action<bool> OnFinishedLoading;

        public SceneData OldDataScene => oldDataScene;
        public SceneData CurrentDataScene => currentDataScene;

        public SceneLoader(SceneData staringScene)
        {
            currentDataScene = staringScene;
            sceneInBuildCount = SceneManager.sceneCountInBuildSettings;
        }

        public bool ValidateSceneData(ref SceneData sceneData)
        {
            // if the scene dose not exist in the scene build list
            if (sceneData.SceneIndex == -1 || sceneData.SceneIndex >= sceneInBuildCount || sceneData.SceneName == "")
            {
                Debug.LogError("Scene does not exist in the builds, add it to the scene build or change scene");
                return false;
            }
            return true;
        }

        public void LoadScene(SceneData newSceneData)
        {
            if (!ValidateSceneData(ref newSceneData))
            {
                Debug.LogError($"Name: {newSceneData.SceneName}, Build Index: {newSceneData.SceneIndex}, does not exist in the scene build list");
                return;
            }

            oldDataScene = currentDataScene;
            currentDataScene = newSceneData;

            GameManager.Instance.StartCoroutine(AsyncLoadScene());
        }

        private IEnumerator AsyncLoadScene()
        {
            // Load New scene
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync((int)currentDataScene.SceneIndex, LoadSceneMode.Additive);
            
            // Unity will complain if this is not done, this should probably be done on the loading object
            Camera.main.GetComponent<AudioListener>().enabled = false;
            GameObject.FindAnyObjectByType<EventSystem>().gameObject.SetActive(false);

            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            // Unload old scene
            AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync((int)oldDataScene.SceneIndex, UnloadSceneOptions.None);

            while (!asyncUnload.isDone)
            {
                yield return null;
            }

            OnFinishedLoading?.Invoke(true);
        }

    }
}
