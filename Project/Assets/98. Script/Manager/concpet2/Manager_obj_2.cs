using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_obj_2 : MonoBehaviour
{
    public static Manager_obj_2 instance = null;
    // Start is called before the first frame update

    //Camera, ����� �Ƹ� ����
    public GameObject Main_Camera;
    public GameObject Camera_position;

    //Eventsystem, ������� �Ƹ� ����
    public GameObject Eventsystem;
    // Start is called before the first frame update

    //Animal
    public GameObject Main_Animal;
    public GameObject Animal_position;

    //�ؽ�Ʈ �̹���, �����̼��� ��ġ�� �����������Ƿ� ���� �������� ����
    //ȿ������ ���� ����
    public Sprite[] Animal_text;
    public AudioClip[] Animal_narration;

    [Header("[ COMPONENT CHECK ]")]
    public GameObject[] Main_Animal_array;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;

            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    void Start()
    {

        Main_Animal_array = new GameObject[Main_Animal.transform.childCount];

        for (int i = 0; i < Animal_position.transform.childCount; i++)
        {
            Main_Animal_array[i] = Main_Animal.transform.GetChild(i).gameObject;
        }

    }
}
