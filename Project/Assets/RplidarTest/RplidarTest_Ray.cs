using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

public class RplidarTest_Ray : MonoBehaviour
{

   
// #if UNITY_EDITOR
//     public bool USE_SENSOR;
//     public static bool useSensor;
//     private static string port = useSensor ? "COM3" : string.Empty;
// #else
//   
// #endif

    private static string port ="COM3";      
    
    private LidarData[] _lidarDatas;
    private RectTransform Img_Rect_transform;

    //=====0714
    public GameObject BALLPrefab; 
    public GameObject MOUSEPrefab;
    public GameObject FPPrefab; 
    public GameObject middlePrefab;
    public bool m_onscan = false;
    private Thread m_thread;
    private bool m_datachanged = false;
    //=====
    private Vector3 Temp_position;

    //=====


    //====1012
    public bool Test_check = false;
    double number = 0f;

    public GameObject Guideline;
    public GameObject TESTUI;
    //

    //1015

    private float Resolution_Y = 1080;
    private float Resolution_X = 1920;
    private float _height = 182;
    
    private readonly float HEIGHT_MAX = 200;

    public float min_x;
    public float min_y;
    public float max_x;
    public float max_y;


    private Camera cameraToLookAt;

    public float Test_degree;
    public float Test_distance;

    [SerializeField]
    public GameObject temp_pos;

    public readonly float SENSOR_ROTATION = 0;

    //1121

    private GameObject UI_Canvas;
    private Camera UI_Camera;


    private float x;
    private float y;
    private float pre_x;
    private float pre_y;

    private bool UI_Active = false;
    private bool BALL_Active = true;
    private bool SF_Active = true;
    private readonly int LIDAR_DATA_SIZE = 720;
    
    //슬라이더를 통한 감도조절기능 추가(민석) 불필요시삭제 2/28/24
    private Slider _sensitivitySlider;
    private TextMeshProUGUI _sensitivityText;

    
    // 센서 측정 런타임 수정을 위한 맴버 선언
    //07.18/24 기준 화면 사이즈 320x180a(cm)
    
    private Dictionary<int, Vector2> _projectorLookUpTable;
    private Button _sensorEditModeButton;
    private TextMeshProUGUI _TMP_sensorEditMode;
    private Image _sensorEditModCheckImage;

    private bool _isSensorEditMode = true;

    private float _offsetX = 0;

    
    
    private float SENSOR_DISTANCE_FROM_PROJECTION = -1180;
    private readonly float SENSEOR_OFFSET_MAX_VALUE =1000;
    
    
    private readonly float SCREEN_RATIO_MIN =0.5f;
    private readonly float SCREEN_RATIO_MAX =10;
    private float _screenRatio = 0.6f;
    
    public float offsetX { get =>_offsetX; set => _offsetX =value; }
    public float sensorDistanceFromProjection { get => SENSOR_DISTANCE_FROM_PROJECTION; set =>SENSOR_DISTANCE_FROM_PROJECTION = value; }
    public float screenRatio { get => _screenRatio;set => _screenRatio = value;}
    
    private Slider _offsetXSlider;
    private Slider _offsetYSlider;
    private Slider _screenRatioSlider;
    
    private TextMeshProUGUI _TMP_HEIGHT;
    private TextMeshProUGUI _TMP_offsetY;
    private TextMeshProUGUI _TMP_ScreenRatio;
    
    ////////////////// 0719- 센서 테스트용 멤버 새로 추가한 부분///////////////////////////////
    
    private float correction_value;// 화면과 유니티에서의 단위를 맞추기 위한 보정값.
    //private float offset = 1.07f; // 발에 정확히 찍히기 위한 오프셋
    private float height = 0.6f;// 빔프로젝터의 지면에서의 높이
    private float distance = 0.23f; // 기기와 화면에서의 직선거리
    private float X_length;//화면의 가로 길이
    private float Y_length;//화면의 세로 길이
    private const float THROW_RATIO = 0.21f; // 빔프로젝터의 거리와 화면 크기 비(빔프로젝터 카탈로그 상 수치)
    private float y_offset; // (0,0)이 화면 중앙이므로 화면 세로 길이 / 2 + 화면에서 센서까지의 거리 추가
    private float screen_ratio;// 화면비
    
    /// /////////////////

    
    
    private void Awake()
    {
        if (SceneManager.GetActiveScene().name.Contains("METAEDU")) return;
// #if  UNITY_EDITOR
//         useSensor = USE_SENSOR;
//         if(useSensor) 
// #endif
            _lidarDatas = new LidarData[LIDAR_DATA_SIZE];
        
        Init();
        
        _height = _height * Resolution_X / Resolution_Y;

    }

    
    /// <summary>
    /// C#기준으로 out을 사용하여 초기화 불필요, 반환형식으로 사용
    /// </summary>
    /// <param name="sliderName"></param>
    /// <param name="slider"></param>
    /// <param name="text"></param>
    private void InitializeSlider(string sliderName, out Slider slider, out TextMeshProUGUI text)
    {
        slider = GameObject.Find(sliderName).GetComponent<Slider>();
        text = slider.transform.GetComponentInChildren<TextMeshProUGUI>();
    }

    private Stack<RectTransform> _sensorDetectedPositionPool;
    private void Init()
    {
        
        InitializeSlider("SensitivitySlider", out _sensitivitySlider, out _sensitivityText);
        InitializeSlider("OffsetXSlider", out _offsetXSlider, out _TMP_HEIGHT);
        InitializeSlider("OffsetYSlider", out _offsetYSlider, out _TMP_offsetY);
        InitializeSlider("ScreenRatioSlider", out _screenRatioSlider, out _TMP_ScreenRatio);
        
        _offsetXSlider.value = offsetX;
        _offsetYSlider.value = sensorDistanceFromProjection;
        _screenRatioSlider.value = screenRatio;
        
        ConfigureSlider(_offsetXSlider, HEIGHT_MAX, value =>
        {
            _height = value;
            _TMP_HEIGHT.text = "HEIGHT : " + _height.ToString("F2");
          
        },minVal:150);
        
        _TMP_HEIGHT.text =  "HEIGHT X: " + _height.ToString("F2");
        _TMP_offsetY.text = "OFFSET Y: " + sensorDistanceFromProjection.ToString("F2");
        _TMP_ScreenRatio.text = "SCREEN RATIO: " + screenRatio.ToString("F2");
    
        ConfigureSlider(_offsetYSlider, SENSEOR_OFFSET_MAX_VALUE, value =>
        {
            sensorDistanceFromProjection = value;
            _TMP_offsetY.text = "OFFSET Y: " + sensorDistanceFromProjection.ToString("F2");
      
        },minVal:-2000);
    
        ConfigureSlider(_screenRatioSlider, SCREEN_RATIO_MAX, value =>
        {
            screenRatio = value;
            _screenRatioSlider.minValue = SCREEN_RATIO_MIN;
            _TMP_ScreenRatio.text = "SCREEN RATIO: " + screenRatio.ToString("F2");
         
        });
  
        _sensorEditModeButton = GameObject.Find("SensorEditModeCheckBox").GetComponentInChildren<Button>();
        _TMP_sensorEditMode = GameObject.Find("SensorEditModeCheckBox").GetComponentInChildren<TextMeshProUGUI>();
        _sensorEditModCheckImage = GameObject.Find("EditModeCheckImage").GetComponent<Image>();
        
        _sensorEditModCheckImage.enabled = _isSensorEditMode;
        _TMP_sensorEditMode.text = _isSensorEditMode ? "Sensor Edit Mode: ON" : "Sensor Edit Mode: OFF";
        
        _sensorEditModeButton.onClick.AddListener(OnEditSensorModeBtnClicked);
        
    }

    private WaitForSeconds _poolReturnWait;
    protected IEnumerator ReturnToPoolAfterDelay(RectTransform obj, Stack<RectTransform> pool) 
    {
        if (_poolReturnWait == null) _poolReturnWait = new WaitForSeconds(FP_Prefab.Limit_Time);

        yield return _poolReturnWait;
        obj.gameObject.SetActive(false);
#if UNITY_EDITOR

#endif
        
        pool.Push(obj); // Return the particle system to the pool
    }
    
    private void SetPool<T>(Stack<T> pool, string path, int poolCount = 500)  where T : UnityEngine.Object
    {
        for (var poolSize = 0; poolSize < poolCount; poolSize++)
        {
            var prefab = Resources.Load<GameObject>(path);

            if (prefab == null)
            {
#if UNITY_EDITOR
                Debug.LogError("this gameObj to pool is null.");
#endif
                return;
            }

            var obj = Instantiate(prefab, UI_Canvas.transform.position, Quaternion.Euler(0, 0, 0), UI_Canvas.transform);

            var component = obj.GetComponent<T>();
            if (component == null)
            {
#if UNITY_EDITOR
                Debug.LogError("the loaded prefab does not have the required component.");
#endif
                return;
            }

            obj.SetActive(false);
            pool.Push(component);
        }
    }
    

    private RectTransform GetFromPool(Stack<RectTransform> pool)
    {
        if (pool.Count <= 0) return null;

        var obj = pool.Pop();
        return obj;
    }

    private void ShowSensorPos(float rectX, float rectY)
    {
        var detectedPosRect = GetFromPool(_sensorDetectedPositionPool);
        
        if (detectedPosRect == null)
        {
#if UNITY_EDITOR
            Debug.LogError("No RectTransform available in the pool.");
#endif
            return;
        }
#if UNITY_EDITOR
//        Debug.Log("Get RectRansfrom Pool.");
#endif

        detectedPosRect.anchoredPosition = new Vector2(rectX, rectY);
        detectedPosRect.gameObject.SetActive(true);
        StartCoroutine(ReturnToPoolAfterDelay(detectedPosRect, _sensorDetectedPositionPool));
    }

    private void ConfigureSlider(Slider slider, float maxValue, UnityEngine.Events.UnityAction<float> onValueChanged,
        float minVal = 0)
    {
        slider.minValue = minVal;
        slider.maxValue = maxValue;
        slider.onValueChanged.AddListener(onValueChanged);
      
    }
    private void OnEditSensorModeBtnClicked()
    {
        _isSensorEditMode = !_isSensorEditMode;
        _sensorEditModCheckImage.enabled = _isSensorEditMode;
        _TMP_sensorEditMode.text = _isSensorEditMode ? "Sensor Edit Mode: ON" : "Sensor Edit Mode: OFF";
    }

    void Start()
    {

        int result = RplidarBinding.OnConnect(port);
        Debug.Log("Connect on " + port + " result:" + result);

        bool r = RplidarBinding.StartMotor();
        Debug.Log("StartMotor:" + r);

        m_onscan = RplidarBinding.StartScan();
        Debug.Log("StartScan:" + m_onscan);

        if (m_onscan)
        {
            m_thread = new Thread(GenerateMesh);
            m_thread.Start();
        }

        Img_Rect_transform = this.GetComponent<RectTransform>();

        UI_Canvas = Manager_Sensor.instance.Get_UIcanvas();
        UI_Camera = Manager_Sensor.instance.Get_UIcamera();
        
        //guide라인이랑 동기화 기능
        min_x = Guideline.GetComponent<RectTransform>().anchoredPosition.x - (Guideline.GetComponent<RectTransform>().rect.width) / 2;
        min_y = Guideline.GetComponent<RectTransform>().anchoredPosition.y - (Guideline.GetComponent<RectTransform>().rect.height) / 2;
        max_x = Guideline.GetComponent<RectTransform>().anchoredPosition.x + (Guideline.GetComponent<RectTransform>().rect.width) / 2;
        max_y = Guideline.GetComponent<RectTransform>().anchoredPosition.y + (Guideline.GetComponent<RectTransform>().rect.height) / 2;

        TESTUI.SetActive(false);

        _sensitivitySlider.onValueChanged.AddListener(_ =>
        {
            FP_Prefab.Limit_Time = _sensitivitySlider.value * 2f;
            _sensitivityText.text = $"sensitivity : {FP_Prefab.Limit_Time:F2}";
        });
      
        
        
        _projectorLookUpTable = new Dictionary<int, Vector2>();
        
        //IGameManager init이후에 동작해야합니다. 따라서 Awake가 아닌 Start에서만 사용해야합니다. 4/4/24
        _sensitivitySlider.value = IGameManager.DEFAULT_SENSITIVITY / 2;
        
        
        
        
        
        ///////////////////////////////////////////////////////////////////////// (1)
        screen_ratio = Resolution_X / Resolution_Y;
        y_offset = ((X_length / screen_ratio) / 2) + distance;
        X_length = height / THROW_RATIO;
        Y_length = X_length / (1920 / 1080);
        correction_value = -(Resolution_X / (_screenRatio)) * (height / THROW_RATIO / Resolution_Y);
        
        ///////////////////////// Pool
        _sensorDetectedPositionPool = new Stack<RectTransform>();
        SetPool(_sensorDetectedPositionPool, "Rplidar/FP");
    }


    void GenerateMesh()
    {
        while (true)
        {
            int datacount = RplidarBinding.GetData(ref _lidarDatas);

            if (datacount == 0)
            {
                Thread.Sleep(5);
            }
            else
            {
                m_datachanged = true;
            }
        }
    }

    
    
    int GenerateKey(int angle, int distance)
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 100 + angle.GetHashCode();
            hash = hash * 100 + distance.GetHashCode();
            return hash;
        }
    }


    private float _timer;
    
    // Update is called once per frame
    void FixedUpdate()
    {

        if (_timer < _sensitivitySlider.value)
        {
            _timer += Time.deltaTime;
            return;
        }
        
        _timer = 0f;
        
    
        if (m_datachanged)
        {
            for (int i = 0; i < 720; i++)
            {
             
                
                //센서 데이터 data[i].theta, distant
                //1. 화면과 센서를 일치화 시키기 위해서 theta를 마이너스 곱해줌, 추가로 회전 시켜주기 위해 Sensor_rotation 추가했고
                //위에서 아래 방향으로 내려다 보는것 기준으 90도 입력하면 댐
                //2. 0.74f는 실제 길이와 유니티내 맵핑이 일치하기 위한 보정값(빔프로젝터의 실제 화면과 오차가 있음), 1.07f는 발 위
                //에 정확히 찍히기 위한 보정값
                // Ex) 실제에서 682 mm -> 유니티 position 상 500, 보정값 0.733 곱해서 맞춰줌 실제 데이터를 position으로 변환함
                //3. 763.565f은 유니티 상의 캔버스 기준이 정가운데이기 때문에 그에 맞추기 위해 y값을 그 만큼 위로 올림
                
                //계산되어있을때? ==> Mathf.Rad2Deg(-_lidarDatas[i].theta)
                var key = GenerateKey((int)_lidarDatas[i].theta * 10, (int)_lidarDatas[i].distant);

                //_lidarDatas[i].distant = Mathf.Clamp(_lidarDatas[i].distant, 0, 2550);
                
                
                ///////////////////////////////////////////////////////////////////////// (2)
                var processedTheta = -_lidarDatas[i].theta * Mathf.Deg2Rad; // 프로젝터값 등을 고려한 값
                var processedDistance = _lidarDatas[i].distant * 1.07f;
               // correction_value = - (Resolution_X / (_screenRatio)) * (height / THROW_RATIO / Resolution_Y);
                
               //6배
                if(_lidarDatas[i].theta >90 && _lidarDatas[i].theta <270)continue;


               x = -screenRatio * (_lidarDatas[i].distant * Mathf.Cos((90-_lidarDatas[i].theta)* Mathf.Deg2Rad));
               y = -screenRatio * (_lidarDatas[i].distant * Mathf.Sin((90-_lidarDatas[i].theta) * Mathf.Deg2Rad) +
                                  SENSOR_DISTANCE_FROM_PROJECTION);
               
               // 라이더 센서 메뉴얼상 각도 시스템에 맞추기 위해 -90도 회전하여 계산값을 도출하였습니다.  
               // x = -screenRatio * (_lidarDatas[i].distant * Mathf.Sin((_lidarDatas[i].theta)* Mathf.Deg2Rad));
               // y = screenRatio * (_lidarDatas[i].distant * Mathf.Cos((_lidarDatas[i].theta) * Mathf.Deg2Rad) +
               //                    SENSOR_DISTANCE_FROM_PROJECTION);
                
                // cal option 1
                // x = correction_value * (_lidarDatas[i].distant * Mathf.Cos(-_lidarDatas[i].theta * Mathf.Deg2Rad));
                // y = correction_value * (_lidarDatas[i].distant * Mathf.Sin(-_lidarDatas[i].theta * Mathf.Deg2Rad) -(Y_length / 2 + distance));
                
                // cal option 2
                // x = correction_value * (_lidarDatas[i].distant * Mathf.Cos(Mathf.PI - (_lidarDatas[i].theta * Mathf.Deg2Rad)));
                // y = correction_value *
                //     (_lidarDatas[i].distant * Mathf.Sin(Mathf.PI - (_lidarDatas[i].theta * Mathf.Deg2Rad)) -
                //      (Y_length / 2 + distance));
                
                // cal option 3
                // x = correction_value * (_lidarDatas[i].distant * Mathf.Cos(Mathf.PI + (_lidarDatas[i].theta * Mathf.Deg2Rad)));
                // y = correction_value *
                //     (_lidarDatas[i].distant * Mathf.Sin(Mathf.PI + (_lidarDatas[i].theta * Mathf.Deg2Rad)) -
                //      (Y_length / 2 + distance));
                    
                    
                    
                // Debug.Log($"좌표 계산결과 {x},{y}");
                // Debug.Log($"해당좌표 거리: {_lidarDatas[i].distant} , 각도: {_lidarDatas[i].theta}");
                
                // if (_projectorLookUpTable.ContainsKey(key))
                // {
                //     //Debug.LogWarning($"LUT REFFERRING....key {key}");
                //     x = _projectorLookUpTable[key].x; 
                //     y = _projectorLookUpTable[key].y;
                // }
                // else if(!_projectorLookUpTable.ContainsKey(key))
                // {
                //     //Debug.LogWarning($"LUT CALCULATING & SAVING....key: {key}");
                //
                //   
                //     
                //     //x = 프로젝터 높이 * 계산수식(크기~) + offsetX
                //     
                //     
                //     var coordinate = new Vector2(x, y);
                //     _projectorLookUpTable.TryAdd(key, coordinate);
                // }

                // if (_isSensorEditMode)
                // {
                //     InstantiateMiddlePointPrefab(x, y);
                // }
                
                if (i % 30 == 0)
                {
                    if (min_x < x && x < max_x)
                    {
                        if (min_y < y && y < max_y)
                        {
                            ShowSensorPos(x, y);
                            //Instant_FP(x, y);
                        }
                    }

                }
            }
            m_datachanged = false;
        }

    }


    public void Instant_Ball(float temp_x, float temp_y)
    {
        GameObject Prefab_pos = Instantiate(BALLPrefab, UI_Canvas.transform.position, Quaternion.Euler(0, 0, 0), UI_Canvas.transform);
        Prefab_pos.GetComponent<RectTransform>().anchoredPosition = new Vector3(temp_x, temp_y, 0);
        Prefab_pos.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 0);
    }
    public void Instant_Mouse(float temp_x, float temp_y)
    {
        GameObject Prefab_pos = Instantiate(MOUSEPrefab, UI_Canvas.transform.position, Quaternion.Euler(0, 0, 0), UI_Canvas.transform);
        Prefab_pos.GetComponent<RectTransform>().anchoredPosition = new Vector3(temp_x, temp_y, 0);
        Prefab_pos.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 0);
    }
    public void Instant_FP(float temp_x, float temp_y)
    {
        GameObject Prefab_pos = Instantiate(FPPrefab, UI_Canvas.transform.position, Quaternion.Euler(0, 0, 0), UI_Canvas.transform);
        Prefab_pos.GetComponent<RectTransform>().anchoredPosition = new Vector3(temp_x, temp_y, 0);
        Prefab_pos.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 0);
    }




    public void InstantiateMiddlePointPrefab(float temp_x, float temp_y)
    {
        
        GameObject Prefab_pos = Instantiate(middlePrefab, UI_Canvas.transform.position, Quaternion.Euler(0, 0, 0), UI_Canvas.transform);
        Prefab_pos.GetComponent<RectTransform>().anchoredPosition = new Vector3(temp_x, temp_y, 0);
        Prefab_pos.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 0);
    }
    void OnDestroy()
    {
        RplidarBinding.EndScan();
        RplidarBinding.EndMotor();
        RplidarBinding.OnDisconnect();
        RplidarBinding.ReleaseDrive();

        //StopCoroutine(GenMesh());

        m_thread?.Abort();

        m_onscan = false;
    }

    public bool UI_Active_ONOFF()
    {
        UI_Active = !UI_Active;

        if (UI_Active)
        {
            TESTUI.SetActive(true);
        }
        else if (UI_Active == false)
        {
            TESTUI.SetActive(false);
        }
        return UI_Active;
    }
    public bool Ball_Active_ONOFF()
    {
        BALL_Active = !BALL_Active;

        return BALL_Active;
    }
    public bool SF_Active_ONOFF()
    {
        SF_Active = !SF_Active;

        return SF_Active;
    }
}