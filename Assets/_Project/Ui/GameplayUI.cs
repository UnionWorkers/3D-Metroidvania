using System;
using TMPro;
using UnityEngine;

public class GameplayUI : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI pointCounter;
    private int MaxPoints = 0;
    private int currentPoints = 0;
    public Action OnScoreCompleted;

    public void OnInitialize(int inMaxPoints)
    {
        if (pointCounter == null)
        {
            gameObject.SetActive(false);
            Debug.LogError("GameplayUI docent have Point Counter Ui component");
            return;
        }

        MaxPoints = inMaxPoints; 
        pointCounter.text = $"{currentPoints} / {MaxPoints}";
        gameObject.SetActive(true);
    }

    public void UpdatePointCounter(int amount)
    {
        currentPoints += amount;
        pointCounter.text = $"{currentPoints} / {MaxPoints}";

        if(currentPoints >= MaxPoints)
        {
            OnScoreCompleted?.Invoke();
        }
    }

}
