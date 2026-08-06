public enum UIType
{
    None,
    Loading = 1,
    Title = 2,
    Option = 3,
    Movable = 4,
    Menu = 5,
    Info = 6,
    Battle = 7,
    GameQuit = 8,
    CharacterClickInfo = 9,
    Resign = 10,
    Map = 11,
    OutBox = 12,
    Dictionary = 13,
    ToMainMenu = 14,
    RaidCrew = 15,
    Inventory = 16,
    ItemCursorSlot = 17,
    IngameCover = 18,
    _Length
}

public enum ScreenChangeType
{
    None,
    ScreenChanger, FadeChanger, SlideChanger,
    _Length
}

public enum RarityType
{
	Common, Rare, Epic, Legendary,
	Length
}

public enum ObjectiveType
{
	BreakThrough, CaptureKing, CaptureAll, CatBurglar, BakeALoaf, MarkPayload,
	Length
}

public enum AllyType
{
	Neutral, White, Black,
	Length
}

public enum ElementType
{
	Physical, Psychic, Fire, Ice, Poison, Electric, 
	Length
}

public enum MovePresetType
{
	None,
	Pawn, King, Queen, Rook, Bishop, Knight,
	Length
}

public enum TileEnterException
{
	Possible,
	TooFar, TileNotExist, Block_Low, Block_High, Block_All, AlreadyOwned,
	Length,
}

public enum TileBaseType
{
	None,
	Dirt, Ochre, Sand, Stone, Water,
	Length,
}

public enum TileDecoType
{
	None,
	Bush, Grass, Snow,
	Length,
}

public enum MoveCheckType
{
	Charge, Jump, Through, Range,
	Length
}

public enum MoveStyleType
{
	Pawn, Bishop, Knight, Rook, King, Queen,
	Length
}