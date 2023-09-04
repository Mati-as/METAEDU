using System;
using UnityEngine;
using UnityEngine.Serialization;

public class ShaderManager : MonoBehaviour
{
    [Header("Background Color Settings")] [Space(10f)]
    public Material skyMaterial;

    public Color defaultSkyColor;
    public Color defaultHorizonColor;

    public Color inPlayBgColor;
    public float colorChangingSpeed;

    public float TimingToBeChanged;
    private float _elapsedTimeForBg;
    private float _progress;
    private float _progressWhenFinished;

    //셰이더 파라미터 컨트롤
    private readonly int _skyColor = Shader.PropertyToID("_SkyColor");
    private readonly int _horizontalColor = Shader.PropertyToID("_HorizonColor");

    [Space(20)] [Header("Animal Color Default Settings")] [Space(10f)]
    public float waitTime;

    [FormerlySerializedAs("_animalMaterial")]
    private int defaultFresnel = 5000;
    public Material animalMaterial;

    public Color startColor;
    public Color inPlayColor;

   

    private readonly int _emissionColor = Shader.PropertyToID("_emissionColor");
   

    [Space(20f)] [Header("Color Settings")] [Space(10f)]
    public float minBrightness;

    public float maxBrightness;

    [FormerlySerializedAs("brightnessIncreasingSpeed")]
    

    private float _brightness;
    private float _elapsedTime;
    private float _lerpProgress; // 추가된 변수

    private void Awake()
    {
        SetAnimalOutlineColor(inPlayColor);
        SetSkyColor(defaultSkyColor);
        SetHorizonColor(defaultHorizonColor);
    }


    private void Update()
    {
        if (GameManager.isGameStarted)
        {
            ChangedSkyColor();
        }

        if (GameManager.isGameFinished)
        {
            GetSkyColorBack();
        }
    }

    

    private void ChangedSkyColor()
    {
        _elapsedTimeForBg += Time.deltaTime;
       

        if (_elapsedTimeForBg > TimingToBeChanged)
        {
            _progress += Time.deltaTime * colorChangingSpeed;

            var currentColor = Color.Lerp(defaultSkyColor, inPlayBgColor, _progress);
            SetSkyColor(currentColor);

            var currentHorizonColor = Color.Lerp(defaultHorizonColor, inPlayBgColor, _progress);
            SetHorizonColor(currentHorizonColor);
        }
    }

    private void GetSkyColorBack()
    {
        _progressWhenFinished += Time.deltaTime * colorChangingSpeed;

        var currentColor = Color.Lerp( inPlayBgColor,defaultSkyColor, _progressWhenFinished);
        SetSkyColor(currentColor);

        var currentHorizonColor = Color.Lerp( inPlayBgColor,defaultHorizonColor, _progressWhenFinished);
        SetHorizonColor(currentHorizonColor);
    }
    
    
    private void OnApplicationQuit()
    {
        SetAnimalOutlineColor(startColor);
        SetSkyColor(defaultSkyColor);
        SetHorizonColor(defaultHorizonColor);
    }


    private void SetSkyColor(Color color)
    {
        skyMaterial.SetColor(_skyColor, color);
    }

    private void SetHorizonColor(Color color)
    {
        skyMaterial.SetColor(_horizontalColor, color);
    }

    private void SetAnimalOutlineColor(Color color)
    {
        animalMaterial.SetColor(_emissionColor, color);
    }

  
}