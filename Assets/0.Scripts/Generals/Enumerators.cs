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
    TileEditor = 19,
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

[System.Flags()]
public enum TileEnterException
{
	Possible		= 0,
	TooFar			= 1 << 0,
	TileNotExist	= 1 << 1,
	Water			= 1 << 4,
	Block_Low		= 1 << 2,
	Block_High		= 1 << 3,
	AlreadyOwned	= 1 << 5,
}

public enum MoveCheckType
{
	None, 
	Charge, Jump, Through, Range,
	Length
}

public enum MoveStyleType
{
	None, 
	Pawn, Bishop, Knight, Rook, King, Queen,
	Length
}