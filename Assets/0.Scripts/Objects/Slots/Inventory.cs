using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
	//몇 칸인지?
	//칸 제한을 걸기 위해서 필요한 두 가지의 숫자
	//가로개수 세로개수
	//Columns  Rows
	//  열      행
	public int columns;
	public int rows;

	//아이템 슬롯을 columns와 rows 개수만큼 준비해야해요!
	//2차원 행렬을 준비!
	//대상을 여러개 저장, 개수가 바뀌지 않고, 순환하는데에 빨라야 해요!
	//배열(Array)
	//      [1,2]
	ItemSlot[,] slots;

	public void Initialize()
	{
		//컴퓨터가 계산하는 방식을 알려드림
		//2차원을 컴퓨터가 만드는 방법!
		//진짜로 램에 2차원 모양으로 저장하는 걸까?
		//아무튼 일렬로 2차원을 구현해야 해요!
		// 123456789
		// 세로로 이동하는 방법 : 가로길이 * 이동하고싶은칸
		//  x, y
		// (1,2)
		// 123
		// 456
		// 789
		// 행, 열 순서로 움직이는 형태가 많다!
		slots = new ItemSlot[rows, columns];
		//위에서 만든 것은 ItemSlot을 담을 수 있는 바구니!
		//술 궤짝을 만들었음!
		//술이 채워지나요?
		for(int row  = 0; row < rows; row++)
		{
			for (int column = 0; column < columns; column++)
			{
				slots[row, column] = new ItemSlot();
			}
		}
	}

	//유니티 버튼에 보여주려면
	//1. public으로 열여놓아야 함
	//2. 반환값이 void 여야함!
	//3. 매개변수가 없거나, 하나인데
	//   int, float, bool, string 중에 하나여야 한다!
	public void HealPotionPlus() //나중에 꼭 지우지 않으면 죽여버리겠다
	{
		ItemContainer potion = DataManager.LoadDataFile<ItemContainer>("LesserHealPotion");
		AddItem(potion, 1);
	}

	public void Sort(System.Comparison<ItemContainer> Method)
	{

	}

	public void AutoQuickInsert(Inventory other)
	{

	}

	public void AutoQuickInsert(Inventory[] other)
	{

	}

	public bool InsertAll(Inventory other)
	{
		return default;
	}
	public bool InsertAll(Inventory other, ItemContainer target)
	{
		return default;
	}

	public void LockSlot(int wantRow, int wantColumn)
	{

	}

	public void UnlockSlot(int wantRow, int wantColumn)
	{

	}

	public int CountItem(ItemContainer wantItem)
	{
		return default;
	}

	public int CountItem(ItemContainer wantItem, out List<ItemSlot> returnSlots)
	{
		returnSlots = default;
		return default;
	}

	public ItemSlot[] GetAllSlot()
	{
		//2차원 배열에서 Length : 전체 길이
		//GetLength(0) : 0번째 차원의 길이 => 여기에서는 행의 길이
		//GetLength(1) : 1번째 차원의 길이 => 여기에서는 열의 길이
		ItemSlot[] result = new ItemSlot[slots.Length];
		//모든 슬롯을 가져오는 방법
		//저희 슬롯이 2차원이라고 생각해봅시다.
		//012  (0,0) (0,1) (0,2)
		//345  (1,0) (1,1) (1,2)
		//678  (2,0) (2,1) (2,2)
		// X  =  Width * R + C
		//012345678
		//                                    (x,y)
		//                                     ^
		int height = slots.GetLength(0);
		int width  = slots.GetLength(1);
		for (int row = 0; row < height; row++)
		{
			//                                         (x,y)
			//                                            ^
			for (int column = 0; column < width; column++)
			{
				//해당 행과 열에 있는 아이템 찾기!
				result[width * row + column] = slots[row, column];
			}
		}

		return result;
	}

	public ItemSlot FindItem(ItemContainer target)
	{

		return default;
	}
	public ItemSlot FindItem(ItemType wantType)
	{

		return default;
	}
	public ItemSlot FindItem(int wantRow, int wantColumn)
	{
		//2차원 배열에서 범위를 넘어가는 경우를 체크하고 싶다!
		//1차원 배열에서부터 생각해볼까요?
		//길이가 5인 배열이 있다고 생각해봅시다.
		//0 1 2 3 4
		//5번칸을 내놓아라~! 없는뎁숑
		//-1번칸을 내놓아라! 모라는 것임
		//이거를 x축으로 한 번, y축으로 한 번
		if (wantRow	   < 0 || wantColumn < 0) return null;
		if (wantRow	   >= slots.GetLength(0)) return null;
		if (wantColumn >= slots.GetLength(1)) return null;
		return slots[wantRow, wantColumn];
	}
	public ItemSlot FindItem(string containWord)
	{

		return default;
	}

	public ItemSlot FindFirstEmptySlot()
	{

		return default;
	}
	public ItemSlot FindLastEmptySlot()
	{

		return default;
	}

	public ItemSlot FindFirstItem(ItemContainer target)
	{

		return default;
	}

	public ItemSlot FindLastItem(ItemContainer target)
	{

		return default;
	}

	//아이템을 추가한 경위
	//원래 바닥에 아이템이 999개 있었습니다.
	//제가 7개 받아올 수 있습니다.
	//제 인벤토리에 추가하고 바닥에 있는 아이템을 삭제한다.
	//아이템 992개는 날아간다.
	//추가하지 못한 개수를 리턴할 것이다!
	public int AddItem(ItemContainer wantItem, int amount = 1)
	{
		//아이템을 추가하는 과정은 어떻게 될까?
		//일단 지금은 모르겠고 첫번째 슬롯에다가 몰빵할까요?
		slots[0,0].AddItem(wantItem, amount);
		return default;
	}

	public int AddItemOnExistSlots(ItemContainer wantItem, int amount)
	{
		return default;
	}

	public int AddItemOnEmptySlots(ItemContainer wantItem, int amount)
	{
		return default;
	}

	public int AddItemToLocation(ItemContainer wantItem, int amount, int row, int column)
	{
		return default;
	}

	//인벤토리 내용 전체 제거가 있는 게임이 있음.
	//마크에서 내가 죽든 상자가 터지든 => 일단 없어짐!
	public ItemSlot[,] Clear()
	{
		ItemSlot[,] origin = slots;
		Initialize();
		return origin;
	}

	public int RemoveItem(System.Predicate<ItemContainer> condition)
	{
		return default;
	}
	public int RemoveItem(ItemContainer wantItem)
	{
		return default;
	}

	public int RemoveItem(ItemContainer wantItem, int amount)
	{
		return default;
	}

	public int RemoveItemOnExistSlots(ItemContainer wantItem, int amount)
	{
		return default;
	}


	public int RemoveItemFromLocation(int row, int column)
	{
		return default;
	}

	public int RemoveItemFromLocation(int row, int column, int amount)
	{
		return default;
	}

	public void MoveItem(int startRow, int startColumn, Inventory targetInventory, int targetRow, int targetColumn, int amount = -1)
	{

	}

	public bool UseItem(ItemContainer target)
	{
		return default;
	}

	public bool UseItem(int row, int column)
	{
		return default;
	}

}
