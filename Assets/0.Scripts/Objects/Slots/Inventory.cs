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
	public void HealPotionPlus(int amount) //나중에 꼭 지우지 않으면 죽여버리겠다
	{
		ItemContainer potion = DataManager.LoadDataFile<ItemContainer>("LesserHealPotion");
		AddItem(potion, amount);
	}

    public void HealPotionMinus(int amount) //나중에 꼭 지우지 않으면 죽여버리겠다
    {
        ItemContainer potion = DataManager.LoadDataFile<ItemContainer>("LesserHealPotion");
        RemoveItem(potion, amount);
    }

    public bool IsEmpty(ItemSlot target) => target?.GetIsEmpty() ?? false;


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

    //     반복기! => 원하는 자료형을 반복적으로 내보내는 친구!
    //                요구할 때마다 하나씩 뽁 나와요
    //                ItemSlot을 요구할 때마다 다음 슬롯을 뽁 내놓는 친구!
	public IEnumerable<ItemSlot> GetAllSlot()
	{
		//2차원 배열에서 Length : 전체 길이
		//GetLength(0) : 0번째 차원의 길이 => 여기에서는 행의 길이
		//GetLength(1) : 1번째 차원의 길이 => 여기에서는 열의 길이
		//ItemSlot[] result = new ItemSlot[slots.Length];

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
                //널이라면 보내주지 않기!
                if (slots[row, column] is null) continue;

				//해당 행과 열에 있는 아이템 찾기!
                //yield return : 결과를 내보내고 나서, 기다리기!
				yield return slots[row, column];
			}
		}
	}
    public IEnumerable<ItemSlot> GetAllSlot(System.Predicate<ItemSlot> pred)
    {
        if(pred is null) yield break;
        foreach(ItemSlot currentSlot in GetAllSlot())
        {
            if(pred(currentSlot)) yield return currentSlot;
        }
    }
    public IEnumerable<ItemSlot> GetAllSlotReverse()
    {
        int height = slots.GetLength(0);
        int width = slots.GetLength(1);
        for (int row = height - 1; row >= 0; row--)
        {
            for (int column = width - 1; column >= 0; column--)
            {
                if (slots[row, column] is null) continue;
                yield return slots[row, column];
            }
        }
    }
    public IEnumerable<ItemSlot> GetAllSlotReverse(System.Predicate<ItemSlot> pred)
    {
        if (pred is null) yield break;
        foreach (ItemSlot currentSlot in GetAllSlotReverse())
        {
            if (pred(currentSlot)) yield return currentSlot;
        }
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

    //제일 왼쪽 위 첫 번째 슬롯을 찾고 싶다!
    //찾은 다음에 그 뒤부터 다시 진행을 할 수 있는 방법!
    //함수를 잠깐 멈춰놓았다가 나중에 다음 코드로 이동하는 것!
    //yield return
    //반복을 나중에 추가로 도는 방법!
    public IEnumerable<ItemSlot> FindFirstEmptySlot() => GetAllSlot(IsEmpty);
    public IEnumerable<ItemSlot> FindLastEmptySlot() => GetAllSlotReverse(IsEmpty);
    public IEnumerable<ItemSlot> FindFirstItem(ItemContainer target) => GetAllSlot((slot) => slot.GetItem() == target);
	public IEnumerable<ItemSlot> FindLastItem(ItemContainer target) => GetAllSlotReverse((slot) => slot.GetItem() == target);

	//아이템을 추가한 경위
	//원래 바닥에 아이템이 999개 있었습니다.
	//제가 7개 받아올 수 있습니다.
	//제 인벤토리에 추가하고 바닥에 있는 아이템을 삭제한다.
	//아이템 992개는 날아간다.
	//추가하지 못한 개수를 리턴할 것이다!
	public int AddItem(ItemContainer wantItem, int amount = 1)
	{
        //아이템을 추가하는 과정은 어떻게 될까?
        //1.이미 아이템이 있으면 거기에 넣어보고
        amount = AddItemOnExistSlots(wantItem, amount);
        //이미 있는 곳에다가 얹어봤는데, 남은 것이 없다면 끝!
        if (amount <= 0) return 0;
        //2.남은 것을 가장 왼쪽 위에 비어있는 슬롯!
        return AddItemOnEmptySlots(wantItem, amount);
	}

	public int AddItemOnExistSlots(ItemContainer wantItem, int amount)
	{
        foreach (ItemSlot currentSlot in FindFirstItem(wantItem))
        {
            if (amount <= 0) return 0;
            amount = currentSlot.AddItem(wantItem, amount);
            currentSlot.NoticeChanged();
        }

        return amount;
    }

	public int AddItemOnEmptySlots(ItemContainer wantItem, int amount)
	{
        foreach (ItemSlot currentSlot in FindFirstEmptySlot())
        {
            if (amount <= 0) return 0;
            amount = currentSlot.AddItem(wantItem, amount);
            currentSlot.NoticeChanged();
        }

        return amount;
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

	public int RemoveItem(System.Predicate<ItemSlot> condition)
	{
		return default;
	}
    //동일한 모든 아이템을 제거!
    //이 때에는 "몇 개 지우라"고 하지 않았기 때문에 "결과적으로 몇 개 지웠는지"알려주기!
	public int RemoveItem(ItemContainer wantItem)
	{
        int result = 0;
        foreach (ItemSlot currentSlot in FindLastItem(wantItem))
        {
            result += currentSlot.RemoveItem(wantItem);
            currentSlot.NoticeChanged();
        }
        return result;
    }

	public int RemoveItem(ItemContainer wantItem, int amount)
	{
        //제일 낮은 스택부터 시작하시려면 이런 느낌으로 Array.Sort라고 하는 걸 돌리고 시작하기!
        //ItemSlot[] targets = FindLastItem(wantItem).ToArray();
        //                            정렬하는 조건을 람다식으로 만들어야 함!
        //Array.Sort(targets, (a, b) => a.GetStack() < b.GetStack() ? 1 : 0);
        //targets.GetMinimum((ItemSlot current) => current.GetStack());
        foreach (ItemSlot currentSlot in FindLastItem(wantItem))
        {
            if (amount <= 0) return 0;
            amount = currentSlot.RemoveItem(wantItem, amount);
            currentSlot.NoticeChanged();
        }

        return amount;
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
