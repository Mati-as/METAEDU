using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using static UnityEditor.PlayerSettings;

public class Manager_Text : MonoBehaviour
{

    private GameObject UI_Text;
    private GameObject UI_Message;
    private GameObject Panel;

    private Sequence Seq_panel;

    [Header("[ COMPONENT CHECK ]")]
    //Common
    public int Content_Seq = 0;
    public int Number_Prev_message = -1;

    public GameObject[] UI_Text_array;
    public GameObject[] UI_Message_array;

    void Start()
    {
        //여기에서 초기화를 해야하는데
        //각 씬별로 해당하는 오브젝트 스크립트를 받아와서 obj_3을 할당해줘야함
        //결국 이 스크립트의 UI_Text를 setting할 수 있도록 해주는건?
        //그럼 뭔가 순차적으로 진행이 되어야할 수도
        //그럼 그냥 초기화를 obj에서 한번에 해주는것은?


        //Init_UI_text();
        //Init_UI_Panel(10f);
    }

    //텍스트 저장
    public void Init_UI_text(GameObject text, GameObject message)
    {
        UI_Text = text;
        UI_Message = message;

        if (UI_Text != null)
        {
            UI_Text_array = new GameObject[UI_Text.transform.childCount];

            for (int i = 0; i < UI_Text.transform.childCount; i++)
            {
                UI_Text_array[i] = UI_Text.transform.GetChild(i).gameObject;
                //Debug.Log(i+"+"+UI_Text_array[i]);
            }
        }

        if (UI_Message != null)
        {
            UI_Message_array = new GameObject[UI_Message.transform.childCount];

            for (int i = 0; i < UI_Message.transform.childCount; i++)
            {
                UI_Message_array[i] = UI_Message.transform.GetChild(i).gameObject;
            }
        }
    }

    //public void Set_Audio_seq_narration(AudioClip[] audio)
    //{
    //    Audio_seq_narration = audio;
    //}
    public void Init_UI_panel(GameObject panel, float time)
    {
        Panel = panel;
        Seq_panel = DOTween.Sequence().SetAutoKill(false);

        Seq_panel.Append(Panel.transform.DOScale(1, 0.1f).From(0));
        Seq_panel.Append(Panel.transform.DOScale(0, 0.1f).From(1).SetDelay(time));
        //Panel.SetActive(false);
    }
    //텍스트 활성화, 이전 텍스트 비활성화, 애니메이션 재생
    public void Change_UI_text(int Number_seq)
    {
        Content_Seq = Number_seq;
        if (Content_Seq != 0)
        {
            UI_Text_array[Content_Seq - 1].SetActive(false);
        }
        UI_Text_array[Content_Seq].SetActive(true);
        UI_Text_array[Content_Seq].transform.DOScale(1, 1f).From(0).SetEase(Ease.OutElastic);
    }

    public void Inactive_UI_Text()
    {
        UI_Text_array[Content_Seq].SetActive(false);
    }

    public void Active_UI_message(int Number)
    {
        //메시지 보여주는 기능
        //메시지에 대한 나레이션은 해당 메시지 오브젝트에 붙어있다고 가정하였음
        //메시지 나레이션은 obj에서 사전 저장된걸 가져다 줌

        Inactive_UI_Text();
        //메시지 최초 실행시
        if (Number_Prev_message != -1)
        {
            UI_Message_array[Number_Prev_message].SetActive(false);

            //해당하는 메시지 오브젝트의 나레이션을 읽어줘야함
            //일단은 나레이션 읽어주는 
            //UI_Message_array[Number].GetComponent<AudioSource>().clip = Manager_obj_3.instance.Msg_narration[Target_num];
            //나레이션, BGM, 이펙트 만약에 이펙트가 다수면?
            //Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/Sandwich/Click_" + randomChar, 0.3f);
            //나레이션에 넣으면 안될 것 같은데
            //이펙트도 여러개여야할 것 같은데

        }
        UI_Message_array[Number].SetActive(true);
        UI_Message_array[Number].transform.DOScale(1, 1f).From(0).SetEase(Ease.OutElastic);

        Number_Prev_message = Number;
    }

    //(임시) 3컨셉을 위한 도구
    public void Changed_UI_message_c3(int Number, int Target_num, bool Eng_mode)
    {
        //(0119 수정 필요) 현재는 각 메시지 오브젝트 안에 있는 오디오 소스로 메시지의 나레이션을 읽어줌
        //해당하는 UI 이미지 스프라이트 변경하고, 오디오 소스의 나레이션 변경하고
        Inactive_UI_Text();
        //메시지 최초 실행시
        if (Number_Prev_message != -1)
        {
            UI_Message_array[Number_Prev_message].SetActive(false);
        }

        if (Eng_mode)
        {
            UI_Message_array[Number].GetComponent<Image>().sprite = Manager_obj_3.instance.Msg_textsprite_eng[Target_num];
            UI_Message_array[Number].GetComponent<AudioSource>().clip = Manager_obj_3.instance.Msg_narration_eng[Target_num];
        }else
        {
            UI_Message_array[Number].GetComponent<Image>().sprite = Manager_obj_3.instance.Msg_textsprite[Target_num];
            UI_Message_array[Number].GetComponent<AudioSource>().clip = Manager_obj_3.instance.Msg_narration[Target_num];
        }
        UI_Message_array[Number].SetActive(true);
        UI_Message_array[Number].transform.DOScale(1, 1f).From(0).SetEase(Ease.OutElastic);
        UI_Message_array[Number].transform.DOScale(0, 1f).From(1).SetEase(Ease.OutElastic).SetDelay(2f);

        Debug.Log("텍스트");
        //금방 비활성화 해야함

        Number_Prev_message = Number;
    }

    public void Inactive_UI_message(int Number)
    {
        UI_Message_array[Number].SetActive(false);
    }

    public void Inactiveall_UI_message()
    {
        for (int i = 0; i < UI_Message.transform.childCount; i++)
        {
            UI_Message_array[i].SetActive(false);
        }
    }
    public void Active_UI_Panel()
    {
        Panel.SetActive(true);
        Seq_panel.Restart();
    }
    public void Inactive_UI_Panel()
    {
        Panel.SetActive(false);
    }
}