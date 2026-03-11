using SceneLoaderUtil;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager gameManager;
        public static GameManager Instance => gameManager;
        private SceneLoader sceneLoader;

        private void Awake()
        {
            if (GameManager.Instance == null)
            {
                gameManager = this;
            }
            else
            {
                DontDestroyOnLoad(gameManager);
            }

            sceneLoader = new(new SceneData(SceneManager.GetActiveScene().buildIndex, SceneManager.GetActiveScene().name));
        }


        public void ChangeScene(ref SceneData sceneData)
        {
            sceneLoader.LoadScene(sceneData);
        }

    }

}