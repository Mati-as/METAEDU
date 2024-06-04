using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Xml;
using DG.Tweening;
using MyCustomizedEditor.Common.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class U_FishOnWater_UIManager : UI_PopUp
{
    private enum UI_Type
    {
        Ready,
        Start,
        Stop,
        Timer,
        Count,
        RankingParent,
        Tutorial,
        Btn_InitialStart, // 버튼 자체 클래스로 컨트롤하며, 현재는 미사용 6/4
        Btn_Restart,
        Btn_Next, // Tutorial -> UserInfo
        CurrentUserInfoUI,
        CurrentUser,
        OnRankingUsers,
        UIHidePosition,
        ScreenDim,
        Btn_StartOnUserInfo
        
    }

    private enum RankingUI
    {
        UserIcon, // 유저 아이콘 스프라이트 
        UserNames, //  랭킹 유저 이름
        UserScores, // 랭킹 유저 점수
        CurrentUserName, // 현재 유저이름
        CurrentUserScore // 현재 유저 점수
    }

    private enum RankUserInfo
    {
        Name,
        Score,
        ScoreSub,
        Max
    }

    private CanvasGroup _canvasGroup;

    private GameObject[] _uiGameObjects;
    private RectTransform[] _uiRectTransforms;

    private TextMeshProUGUI _fishCountTMP;
    private TextMeshProUGUI _timerTMP;

    private TextMeshProUGUI[] _TMP_usersOnRankScores;
    private TextMeshProUGUI[] _TMP_usersOnRankNames;
    private TextMeshProUGUI[] _TMP_currentUser;

    private RectTransform[] _usersOnRankingRects;

    private Button _restartButton;
    private Button _nextButton;
    private Button _initialStartButton;
    private Button _startButtonOnUserInfo;

    private const int USER_ON_RANKING_COUNT = 8;
    private string[][] _TMP_usersOnRankingData;
    private float _intervalBtwStartAndReady = 1f;
    private bool _isAnimating; // 더블클릭 방지 논리연산자 입니다. 
    public static event Action OnStartUIAppear; // 시작 UI가 표출될 때 의 이벤트입니다. 
    public static event Action OnReadyUIAppear; // 준비~! UI 표출 시
    public static event Action OnRestartBtnClicked; // 그만 이후 초기화가 끝난경우

    private U_FishOnWater_GameManager _gm;
    private bool _isOnFirstRound = true; //초기 버튼 관련 로직
    public Slider _fishSpeedSlider { get; private set; }
    private TextMeshProUGUI _TMP_fishSpeed;
    private TextMeshProUGUI _TMP_introUserName;
    private Vector3[] _defaultanchorPosArray;
    private Vector3[] _defaultSizeArray;

    public override bool Init()
    {
        _gm = GameObject.FindWithTag("GameManager").GetComponent<U_FishOnWater_GameManager>();

        var sliderParent = GameObject.Find("Slider_FishSpeed");
        _fishSpeedSlider = sliderParent.GetComponent<Slider>();
        _fishSpeedSlider.onValueChanged.AddListener(_ =>
        {
            _gm.fishSpeed = _fishSpeedSlider.value;
            _TMP_fishSpeed.text = "물고기 속도: " + _gm.fishSpeed;
        });

        _TMP_fishSpeed = sliderParent.GetComponentInChildren<TextMeshProUGUI>();

        BindObject(typeof(UI_Type));

        InitializeUIElements();
        InitializeRankingElements();

        U_FishOnWater_GameManager.OnReady -= OnReadyAndStart;
        U_FishOnWater_GameManager.OnReady += OnReadyAndStart;

        U_FishOnWater_GameManager.OnRoundFinished -= PopUpStopUI;
        U_FishOnWater_GameManager.OnRoundFinished += PopUpStopUI;

       
        
        UI_Scene_Button.onBtnShut -= OnStartButtonClicked;
        UI_Scene_Button.onBtnShut += OnStartButtonClicked;
        
        

        _uiGameObjects[(int)UI_Type.Tutorial].SetActive(true);

        DOVirtual.Float(0, 1, 0.5f,
            value => { _uiRectTransforms[(int)UI_Type.Tutorial].localScale = Vector3.one * value; }).OnComplete(() =>
        {
            _uiGameObjects[(int)UI_Type.Btn_Next].SetActive(true);
            _uiRectTransforms[(int)UI_Type.Btn_Next].localScale = Vector3.zero;
            DOVirtual.Float(0, 1, 0.5f,
                value => { _uiRectTransforms[(int)UI_Type.Btn_Next].localScale = Vector3.one * value; }).SetDelay(1f);
        });

        return true;
    }
    
    

    private void InitializeUIElements()
    {
        var uiElementCount = Enum.GetValues(typeof(UI_Type)).Length;
        _uiGameObjects = new GameObject[uiElementCount];
        _uiRectTransforms = new RectTransform[uiElementCount];
        _defaultanchorPosArray = new Vector3[uiElementCount];
        _defaultSizeArray = new Vector3[uiElementCount];
        
        for (var i = 0; i < uiElementCount; i++)
        {
            _uiGameObjects[i] = GetObject(i);
            _uiRectTransforms[i] = _uiGameObjects[i].GetComponent<RectTransform>();
            _defaultSizeArray[i] = _uiRectTransforms[i].localScale;
            _defaultanchorPosArray[i] = _uiRectTransforms[i].anchoredPosition;
            _uiGameObjects[i].SetActive(false);
        }
        _uiGameObjects[(int)UI_Type.ScreenDim].SetActive(true);
        _uiRectTransforms[(int)UI_Type.ScreenDim].localScale = Vector3.one;
        _uiGameObjects[(int)UI_Type.Timer].SetActive(true);
        _uiRectTransforms[(int)UI_Type.Timer].localScale = _defaultSizeArray[(int)UI_Type.Timer];
        _uiGameObjects[(int)UI_Type.Count].SetActive(true);
        _uiRectTransforms[(int)UI_Type.Count].localScale = _defaultSizeArray[(int)UI_Type.Count];

        
        
        _timerTMP = _uiGameObjects[(int)UI_Type.Timer].GetComponentInChildren<TextMeshProUGUI>();
        _fishCountTMP = _uiGameObjects[(int)UI_Type.Count].GetComponent<TextMeshProUGUI>();
       
        var userNameUIParentTransform = _uiGameObjects[(int)UI_Type.CurrentUserInfoUI].transform;
        _TMP_introUserName = userNameUIParentTransform.Find("Text_Name")
            .GetComponent<TextMeshProUGUI>();
     
        
        
        _restartButton = _uiGameObjects[(int)UI_Type.Btn_Restart].GetComponent<Button>();
        _restartButton.onClick.AddListener(OnRestartBtnClick);

        _initialStartButton = _uiGameObjects[(int)UI_Type.Btn_InitialStart].GetComponent<Button>();

        _startButtonOnUserInfo = _uiGameObjects[(int)UI_Type.Btn_StartOnUserInfo].GetComponent<Button>();
        _startButtonOnUserInfo.onClick.AddListener(OnrRestartBtnOnUserInfoClicked);
        
        _nextButton = _uiGameObjects[(int)UI_Type.Btn_Next].GetComponent<Button>();
        _nextButton.onClick.AddListener(ShowUserInfo);
    }

    private void ShowUserInfo()
    {
        if (!_isAnimating) StartCoroutine(ShowUserInfoCo());
    }

    private IEnumerator ShowUserInfoCo()
    {
        _isAnimating = true;
        yield return _waitInterval;

        if (_uiGameObjects[(int)UI_Type.Tutorial].activeSelf)
        {
            yield return _uiRectTransforms[(int)UI_Type.Tutorial]
                .DOAnchorPos(_uiRectTransforms[(int)UI_Type.UIHidePosition].anchoredPosition, 0.8f)
                .SetEase(Ease.InOutSine)
                .WaitForCompletion();
            _uiGameObjects[(int)UI_Type.Tutorial].SetActive(false);
        }
      

        _uiRectTransforms[(int)UI_Type.CurrentUserInfoUI].anchoredPosition =
            _defaultanchorPosArray[(int)UI_Type.CurrentUserInfoUI];
        _uiRectTransforms[(int)UI_Type.CurrentUserInfoUI].localScale = Vector3.zero;
        _uiGameObjects[(int)UI_Type.CurrentUserInfoUI].SetActive(true);

        DOVirtual.Float(0, 1, 0.5f,
            value => { _uiRectTransforms[(int)UI_Type.CurrentUserInfoUI].localScale = Vector3.one * value; });

        _TMP_introUserName.text = _gm.currentUserName;
        _uiRectTransforms[(int)UI_Type.CurrentUserInfoUI].anchoredPosition =
            _uiRectTransforms[(int)UI_Type.UIHidePosition].position;

        _uiRectTransforms[(int)UI_Type.CurrentUserInfoUI]
            .DOAnchorPos(_defaultanchorPosArray[(int)UI_Type.CurrentUserInfoUI], 0.8f).SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                if (_isOnFirstRound)
                {
                    _uiGameObjects[(int)UI_Type.Btn_InitialStart].SetActive(true);
                    
                    _uiRectTransforms[(int)UI_Type.Btn_InitialStart].localScale = Vector3.zero;
                    DOVirtual.Float(0, 1, 0.5f,
                            value => { _uiRectTransforms[(int)UI_Type.Btn_InitialStart].localScale = Vector3.one * value; })
                        .OnComplete(()=>
                        {
                            _isOnFirstRound = false;
                        }).SetDelay(1f);
                    
                }
                else
                {
                  
                    _uiGameObjects[(int)UI_Type.Btn_StartOnUserInfo].SetActive(true);
                    _uiRectTransforms[(int)UI_Type.Btn_StartOnUserInfo].localScale = Vector3.zero;
                    DOVirtual.Float(0, 1, 0.5f,
                            value => { _uiRectTransforms[(int)UI_Type.Btn_Restart].localScale = Vector3.one * value; })
                        .SetDelay(1f).OnComplete(() =>
                        {
                            DOVirtual.Float(0, 1, 0.45f, scale
                                =>
                            {
                                _uiRectTransforms[(int)UI_Type.Btn_StartOnUserInfo].localScale = Vector3.one * scale;
                            });
                        });
                }


                
                
            });
        

        _isAnimating = false;
    }


    private Image _screenDim;
    private float _defaultAlpha;
    private void OnStartButtonClicked()
    {
        _screenDim = _uiGameObjects[(int)UI_Type.ScreenDim].GetComponent<Image>();
        _defaultAlpha = _screenDim.material.color.a; 
        _screenDim.DOFade(0, 0.55f);
        _uiRectTransforms[(int)UI_Type.CurrentUserInfoUI]
            .DOAnchorPos(_uiRectTransforms[(int)UI_Type.UIHidePosition].anchoredPosition, 0.5f)
            .SetEase(Ease.InOutSine);
    }

    private void InitializeRankingElements()
    {
        _TMP_usersOnRankingData = new string[USER_ON_RANKING_COUNT][];
        for (var i = 0; i < _TMP_usersOnRankingData.Length; i++)
            _TMP_usersOnRankingData[i] = new string[(int)RankUserInfo.Max];

        _TMP_usersOnRankNames = new TextMeshProUGUI[USER_ON_RANKING_COUNT];
        _TMP_usersOnRankScores = new TextMeshProUGUI[USER_ON_RANKING_COUNT];

        var usersOnRankingObj = _uiGameObjects[(int)UI_Type.OnRankingUsers];
        _usersOnRankingRects = new RectTransform[USER_ON_RANKING_COUNT];
        for (var i = 0; i < USER_ON_RANKING_COUNT; i++)
        {
            var userTransform = usersOnRankingObj.transform.GetChild(i);
            _usersOnRankingRects[i] = userTransform.GetComponent<RectTransform>();

            _TMP_usersOnRankNames[i] = userTransform.Find("Text_UserName")?.GetComponent<TextMeshProUGUI>();
            _TMP_usersOnRankScores[i] = userTransform.Find("Text_Score_Value")?.GetComponent<TextMeshProUGUI>();
        }

        var currentUser = _uiGameObjects[(int)UI_Type.CurrentUser];
        var currentUserTransform = currentUser.transform;
        _TMP_currentUser = new TextMeshProUGUI[(int)RankUserInfo.Max];
        _TMP_currentUser[(int)RankUserInfo.Name] = currentUserTransform.GetChild(1).GetComponent<TextMeshProUGUI>();
        _TMP_currentUser[(int)RankUserInfo.Score] = currentUserTransform.GetChild(2).GetComponent<TextMeshProUGUI>();
        _TMP_currentUser[(int)RankUserInfo.ScoreSub] = currentUserTransform.GetChild(3).GetComponent<TextMeshProUGUI>();
    }

    private void OnDestroy()
    {
        UI_Scene_Button.onBtnShut -= OnStartButtonClicked;
        U_FishOnWater_GameManager.OnReady -= OnReadyAndStart;
        U_FishOnWater_GameManager.OnRoundFinished -= PopUpStopUI;
    }

    private void Update()
    {
        if (!_gm.isOnReInit)
        {
            _timerTMP.text = _gm.remainTime.ToString("00") + "초";
            _fishCountTMP.text =
                _gm.FishCaughtCount == _gm.FISH_COUNT ? "물고기를 모두 다 잡았어요!" : _gm.FishCaughtCount + " 마리";
        }
    }

    public void OnReadyAndStart()
    {
        PopUpStartUI();
    }

    private void ParseXML()
    {
        XmlNode root = _gm.xmlDoc.DocumentElement;
        var nodes = root.SelectNodes("StringData");

        var userScores = new List<(string username, string score)>();

        foreach (XmlNode node in nodes)
        {
            var username = node.Attributes["username"].Value;
            var score = node.Attributes["score"].Value;

            userScores.Add((username, score));
        }

        // 첫 번째 숫자(A)를 기준으로 내림차순 정렬, A 값이 같으면 두 번째 숫자(B)를 기준으로 정렬
        userScores = userScores.OrderByDescending(x =>
            {
                var splitScore = x.score.Split('/');
                return int.Parse(splitScore[0]); // A 값을 기준으로 정렬
            })
            .ThenByDescending(x =>
            {
                var splitScore = x.score.Split('/');
                return int.Parse(splitScore[1]); // B 값을 기준으로 정렬
            })
            .ToList();


        for (var i = 0; i < 8 && i < userScores.Count; i++)
        {
            _TMP_usersOnRankingData[i][(int)RankUserInfo.Name] = userScores[i].Item1;
            _TMP_usersOnRankingData[i][(int)RankUserInfo.Score] = userScores[i].Item2;


            _TMP_usersOnRankNames[i].text = _TMP_usersOnRankingData[i][(int)RankUserInfo.Name];
            _TMP_usersOnRankScores[i].text = _TMP_usersOnRankingData[i][(int)RankUserInfo.Score];

            // 현재 유저가 랭킹에 오르는 것에 성공한 경우 애니메이션 --------------------
            if (_TMP_usersOnRankNames[i].text == _gm.currentUserName &&
                _TMP_usersOnRankScores[i].text == _gm.currentUserScore)
            {
#if UNITY_EDITOR
                Debug.Log($"On Ranking! {i} ");
# endif
                var seq = DOTween.Sequence();
                seq.Append(_usersOnRankingRects[i].DOScale(1.05f, 0.15f).SetEase(Ease.InOutSine));
                seq.Append(_usersOnRankingRects[i].DOScale(0.95f, 0.15f).SetEase(Ease.InOutSine));
                seq.SetLoops(10);

                seq.Append(_usersOnRankingRects[i].DOScale(1f, 0.5f));
            }
        }
    }

    private void PopUpStartUI()
    {
        StartCoroutine(PopUpStartUICoroutine());
    }

    private void PopUpStopUI()
    {
        StartCoroutine(PopUpStopUICoroutine());
    }


    private WaitForSeconds _wait;
    private WaitForSeconds _waitInterval;
    private WaitForSeconds _waitForReady;
    private float _waitTIme = 4.5f;

    private readonly float ANIM_DURATION_START_AND_READY_STOP = 0.35f;

    private IEnumerator PopUpStartUICoroutine()
    {
        if (_waitInterval == null) _waitInterval = new WaitForSeconds(0.4f);
        yield return _waitForReady;
        _uiGameObjects[(int)UI_Type.Ready].gameObject.SetActive(true);
        yield return DOVirtual.Float(0, 1, 1,
            scale => { _uiRectTransforms[(int)UI_Type.Ready].localScale = Vector3.one * scale; }).OnStart(
            () =>
            {
                OnReadyUIAppear?.Invoke();
                _fishCountTMP.enabled = true;
                _timerTMP.enabled = true;
                _fishCountTMP.text = _gm.FishCaughtCount + " 마리";
                _timerTMP.text = _gm.remainTime.ToString("F1") + "초";
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/HandFlip2/Ready", 0.8f);
            }).WaitForCompletion();
        yield return _waitInterval;
        yield return DOVirtual.Float(1, 0, ANIM_DURATION_START_AND_READY_STOP,
                scale => { _uiRectTransforms[(int)UI_Type.Ready].localScale = Vector3.one * scale; })
            .WaitForCompletion();


        yield return _waitInterval;
        _uiRectTransforms[(int)UI_Type.Start].gameObject.SetActive(true);
        yield return DOVirtual.Float(0, 1, ANIM_DURATION_START_AND_READY_STOP,
            scale => { _uiRectTransforms[(int)UI_Type.Start].localScale = Vector3.one * scale; }).OnStart(
            () =>
            {
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/HandFlip2/Start", 0.8f);
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/HandFlip2/Whistle", 0.4f);
                OnStartUIAppear?.Invoke();
#if UNITY_EDITOR
                Debug.Log("UI Invoke");
#endif
            }).WaitForCompletion();
        yield return _waitInterval;
        yield return DOVirtual.Float(1, 0, 1,
                scale => { _uiRectTransforms[(int)UI_Type.Start].localScale = Vector3.one * scale; })
            .WaitForCompletion();
    }

    private IEnumerator PopUpStopUICoroutine()
    {
        if (_waitInterval == null) _waitInterval = new WaitForSeconds(0.3f);

        _fishCountTMP.text = _gm.FishCaughtCount >= 20 ? "물고기를 전부 잡았어요!" : "시간이 다 지났어요!";


        yield return _waitInterval;
        _uiRectTransforms[(int)UI_Type.Stop].gameObject.SetActive(true);
        yield return DOVirtual.Float(0, 1, 0.45f,
            scale => { _uiRectTransforms[(int)UI_Type.Stop].localScale = Vector3.one * scale; }).OnStart(
            () =>
            {
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/HandFlip2/Stop", 0.8f);
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/HandFlip2/Whistle", 0.4f);
            }).WaitForCompletion();
        yield return _waitInterval;
        yield return DOVirtual.Float(1, 0, 1,
                scale => { _uiRectTransforms[(int)UI_Type.Stop].localScale = Vector3.one * scale; })
            .WaitForCompletion();
        //OnUIFinished?.Invoke();

        _fishCountTMP.text = _gm.FishCaughtCount >= 20 ? "물고기를 전부 잡았어요!" : $"물고기를 {_gm.FishCaughtCount}마리 잡았어요";

        PopUpRankingSystem();
    }

    private void LoadRankingInfo()
    {
        _TMP_currentUser[(int)RankUserInfo.Name].text = _gm.currentUserName;

        _TMP_currentUser[(int)RankUserInfo.ScoreSub].text =
            $"{_gm.FishCaughtCount} 마리 / {_gm.remainTime:F2} 초 ";

        _TMP_currentUser[(int)RankUserInfo.Score].text = "= " + _gm.currentUserScore;
        ParseXML();
    }

    private IEnumerator PopUpRankSystemCo()
    {
        if (_waitInterval == null) _waitInterval = new WaitForSeconds(0.3f);

        _fishCountTMP.enabled = false;
        _timerTMP.enabled = false;

        LoadRankingInfo();
        yield return _waitInterval;
        
        _uiGameObjects[(int)UI_Type.RankingParent].SetActive(true);
        _uiGameObjects[(int)UI_Type.CurrentUser].SetActive(true);
        _uiGameObjects[(int)UI_Type.OnRankingUsers].SetActive(true);
        _uiGameObjects[(int)UI_Type.Btn_Restart].SetActive(true);
        yield return
            DOVirtual.Float(0, 1, 0.45f, scale
                =>
            {
                _uiRectTransforms[(int)UI_Type.RankingParent].localScale = Vector3.one * scale;
            });
    }

    private void PopUpRankingSystem()
    {
        StartCoroutine(PopUpRankSystemCo());
    }

    public void OnRestartBtnClick()
    {
        if (_isAnimating)
        {
#if UNITY_EDITOR
            Debug.Log("This UI has already activated");
#endif
            return;
        }
       
       StartCoroutine(OnRestartBtnClickCo());
       
       
     
    }

    private void OnrRestartBtnOnUserInfoClicked()
    {
        if (_isAnimating) return;
        _isAnimating = true;
        
        OnRestartBtnClicked?.Invoke();
        _screenDim.DOFade(0, 0.55f);
        _uiRectTransforms[(int)UI_Type.CurrentUserInfoUI]
            .DOAnchorPos(_uiRectTransforms[(int)UI_Type.UIHidePosition].anchoredPosition, 0.5f)
            .SetEase(Ease.InOutSine);
        DOVirtual.Float(1, 0, 0.35f, scale
            =>
        {
            _uiRectTransforms[(int)UI_Type.Btn_StartOnUserInfo].localScale = Vector3.one * scale;
        }).OnComplete(() =>
        {
            _isAnimating = false;
        });
    }
    


    private IEnumerator OnRestartBtnClickCo()
    {
        _isAnimating = true;
        yield return _waitInterval;

#if UNITY_EDITOR
        Debug.Log("Shrink");
#endif
        
        _gm.SetUserInfo();
        _TMP_introUserName.text = _gm.currentUserName;
        
        yield return DOVirtual.Float(1, 0, 0.45f,
                scale => { _uiRectTransforms[(int)UI_Type.RankingParent].localScale = Vector3.one * scale; })
            .WaitForCompletion();
        _screenDim.DOFade(0.55f, 0.5f);
        _uiGameObjects[(int)UI_Type.RankingParent].SetActive(false);
        _isAnimating = false;
        
        ShowUserInfo();
    }
}