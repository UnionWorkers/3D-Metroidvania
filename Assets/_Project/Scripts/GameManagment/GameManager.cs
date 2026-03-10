using UnityEngine;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager gameManager;
        public static GameManager Instance => gameManager;

        [SerializeField] private SceneLoader sceneLoader;

        private void Awake()
        {
            if (GameManager.Instance == null)
            {
                DontDestroyOnLoad(gameManager);
            }
        }


        public void ChangeScene(ref SceneData sceneData)
        {
            sceneLoader.LoadScene(ref sceneData);
        }

    }

}