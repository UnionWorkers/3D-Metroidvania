using Managers;
using SceneLoaderUtil;
using UnityEngine;
using Utils.SceneLoader;

public class NewSceneTrigger : MonoBehaviour
{
    [SerializeField] private SceneReference sceneReference;

    private void Awake()
    {
        if (sceneReference == null)
        {
            sceneReference = GetComponent<SceneReference>();
            if (sceneReference == null)
            {
                Debug.LogWarning(gameObject.name + ": There is no sceneReference on this game object");
                return;
            }
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        if (GameManager.Instance == null || sceneReference == null) { return; }

        if (collider.CompareTag("Player"))
        {
            SceneData sceneData = sceneReference.SceneData;
            GameManager.Instance.ChangeScene(ref sceneData);
        }
    }

}
