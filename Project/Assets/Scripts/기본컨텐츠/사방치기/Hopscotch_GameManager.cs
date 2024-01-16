using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

public class Hopscotch_GameManager : MonoBehaviour
{
  
    [Header("Reference")] 
    [SerializeField]
    private Hopscotch_EffectController effectController;
    [Space(20f)]
    
    public Transform stepGroup;

    private RectTransform[] _numberTextRects;
    private Transform[] _steps;
    private int _stepCount;

    private ParticleSystem _inducingParticle;
    private ParticleSystem _successParticle;
    private readonly string PATH ="게임별분류/기본컨텐츠/Hopscotch/";

    public float successParticleDuration;
    private bool _isSuccesssParticlePlaying;
    public float offset;
    
    
    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        GameObject inPlayTexts = GameObject.Find("InPlayTexts");
        
        if (inPlayTexts != null)
        {
           
            _numberTextRects = inPlayTexts.GetComponentsInChildren<RectTransform>()
                .Where(rt => rt.gameObject != inPlayTexts)
                .ToArray();
        }
        else
        {
            Debug.LogError("GameObject named 'InPlayTexts' not found in the scene.");
        }
        

        DoIntroMove();
    }
    
#if UNITY_EDITOR
public bool isManuallyInvoked;
private void Update()
{
    if (isManuallyInvoked)
    {
        onStageClear?.Invoke();
        isManuallyInvoked = false;
    }


}
#endif

    private void OnClick()
    {
        // 발판 밟은 직후에는 다음 발판을 누를 수 없게합니다.
        if (_isSuccesssParticlePlaying) return;

        if (CheckOnStep())
        {
            PlaySuccessParticle(_currentStep);
        }
        
    }

    private void BindEvent()
    {
        Hopscotch_EffectController.Hopscotch_OnClick -= OnClick;
        Hopscotch_EffectController.Hopscotch_OnClick += OnClick;
        onStageClear -= OnStageClear;
        onStageClear += OnStageClear;
    }

    private void OnDestroy()
    {
        onStageClear -= OnStageClear;
        Hopscotch_EffectController.Hopscotch_OnClick -= OnClick;
    }

    private void Init()
    {
        BindEvent();
        LoadParticles();
        GetSteps();
    }

    private void LoadParticles()
    {
        var psPrefab1 = Resources.Load<GameObject>(PATH + "CFX_inducingParticle");
        var psPrefab2 = Resources.Load<GameObject>(PATH + "CFX_successParticle");
        
        if(psPrefab1 != null)
        {
            _inducingParticle = Instantiate(psPrefab1 ,transform).GetComponent<ParticleSystem>();;
            _inducingParticle.Stop();
        }
        else
        { 
#if UNITY_EDITOR
         
            Debug.LogError($"널에러 발생: {PATH}" + "CFX_inducingParticle");
#endif
        }
        
        if(psPrefab2 != null)
        {
            _successParticle = Instantiate(psPrefab2,transform).GetComponent<ParticleSystem>();;
            _successParticle.Stop();
        }


        // var psPrefab3 = Resources.Load<GameObject>(PATH + "CFX_StageClear_Bubble");
        //
        // if (psPrefab3 != null)
        // {
        //     _stageClearBubble = Instantiate(psPrefab3, transform).GetComponent<ParticleSystem>();
        //     _stageClearBubble.Stop();
        // }
    }

    private void GetSteps()
    {
        _stepCount = stepGroup.childCount;
        _steps = new Transform[_stepCount];
        
        for (int i = 0; i < _stepCount; ++i)
        {
            _steps[i] = stepGroup.GetChild(i);
        }

        targetPos = new Vector3[_stepCount];
        for (var i = 0; i < _stepCount; i++) targetPos[i] = _steps[i].transform.position;
        
        foreach (var step in _steps)
        {
            step.position += defaultOffset;
        }
    }

    public float numberDoScaleSize;
    private float _defalutSize;


    private void OnScaleSequenceKilled(RectTransform number)
    {
        if (_scaleBackSequence.IsActive())
        {
            _scaleBackSequence.Kill();
        }

        _scaleBackSequence = DOTween.Sequence();
        
        _scaleBackSequence.Append(number.DOScale(1.7f, 0.8f).SetEase(Ease.Linear));
        _scaleBackSequence.Play();
    }
    
    private void DoScaleUp(RectTransform number)
    {
       
        _currentScaleSequence = DOTween.Sequence();

        _currentScaleSequence
            .Append(number
            .DOScale(numberDoScaleSize, 1.2f)
            .OnKill(() =>
                {

                    OnScaleSequenceKilled(number);
                })
            
            .SetEase(Ease.OutBounce))
           
            .Append(number
            .DOScale(1.7f, 1.2f)
            .SetDelay(0.3f)
           
            .OnKill(() =>
            {

                OnScaleSequenceKilled(number);
            })
            .SetEase(Ease.OutBounce))
            .SetLoops(-1, LoopType.Yoyo); // 무한 반복 설정

        _currentScaleSequence.Play();
    }

    private int _currentStep = 0;
    private bool _isGameFinished;

    private void PlayInducingParticle(int currentPosition)
    {
        if (currentPosition >= _steps.Length) return;
       
        
        
            _inducingParticle.Stop();
            _inducingParticle.gameObject.transform.position = AddOffset(_steps[currentPosition].position);
            _inducingParticle.gameObject.SetActive(true);
            _inducingParticle.Play();
        
    }

    private Sequence _currentScaleSequence;
    private Sequence _scaleBackSequence;
    public static event Action onStageClear;
    private Vector3 AddOffset(Vector3 position)
    {
        return position + Vector3.down * offset;
    }

    public float nextStepInducingParticleDelay;
    private void PlaySuccessParticle( int currentPosition)
    {
            _currentScaleSequence.Kill();
                                      
#if UNITY_EDITOR
            Debug.Log($"succuessful! : currentStep : {_currentStep}");
#endif
            
            _inducingParticle.Stop();
            _successParticle.Stop();
            _successParticle.gameObject.transform.position = AddOffset(_steps[currentPosition].position);
            _successParticle.gameObject.SetActive(true);
            _successParticle.Play();

            _isSuccesssParticlePlaying = true;
            DOVirtual
                .Float(0, 0, successParticleDuration, val => val++)
                .OnComplete(() =>
                {
                    _successParticle.Stop();
                    _successParticle.gameObject.SetActive(false);
                    
                    DOVirtual
                        .Float(0, 0, nextStepInducingParticleDelay, val => val++)
                        .OnComplete(() =>
                        {
                            _isSuccesssParticlePlaying = false;
                            _currentStep++;
                            
#if UNITY_EDITOR
                            Debug.Log($"currentStep : {_currentStep} , step Count:{_stepCount}");
#endif
                            if (_currentStep >= _stepCount)
                            {
#if UNITY_EDITOR
                                Debug.Log($"Replay Invoked!");
#endif
                                onStageClear?.Invoke();
                            }
                            else
                            {
                                DoScaleUp(_numberTextRects[_currentStep]);
                                PlayInducingParticle(_currentStep);
                            }
                          
                        });
                });
    }
    
    public Vector3[] targetPos;
    public float stackInterval;
    public Vector3 defaultOffset;

    private void DoIntroMove()
    {
        for (var i = 0; i < _stepCount; ++i)
        {
            var i1 = i;
            _steps[i].transform
                .DOMove(targetPos[i], 1f + stackInterval * i)
                .OnComplete(() =>
                {
                    if (i1 >= _stepCount - 1)
                    {
                        DOVirtual
                            .Float(0, 0, 2, val => val++)
                            .OnComplete(() =>
                            {
#if UNITY_EDITOR
                                Debug.Log("시작시 1 Doscale");
#endif
                                if (i1 >= _stepCount - 1) DoScaleUp(_numberTextRects[0]);
                                
                                //DoScaleUp(_numberTextRects[1]);
                                if (i1 >= _stepCount - 1) PlayInducingParticle(0);
                            });
                    }
                      
                })
                .SetDelay(2f);
        }
    }

    public float waitTimeToRestartGame;
    public ParticleSystem _stageClearBubble;

    private void OnStageClear()
    {
        _currentStep = 0;
        
        _stageClearBubble.Play();
        
        DOVirtual.Float(0, 0, waitTimeToRestartGame, val => val++)
            .OnComplete(() =>
            {
                PlayInducingParticle(_currentStep);
            });

    }
    private bool CheckOnStep()
    {

        foreach (var hit in effectController.hits)
        {
            if (hit.transform.gameObject.name == "Step_" + _currentStep.ToString())
            {
                
#if UNITY_EDITOR
                Debug.Log("Step_" + _currentStep.ToString());
#endif
                return true;
            }
        }
        return false;
    }
}
