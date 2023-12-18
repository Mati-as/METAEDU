using System;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class Playgorund_Ball_Base : MonoBehaviour
{
    [SerializeField] private BallInfo ballInfo;

    //Size Settings.
    [Range(0, 2)] public int size;

    //Color Settings.
    private static Color[] colors;
    private Color _color;
    private int _currentColorIndex;

    private Material _material;
    private MeshRenderer _meshRenderer;

    //Sound Settings
    private AudioClip sound;
    private AudioSource[] _audioSources;
    private AudioSource[] _xylophoneAudioSources;
    private AudioSource[] holeSoundSource;
    private int _audioSize = 5;


    //클릭 가능 여부 판정을 위해 Collider 할당 및 제어.
    private Collider _collider;

    public static event Action OnBallIsOnTheNet;
    public static event Action OnBallIsInTheHole;

    [SerializeField] private Playground_BallSpawner _ballSpawner;

    private Rigidbody _rb;

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        _path = new Vector3[3];

        SetAudio();

        //material은 static이기 때문에, 직접적으로 수정하지 않기 위한 tempMat 설정  .
        GetComponents();

        ColorInit();
        SetColor();

        SetScale();
    }

    private void Subscribe()
    {
        OnBallIsOnTheNet += Respawn;
        OnBallIsOnTheNet += Respawn;

        OnBallIsInTheHole += Respawn;
        OnBallIsInTheHole += Respawn;
    }


    protected virtual void OnGoal()
    {
    }

    protected virtual void OnHitWall()
    {
    }

    protected virtual void OnEnterHole()
    {
    }

    private Vector3[] _path;

    private void OnTriggerEnter(Collider other)
    {
        
        
        if (size == (int)BallInfo.BallSize.Small && other.transform.gameObject.name == "Hole")
        {
            // 중복클릭방지
            _collider.enabled = false;

            _path[0] = transform.position;
            _path[1] = other.transform.position - Vector3.down * ballInfo.offset;
            _path[2] = other.transform.position + Vector3.down * ballInfo.depth;

            transform.DOPath(_path, ballInfo.durationIntoHole, PathType.CatmullRom)
                .OnComplete(() => { DOVirtual.Float(0, 1, ballInfo.respawnWaitTime, value => value++)
                    .OnComplete(() =>
                    {
                        Respawn();
                    }); 
                });
        }

        else if (other.transform.gameObject.name == "Net")

        {
            transform.DOScale(0, 1.5f).SetEase(Ease.InBounce).OnComplete(() =>
            {
                gameObject.SetActive(false);
                DOVirtual.Float(0, 1, ballInfo.respawnWaitTime, value => value++)
                    .OnComplete(() => { Respawn(); });
            });
        }
     
      
        
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.transform.gameObject.name == "Wall")
        {

            FindAndPlayAudio(_xylophoneAudioSources);
        }

        if (other.transform.gameObject.name != "Ground" &&
            other.transform.gameObject.name == "Big"||
            other.transform.gameObject.name == "Small" ||
            other.transform.gameObject.name == "Medium")
        {
            FindAndPlayAudio(_audioSources);

        }
    }

  

    private void GetComponents()
    {
        var tempMat = GetComponent<Material>();
        _material = tempMat;
        _meshRenderer = GetComponent<MeshRenderer>();

        _collider = GetComponent<Collider>();

        _rb = GetComponent<Rigidbody>();
    }

    private void ColorInit()
    {
        if (colors == null) colors = new Color[3];

        colors[(int)BallInfo.BallColor.Red] = ballInfo.colorDef[(int)BallInfo.BallColor.Red];
        colors[(int)BallInfo.BallColor.Yellow] = ballInfo.colorDef[(int)BallInfo.BallColor.Yellow];
        colors[(int)BallInfo.BallColor.Blue] = ballInfo.colorDef[(int)BallInfo.BallColor.Blue];
    }

    private void SetColor()
    {
        _currentColorIndex = Random.Range((int)BallInfo.BallColor.Red, (int)BallInfo.BallColor.Blue + 1);
        _color = colors[_currentColorIndex];

        _meshRenderer.material.color = _color;
    }

    private void SetScale()
    {
        transform.localScale =
            ballInfo.ballSizes[size] * Vector3.one *
            Random.Range(1 - ballInfo.sizeRandomInterval, 1 + ballInfo.sizeRandomInterval);
    }


    private void Respawn()
    {
#if UNITY_EDITOR
        Debug.Log("Ball is Respawned");
#endif

        gameObject.SetActive(true);

        transform.DOScale(ballInfo.ballSizes[size], 1.5f).SetEase(Ease.InBounce); 
#if UNITY_EDITOR
        Debug.Log($"current rotation : {transform.rotation}");
#endif

        SetColor();
        _collider.enabled = true;
        transform.position = _ballSpawner.spawnPositions[Random.Range(0, 3)].position;
        transform.Rotate(Vector3.left);
        _rb.AddForce(-_ballSpawner.spawnPositions[0].up * ballInfo.respawnPower, ForceMode.Impulse);
        
    }
    
    private void FindAndPlayAudio(AudioSource[] audioSources, bool recursive = false,
        float volume = 0.8f)
    {
#if UNITY_EDITOR
        Debug.Log("sound play (walls)");
#endif
            var availableAudioSource = Array.Find(audioSources, audioSource => !audioSource.isPlaying);

            if (availableAudioSource != null) FadeInSound(availableAudioSource, volume);

#if UNITY_EDITOR
#endif
        
    }

    private void FadeOutSound(AudioSource audioSource, float target = 0.1f, float fadeInDuration = 2.3f,
        float duration = 1f)
    {
        audioSource.DOFade(target, duration).SetDelay(fadeInDuration).OnComplete(() =>
        {
#if UNITY_EDITOR
#endif
            audioSource.Stop();
        });
    }

    private void FadeInSound(AudioSource audioSource, float targetVolume = 1f, float duration = 0.3f)
    {
#if UNITY_EDITOR
#endif
        audioSource.Play();
        audioSource.DOFade(targetVolume, duration).OnComplete(() => { FadeOutSound(audioSource); });
    }
    
      private void SetAudio()
    {
        _audioSources = new AudioSource[ballInfo.audioSize];
        _xylophoneAudioSources =  new AudioSource[ballInfo.audioSize];
        holeSoundSource =  new AudioSource[ballInfo.audioSize];
        for (var i = 0; i < _audioSources.Length; i++)
        {
            _audioSources[i] = gameObject.AddComponent<AudioSource>();
            _audioSources[i].clip = Resources.Load<AudioClip>("Audio/Playground/Ball");
            _audioSources[i].volume = ballInfo.volume;
            _audioSources[i].spatialBlend = 0f;
            _audioSources[i].outputAudioMixerGroup = null;
            _audioSources[i].playOnAwake = false;
            _audioSources[i].loop = false;

            if (size ==(int)BallInfo.BallSize.Small)
            {
                _audioSources[i].pitch = 1.3f;
            }
            else if (size ==(int)BallInfo.BallSize.Medium)
            {
                _audioSources[i].pitch = 0.8f;
            }
            else if (size ==(int)BallInfo.BallSize.Large)
            {
                _audioSources[i].pitch = 0.5f;
            }
            
        }
        
        
        
        for (var i = 0; i < _audioSources.Length; i++)
        {
            _xylophoneAudioSources[i] = gameObject.AddComponent<AudioSource>();
            _xylophoneAudioSources[i].clip = Resources.Load<AudioClip>("Audio/Playground/Xylophone");
            _xylophoneAudioSources[i].volume = ballInfo.volume;
            _xylophoneAudioSources[i].spatialBlend = 0f;
            _xylophoneAudioSources[i].outputAudioMixerGroup = null;
            _xylophoneAudioSources[i].playOnAwake = false;

            if (size ==(int)BallInfo.BallSize.Small)
            {
                _xylophoneAudioSources[i].pitch = 1.25f;
            }
            else if (size ==(int)BallInfo.BallSize.Medium)
            {
                _xylophoneAudioSources[i].pitch = 1.1f;
            }
            else if (size ==(int)BallInfo.BallSize.Large)
            {
                _xylophoneAudioSources[i].pitch = 0.95f;
                
            }
            
        }

        if (size == (int)BallInfo.BallSize.Small)
        {
            for (var i = 0; i < _audioSources.Length; i++)
            {
                holeSoundSource[i] = gameObject.AddComponent<AudioSource>();
                holeSoundSource[i].clip = Resources.Load<AudioClip>("Audio/Playground/Hole");
                holeSoundSource[i].volume = ballInfo.volume;
                holeSoundSource[i].spatialBlend = 0f;
                holeSoundSource[i].outputAudioMixerGroup = null;
                holeSoundSource[i].playOnAwake = false;

                if (size ==(int)BallInfo.BallSize.Small)
                {
                    holeSoundSource[i].pitch = 1.25f;
                }
                
            }
        }
       
    }
      
      
}