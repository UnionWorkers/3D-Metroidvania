using UnityEngine;
using UnityEngine.UI;

public class CreditMenuUI : MonoBehaviour
{
    [SerializeField] private Button backButton = null;

    private void Awake()
    {
        if (backButton == null) { return; }

        backButton.onClick.AddListener(Back);
    }

    public void ChangeState(bool state)
    {
        gameObject.SetActive(state);
    }

    private void Back()
    {
        ChangeState(false);
    }

}
