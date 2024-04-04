using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class IGameManager : MonoBehaviour
{
    public static Ray GameManager_Ray { get; private set; }
    public static RaycastHit[] GameManager_Hits { get; set; }
    public static bool isStartButtonClicked { get; private set; }
    protected static bool isInitialized { get;  set; }
    public static event Action On_GmRay_Synced;
    protected virtual int TARGET_FRAME { get; } = 60;
    
    protected  float BGM_VOLUME = 0.105f;

    public static float defaultSensitivity { get; set; }
    
 

    protected virtual void Awake()
    {
        Init();
    }


    protected virtual void Init()
    {
        isStartButtonClicked = false;
        BindEvent();
        SetResolution(1920, 1080, TARGET_FRAME);
        PlayNarration();
        isInitialized = true;

        DOVirtual.Float(0, 0, 1.25f, _ => { }).OnComplete(() =>
        {
            Managers.Sound.Play(SoundManager.Sound.Effect,
                "Audio/나레이션/Narrations/" + SceneManager.GetActiveScene().name + "_Intro");
        });
        
        int uiLayer = LayerMask.NameToLayer("UI");
        LayerMask maskWithoutUI = ~(1 << uiLayer);
        layerMask = maskWithoutUI;

    }


    public LayerMask layerMask;

    protected void OnOriginallyRaySynced()
    {

        GameManager_Ray = RaySynchronizer.initialRay;
        GameManager_Hits = Physics.RaycastAll(GameManager_Ray, Mathf.Infinity, layerMask);

        On_GmRay_Synced?.Invoke();
    }


    /// <summary>
    ///     onRaySync 구현 포인트
    ///     1. EffectManager ray와 동기화 (필수)
    ///     EffectManager 내부에서 처리할 로직처리
    ///     2. 나머지 RaySync가 필요한 경우의 게임로직 처리..
    /// </summary>
    protected virtual void OnRaySynced()
    {
        
       
        if (!isStartButtonClicked) return;
        if (!isInitialized) return;

    }

    protected virtual void BindEvent()
    {
#if UNITY_EDITOR
        Debug.Log("Ray Sync Subscribed");
#endif
        //1차적으로 하드웨어에서 동기화된 Ray를 GameManger에서 읽어옵니다.
        RaySynchronizer.OnGetInputFromUser -= OnOriginallyRaySynced;
        RaySynchronizer.OnGetInputFromUser += OnOriginallyRaySynced;

        //On_GmRay_Synced에서 나머지 Ray관련 로직 분배 및 처리합니다. 
        On_GmRay_Synced -= OnRaySynced;
        On_GmRay_Synced += OnRaySynced;

        UI_Scene_Button.onBtnShut -= OnStartButtonClicked;
        UI_Scene_Button.onBtnShut += OnStartButtonClicked;
    }
    
    private void OnDestroy()
    {
        RaySynchronizer.OnGetInputFromUser -= OnOriginallyRaySynced;
        On_GmRay_Synced -= OnRaySynced;
        UI_Scene_Button.onBtnShut -= OnStartButtonClicked;
    }


    protected virtual void OnStartButtonClicked()
    {
        // 버튼 클릭시 Ray가 게임로직에 영향미치는 것을 방지하기위한 약간의 Delay 입니다. 
        DOVirtual
            .Float(0, 1, 0.5f, _ => { })
            .OnComplete(() => { isStartButtonClicked = true; });
    }


    protected virtual void PlayNarration()
    {
        //delay for narration.
        DOVirtual.Float(0, 1, 2f, _ => { })
            .OnComplete(() =>
            {
                Managers.Sound.Play(SoundManager.Sound.Narration,
                    "Audio/Narration/" + $"{SceneManager.GetActiveScene().name}", 0.5f);
            });

        Managers.Sound.Play(SoundManager.Sound.Bgm, "Audio/Bgm/" + $"{SceneManager.GetActiveScene().name}", BGM_VOLUME);
    }

    private void SetResolution(int width, int height, int targetFrame)
    {
        Screen.SetResolution(width, height, Screen.fullScreen);
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = targetFrame;

#if UNITY_EDITOR
        Debug.Log(
            $"Game Title: {SceneManager.GetActiveScene().name}," +
            $" Frame Rate: {TARGET_FRAME}, vSync: {QualitySettings.vSyncCount}");
#endif
    }
}