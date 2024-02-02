using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_slider_setting : MonoBehaviour
{
    // Start is called before the first frame update
    //���� �޴����� ��ü �Ҹ� �����ϱ� ���� �Լ�
    //�ش� �ϴ� ������Ʈ ��� �ν����� â���� üũ�ϰ� �ش��ϴ� �Լ� ȣ��
    //�� �����̴��� ���� �ٲ��� ���� ���� �Լ� ȣ��

    private Slider volume_slider;
    public bool All_sound = false;
    public bool Effect = false;
    public bool BGM = false;

    private float tmp_volume;
    private float volume;

    void Start()
    {
        volume_slider = this.gameObject.GetComponent<Slider>();
        volume = volume_slider.value;
        if (tmp_volume != volume)
        {
            if (All_sound == true)
            {
                //volume_slider.value = Manager_audio.instance.Get_all_volume();
            }
            else if (Effect == true)
            {
                volume_slider.value = Manager_audio.instance.Get_Effect_volume();
            }
            else if (BGM == true)
            {
                volume_slider.value = Manager_audio.instance.Get_BGM_volume();
            }
        }
        //�ʱ� ���� �������°� �ʿ��� ��?
    }
   
    // Update is called once per frame
    void Update()
    {
        volume = volume_slider.value;
        if (tmp_volume != volume)
        {
            if (All_sound == true)
            {
                Manager_audio.instance.Set_all_sound_volume(volume_slider.value);
            }
            else if (Effect == true)
            {
                Manager_audio.instance.Set_effect_sound_volume(volume_slider.value);
            }
            else if (BGM == true)
            {
                Manager_audio.instance.Set_BGM_volume(volume_slider.value);
            }
        }
        tmp_volume = volume;
    }
}
