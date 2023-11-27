using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Image_Move : MonoBehaviour
{
    public GameManager _gameManager;
   
    public float moveSpeed;
    //public Image imageA;
    private GameObject UI_Canvas;
    private Camera _mainCamera;
    
    private GraphicRaycaster GR;
    private PointerEventData PED;
    private Vector3 screenPosition;
    private InputAction _spaceAction;
    
    private float movement;
    private Vector3 moveDirection;

    // 현재는 SpaceBar click 시 입니다. 11/27/23
    public static event Action OnStep;

    private void Awake()
    {
        _mainCamera = GameObject.FindWithTag("UICamera").GetComponent<Camera>();
        
        _gameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        
        //newInputSystem 에서 SpaceBar를 InputAction으로 사용하는 바인딩 로직
        _spaceAction = new InputAction("Space", binding: "<Keyboard>/space", interactions: "press");
        _spaceAction.performed += OnSpaceBarPressed;
    }

    private void Start()
    {
        UI_Canvas = Manager_Sensor.instance.Get_UIcanvas();
        GR = UI_Canvas.GetComponent<GraphicRaycaster>();
        PED = new PointerEventData(null);
    }

    
    /// <summary>
    /// OnEnable,Disable에서 InputSystem관련 Action을 사용여부를 끄거나 켜줘야합니다.(구독,해제)
    /// </summary>
    private void OnEnable()
    {
        _spaceAction.Enable();
    }

    private void OnDisable()
    {
        _spaceAction.Disable();
    }

    private void Update()
    {
        Move();
    }
    
    private void OnSpaceBarPressed(InputAction.CallbackContext context)
    {
        //UI클릭을 위한 RayCast를 발생 및 Ray저장 
        ShootRay();
        
        //GameManager의 RayCast를 발생 
        OnStep?.Invoke();
        
    }
    
    /// <summary>
    /// 1. GameManager에서 로직처리를 위한 ray 정보를 업데이트
    /// 2. UI에 rayCast하고 Button 컴포넌트의 onClick이벤트 실행
    /// </summary>
    private void ShootRay()
    {
        screenPosition = _mainCamera.WorldToScreenPoint(transform.position);
        
        //GameManager에서 Cast할 _Ray를 업데이트.. (플레이 상 클릭)
        _gameManager._ray = Camera.main.ScreenPointToRay(screenPosition);
        
        // GameManger에서 Ray 발생시키므로, 아래 로직 미사용 (11/27/23)
        // var ray = Camera.main.ScreenPointToRay(screenPosition);
        // RaycastHit hit;
        // if (Physics.Raycast(ray, out hit)) Debug.Log(hit.transform.name);


        PED.position = screenPosition;
        var results = new List<RaycastResult>();
        GR.Raycast(PED, results);

        if (results.Count > 0)
            for (var i = 0; i < results.Count; i++)
            {
#if UNITY_EDITOR
                Debug.Log(results[i].gameObject.name);
#endif
                results[i].gameObject.TryGetComponent(out Button button);
                button?.onClick?.Invoke();
            }
    }
    
    /// <summary>
    /// - 가을소풍에서만 사용 -
    /// 가을소풍에서 storyUI 진행 시 TimeScale이 0이 되는데,이 때 UI를 진행시키기 위한 메소드입니다.
    /// 다른게임에서는 해당 메소드를 사용하지 않을 예정입니다 11/27/23
    /// </summary>

    private void Move()
    {
        var horizontalInput = Input.GetAxis("Horizontal");
        var verticalInput = Input.GetAxis("Vertical");
        moveDirection = new Vector3(horizontalInput, verticalInput, 0f).normalized;
        movement = moveSpeed * Time.deltaTime;
        transform.Translate(moveDirection * movement);
    }

}