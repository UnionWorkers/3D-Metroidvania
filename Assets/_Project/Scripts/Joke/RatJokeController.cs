using System.Collections.Generic;
using Entities.Controller;
using UnityEngine;

public class RatJokeController : MonoBehaviour
{
    enum JokeStage
    {
        CloseDoor,
        MoveRatsForward,
        OpenDoors,
        MoveRifle,
        LookAtPlayer,
        Dance,
        Shoot
    }
    [SerializeField] List<RatBasteredJokeAnimation> jokeAnimations = new();

    private JokeStage jokeStage;
    private int currentIndex = 0;

    private float moveLocation = 5f;

    private void NextStage()
    {
        int index = (int)jokeStage;
        index++;
        jokeStage = (JokeStage)index;
    }

    void Update()
    {
        switch (jokeStage)
        {
            case JokeStage.CloseDoor:


                NextStage();
                break;

            case JokeStage.MoveRatsForward:
                bool isDone = true;
                if (transform.localPosition.z <= moveLocation)
                {
                    Debug.Log("sad");
                    transform.localPosition = new(transform.localPosition.x, transform.localPosition.y, moveLocation);
                }
                else
                {
                    isDone = false;
                }

                if (isDone)
                {
                    NextStage();
                    return;
                }

                transform.position += Vector3.back * Time.deltaTime;

                break;

            case JokeStage.OpenDoors:

                jokeAnimations[currentIndex].AnimatedDoors();

                if (jokeAnimations[currentIndex].AllDoorsOpen)
                {
                    currentIndex++;
                }

                if (currentIndex >= jokeAnimations.Count)
                {
                    NextStage();
                }

                break;

            case JokeStage.MoveRifle:
                NextStage();

                break;

            case JokeStage.LookAtPlayer:
                NextStage();

                break;

            case JokeStage.Dance:
                NextStage();
                break;

            case JokeStage.Shoot:
                FindAnyObjectByType<PlayerController>().transform.GetComponent<IHealth>().TakeDamage(999);
                break;
        }
    }
}
