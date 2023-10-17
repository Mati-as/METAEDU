
using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Serialization;


public class FootstepController : MonoBehaviour
{
    [Header("button info")] [Space(10f)]
    public int footstepGroupOrder;

    private CapsuleCollider _collider;
   
    [Space(10f)]
    [SerializeField]
    private FootstepManager footstepManager;
    public float buttonChangeDuration;
    [Space(20f)]
    private GroundFootStepData _groundFootStepData;
    
    
  
   
    private Button _button;
    [SerializeField]
    
    [Header("Audio")] 
    private AudioSource _audioSource;
    
    private SpriteRenderer _spriteRenderer;
    [Space(20f)] [Header("Tween Parameters")]
    public float maximizedSize;
    private Transform _buttonRectTransform;
    private Vector2 _originalSizeDelta;
    
    [Space(20f)] [Header("Reference : 마지막 버튼에만 할당할 것 (non-nullable)")] 
    [SerializeField]
    public GameObject animalByLastFootstep;
    [SerializeField] public string animalNameToCall;

    
    

    private Vector3 _defaultSize;
    private void Awake()
    {
        _defaultSize = transform.localScale;
        
        FootstepManager.OnFootstepClicked -= OnButtonClicked;
        FootstepManager.OnFootstepClicked += OnButtonClicked;
        _collider = GetComponent<CapsuleCollider>();
    }
  

    private void Start()
    {
        _groundFootStepData = footstepManager.GetGroundFootStepData();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.DOFade(0, 0.05f);
        UpScale();
    }

    private LTDescr upscaleAnim;

    private void UpScale()
    {
       
        if (!_isClicked)
        {
            _spriteRenderer.DOFade(1, 2.0f);
            transform.localScale = _defaultSize;
            LeanTween.scale(gameObject,
                    _defaultSize * _groundFootStepData.scaleUpSize,
                    _groundFootStepData.sizeChangeDuration)
                .setEase(LeanTweenType.easeOutBounce)
                .setOnComplete(() =>
                {
                    DownScale();
                });
        }
  
    }
    

    private void DownScale()
    {
        
        if (!_isClicked)
        {
            transform.localScale = _defaultSize * _groundFootStepData.scaleUpSize;
           
            LeanTween.scale(gameObject,
                    _defaultSize * _groundFootStepData.defaultFootstepSize,
                    _groundFootStepData.sizeChangeDuration)
                .setEase(LeanTweenType.easeOutBounce)
                .setOnComplete(()=>
                {
                    UpScale();
                });
        }
 
    }

    private void OnDestroy()
    {
        FootstepManager.OnFootstepClicked -= OnButtonClicked;
    }

    private bool _isUIPlayed;
    private bool _isClicked =false;
    private void OnButtonClicked()
    {
            //중복클릭 방지용 콜라이더 비활성화.
            _collider.enabled = false;
            
            
            FadeOutSprite();
       
            if (animalByLastFootstep != null && animalNameToCall != string.Empty)
            {
                if (FootstepManager.currentlyClickedObjectName == animalByLastFootstep.name)
                {
                   Debug.Log("동물 소환");
                    animalByLastFootstep.SetActive(true);
                    //tween 추가하세요
                }
            }
        
            if (_audioSource != null)
            {
                _audioSource.Play();
            }
            if (!_isUIPlayed)
            {
                _isUIPlayed = true;
            }
        
            Debug.Log("The footstep's been Clicked");
        
     
    }

   
    private Tweener _fadeInTweener; 
    

    private void OnEnable()
    {
      
        _fadeInTweener = _spriteRenderer.DOFade(1, 0.85f);
        _collider.enabled = true;
    }


    private void FadeOutSprite()
    {
        _fadeInTweener.Kill();
        _spriteRenderer.DOFade(0, 0.85f).OnComplete(() =>
        {
            _collider.enabled = true;
            UpScale();
            this.gameObject.SetActive(false);
            
            // Destroy(this.gameObject);
        });
    }
    
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        var targetSize = _originalSizeDelta * maximizedSize;
        //_buttonRectTransform.DOSizeDelta(targetSize, buttonChangeDuration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
     
      //  _buttonRectTransform.DOSizeDelta(_originalSizeDelta,buttonChangeDuration);
    }
}
