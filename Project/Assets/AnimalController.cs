using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalController : MonoBehaviour
{
    
    public AnimalData _animalData;
    
    void Start()
    {
        //중복 구독 방지
        GameManager.AllAnimalsInitialized -= OnAllAnimalsInitialized;
        GameManager.AllAnimalsInitialized += OnAllAnimalsInitialized;

        InitializeTransform();

        // Instantiate(_animalData.animalPrefab, _animalData.initialPosition, _animalData.initialRotation);

    }
    private void OnAllAnimalsInitialized()
    {
        GameManager.isAnimalTransformSet = true;
    }
    
    void OnDestroy()
    {
        GameManager.AllAnimalsInitialized -= OnAllAnimalsInitialized;
    }

    private void InitializeTransform()
    {
        _animalData.initialPosition = transform.position;
        _animalData.initialRotation = transform.rotation;
        
        if (GameManager.isAnimalTransformSet == false)
        { 
            GameManager.AnimalInitialized();
            Destroy(gameObject);
        }
    }
}
