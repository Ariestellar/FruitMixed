using System;
using System.Collections;
using System.Threading.Tasks;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Services;
using TMPro;
using UnityEngine;

namespace CodeBase.Infrastructure.Factory
{
    public class EffectsFactory : IService {
		private readonly AssetProvider _assets;
        private readonly ICoroutineRunner _coroutineRunner;

        public EffectsFactory(AssetProvider assets, ICoroutineRunner coroutineRunner) {
            _assets = assets;
            _coroutineRunner = coroutineRunner;
        }

        public async Task CreateExplosionWithFire(Vector3 spawnPosition) {
            GameObject explosionEffect = await _assets.LoadObject(AssetAddress.EXPLOSION_EFFECT_WITH_FIRE, spawnPosition, Quaternion.identity);
            _coroutineRunner.StartCoroutine(DelayForUnload(1, explosionEffect)); 
        }

        public async void CreateExplosionWithSmoke(Vector3 spawnPosition) {
            GameObject explosionEffect = await _assets.LoadObject(AssetAddress.EXPLOSION_EFFECT_WITH_SMOKE, spawnPosition, Quaternion.identity);
            _coroutineRunner.StartCoroutine(DelayForUnload(1, explosionEffect)); 
        }

        public async Task<BlenderAnimation> CreateBlenderAnimation() 
            => await _assets.LoadObject<BlenderAnimation>(AssetAddress.BLENDER_ANIMATION);

        private IEnumerator DelayForUnload(float secondDelay, GameObject effect) {
            yield return new WaitForSeconds(secondDelay);
            _assets.Unload(effect);
        }

        public async Task<GameObject> CreateMultiplierEffect(Transform spawnMultiplierEffect, int multiplier)
        {
            GameObject multiplierEffect = await _assets.LoadObject(AssetAddress.MULTIPLIER_EFFECT, spawnMultiplierEffect);
            multiplierEffect.GetComponentInChildren<TextMeshProUGUI>().text = string.Format("x{0}", multiplier);
            _assets.UnloadWithDelay(multiplierEffect, 1);
            return multiplierEffect;
        }

        public async void CreateCrystalAddEffect(Vector3 spawnPosition) {
            GameObject crystalAddEffect = await _assets.LoadObject(AssetAddress.CRYSTAL_ADD_EFFECT, spawnPosition, Quaternion.identity);
            _assets.UnloadWithDelay(crystalAddEffect, 1);
        }
    }
}