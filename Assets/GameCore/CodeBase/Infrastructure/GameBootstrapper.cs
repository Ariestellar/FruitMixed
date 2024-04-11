using CodeBase.Infrastructure;
using CodeBase.Infrastructure.States;
using UnityEngine;
using YG;

public class GameBootstrapper : MonoBehaviour, ICoroutineRunner
{
    [SerializeField] private bool _isDebug;    
    [SerializeField] private YandexGame _yandexGame;     
    private Game _game;
       
    private void Awake() { 
        YandexGame.GetDataEvent += Launch;
        if(_isDebug) Debug.Log("Подписались на получение данных с YandexSDK, ожидаем получения..."); 
        _game = new Game(this, _yandexGame, _isDebug); 
        DontDestroyOnLoad(this);
    }

    private void Launch() {
        YandexGame.GetDataEvent -= Launch;
        if(_isDebug) Debug.Log("Получили данные из YandexSDK. Имя игрока: " + YandexGame.playerName);        
        _game.generalStateMachine.Enter<BootstrapState>(); 
    }
}