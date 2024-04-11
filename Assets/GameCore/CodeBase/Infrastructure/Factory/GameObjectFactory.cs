using System;
using System.Threading.Tasks;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Services;
using UnityEngine;

namespace CodeBase.Infrastructure.Factory
{
    public class GameObjectFactory : IService {
		private readonly AssetProvider _assets;
        public GameObjectFactory(AssetProvider assets) => _assets = assets;
        public async Task<Fruit> CreateFruit(FruitType fruit, Vector3 spawnPosition, Quaternion quaternion, FruitState fruitState) { 
            Fruit fruitGameObject = await _assets.LoadObject<Fruit>(fruit.ToString(), spawnPosition, quaternion);
            fruitGameObject.Construct(fruitState);
            return fruitGameObject;
        }

        public async Task<Bomb> CreateBomb(Vector3 spawnPosition, Quaternion quaternion) 
            => await _assets.LoadObject<Bomb>(AssetAddress.BOMB, spawnPosition, quaternion);
    }
}