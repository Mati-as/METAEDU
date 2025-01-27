using System;
using System.Collections;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>
///    각 씬별 GameManager는 IGameManager를 상속받아 구현됩니다.
/// </summary>
public abstract class IGameManager : MonoBehaviour
{
    
    protected  readonly float CLICKABLE_DELAY = 0.12f;
    protected WaitForSeconds _waitForClickable;
    public bool isClickable { get; private set; }

    /// <summary>
    /// 08/13/2024
    /// 1.Click빈도수를 유니티상에서 제어할때 사용합니다.
    /// 2.물체가 많은경우 정확도 이슈로 센서자체를 필터링 하는것은 권장되지 않다고 판단하고 있습니다.
    /// </summary>
    protected void SetClickableWithDelay() => StartCoroutine(SetClickableWithDelayCo());

    IEnumerator SetClickableWithDelayCo()
    {
        if(_waitForClickable ==null) _waitForClickable = new WaitForSeconds(CLICKABLE_DELAY);
        
        isClickable = false;
        yield return _waitForClickable;
        isClickable = true;
    }
     
    
    
    private bool _isStartBtnClicked;
    
    public bool isStartButtonClicked
    {
        get
        {
            return _isStartBtnClicked;
        }
        set
        {
            _isStartBtnClicked = value;
        }
    } // startButton 클릭 이전,이후 동작 구분용입니다. 
    protected bool isInitialized { get; private set; }
    protected int TARGET_FRAME { get; } = 60; //

    protected float BGM_VOLUME { get; set; } = 0.105f;

    public static float DEFAULT_SENSITIVITY { get; protected set; } //런타임에서 고정
    protected float SHADOW_MAX_DISTANCE { get; set; } //런타임에서 고정

    public LayerMask layerMask;

    // Ray 및 상호작용 관련 멤버
    public static Ray GameManager_Ray { get; private set; } // 마우스, 발로밟은 위치에 Ray 발사. 

    public RaycastHit[]
        GameManager_Hits { get; set; } // Ray에 따른 객체를 GameManager에서만 컨트롤하며, 다른객체는 이를 참조합니다. 즉 추가적인 레이를 발생하지 않습니다. 

    public static event Action On_GmRay_Synced;
    public static event Action<string,DateTime> OnSceneLoad;

    protected virtual void Awake()
    {
        Init();
      
    }
    


    protected virtual void Init()
    {
        if (isInitialized)
        {
#if UNITY_EDITOR
            Debug.LogWarning("Scene is already initialized.");
#endif

          
            return;
        }
     
        
        ManageProjectSettings(SHADOW_MAX_DISTANCE, DEFAULT_SENSITIVITY);
        BindEvent();
        SetResolution(1920, 1080, TARGET_FRAME);

        if (!SceneManager.GetActiveScene().name.Contains("LAUNCHER"))
        {
            PlayNarration();
        }
        
        var uiLayer = LayerMask.NameToLayer("UI");
        LayerMask maskWithoutUI = ~(1 << uiLayer);
        layerMask = maskWithoutUI;
        
#if UNITY_EDITOR
        Debug.Log("scene is initialzied");
#endif
        Debug.Log("scene is initialzied");
        OnSceneLoad?.Invoke(SceneManager.GetActiveScene().name,System.DateTime.Now);
        isInitialized = true;
    }


    protected void OnOriginallyRaySynced()
    {
        GameManager_Ray = RaySynchronizer.initialRay;
        GameManager_Hits = Physics.RaycastAll(GameManager_Ray, Mathf.Infinity, layerMask);

        On_GmRay_Synced?.Invoke();
    }

    protected virtual void ManageProjectSettings(float defaultShadowMaxDistance, float defaultSensitivity)
    {
        DEFAULT_SENSITIVITY = defaultSensitivity;
        // Shadow Settings-------------- 게임마다 IGameMager상속받아 별도 지정
        var urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

        if (urpAsset != null)
        {
            // Max Distance 값을 설정합니다.
            SHADOW_MAX_DISTANCE = defaultShadowMaxDistance;
            urpAsset.shadowDistance = SHADOW_MAX_DISTANCE;
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError("Current Render Pipeline is not Universal Render Pipeline.");
#endif
            
        }
    }

    /// <summary>
    ///   1. Raysynchronizer에서 첫번째 ray 동기화합니다. 
    /// </summary>
    public virtual void OnRaySynced()
    {
        
    }

    /// <summary>
    /// 1. 게임종료, 스타트버튼 미클릭등 다양한 로직에서, 더이상 Ray발사(클릭)을 허용하지 않아야합니다.
    /// 2. 이는 씬이동간 에러방지, 게임내에서 로직충돌등을 방지하기 위해 사용합니다.
    /// 3. 이를 일괄적으로 검사 및 클릭가능여부를 반환합니다. 
    /// </summary>
    protected  bool PreCheck()
    {
        if (!isStartButtonClicked)
        {
#if UNITY_EDITOR
            Debug.Log("StartBtn Should be Clicked");
#endif
            return false ;
        }

        if (!isInitialized)
        {
#if UNITY_EDITOR
            Debug.Log("Scene hasn't been initialized yet, Can't be clicked");
#endif
            return false;
        }



        if (Managers.isGameStopped)
        {
#if UNITY_EDITOR
            Debug.Log("gameStopped, Can't be clicked");
#endif
            return false;
        }
        return true;
        
    }

    protected virtual void BindEvent()
    {
#if UNITY_EDITOR
        Debug.Log("Ray Sync Subscribed, RayHits being Shared");
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

    protected virtual void OnDestroy()
    {
        RaySynchronizer.OnGetInputFromUser -= OnOriginallyRaySynced;
        UI_Scene_Button.onBtnShut -= OnStartButtonClicked;
        On_GmRay_Synced -= OnRaySynced;
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
        // skip narration if it is the launcher scene
        if (!IsLauncherScene())
        {
            // delay for narration
            DOVirtual.Float(0, 1, 1.5f, _ => { })
                .OnComplete(() =>
                {

                   var isPlaying=  Managers.soundManager.Play(SoundManager.Sound.Narration,
                        "Audio/나레이션/Intro/" + SceneManager.GetActiveScene().name + "_Intro", 0.8f);
                    
#if UNITY_EDITOR
                    Debug.Log(
                        $"Intro Narration Playing........{isPlaying}");
#endif
                });

            Managers.soundManager.Play(SoundManager.Sound.Bgm, $"Audio/Bgm/{SceneManager.GetActiveScene().name}",
                BGM_VOLUME);
        }
    }

    private bool IsLauncherScene()
    {
        return SceneManager.GetActiveScene().name.Contains("LAUNCHER");
    }

    private void SetResolution(int width, int height, int targetFrame)
    {
        Screen.SetResolution(width, height, Screen.fullScreen);
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = targetFrame;

#if UNITY_EDITOR
        Debug.Log(
            $"Game Title: {SceneManager.GetActiveScene().name}," +
            $" Frame Rate: {TARGET_FRAME}, vSync: {QualitySettings.vSyncCount}" +
            $"Shadow Max Distance: {SHADOW_MAX_DISTANCE},");
#endif
    }

    private void OnApplicationQuit()
    {
        
    }
}