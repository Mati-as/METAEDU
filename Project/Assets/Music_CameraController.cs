using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Music_CameraController : MonoBehaviour
{

    private Vector3 _defaultPosition;
    void Start()
    {
        BindEvent();
        _defaultPosition = transform.position;
    }


    private void OnDestroy()
    {
        Music_BubbleController.bigBubbleEvent -= OnBigBubbleExplode;
    }

    private void OnBigBubbleExplode()
    {
        transform.DOShakePosition(1.5f, 2, 11).OnComplete(() =>
        {
            transform.DOMove(_defaultPosition, 1f);
        });
    }
    private void BindEvent()
    {
        Music_BubbleController.bigBubbleEvent -= OnBigBubbleExplode;
        Music_BubbleController.bigBubbleEvent += OnBigBubbleExplode;
    }
}
