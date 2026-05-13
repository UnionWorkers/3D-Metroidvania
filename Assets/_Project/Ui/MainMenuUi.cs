using System.Collections.Generic;
using Managers;
using SceneLoaderUtil;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.SceneLoader;



public class MainMenuUi : MonoBehaviour
{
    [SerializeField] private Transform buttonHolder = null;
    [SerializeField] private SceneReference nextScene;
    [SerializeField] private CreditMenuUI creditMenuUI;
    private List<(string ButtonText, UnityAction subscribingFunction)> buttonList = new();


    private void Awake()
    {
        if (buttonHolder == null)
        {
            Debug.LogError("ButtonHolder is null, fix for menu to work");
            return;
        }

        buttonList.Add(("New Game", NewGame));
        buttonList.Add(("Continue", Continue));
        buttonList.Add(("Credits", Credits));
        buttonList.Add(("Quit", Quit));
    }

    private void Start()
    {
        int loopIndex = 0;
        foreach (Transform buttonTransform in buttonHolder)
        {
            if (loopIndex < buttonList.Count)
            {
                Button button = buttonTransform.GetComponent<Button>();
                TextMeshProUGUI buttonText = buttonTransform.GetComponentInChildren<TextMeshProUGUI>();

                button.onClick.AddListener(buttonList[loopIndex].subscribingFunction);
                buttonText.text = buttonList[loopIndex].ButtonText;

                if (buttonList[loopIndex].subscribingFunction == Continue)
                {
                    button.interactable = false;
                }

                loopIndex++;
            }
            else
            {
                break;
            }
        }
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeGameState(GameState.MainMenu);
        }
    }

    private void NewGame()
    {
        SceneData sceneData = nextScene.SceneData;
        GameManager.Instance.ChangeScene(ref sceneData);
    }

    private void Continue() { }

    private void Credits()
    {
        if (creditMenuUI == null) { return; }

        creditMenuUI.ChangeState(true);
    }

    private void Quit()
    {
#if UNITY_EDITOR
        if (Application.isEditor)
        {
            EditorApplication.ExitPlaymode();
            return;
        }
#endif
        Application.Quit();
    }



}
