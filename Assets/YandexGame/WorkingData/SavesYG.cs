
namespace YG
{
    [System.Serializable]
    public class SavesYG
    {
        // "Технические сохранения" для работы плагина (Не удалять)
        public int idSave;
        public bool isFirstSession = true;
        public string language = "ru";
        public bool promptDone;

        // Ваши сохранения
        public int topScore = 0;
        public int currentScore = 0;
        public float[] positionFruitX;
        public float[] positionFruitY;
        public float[] positionFruitZ;
        public float[] rotationFruitX;
        public float[] rotationFruitY;
        public float[] rotationFruitZ;
        public int[] typeFruit;

        public int numberOfBomb;
        public int numberOfBlend;
        public int numberOfMultifruit;
        public int numberOfCrystal;

        // Поля (сохранения) можно удалять и создавать новые. При обновлении игры сохранения ломаться не должны


        // Вы можете выполнить какие то действия при загрузке сохранений
        public SavesYG()
        {
            // Допустим, задать значения по умолчанию для отдельных элементов массива

        }
    }
}
