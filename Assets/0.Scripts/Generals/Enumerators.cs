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
	TooFar, TileNotExist, Blocked, AlreadyOwned,
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