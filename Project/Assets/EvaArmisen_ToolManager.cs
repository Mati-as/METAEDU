using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EvaArmisen_ToolManager : MonoBehaviour
{
    private enum ToolList
    {
        Eraser,
        Stamp,
        Reset,
        Download,
        Count
    }

    private enum UI
    {
        Tools,
        FlowerStamps
    }

    public bool _isEraserMode { get; private set; }
    private Button[] _toolBtns;
    private TextMeshProUGUI[] _toolTexts;
    private Sprite[] _stamps;
    private Button[] _stampBtns;
    private Sprite _currentSprite;
    public int currentStampIndex { get; private set; }
    private CanvasGroup _cvsGroup;
    public int FLOWER_STAMP_COUNT;
    private SpriteState _originalState;
    public Dictionary<int, int> currentStampIndexMap { get; private set; } // id:currentIndex
    public RectTransform rectFlowerStamp { get; private set; }
    public Vector3 flowerStampDefaultPos { get; private set; }
    public Vector3 hidePos { get; private set; }
    public readonly float HIDE_POS_AMOUNT = 2.1f;
    private bool[] _isUIOn;
    private bool[] _isUIAnimWorking;

    public static event Action OnResetClicked;
   

    public void Init()
    {
        
        
        UI_Scene_Button.onBtnShut -= OnStartBtnClicked;
        UI_Scene_Button.onBtnShut += OnStartBtnClicked;

        _isUIOn = new bool[(int)ToolList.Count];
        _toolTexts = new TextMeshProUGUI[(int)ToolList.Count];
        _isUIAnimWorking = new bool[(int)ToolList.Count];

        var toolsParent = transform.GetChild((int)UI.Tools);
        _toolBtns = new Button[(int)ToolList.Count];
        for (var i = 0; i < (int)ToolList.Count; i++)
        {
#if UNITY_EDITOR
            Debug.Log($"tool Assigned: {(ToolList)i}");
#endif
            _toolBtns[i] = toolsParent.GetChild(i).GetComponent<Button>();
            _toolTexts[i] = toolsParent.GetChild(i).GetComponentInChildren<TextMeshProUGUI>();
        }

        currentStampIndexMap = new Dictionary<int, int>();
        var flowerStamps = transform.GetChild((int)UI.FlowerStamps);
        FLOWER_STAMP_COUNT = flowerStamps.childCount;
        _stamps = new Sprite[FLOWER_STAMP_COUNT];
        _stampBtns = new Button[FLOWER_STAMP_COUNT];

        _cvsGroup = transform.GetComponent<CanvasGroup>();
        _cvsGroup.alpha = 0;

        rectFlowerStamp = flowerStamps.GetComponent<RectTransform>();

        flowerStampDefaultPos = rectFlowerStamp.anchoredPosition;
        hidePos = flowerStampDefaultPos + Vector3.down * HIDE_POS_AMOUNT;
        rectFlowerStamp.anchoredPosition = hidePos;
        flowerStamps.GetComponent<Button>();
        _toolBtns[(int)ToolList.Stamp].onClick.AddListener(() =>
        {
            if (_isUIAnimWorking[(int)ToolList.Stamp]) return;
            _isUIAnimWorking[(int)ToolList.Stamp] = true;
            _toolTexts[(int)ToolList.Stamp].text = _isUIOn[(int)ToolList.Stamp] ? "도장 고르기\nOFF" : "도장 고르기\nON";
            var target = _isUIOn[(int)ToolList.Stamp] ? hidePos : flowerStampDefaultPos;
            OnUIClicked(rectFlowerStamp, target, ToolList.Stamp);
        });

        _toolBtns[(int)ToolList.Reset].onClick.AddListener(() => { OnResetClicked?.Invoke(); });

        _toolBtns[(int)ToolList.Eraser].onClick.AddListener(() =>
        {
            if (_isUIAnimWorking[(int)ToolList.Eraser]) return;
            
            _isUIAnimWorking[(int)ToolList.Eraser] = true;
            _isEraserMode = !_isEraserMode;
            _toolTexts[(int)ToolList.Eraser].text = _isEraserMode ? "지우개\nON" : "지우개\nOFF";
            StartCoroutine(OnClickCo(ToolList.Eraser));
        });

        for (var i = 0; i < FLOWER_STAMP_COUNT; i++)
        {
            _stampBtns[i] = flowerStamps.GetChild(i).GetComponent<Button>();
            _stamps[i] = flowerStamps.GetChild(i).GetChild(0).GetComponent<Sprite>();

            var index = i; // 변수 라이프 사이클로 인해 캐싱 필요합니다. 

            _stampBtns[i].onClick.AddListener(() =>
            {
                if (currentStampIndexMap.ContainsKey(_stampBtns[index].GetInstanceID()))
                {
                    OnClick(currentStampIndexMap[_stampBtns[index].GetInstanceID()]);
                    _isEraserMode = false;

                }
            });


            currentStampIndexMap.Add(_stampBtns[i].GetInstanceID(), i);
        }

        _originalState = _stampBtns[0].spriteState;
    }

    private void OnDestroy()
    {
        UI_Scene_Button.onBtnShut -= OnStartBtnClicked;
    }

    private void OnClick(int btnIndex)
    {
        currentStampIndex = btnIndex;

        foreach (var button in _stampBtns) button.spriteState = _originalState;

        var pressedState = new SpriteState();
        pressedState.pressedSprite = _stampBtns[currentStampIndex].spriteState.pressedSprite;
        _stampBtns[currentStampIndex].spriteState = pressedState;
    }

    private void OnStartBtnClicked()
    {
        DOVirtual.Float(0, 1, 0.5f, val => { _cvsGroup.alpha = val; }).SetDelay(0.3f);
    }

    private void OnUIClicked(RectTransform rect, Vector2 target, ToolList toolName)
    {
        StartCoroutine(OnUIClickedCo(rect, target, toolName));
    }

    private IEnumerator OnUIClickedCo(RectTransform rect, Vector2 target, ToolList toolName)
    {
#if UNITY_EDITOR
        Debug.Log($"원래위치로! : {target}!");
#endif
        yield return rect.DOAnchorPos(target, 0.7f).WaitForCompletion();
        _isUIOn[(int)toolName] = !_isUIOn[(int)toolName];
        _isUIAnimWorking[(int)toolName] = false;
    }
    
    private IEnumerator OnClickCo(ToolList toolName)
    {
        yield return DOVirtual.Float(0, 0, 1.0f, _ => { }).WaitForCompletion();
        _isUIOn[(int)toolName] = !_isUIOn[(int)toolName];
        _isUIAnimWorking[(int)toolName] = false;
    }
}