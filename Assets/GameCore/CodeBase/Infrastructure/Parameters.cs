public enum Scenes { GameScene, Initialization }

public enum FruitType{ 
    Blueberry = 0, 
    Raspberry = 1,
    Blackberries = 2,
    Strawberry = 3,
    Tangerine = 4,
    Plum  = 5,
    Lemon = 6,
    Apple   = 7,
    Orange = 8,
    Pomegranate = 9,
    Watermelon = 10,
    Multifruit = 11
}

public enum FruitState
{
    Ready = 1,
    Dropping = 2,
    Collision = 3,
}

public enum ObjectInHand
{
    Bomb = 0,
    Blender = 1,
    Multifruit = 2,
    Fruit = 3,
    None = 4,
}

public enum UIMenu
{    
    None = 0,
    Help = 1,
    AD = 1,
}

public enum StateGame
{    
    Loading = 0,
    Game = 1,
    Final = 2
}