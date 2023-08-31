using System.Collections;
using System.Collections.Generic;
using KoreanTyper;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class InstructionUI : MonoBehaviour
{
    private TMP_Text tmpText;

    [FormerlySerializedAs("startTimeOffset")]
    public float startTimeOffsetSeconds; // 게임시작후 몇 초 후 UI재생할 건지

    public float textPrintingSpeed;


    public Dictionary<string, string> animalNameToKorean = new();
    private string roundInstruction;

    [Header("On Correct")] [Space(10f)] public float onCorrectOffsetSeconds;
    public string onCorrectMessage = "정답입니다!";
    public string onFinishMessage = "동물친구들을 모두 찾았어요!";

    private void Awake()
    {
        animalNameToKorean.Add("tortoise", "거북이");
        animalNameToKorean.Add("cat", "고양이");
        animalNameToKorean.Add("rabbit", "토끼");
        animalNameToKorean.Add("dog", "강아지");
        animalNameToKorean.Add("parrot", "앵무새");
        animalNameToKorean.Add("mouse", "쥐");

        tmpText = GetComponentInChildren<TMP_Text>();
        tmpText.text = string.Empty;
    }


    private bool _isQuizPlaying;
    private bool _isCorrectMessagePlaying;
    private UnityEvent _changeUI;


    public void PlayOnCorrectMessage()
    {
        if (GameManager.isGameStarted && GameManager.isCorrected && _isCorrectMessagePlaying == false)
        {
            _isCorrectMessagePlaying = true; //중복재생 방지
            Debug.Log("정답문구 업데이트");
            _typeInCoroutine = StartCoroutine(TypeIn(onCorrectMessage, onCorrectOffsetSeconds));
        }
    }

    public void PlayQuizMessage()
    {
        _isCorrectMessagePlaying = false; // PlayOnCorrectMessage 재생을 위한 초기화.


        if (GameManager.isGameStarted && _isQuizPlaying == false && !GameManager.isCorrected)
        {
#if DEFINE_TEST
            Debug.Log("퀴즈 업데이트");
#endif

            _isQuizPlaying = true;
            roundInstruction = $"{animalNameToKorean[GameManager.answer]}의 그림자를 찾아보세요";
            _typeInCoroutine = StartCoroutine(TypeIn(roundInstruction, startTimeOffsetSeconds));
        }
    }

    public void PlayFinishMessage()
    {
        _isCorrectMessagePlaying = false; // PlayOnCorrectMessage 재생을 위한 초기화.


        if (_isQuizPlaying == false && GameManager.isGameFinished)
        {
            _isQuizPlaying = true;
            roundInstruction = onFinishMessage;
            _typeInCoroutine = StartCoroutine(TypeIn(roundInstruction, startTimeOffsetSeconds));
        }
    }

    public void InitializeMessage()
    {
#if DEFINE_TEST
        Debug.Log("퀴즈 초기화");
#endif

        _isQuizPlaying = false;
        _typeInCoroutine = StartCoroutine(TypeIn(string.Empty, 0));
    }


    public IEnumerator TypeIn(string str, float offset)
    {
        tmpText.text = ""; // 초기화
        yield return new WaitForSeconds(offset); // 1초 대기

        var strTypingLength = str.GetTypingLength(); // 최대 타이핑 수 구함
        for (var i = 0; i <= strTypingLength; i++)
        {
            // 반복문
            tmpText.text = str.Typing(i); // 타이핑
            yield return new WaitForSeconds(textPrintingSpeed);
        } // 0.03초 대기


        yield return new WaitForNextFrameUnit();
    }

    private Coroutine _typeInCoroutine;
}