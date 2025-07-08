using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class SignalResponder : MonoBehaviour
{

    [SerializeField] private List<GameObject> debris = new List<GameObject>();

    [SerializeField] private GameObject Asteroid;
    [SerializeField] private GameObject Dart;
    [SerializeField] private ParticleSystem CollisionParticles;

    public void TurnOffDebris()
    {
        for (int i = 0; i < debris.Count; i++)
        {
            debris[i].SetActive(false);
        }
    }

    public void ActivateBigDebris()
    {
        debris[0].SetActive(true);

        debris[0].GetComponent<SplineAnimate>().Play();
    }

    public void ActivateAsteroid()
    {
        if (Asteroid != null)
        {
            Asteroid.SetActive(true);
            Asteroid.GetComponent<SplineAnimate>().Play();
        }
    }

    public void ActivateDart()
    {
        if (Dart != null)
        {
            Dart.SetActive(true);
            Dart.GetComponent<SplineAnimate>().Play();
        }
    }

    public void DartCollision()
    {
        if (Dart != null)
        {
            Dart.SetActive(false);
        }
        if (CollisionParticles != null)
        {
            CollisionParticles.Play();
        }
    }
}
