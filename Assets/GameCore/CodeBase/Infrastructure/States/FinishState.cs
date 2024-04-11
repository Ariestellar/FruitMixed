using System;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services;
using YG;

namespace CodeBase.Infrastructure.States
{
    public class FinishState : IState
    {
        private readonly GameStateMachine _stateMachine;
        private readonly AllServices _services;
        private SessionData _sessionData;
        private UIFactory _uiFactory;

        public FinishState (GameStateMachine stateMachine, AllServices services) {
            _stateMachine = stateMachine;
            _services = services;
        }

        public async void Enter() { 
            _sessionData = _services.Single<SessionData>();
            _uiFactory = _services.Single<UIFactory>();
            _sessionData.currentGameState = StateGame.Final;
            ResetValues(); 
            SaveValues(_sessionData);
            YandexGame.SaveProgress();
            
            GameOverPanel gameOverPanel = await _uiFactory.CreateGameOverPanel(_sessionData.currentScore, _sessionData.topScore);
            gameOverPanel.RestartButton.onClick.AddListener(Restart);
            if (_sessionData.currentScore > YandexGame.savesData.topScore) SaveBestScore(_sessionData.currentScore);
        }

        private void SaveValues(SessionData sessionData) {
            YandexGame.savesData.numberOfBomb = sessionData.numberOfBomb;
            YandexGame.savesData.numberOfBlend = sessionData.numberOfBlend;
            YandexGame.savesData.numberOfMultifruit = sessionData.numberOfMultifruit;
            YandexGame.savesData.numberOfCrystal = sessionData.numberOfCrystal;
        }

        public void Exit() {
            
        } 
        
        private void Restart() {
            ResetValues();
            _stateMachine.Enter<LoadGameSceneState, Scenes>(Scenes.GameScene);
        }

        private void SaveBestScore(int topScore) {
            YandexGame.savesData.topScore = topScore;
            YandexGame.SaveProgress();
            YandexGame.NewLeaderboardScores("FruitMixLB", topScore);
        }

        private void ResetValues() {
            _sessionData.currentScore = 0;
            YandexGame.savesData.currentScore = 0;
            YandexGame.savesData.positionFruitX = new float[0];
            YandexGame.savesData.positionFruitY = new float[0];
            YandexGame.savesData.positionFruitZ = new float[0];
            YandexGame.savesData.rotationFruitX = new float[0];
            YandexGame.savesData.rotationFruitY = new float[0];
            YandexGame.savesData.rotationFruitZ = new float[0];
            YandexGame.savesData.typeFruit = new int[0];            
        }
    }
}