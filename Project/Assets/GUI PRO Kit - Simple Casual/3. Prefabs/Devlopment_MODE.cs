using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Devlopment_MODE : MonoBehaviour
{
    public GameObject Button_Dev;
    public GameObject Button_Demo;

    private bool Mode_Development = false;
    
    // Start is called before the first frame update
    void Start()
    {
        //���� ���� ��� üũ�ϰ� �ش� ���� Ȱ��ȭ   
        Debug.Log("������ ��� ȭ��");
    }

    public void Mode()
    {
        if (Mode_Development == false)
        {

            //���� ������ ��ġ O
            //UIȰ��ȭ
            Debug.Log("������ ��� Ȱ��ȭ ��");
            Button_Demo.SetActive(false);
            Button_Dev.SetActive(true);
        }
        else if(Mode_Development == true)
        {

            //�������� �ʰ� ��ġ ����
            //UI ��Ȱ��ȭ
            Debug.Log("���� ��� Ȱ��ȭ ��");
            Button_Dev.SetActive(false);
            Button_Demo.SetActive(true);
        }
        Mode_Development =!Mode_Development;

    }
}
