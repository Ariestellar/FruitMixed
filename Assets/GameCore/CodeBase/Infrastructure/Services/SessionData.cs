using UnityEngine;

namespace CodeBase.Infrastructure.Services
{
    public class SessionData : IService
    {
        public int currentScore;
        public int topScore;
        public Transform fruitsInGlasseParent;
        public int numberOfBomb;
        public int numberOfBlend;
        public int numberOfMultifruit;
        public int numberOfCrystal;
        public StateGame currentGameState;
        public bool isOffSound;
        public SessionData() => currentGameState = StateGame.Loading;
    }
}