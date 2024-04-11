using CodeBase.Infrastructure.Services;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace CodeBase.Infrastructure.AssetManagement
{
	public class AssetProvider : IService
	{   
		public async Task<GameObject> LoadObject(string assetId, Transform parent = null) {
            GameObject gameObject = await Addressables.InstantiateAsync(assetId, parent).Task;
			return gameObject;
		} 

        public async Task<GameObject> LoadObject(string assetId, Vector3 position, Quaternion rotation) {
            GameObject gameObject = await Addressables.InstantiateAsync(assetId, position, rotation).Task;
			return gameObject;
		} 

        public async Task<T> LoadObject<T>(string assetId, Transform parent = null)
            => await GetObject<T>(Addressables.InstantiateAsync(assetId, parent)); 

        public async Task<T> LoadObject<T>(string assetId, Vector3 position, Quaternion rotation) 
            => await GetObject<T>(Addressables.InstantiateAsync(assetId, position, rotation));

        public async Task<T> LoadAsset<T>(string name) { 
            var handle = Addressables.LoadAssetAsync<T>(name);
            T asset = await handle.Task;
            if (handle.Status != AsyncOperationStatus.Succeeded) Debug.LogError(string.Format("Ассет {0} не был загружен по причине {1} ", name, handle.DebugName));            
            return asset;
        }

        private async Task<T> GetObject<T>(AsyncOperationHandle<GameObject> handle) {
            GameObject gameObject = await handle.Task;
            if(handle.Status != AsyncOperationStatus.Succeeded) Debug.LogError("Объект не был загружен: " + gameObject.name);
            if(gameObject.TryGetComponent(out T component) == false) throw new NullReferenceException($"Объект типа {typeof(T)} равен null");
            return component;
        } 

        public void Unload(GameObject gameObject)
        {
            if(gameObject == null){
                Debug.LogError("Попытка выгрузить несуществующий объект: " + gameObject.name);
                return;
            }                
            gameObject.SetActive(false);
            Addressables.ReleaseInstance(gameObject);
            gameObject = null;
        }

        public void UnloadWithDelay(GameObject gameObject, float sec) => UnityEngine.GameObject.Destroy(gameObject, sec);

        public void UnLoadAsset<T>(T asset) {
            if(asset != null) Addressables.Release<T>(asset);
        }
    }
}