using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Continuous rotation of a UI element (used for the HUD rings on the start page).</summary>
public class CyberSpin : MonoBehaviour
{
    public float degreesPerSecond = 12f;
    void Update() { transform.Rotate(0f, 0f, -degreesPerSecond * Time.unscaledDeltaTime); }
}
