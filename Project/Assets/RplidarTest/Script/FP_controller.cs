using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FP_controller : MonoBehaviour
{
    //ȭ��� Ȱ��ȭ �Ǿ��ִ� FP RT����Ʈ
    private List<RectTransform> FP_pos_controller = new List<RectTransform>();

    private float FP_x, FP_y;
    private float FP_Gap=80;

    void Start()
    {
        
    }

    public bool Check_FPposition(RectTransform FP)
    {
        for(int i =0;i< FP_pos_controller.Count; i++)
        {
            Debug.Log("�����ϴ� FP ī��Ʈ" + FP_pos_controller.Count);
            FP_x = FP_pos_controller[i].anchoredPosition.x;
            FP_y = FP_pos_controller[i].anchoredPosition.y;

            if (FP_x - FP.anchoredPosition.x < -FP_Gap || FP_x - FP.anchoredPosition.x > FP_Gap)
            {
                if (FP_y - FP.anchoredPosition.y < -FP_Gap || FP_y - FP.anchoredPosition.y > FP_Gap)
                {
                    //���ÿ� ���� ��ǥ�� ������ ���� ���� �����ϴ� ���� �߻���
                    //���İ��� 2������ ��ǥ���� �ԷµǸ鼭 �ش� ���� ��ó�� ���� ������ �������� ������ �Ǻ���

                    //for�� �� ���� ���������� false ���ذ��� true ����

                    //�ϴ� ����
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        //for���� ���ʿ� ���� ���� ���� true ����
        return true;
    }

    public void Add_FPposition(RectTransform FP)
    {
        FP_pos_controller.Add(FP);
    }

    public void Delete_FPposition()
    {
        FP_pos_controller.RemoveAt(0);
    }
}
