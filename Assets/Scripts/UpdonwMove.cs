using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;


public class UpdonwMove : MonoBehaviour
{
    public float targetLocalXPosition = -15f;
    public float moveDuration = 2f;

    void Start()
    {
        transform.DOLocalMoveY(targetLocalXPosition, moveDuration).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
    }
}
