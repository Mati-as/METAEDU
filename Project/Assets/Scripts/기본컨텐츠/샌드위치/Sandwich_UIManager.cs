using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyCustomizedEditor.Common.Util;
using UnityEngine;

public class Sandwich_UIManager : UI_PopUp
{


    enum SandwichUI
    {
        RectUI_Sandwich_AnimalReaction,
        Complete
    }



    private GameObject _animalReaction;
    private GameObject _complete;
    
    private Vector3 _reactionDefaultScale;
    private Vector3 _completeDefaultScale;
    
    private RectTransform _rectAnimalReaction;
    private RectTransform _rectComplete;
    
    public override bool Init()
    {

        BindObject(typeof(SandwichUI));
        
        _animalReaction = GetObject((int)SandwichUI.RectUI_Sandwich_AnimalReaction);
        _rectAnimalReaction = _animalReaction.GetComponent<RectTransform>();
        _reactionDefaultScale = _rectAnimalReaction.localScale;
        _rectAnimalReaction.localScale = Vector3.zero;
        _animalReaction.SetActive(false);
      
        
        _complete = GetObject((int)SandwichUI.Complete);
        _rectComplete = _complete.GetComponent<RectTransform>();
        _completeDefaultScale = _rectComplete.localScale;
        _rectComplete.localScale = Vector3.zero;
        _complete.SetActive(false);

        sandwich_AnimalController.onFinishEating -= OnFinishEationg;
        Sandwitch_GameManager.onSandwichMakingFinish -= OnSandwichMakingFinish;

        sandwich_AnimalController.onFinishEating += OnFinishEationg;
        Sandwitch_GameManager.onSandwichMakingFinish += OnSandwichMakingFinish;
        

        return true;
    }

    private void OnFinishEationg()
    {
        _animalReaction.SetActive(true);
        _rectAnimalReaction
            .DOScale(_reactionDefaultScale, 0.75f)
            .SetEase(Ease.OutBounce)
            .OnStart(() =>
            {
                Managers.Sound.Play(SoundManager.Sound.Effect,
                    "Audio/Gamemaster Audio - Fun Casual Sounds/Comedy_Cartoon/beep_zap_fun_03");
            })
            .OnComplete(() =>
        {
            DOVirtual.Float(0, 0, 3f,
                _ => { }).OnComplete(() => { _rectAnimalReaction.DOScale(Vector3.zero, 0.75f).SetEase(Ease.OutBounce);});
        });
        
        
        
    }
    
    private void OnSandwichMakingFinish()
    {
        _complete.SetActive(true);
        _rectComplete
            .DOScale(_completeDefaultScale, 0.75f)
            .SetEase(Ease.OutBounce)
            .OnStart(() =>
            {
                // Managers.Sound.Play(SoundManager.Sound.Effect,
                //     "Audio/Gamemaster Audio - Fun Casual Sounds/Comedy_Cartoon/beep_zap_fun_03");
            })
            .OnComplete(() =>
            {
                DOVirtual.Float(0, 0, 1.45f,
                    _ => { }).OnComplete(() => { _rectComplete.DOScale(Vector3.zero, 0.75f).SetEase(Ease.OutBounce);});
            });
        
        
        
    }
    
    
    
    
}
