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

		return default;
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
