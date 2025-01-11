using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;

public class Manager_Seq_2 : MonoBehaviour
{

    public static Manager_Seq_2 instance = null;

    private Manager_Text Manager_Text;
    private Manager_Anim_2  Manager_Anim;
    private Manager_Narr Manager_Narr;

    public GameObject Eventsystem;

    private bool toggle = true;

    //���� �׷�
    private int On_game;

    [Header("[ COMPONENT CHECK ]")]

    public int Content_Seq = 0;
    //���� ���� ó�� ��ſ� 0���� ����
    public float Sequence_timer = 0f;
    //�ð�, ���� ����?

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

    // Start is called before the first frame update
    void Start()
    {
        Manager_Text = this.gameObject.GetComponent<Manager_Text>();
        Manager_Anim = this.gameObject.GetComponent<Manager_Anim_2>();
        Manager_Narr = this.gameObject.GetComponent<Manager_Narr>();

        Eventsystem.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

        //�ٵ� ������Ʈ�� ��� �װ� ������ ��� ���ư��ٵ�?

        Sequence_timer -= Time.deltaTime;
        if (Sequence_timer < 0)
        {
            if (toggle == true)
            {
                toggle = false;
                Act();
                //Debug.Log("timer done");
            }
        }
    }


    //���ʷ� ������ ������ �� ��Ʈ�� �� �������� �̰� ���ư��� �ǰ�
    //�ƴϸ� ��Ʈ�� �����ϰ� �����ϴ� �ɷ� �ϵ�,


    void Act()
    {
        Manager_Text.Change_UI_text(Content_Seq);
        Manager_Narr.Change_Audio_narr(Content_Seq);
        Manager_Anim.Change_Animation(Content_Seq);

        //����� �̹� 2���ε� �ؽ�Ʈ, �����̼��� ���� 1���̾ ���� �������� ����
        if (Content_Seq == 2)
        {
            Init_Game_hide();
        }
        else if (Content_Seq == 3)
        {
            Manager_Text.Inactiveall_UI_message();

            Content_Seq += 1;
            toggle = true;
            Timer_set();
        }
        else if (Content_Seq == 4)
        {
            Init_Game_reveal();
        }
        else if (Content_Seq == 5)
        {
            Manager_Text.Inactiveall_UI_message();
            Init_Game_read();

            Content_Seq += 1;
            toggle = true;
            Timer_set();
        }
        else if ( 6 <= Content_Seq && 12 >= Content_Seq)
        {
            Game_read();
        }
        else if (Content_Seq == 13)
        {
            //End, Ŭ�� Ȱ��ȭ
            Eventsystem.SetActive(true);
        }
        else
        {
            Content_Seq += 1;
            toggle = true;
            Timer_set();
        }
    }

    void Init_Game_hide()
    {
        //�޽����� ���, T - L - E - R - P - Z - F ����, hide, reveal

        Eventsystem.SetActive(true);
        On_game = 0;
        //Manager_Anim.Hide_All_animal();
    }
    void Init_Game_reveal()
    {
        Eventsystem.SetActive(true);
        Manager_Anim.Active_click_animal();
        //�ٽ� ���� ��ũ��Ʈ Ȱ��ȭ
        On_game = 0;
    }
    void Init_Game_read()
    {
        //������ ��ġ ���� ����
        On_game = 0;
        Eventsystem.SetActive(false);
        //���ʺ��� ������� ����ǵ��� ���� ���� �ʿ�
        Manager_Anim.Reset_Seq_animal(0);
        Manager_Anim.Reset_Seq_animal(1);
        Manager_Anim.Reset_Seq_animal(2);
        Manager_Anim.Reset_Seq_animal(3);
        Manager_Anim.Reset_Seq_animal(4);
        Manager_Anim.Reset_Seq_animal(5);
        Manager_Anim.Reset_Seq_animal(6);
    }
    void Game_read()
    {
        On_game += 1;
        toggle = true;
        Content_Seq += 1;
        Timer_set();
        //�ش��ϴ� ���� �ִϸ��̼� ���
    }
    void Timer_set()
    {
        Sequence_timer = 5f;
        //���� �� �κ��� ���߿��� ������ ���ִ��� �ƴϸ� �� Ư�� �κи� �ٸ� �����ͷ� �־��ִ��� �ؾ���
    }

    public void animal_button(int Num_button)
    {
        if (Content_Seq == 2)
        {
            Manager_Text.Active_UI_message(Num_button);
            Manager_Anim.Hide_Seq_animal(Num_button);

            On_game += 1;
        }
        else if (Content_Seq == 4)
        {
            Manager_Text.Active_UI_message(Num_button+7);
            Manager_Anim.Reveal_Seq_animal(Num_button);

            //�� ������ �ִϸ��̼��� ���� ���� �� �� ���� �����ð��� �ΰ� ���� ���� Ŭ���� �� �ֵ��� ��
            Manager_Text.Active_UI_Panel();
            On_game += 1;
        }
        else if (Content_Seq == 13)
        {
            Manager_Text.Active_UI_message(Num_button + 7);
            Manager_Anim.Reveal_Seq_animal(Num_button);

        }

        if (On_game == 7)
        {

            //ȿ���� ���, ����Ʈ ����
            Eventsystem.SetActive(false);
            Content_Seq += 1;
            toggle = true;
            Timer_set();
        }
    }
}
