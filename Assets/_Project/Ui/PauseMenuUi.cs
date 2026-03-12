using System.Collections.Generic;
using Managers;
using SceneLoaderUtil;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PauseMenuUi : MonoBehaviour
{
    private bool hasInitialized = false;
    [SerializeField] private Transform buttonHolder = null;
    [SerializeField] private SceneReference mainMenuScene;

    private List<(string ButtonText, UnityAction subscribingFunction)> buttonList = new();

    private void Awake()
    {
        if (hasInitialized)
        {
            return;
        }

        if (buttonHolder == null)
        {
            buttonHolder = transform.Find("ButtonHolder");
            if (buttonHolder == null)
            {
                Debug.LogError("ButtonHolder is null, fix for menu to work");
                return;
            }
        }

        if (mainMenuScene == null)
        {
            mainMenuScene = GetComponent<SceneReference>();
            if (mainMenuScene == null)
            {
                Debug.LogError("MainMenuScene is null, fix for menu to work");
                return;
            }

        }

        buttonList.Add(("Resume", Resume));
        buttonList.Add(("Quit", Quit));
    }

    private void Start()
    {
        if (hasInitialized)
        {
            return;
        }

        int loopIndex = 0;
        foreach (Transform buttonTransform in buttonHolder)
        {
            if (loopIndex < buttonList.Count)
            {
                Button button = buttonTransform.GetComponent<Button>();
                TextMeshProUGUI buttonText = buttonTransform.GetComponentInChildren<TextMeshProUGUI>();

                button.onClick.AddListener(buttonList[loopIndex].subscribingFunction);
                buttonText.text = buttonList[loopIndex].ButtonText;

                loopIndex++;
            }
            else
            {
                break;
            }
        }
    }

    public void ChangeActiveState(bool state)
    {
        gameObject.SetActive(state);
    }

    private void Resume()
    {
        GameManager.Instance.ChangeGameState(GameState.Running);
    }

    private void Quit()
    {
        SceneData sceneData = mainMenuScene.SceneData;
        Debug.Log($"{sceneData}, {sceneData.SceneName}, {sceneData.SceneIndex}");
        GameManager.Instance.ChangeScene(ref sceneData);
    }
}
