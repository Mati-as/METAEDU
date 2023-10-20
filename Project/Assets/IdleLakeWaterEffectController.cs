using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using DG.Tweening.Plugins.Options;
using Unity.VisualScripting;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using MyCustomizedEditor;
#endif
public class IdleLakeWaterEffectController : MonoBehaviour
{
    [SerializeField] 
    private ParticleSystem particleSystemPrefab;
    private void Start()
    {
        var trigger = GetComponent<EventTrigger>();
        var entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener(data => { OnClicked((PointerEventData)data);});
        trigger.triggers.Add(entry);
    }
    private List<ParticleSystem> particlePool = new List<ParticleSystem>();
    private int poolSize = 15;
    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            ParticleSystem particleInstance = Instantiate(particleSystemPrefab, transform);
            particleInstance.gameObject.SetActive(false);
            particlePool.Add(particleInstance);
        }
    }
    private ParticleSystem GetFromPool()
    {
        foreach (var particle in particlePool)
        {
            if (!particle.gameObject.activeInHierarchy)
            {
                return particle;
            }
        }

      
        ParticleSystem newParticle = Instantiate(particleSystemPrefab, transform);
        particlePool.Add(newParticle);
        return newParticle;
    }
    private void OnClicked(PointerEventData eventData)
    {
        Ray ray = Camera.main.ScreenPointToRay(eventData.position);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject.name == "Water" || hit.collider.name != "Terrain" && hit.collider.gameObject.name != null)
            {
                ParticleSystem particle = GetFromPool();
                particle.transform.position = hit.point;
                particle.gameObject.SetActive(true);
                particle.Play();
                StartCoroutine(DeactivateAfterPlay(particle));
            }
        }
    }
    
    
    private IEnumerator DeactivateAfterPlay(ParticleSystem particle)
    {
        while (particle.isPlaying)
        {
            yield return null;
        }
        particle.gameObject.SetActive(false);
    }
}
