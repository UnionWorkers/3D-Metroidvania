using System.Collections.Generic;
using System.Threading;
using UnityEngine;


public class RatBasteredJokeAnimation : MonoBehaviour
{
    public struct RatJokeComponents
    {
        private Transform rat;
        private Transform rifle;
        private Transform ventDoor;

        public RatJokeComponents(Transform inRat, Transform inRifle, Transform inVentDoor)
        {
            rat = inRat;
            rifle = inRifle;
            ventDoor = inVentDoor;
        }

        public Transform Rat => rat;
        public Transform Rifle => rifle;
        public Transform VentDoor => ventDoor;
    }


    List<RatJokeComponents> ratJokes = new();
    private float MaxRotation = 180;
    private float currentRotation = 0;
    private float accelerationTime = 1f;
    private float speedUp = 0.05f;
    public bool AllDoorsOpen = false;

    private void Awake()
    {
        foreach (Transform rat in transform)
        {
            RatJokeComponents components = new(rat.Find("RatBastard"), rat.Find("Rifle"), rat.Find("VentDoor"));
            ratJokes.Add(components);
        }
    }

    public void AnimatedDoors()
    {
        if (AllDoorsOpen) { return; }
        
        if (currentRotation < MaxRotation)
        {
            currentRotation += MaxRotation * (Time.deltaTime / accelerationTime);
            accelerationTime -= speedUp;
        }
        else
        {
            currentRotation = MaxRotation;
            AllDoorsOpen = true;
        }

        foreach (var rat in ratJokes)
        {
            rat.VentDoor.eulerAngles += new Vector3(0, currentRotation, 0);
        }
    }

}

