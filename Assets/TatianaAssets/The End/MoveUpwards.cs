///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt, SoSe25
///   AI was used: GPT
///   ESA PROJECT STAGE:
///   Last Change: 06.10.2025
///   Created: 06.10.2025

/// cinematic movement of end text, upwards 

using UnityEngine;

public class MoveUpwards : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 0.2f; // units per second
    //public float topY = 5.0f;  // stop position on Y axis // this can make sense when it's clear wow long is text and if is there any final text we can pause at the end

    [Header("Start Position")]
    [SerializeField] private float bottomY = 0.0f; // start position on Y axis

    [SerializeField] private bool movingUp = true;

    void Start()
    {
        // Set starting position
        Vector3 startPos = transform.position;
        //startPos.y = bottomY;
        //transform.position = startPos;
    }

    void Update()
    {
        if (movingUp)
        {
            transform.Translate(Vector3.up * speed * Time.deltaTime);

            /*
            // Stop when reaching topY
            if (transform.position.y >= topY)
            {
                transform.position = new Vector3(transform.position.x, topY, transform.position.z);
                movingUp = false;
            }
            */
        }
    }
}
