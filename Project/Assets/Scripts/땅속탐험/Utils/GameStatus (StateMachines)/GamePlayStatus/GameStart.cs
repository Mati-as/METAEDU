using System;
using UnityEngine;


public class GameStart : MonoBehaviour, IState
    {
        
        public IState.GameStateList Gamestate
        {
            get => IState.GameStateList.GameStart;
            set => Gamestate = value;
        }

        public  void Enter()
        {
            
        }

        public void Update()
        {
        }

        public void Exit()
        {
        }

        //구독처리
        private void Start()
        {
        }
    }

