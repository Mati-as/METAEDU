public abstract class BaseState : IState
{

    public IState.GameStateList Gamestate
    {
        get => Gamestate;
        set => Gamestate = value; // 값을 설정합니다.
    }
    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}