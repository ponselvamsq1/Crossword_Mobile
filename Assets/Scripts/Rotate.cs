using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public float rotationAngle; 
    public float rotationDuration ;
    public float pauseDuration;
    public int rotationsBeforePause;

    void Start()
    {
        StartCoroutine(RotateRoutine());
    }

    IEnumerator RotateRoutine()
    {
        int rotationCount = 0;

        while (true)
        {
            yield return StartCoroutine(SmoothRotateZ(rotationAngle, rotationDuration));

            rotationCount++;

            // Wait for next rotation
            yield return new WaitForSeconds(0f);

            // Pause after 5 rotations
            if (rotationCount >= rotationsBeforePause)
            {
                yield return new WaitForSeconds(pauseDuration);
                rotationCount = 0; // Reset rotation count
            }
        }
    }

    IEnumerator SmoothRotateZ(float angle, float duration)
    {
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = transform.rotation * Quaternion.Euler(0, 0, -angle);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = endRotation; // Ensure final rotation is exact
    }
}
