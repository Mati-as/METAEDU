using System;
using DG.Tweening;
using UnityEngine;

public class Crab_VideoContentPlayer : Base_VideoContentPlayer
{
    [Header("Particle and Audio Setting")] [SerializeField]
    private ParticleSystem _particleSystem;

    [SerializeField] private AudioSource _particleSystemAudioSource;
    public static bool _isShaked;

    private bool _isReplayEventTriggered;

    public static event Action OnReplay;

    // Start is called before the first frame update
    private void Start()
    {
        Init();

        Crab_EffectController.onClicked -= OnClicked;
        Crab_EffectController.onClicked += OnClicked;
    }

    private void Update()
    {
        if (videoPlayer.isPlaying && !_isReplayEventTriggered)
        {
            // 비디오의 현재 재생 시간과 총 재생 시간을 가져옴

            var currentTime = videoPlayer.time;
            var totalDuration = videoPlayer.length;

            // 비디오가 95% 이상 재생되었는지 확인
            if (currentTime / totalDuration >= 0.95)
            {
                _isReplayEventTriggered = true;
                ReplayTriggerEvent();

                DOVirtual.Float(1, 0, 2f, speed => { videoPlayer.playbackSpeed = speed; }).OnComplete(() =>
                {
#if UNITY_EDITOR
                    Debug.Log("RelayTriggerReady");
#endif
                    videoPlayer.time = 0;

                    DOVirtual.Float(0, 0, 1f, duration => { })
                        .OnComplete(() =>
                        {
                            _isReplayEventTriggered = false;
                            _isShaked = false;
                        });
                });
            }
        }
    }

    private void OnDestroy()
    {
        Crab_EffectController.onClicked -= OnClicked;
    }

    protected override void Init()
    {
        base.Init();

        DOVirtual.Float(1, 0, 1.1f, speed =>
        {
            videoPlayer.playbackSpeed = speed;
             // 점프메세지 출력 이후 bool값 수정되도록 로직변경 필요할듯 12/26
        });
    }

    private void OnClicked()
    {
        if (!_initiailized) return;

        if (!_isShaked)
        {
            DOVirtual.Float(0, 1, 2f, speed => { videoPlayer.playbackSpeed = speed; })
                .OnComplete(() => { _isShaked = true; });

            transform.DOShakePosition(2.25f, 2.5f, randomness: 90, vibrato: 5);
        }
    }

#if UNITY_EDITOR
    public bool manuallyTrigger;
#endif
    private void ReplayTriggerEvent()
    {
        _particleSystem.Play();
        SoundManager.FadeInAndOutSound(_particleSystemAudioSource);
        OnReplay?.Invoke();
    }
}