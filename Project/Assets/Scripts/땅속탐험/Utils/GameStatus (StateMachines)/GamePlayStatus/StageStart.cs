using System;

    public class StageStart : BaseState
    {
        public IState.GameStateList Gamestate
        {
            get => Gamestate;
            set => Gamestate = value; // 값을 설정합니다.
        }
      
        public override void Enter()
        {
        }

        public override void Update()
        {
        }

        public override void Exit()
        {
        }
    }

