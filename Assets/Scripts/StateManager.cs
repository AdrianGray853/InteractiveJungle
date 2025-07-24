using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum GameState
{
    Start, Playing, GameOver, GameWin
}
public enum PlayerState
{
    Waiting, Playing, Dead
}

public static class StateManager
{
    private static GameState _state;
    private static PlayerState _playerState;
    
    public delegate void GameStateDelegate(GameState value);
    public delegate void PlayerStateDelegate(PlayerState value);
    public static event GameStateDelegate OnGameStateChange;
    public static event PlayerStateDelegate OnPlayerStateChange;

    public static GameState GetGameState()
    {
        return _state;
    }

    public static void SetGameState(GameState s)
    {
        _state = s;
        OnGameStateChange?.Invoke(s);
    }
    
    public static bool IsGamePlaying()
    {
        return _state == GameState.Playing;
    }
    
    public static PlayerState GetPlayerState()
    {
        return _playerState;
    }

    public static void SetPlayerState(PlayerState s)
    {
        _playerState = s;
        OnPlayerStateChange?.Invoke(s);
    }
    
    public static bool IsPlayerWaiting()
    {
        return _playerState == PlayerState.Waiting;
    }
    
    public static bool IsPlayerDead()
    {
        return _playerState == PlayerState.Dead;
    }
}