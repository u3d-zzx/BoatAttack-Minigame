using BoatAttack;
using UnityEngine;

public class DemoRun : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(RaceManager.SetupRace());
    }
}