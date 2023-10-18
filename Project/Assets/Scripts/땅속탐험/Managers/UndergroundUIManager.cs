using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using KoreanTyper;
using UniRx;
using System.Linq;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;



public class UndergroundUIManager : MonoBehaviour
{
    enum UI
    {
        HowToPlayA,
        HowToPlayB,
        StoryA,
        StoryB,
        Finish
    }
    
    
    [Header("References")]//-------------------------------
    public GroundGameManager gameManager;

    [Header("Intervals")] //-------------------------------
    public float stagesInterval;
    public float UIInterval;
    
    
    [Space(30f)]
    
    [Header("Tutorial UI Parts")]//-------------------------------
    public GameObject tutorialUIGameObject;
    public CanvasGroup tutorialUICvsGroup;
    public TMP_Text tutorialTMPText;
    public RectTransform tutorialUIRectTransform;
    [FormerlySerializedAs("tutorialAwayPosition")] public RectTransform tutorialAwayTransfrom;
    [Space(10f)] [Header("Tutorial Message Settings")]

    public string tutorialMessage;

    [Space(30f)]
    [Header("Story UI")] //-------------------------------
    [Space(10f)]
    public GameObject storyUIGameObject;
    public CanvasGroup storyUICvsGroup;
    public RectTransform storyUIRectTransform;
    [SerializeField]
    private RectTransform _storyUIInitialRectPos;
    [SerializeField]
    private TMP_Text _storyUITmp;
    [SerializeField]
    private UIAudioController _uiAudioController;
    
    [Space(10f)]
    [Header("Story Message Settings")]
    [Space(10f)]
    
    public string _firstUIMessage = " 땅속에는 다양한 동물친구가 살고 있어요! 저를 따라오면서 구경해볼까요?";
    public string _lastUIMessage = "우와! 친구들을 모두 찾았어요!"; 
     
     [Space(30f)]
     [Header("Audio")]  [Space(10f)]//-------------------------------
  
     
     [Space(30f)]
     [Header("etc")]  [Space(10f)]//-------------------------------
    [SerializeField] 
    private Transform playerIcon;
    [SerializeField] 
    private Transform playerIconDefault;
    [SerializeField] 
    private Transform playerIconMovePosition;
    
    private readonly Dictionary<float, WaitForSeconds> waitForSecondsCache = new();
    private WaitForSeconds GetWaitForSeconds(float seconds)
    {
        if (!waitForSecondsCache.ContainsKey(seconds)) waitForSecondsCache[seconds] = new WaitForSeconds(seconds);
        return waitForSecondsCache[seconds];
    }
    
    private void Awake()
    {
        tutorialUIGameObject.SetActive(true);
        _storyUIInitialRectPos = storyUIRectTransform;  
        DOTween.Init();
        Init();
        tutorialUICvsGroup.DOFade(1, 1);
    }

    private void Init()
    {
        storyUICvsGroup.alpha = 0;
        tutorialUICvsGroup.alpha = 0;
    }
    private void Start()
    {
        gameManager.UIintroDelayTime
            .Subscribe(_ => SetUIIntroUsingUniRx())
            .AddTo(this);

        gameManager.currentStateRP
            .Do(currentState => Debug.Log($"Current state is: {currentState.GameState}"))
            .Where(_currentState => _currentState.GameState== IState.GameStateList.GameStart)
            .Subscribe(_ =>  OnGameStart())
            .AddTo(this);
        
        gameManager.currentStateRP
            .Where(_currentState =>_currentState.GameState == IState.GameStateList.StageStart)
            .Subscribe(_ =>  OnStageStart())
            .AddTo(this);
        
        gameManager.isGameFinishedRP
            .Where(value =>value == true)
            .Delay(TimeSpan.FromSeconds(8f))
            .Subscribe(_ =>  OnGameOver())
            .AddTo(this);
        
    }

    [SerializeField] private GameObject buttonToDeactivate;

    private void OnGameOver()
    {
        Debug.Log("종료UI표출");
        buttonToDeactivate.SetActive(false);
       
       
      

        LeanTween.move(storyUIRectTransform, Vector2.zero, 3f)
            .setEase(LeanTweenType.easeInOutBack)
            .setOnComplete(() => LeanTween.delayedCall(1.0f,MoveUIRight));
        
        
            UpdateUI(storyUICvsGroup, _storyUITmp, _lastUIMessage);
    }
    private void OnGameStart()
    {
        LeanTween.move(tutorialUIRectTransform,
                new Vector2(0, tutorialAwayTransfrom.position.y),
                2f)
            .setEase(LeanTweenType.easeInOutBack);
            
        Observable.Timer(TimeSpan.FromSeconds(stagesInterval))
            .Do(_ => UpdateUI(storyUICvsGroup, _storyUITmp, _firstUIMessage)).Subscribe().AddTo(this);
    }

    public RectTransform uiAwayPosition;
    private void MoveUIRight()
    {
        LeanTween.move(storyUIRectTransform, uiAwayPosition.position, 3f).setEase(LeanTweenType.easeInOutBack);
    }

    private void OnStageStart()
    {
       
        Observable.Timer(TimeSpan.FromSeconds(0))
            .Do(_ =>{
                LeanTween.move(storyUIRectTransform,
                    new Vector2(0, tutorialAwayTransfrom.position.y),
                    3f).setEase(LeanTweenType.easeInOutBack);

                Debug.Log("UI OnstageStart");
                storyUICvsGroup.DOFade(0, 0.5f);
            }).Subscribe().AddTo(this);
        
    }
    
    private void UpdateUI(CanvasGroup cvsGroup, TMP_Text tmptext, string message)
    {
        ActivateWithFadeInUI(cvsGroup);
        ChangeUIText(tmptext,message);
    }
    private void ActivateWithFadeInUI(CanvasGroup cvsGroup)
    {
        cvsGroup.alpha = 0;
        cvsGroup.DOFade(1, 1);
    }

    private void ChangeUIText(TMP_Text tmptext, string message)
    {
        tmptext.text = message;
    }
    

    [SerializeField] private float cameraMoveTime;
    private float cameraMoveElapsed;
    
    public static float INTRO_UI_DELAY;
    public static float INTRO_UI_DELAY_SECOND;
    public GameObject howToPlayUI;
    private void SetUIIntroUsingUniRx()
    {
        Observable.Timer(TimeSpan.FromSeconds(INTRO_UI_DELAY))
            .Do(_ =>
            {
                howToPlayUI.SetActive(true);
                Debug.Log("Second introduction message.");
            });
        
    }
    

    
}