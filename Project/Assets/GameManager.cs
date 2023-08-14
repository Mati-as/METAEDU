
using System.Collections.Generic;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    public Dictionary<string, GameObject> animalDict = new Dictionary<string, GameObject>();

    public GameObject cow;
    public GameObject horse;
    public GameObject pig;
    public GameObject swan;
    public GameObject chicken;

    public Transform AnimalMovePosition; //when user corrects an answer.

    private string selectedAnimal;

    public float gamestartTime;
    public static bool IsGameStarted;
    private float elapsedTime;

    Vector3 m_vecMouseDownPos;

    private void Awake()
    {
        animalDict.Add("Cow", cow);
        animalDict.Add("Horse", horse);
        animalDict.Add("Pig", pig);
        animalDict.Add("Swan", swan);
        animalDict.Add("Chicken", chicken);
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;

        if (gamestartTime < elapsedTime && !IsGameStarted)
        {
            IsGameStarted = true;
        }

        if (IsGameStarted)
        {
            CheckMovable();
            Debug.Log($"isMovable: {isMovable}");
            SelectObject();


            if (selectedAnimal != null)
            {

                SetAnimal(selectedAnimal);
                // 동물의 움직임이 시작됨을 표시
            }

            ContinueMovingAnimal();

        }


    }

    /// <summary>
    /// 오브젝트를 선택하는 함수 입니다. 
    /// Linked lIst를 활용해 자료를 검색하고 해당하는 메세지를 카메라 및, 게임 다음 동작에 전달합니다. 
    /// </summary>
    private void SelectObject()
    {

#if UNITY_EDITOR
        // 마우스 클릭 시
        if (Input.GetMouseButtonDown(0))

#else
        // 터치 시
        if (Input.touchCount > 0)
#endif
        {

#if UNITY_EDITOR
            m_vecMouseDownPos = Input.mousePosition;
#else
            m_vecMouseDownPos = Input.GetTouch(0).position;
            if (Input.GetTouch(0).phase != TouchPhase.Began)
                return;
#endif
            // 카메라에서 스크린에 마우스 클릭 위치를 통과하는 광선을 반환합니다.
            Ray ray = Camera.main.ScreenPointToRay(m_vecMouseDownPos);
            RaycastHit hit;

            // 광선으로 충돌된 collider를 hit에 넣습니다.
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.name != null)
                {

                    selectedAnimal = hit.collider.name;
                    elapsedTime = 0;
                    
                }


            }

        }

    }

    public float movingTimeSec;
    public float waitingTime;
    private bool isMovable;

    private GameObject defaultAnimalPosition; // 동물 이동의 시작위치 지정.


    public void CheckMovable()
    {
        if (elapsedTime < movingTimeSec)
        {
            isMovable = false;
        }
        else
        {
            isMovable = true;
        }


    }

    public void ContinueMovingAnimal()
    {
        if (isMovable == false)
        {
            Debug.Log($"{selectedAnimal} is Moving!");

            float t = Mathf.Clamp01(elapsedTime / movingTimeSec);
            // Lerp2D.EaseInQuad 함수에 대한 정의가 누락되어 있어 직접 t 값을 사용합니다.
            if (defaultAnimalPosition != null)
            {
                defaultAnimalPosition.transform.position = Vector3.Lerp(defaultAnimalPosition.transform.position, AnimalMovePosition.position, t);
            }
        }


    }

    public void SetAnimal(string animalName)
    {
        if (animalDict.TryGetValue(animalName, out GameObject animalObj))
        {
            if (animalObj != null)
            {
                defaultAnimalPosition = animalObj;
            }


        }
    }
}
