using System.Collections;
using Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

public struct SceneData
{
    public int SceneIndex;
    public string SceneName;

    public SceneData(int inSceneIndex, string inSceneName)
    {
        SceneIndex = inSceneIndex;
        SceneName = inSceneName;
    }

}

public class SceneLoader
{
    private SceneData oldDataScene;
    private SceneData currentDataScene;
    private bool hasFinishedLoading = false;

// fix :/

    public void LoadScene(ref SceneData newSceneData)
    {
        oldDataScene = currentDataScene;
        currentDataScene = newSceneData;

        GameManager.Instance.StartCoroutine(AsyncLoadScene());
    }

    private IEnumerator AsyncLoadScene()
    {
        // Load New scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync((int)currentDataScene.SceneIndex, LoadSceneMode.Additive);

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

        hasFinishedLoading = true;
    }

}
