using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Scene_Button : MonoBehaviour
{
  
  private Message_anim_controller _animController;
  private Button _btn;
  private Image _btnImage;
  private TMP_Text tmp;

  private int _remainTime;

  
  /*
   * onBtnClicked와 Message_anim_controller의 onIntroUIOff 이벤트는 같은 기능을 수행
   * 사용자가 버튼을 클릭하여 event를 수행하는지, 혹은 10초(대기시간 제한)이 지나서 이벤트가 수행되는지에 대한
   * 구분만 되어있음. 이를 방지하기위해 bool연산자 사용. 
   *
   */
  public static event Action onBtnClicked;

  private void Awake()
  {
      _animController = FindActiveMessageAnimController();
      
  }

  private void Start()
  {
    
      _btn = GetComponent<Button>();
      _btnImage = GetComponent<Image>();
      _btn.onClick.AddListener(OnClicked);
      tmp = GetComponentInChildren<TMP_Text>();
     
      
      Message_anim_controller.onIntroUIOff -= OnAnimOff;
      Message_anim_controller.onIntroUIOff += OnAnimOff;
     
      _btnImage.DOFade(0, 0.01f);
      _btnImage
          .DOFade(1, 0.5f)
          .SetDelay(3f);
      
      tmp.DOFade(0, 0.01f);
      tmp
          .DOFade(1, 0.5f)
          .SetDelay(3f);
  }
  
  private Message_anim_controller FindActiveMessageAnimController()
  {
     
      foreach (Transform child in transform.parent)
      {
          if (child.gameObject.activeInHierarchy)
          {
              Message_anim_controller controller = child.GetComponent<Message_anim_controller>();
              if (controller != null)
              {
                  return controller; 
              }
          }
      }
      return null; 
  }

  private float _elapsedTime;
  private void Update()
  {
      _elapsedTime += Time.deltaTime;
      _remainTime = (int)(Message_anim_controller._autoShutDelay - _elapsedTime);
      tmp.text = $"시작({_remainTime})";
  }

  private void OnDestroy()
  {
      Message_anim_controller.onIntroUIOff -= OnAnimOff;
  }

  private bool _isBtnEventInvoked;

  private void OnClicked()
  {
      if (_animController != null)
      {
          _animController.Animation_Off();
          if (!_isBtnEventInvoked)
          {   
              
              _isBtnEventInvoked = true;
              
              onBtnClicked?.Invoke();
         
              FadeOutBtn();
          }
       
      }
      else
      {
          #if UNITY_EDITOR
          Debug.Log("AnimalController is null");
          #endif
      }
  }

  private void OnAnimOff()
  {
      if (!_isBtnEventInvoked)
      {

          _isBtnEventInvoked = true;
          
          onBtnClicked?.Invoke();
          FadeOutBtn();
    
      }
  }

  private void FadeOutBtn()
  {
      _btnImage.DOFade(0, 0.5f)
          .OnComplete(() => {gameObject.SetActive(false); });
  }
}
