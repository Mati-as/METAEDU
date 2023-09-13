using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using System.Collections.Generic;


public class AnimalController : MonoBehaviour
{
    [Header("Reference")] [Space(10f)] 
    public GameManager _gameManager;
    public AnimalData _animalData;
    [SerializeField]
    private ShaderAndCommon _shaderAndCommon;
    
    [Header("Initial Setting")] [Space(10f)]
    [Header("On GameStart")] [Space(10f)]
    [Header("On Round Is Ready")] [Space(10f)]
    [Header("On Round Started")] [Space(10f)]
    private float _moveInElapsed;
    [Header("On Corrected")] [Space(10f)]
    private float _increaseSizeLerp;
    private float _currentSizeLerp;
    private bool isAnswer;
    [Header("On Round Finished")] [Space(10f)]
    [Header("On GameFinished")] [Space(10f)]
    
    //▼ 동물 이동 로직
    private readonly string TAG_ARRIVAL= "arrival";
    private bool isTouchedDown;
    private Animator _animator;
    
    public bool IsTouchedDown
    {
        get { return isTouchedDown;}
        set { isTouchedDown = value; }
    }
    
    
    /*
     아래 코루틴 변수들은 IEnumerator 컨테이너 역할만 담당합니다.
     어떤 함수가 사용되는지는 StartCoroutine에서확인 및 디버깅 해야합니다.
     */
    private Coroutine _coroutineA;
    private Coroutine _coroutineB;
    private Coroutine _coroutineC;
    private Coroutine _coroutineD;
    private Coroutine[] _coroutines;
    
    // 코루틴 WaitForSeconds 캐싱 자료사전
    private Dictionary<float, WaitForSeconds> waitForSecondsCache = new Dictionary<float, WaitForSeconds>();

    private WaitForSeconds GetWaitForSeconds(float seconds)
    {
        if (!waitForSecondsCache.ContainsKey(seconds))
        {
            waitForSecondsCache[seconds] = new WaitForSeconds(seconds);
        }
        return waitForSecondsCache[seconds];
    }
    
    // ▼ Unity Loop  -----------------------------------------------
    private void Awake()
    {
        SetCoroutine();
        SubscribeGameManagerEvents();
      
    }
    
    void Start()
    {
        InitializeTransform();
    }
    
    void OnDestroy()
    {
        UnsubscribeGamaManagerEvents();
    }
    
    
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(TAG_ARRIVAL))
        {
            isTouchedDown = true;
#if UNITY_EDITOR
            Debug.Log("Touched Down!");
#endif
        }
    }

    
    // ▼ 메소드 목록 ----------------------------------------------------------------------------

 
    //이벤트 처리 위한 구독
    private void SubscribeGameManagerEvents()
    {
        // 중복 구독 방지.
        GameManager.onAllAnimalsInitialized -= OnOnAllAnimalsInitialized;
        GameManager.onAllAnimalsInitialized += OnOnAllAnimalsInitialized;

        GameManager.onGameStartEvent -= OnGameStart;
        GameManager.onGameStartEvent += OnGameStart;
        
        GameManager.onRoundReadyEvent -= OnRoundReady;
        GameManager.onRoundReadyEvent += OnRoundReady;

        GameManager.onCorrectedEvent -= OnCorrect;
        GameManager.onCorrectedEvent += OnCorrect;

        GameManager.onRoundFinishedEvent -= OnRoundFinished;
        GameManager.onRoundFinishedEvent += OnRoundFinished;

        GameManager.onRoundStartedEvent -= OnRoundStarted;
        GameManager.onRoundStartedEvent += OnRoundStarted;
    }

    private void UnsubscribeGamaManagerEvents()
    {
        GameManager.onAllAnimalsInitialized -= OnOnAllAnimalsInitialized;
        GameManager.onGameStartEvent -= OnGameStart;
        GameManager.onRoundReadyEvent -= OnRoundReady;
        GameManager.onCorrectedEvent -= OnCorrect;
        GameManager.onRoundFinishedEvent -= OnRoundFinished;
        GameManager.onRoundStartedEvent -= OnRoundStarted;
    }
    
    // 1. 상태 기준 분류 --------------------------------------------
    private void OnOnAllAnimalsInitialized()
    {
        GameManager.isAnimalTransformSet = true;
    }

    private void OnGameStart()
    {
        // ▼ 1회 실행. 
        _animator = GetComponent<Animator>();
        SubscribeGameManagerEvents();
    }

    private void OnRoundReady()
    {
        isTouchedDown = false;
    }
   
    private void OnRoundStarted()
    {
        // ▼ 이전 코루틴 중지.
        StopCoroutineWithNullCheck(_coroutines);
        
        // ▼ 1회 실행. 
        InitialzeAllAnimatorParams(_animator);
        InitializeSize();
        StandAnimalUpright();
        
        // ▼ 코루틴.
        _coroutines[0] = StartCoroutine(MoveAndRotateCoroutine());
        _coroutines[1] = StartCoroutine(SetRandomAnimationWhenWhenRoundStartCoroutine());
    }

    private void OnCorrect()
    {
        // ▼ 이전 코루틴 중지.
        
        
        // ▼ 1회 실행. 
        if (CheckIsAnswer())
        {
            SetCorrectedAnim(_animator);
        }
        
        // ▼ 코루틴.
        _coroutines[0] = StartCoroutine(IncreaseScaleCoroutine());
        _coroutines[1] = StartCoroutine(MoveToSpotLightCoroutine());
        
    }
    
    private void OnRoundFinished()
    {
        // ▼ 1회 실행. 
        InitialzeAllAnimatorParams(_animator);
        
        // ▼ 코루틴.
        _coroutines[0] = StartCoroutine(DecreaseScale());
        
       // if (animalData.inPlayPosition != null) animalData.inPlayPosition = null;
    }


    private void OnGameFinished()
    {
        
    }
    
    
    // 2. IEnumerator 및 기타 함수 ------------------------------------------------------------------------

    /// <summary>
    /// 코루틴 종료 함수 입니다.
    /// </summary>
    /// <param name="coroutines"></param>
    private void StopCoroutineWithNullCheck(Coroutine[] coroutines)
    {
        Debug.Log("코루틴 종료");
        foreach (Coroutine cR in coroutines)
        {
            if (cR  != null)
            {
                StopCoroutine(cR);
            }
        }
    }
    
    
    /// <summary>
    /// 코루틴을 배열에 저장합니다.
    /// </summary>
    private void SetCoroutine()
    {
        _coroutines = new Coroutine[4];
        _coroutines[0] = _coroutineA;
        _coroutines[1] = _coroutineB;
        _coroutines[2] = _coroutineC;
        _coroutines[3] = _coroutineD;

    }
  
    
    /// <summary>
    /// 지금 현재 동물이 정답과 일치하는지 체크 및 bool값을 반환합니다.
    /// </summary>
    private bool CheckIsAnswer()
    {
        if (_animalData.englishName == GameManager.answer)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
   

    /// <summary>
    /// 씬에 배치된 동물객체의 위치를 저장합니다. (개발 편의를 위해 분리 설계 하였습니다.)
    /// </summary>
    private void InitializeTransform()
    {
        _animalData.initialPosition = transform.position;
        _animalData.initialRotation = transform.rotation;
        
        if (GameManager.isAnimalTransformSet == false)
        { 
            GameManager.AnimalInitialized();
            Destroy(gameObject);
        }
    }
    
    private void InitializeSize() => gameObject.transform.localScale = Vector3.one * _animalData.defaultSize;
    
    IEnumerator MoveToTouchDownPlace()
    {
        while (isTouchedDown == false)
        {
            yield return null;
        }
    }
    
 
    
   
    // ReSharper disable Unity.PerformanceAnalysis
    IEnumerator IncreaseScaleCoroutine()
    {
        //초기화.
        _currentSizeLerp = 0f;
        
        while (CheckIsAnswer() && !GameManager.isRoundFinished)
        {
            IncreaseScale(gameObject, _animalData.defaultSize, _animalData.increasedSize);
            
            if (GameManager.isRoundFinished)
            {
                Debug.Log("Increase 코루틴 종료");
                StopCoroutineWithNullCheck(_coroutines);
            }
            
            yield return null;
        }
        
        
    }
    private void IncreaseScale(GameObject gameObject ,float defaultSize, float increasedSize)
    {
        _currentSizeLerp += _shaderAndCommon.sizeIncreasingSpeed * Time.deltaTime;
        _currentSizeLerp = Mathf.Clamp(_currentSizeLerp,0,1);

        _increaseSizeLerp =
            Lerp2D.EaseInBounce(
                defaultSize, increasedSize,
                _currentSizeLerp);


        gameObject.transform.localScale = Vector3.one * _increaseSizeLerp;
        
      
    }

    private bool _isDecreasingScale;
    IEnumerator DecreaseScale()
    {
        //초기화
        _currentSizeLerp = 0f;
        _isDecreasingScale = false;
        
        while (CheckIsAnswer() && GameManager.isRoundFinished)
        {
            _isDecreasingScale = true;
            DecreaseScale(gameObject, _animalData.defaultSize, _animalData.increasedSize);
            
            if (!GameManager.isRoundFinished)
            {
                Debug.Log("Decrease 코루틴 종료");
                StopCoroutineWithNullCheck(_coroutines);
            }
            
            yield return null;
        }
    }
    
    private void DecreaseScale(GameObject gameObject, float defaultSize, float increasedSize)
    {
        _currentSizeLerp += _shaderAndCommon.sizeDecreasingSpeed * Time.deltaTime;
        _currentSizeLerp = Mathf.Clamp(_currentSizeLerp,0,1);
        
        _increaseSizeLerp =
            Lerp2D.EaseOutBounce(increasedSize,defaultSize,_currentSizeLerp);
        
        gameObject.transform.localScale = Vector3.one * _increaseSizeLerp;
        
    }

    private bool _isAnimRandomized;
    private void SetCorrectedAnim(Animator selectedAnimator)
    {
        _isAnimRandomized = false;
            
        selectedAnimator.SetBool(AnimalData.SELECTABLE_A,false);
        selectedAnimator.SetBool(AnimalData.SELECTABLE_B,false);
        selectedAnimator.SetBool(AnimalData.SELECTABLE_C,false);
            
        if (_isAnimRandomized == false)
        {
            var randomAnimNum = Random.Range(0, 3);
            _isAnimRandomized = true;

            switch (randomAnimNum)
            {
                case 0:
                    selectedAnimator.SetBool(AnimalData.ROLL_ANIM, true);
                    break;
                case 1:
                    selectedAnimator.SetBool(AnimalData.FLY_ANIM, true);
                    break;
                case 2:
                    selectedAnimator.SetBool(AnimalData.SPIN_ANIM, true);
                    break;
            }
        }
    }

    IEnumerator OnRoundFinishedCoroutine()
    {
        yield return null;
    }
    private void InitialzeAllAnimatorParams(Animator animator)
    {
            animator.SetBool(AnimalData.RUN_ANIM, false);
            animator.SetBool(AnimalData.FLY_ANIM, false);
            animator.SetBool(AnimalData.ROLL_ANIM, false);
            animator.SetBool(AnimalData.SPIN_ANIM, false);
            animator.SetBool(AnimalData.SELECTABLE_A,false);
            animator.SetBool(AnimalData.SELECTABLE_B,false);
            animator.SetBool(AnimalData.SELECTABLE_C,false);
    }

   
    IEnumerator MoveAndRotateCoroutine()
    {
        _moveInElapsed = 0f;
        
        while (GameManager.isRoundStarted)
        {
            MoveAndRotate(_animalData.moveInTime,_animalData.rotationSpeedInRound);
            
            
            if (GameManager.isCorrected)
            {
                StopCoroutineWithNullCheck(_coroutines);
                break;
            }
            
            yield return null;
        }
       
    }

    private void MoveAndRotate(float moveInTime, float rotationSpeedInRound)
    {
       
        Vector3 position = _animalData.inPlayPosition.position;
        GameObject o;
        _moveInElapsed += Time.deltaTime;
            
        gameObject.transform.position = new Vector3(
            Mathf.Lerp(gameObject.transform.position.x, position.x, _moveInElapsed / moveInTime),
            (o = gameObject).transform.position.y,
            Mathf.Lerp(o.transform.position.z, position.z, _moveInElapsed / moveInTime)
        );
        
        gameObject.transform.Rotate(0, rotationSpeedInRound * Time.deltaTime, 0);
       
    }

    private float elapsedForAnimationWhenRoundStart;
    
    private void SetRandomAnimationWhenRoundStart(bool boolean)
    {
        int randomAnimNum = Random.Range(0, 3);
        _isAnimRandomized = true;

        switch (randomAnimNum)
        {
            case 0:
                _animator.SetBool(AnimalData.JUMP_ANIM, boolean);
                break;
            case 1:
                _animator.SetBool(AnimalData.SIT_ANIM, boolean);
                break;
            case 2:
                _animator.SetBool(AnimalData.BOUNCE_ANIM, boolean);
                break;
        }
    }

    private void InitializeAnimation(bool boolean)
    {
        _animator.SetBool(AnimalData.JUMP_ANIM, boolean);
        _animator.SetBool(AnimalData.SIT_ANIM, boolean);
        _animator.SetBool(AnimalData.BOUNCE_ANIM, boolean);
    }

    IEnumerator SetRandomAnimationWhenWhenRoundStartCoroutine()
    {
        while (!GameManager.isCorrected)
        {
            yield return GetWaitForSeconds(_animalData.animationPlayInterval);
            SetRandomAnimationWhenRoundStart(true);
            yield return GetWaitForSeconds(_animalData.animationDuration);
            InitializeAnimation(false);
            
            yield return null;
        }
    }
    
    private void StandAnimalUpright()
    {
        gameObject.transform.rotation = Quaternion.Euler(0,gameObject.transform.rotation.y,0);
    }

    private float _elapsedForMovingToSpotLight;
   
    IEnumerator MoveToSpotLightCoroutine()
    {
        
        _elapsedForMovingToSpotLight = 0f;
        while (!GameManager.isRoundFinished)
        {
            Debug.Log("Animal is Moving to Spotlight...");
            _elapsedForMovingToSpotLight += Time.deltaTime;
            MoveToSpotLight();
            yield return null;
        }
        
     
    }

    private void MoveToSpotLight()
    {
        float t = Mathf.Clamp01(_elapsedForMovingToSpotLight / _animalData.movingTimeSecWhenCorrect);
        

        if (gameObject.name == GameManager.answer)
        {
            gameObject.transform.position =
                Vector3.Lerp(gameObject.transform.position,
                    AnimalData.SPOTLIGHT_POSITION_FOR_ANIMAL.position, t);

            Vector3 directionToTarget =
                AnimalData.LOOK_AT_POSITION.position - gameObject.transform.position;

            Quaternion targetRotation =
                Quaternion.LookRotation(directionToTarget);
            gameObject.transform.rotation =
                Quaternion.Slerp(
                    gameObject.transform.rotation, targetRotation, _animalData.rotationSpeedWhenCorrect * Time.deltaTime);
        }
      
    }
    
    
}
