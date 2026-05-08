using System.Collections.Generic;
using Managers;
using SceneLoaderUtil;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.SceneLoader;

public class ShowCaseUi : MonoBehaviour
{
    private bool hasInitialized = false;
    [SerializeField] private Transform buttonHolder = null;

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

        buttonList.Add(("Continue", Continue));
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

    private void Continue()
    {
        GameManager.Instance.ChangeGameState(GameState.Running);
        ChangeActiveState(false);
    }

}
