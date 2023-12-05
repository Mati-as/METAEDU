using DG.Tweening;
using UnityEngine;

public class StarryNight_StarlightController : MonoBehaviour
{
    
    public Light[] spotLights;

    [Header("Color")] public Color[] colors;
    [Header("Interval")] 
    
    [Range(0.5f, 20)]
    public float intervalMin;
    [Range(0.5f, 20)]
    public float intervalMax;
    
    private float _determinedInterval;
    
    [Space(15f)] 
    
    [Header("Intensity")]
    
    public float defaultIntensity;
    
    [Range(0, 5)]
    public float intensityMin;
    [Range(0, 5)]
    public float intensityMax;

    private float _determinedIntensity;
    private void Start()
    {
        Init();
        DOTween.Init();
       
        _determinedIntensity = Random.Range(intensityMin, intensityMax);
        _determinedInterval = Random.Range(intervalMin, intervalMax);
        foreach (var light in spotLights) DoVirtualIntensityIncrease(light);
    }

    private void Init()
    {
        var count = 0;
        foreach (var lit in spotLights)
        {
            lit.intensity = defaultIntensity;
            lit.color = colors[count];
            count++;
        }
    }

    private void DoVirtualIntensityIncrease(Light light)
    {
        _determinedIntensity = Random.Range(intensityMin, intensityMax);
        _determinedInterval = Random.Range(intervalMin, intervalMax);
        
        DOVirtual.Float(defaultIntensity, _determinedIntensity, 0.6f
                , value => light.intensity = value)
            .OnComplete(() => DoVirtualIntensityDecrease(light,_determinedIntensity,_determinedInterval))
            .SetDelay(_determinedInterval);
    }


    private void DoVirtualIntensityDecrease(Light light,float intensity,float delay)
    {
        DOVirtual.Float(intensity, defaultIntensity, 0.8f
                , value => light.intensity = value)
            .OnComplete(() => DoVirtualIntensityIncrease(light)
            ).SetDelay(delay);
    }
}