using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class CrabVideoGameManager : Base_VideoGameManager
{
    
#if UNITY_EDITOR
    [Header("Debug Only")]
    public bool ManuallyReplay;
    [FormerlySerializedAs("_particleSystem")]
    [Space(15f)]
#endif
  
    [Header("Particle and Audio Setting")] [SerializeField]
    private ParticleSystem[] _particleSystems;

    [SerializeField] private AudioSource _particleSystemAudioSource;
    public static bool _isShaked;

    private bool _isReplayEventTriggered;

    public bool _isCrabAppearable { get; private set; }

    public static event Action OnReplay;
    public GameObject UI_Scene;
    // Start is called before the first frame update
    private void Start()
    {
        Init();
        
        _isCrabAppearable = true;
        CrabEffectManager.Crab_OnClicked -= CrabOnClicked;
        CrabEffectManager.Crab_OnClicked += CrabOnClicked;
    }

    public float replayOffset;

    private void Update()
    {
        if (videoPlayer.isPlaying && !_isReplayEventTriggered)
        {
            // 비디오의 현재 재생 시간과 총 재생 시간을 가져옴

            var currentTime = videoPlayer.time;
            var totalDuration = videoPlayer.length;

            // 비디오가 95% 이상 재생되었는지 확인
            if (currentTime / totalDuration >= 0.99
#if UNITY_EDITOR
                || ManuallyReplay
#endif
               )
            {
                DOVirtual.Float(0, 0, replayOffset, nullParam => { })
                    .OnComplete(() =>
                    {
                        _isReplayEventTriggered = true;
                        ReplayTriggerEvent();
                    });


                DOVirtual.Float(1, 0, 2f, speed => { videoPlayer.playbackSpeed = speed; }).OnComplete(() =>
                {
#if UNITY_EDITOR
                    Debug.Log("RelayTriggerReady");
#endif
                    videoPlayer.time = 0;

                    DOVirtual.Float(0, 0, 1f, duration => { })
                        .OnComplete(() =>
                        {
#if UNITY_EDITOR
                            ManuallyReplay = false;
#endif
                            _isReplayEventTriggered = false;
                            _isShaked = false;
                        });
                });
            }
        }
    }

    private void OnDestroy()
    {
        CrabEffectManager.Crab_OnClicked -= CrabOnClicked;
    }

    
    protected override void Init()
    {
        base.Init();
        UI_Scene.SetActive(false);
        DOVirtual.Float(1, 0, 1.1f, speed =>
        {
            videoPlayer.playbackSpeed = speed;
            // 점프메세지 출력 이후 bool값 수정되도록 로직변경 필요할듯 12/26
        }).OnComplete(() =>
        {
            UI_Scene.SetActive(true);
        });
    }

    public int crabAppearClickCount;
    private int _currentClickCount;

    public static event Action OnCrabAppear;
    private void CrabOnClicked()
    {
        if (!_initiailized) return;

        
        if (!_isShaked)
        {
            transform.DOShakePosition(2.25f, 1f+(0.1f * _currentClickCount), randomness: 90, vibrato: 5);
        }

        _currentClickCount++;

        if (_currentClickCount > crabAppearClickCount)
        {
            OnCrabAppear?.Invoke();
            _isCrabAppearable = false;
            DOVirtual.Float(0, 1, 1f, speed => { videoPlayer.playbackSpeed = speed; })
                .OnComplete(() => { _isShaked = true; });
        }
      
    }
    
    private void ReplayTriggerEvent()
    {
        foreach (var ps in _particleSystems)
        {
            ps.Play();
        }

        _currentClickCount = 0;
        _isCrabAppearable = true;
        SoundManager.FadeInAndOutSound(_particleSystemAudioSource);
        OnReplay?.Invoke();
    }
}