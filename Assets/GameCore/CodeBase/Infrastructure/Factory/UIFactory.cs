using System;
using System.Threading.Tasks;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Services;
using UnityEngine;

namespace CodeBase.Infrastructure.Factory
{
    public class UIFactory : IService
	{
		private readonly AssetProvider _assets;

        public UIFactory(AssetProvider assets)
        {
            _assets = assets;
        }

        public async Task<GameOverPanel> CreateGameOverPanel(int currentTotalScore, int topScore) {
            GameOverPanel gameOverPanel = await _assets.LoadObject<GameOverPanel>(AssetAddress.GAME_OVER_PANEL);
            gameOverPanel.Construct(Convert.ToString(currentTotalScore), Convert.ToString(topScore));
            return gameOverPanel;
        }

        public async Task<GameObject> CreateHelpPanel() => await _assets.LoadObject(AssetAddress.HELP_PANEL);
        public async Task<ADDialogPanel> CreateADDialogPanel() => await _assets.LoadObject<ADDialogPanel>(AssetAddress.AD_DIALOG_PANEL);
    }
}