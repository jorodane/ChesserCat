using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    //인벤토리에서 static으로 만들긴 할 건데!
    //주의할 점!
    //static은 해당 프로그램이 종료될 때까지 유지!
    //인게임 플레이가 종료되거나 세이브되거나 다시 시작하거나 등등
    //다채로운 상황에서 얘를 관리해주셔야 함!
    public static ItemSlot cursorSlot = new ItemSlot();

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
        for (int row = 0; row < rows; row++)
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
    readonly string[] itemList = { "LesserHealPotion", "Cake", "HotChoco", "SantaHat", "WitchHat" };
    public void HealPotionPlus(int amount) //나중에 꼭 지우지 않으면 죽여버리겠다
    {
        int index = Random.Range(0, itemList.Length);
        ItemContainer potion = DataManager.LoadDataFile<ItemContainer>(itemList[index]);
        AddItem(potion, amount);
    }

    public void HealPotionMinus(int amount) //나중에 꼭 지우지 않으면 죽여버리겠다
    {
        ItemContainer potion = DataManager.LoadDataFile<ItemContainer>("LesserHealPotion");
        RemoveItem(potion, amount);
    }

    public bool IsEmpty(ItemSlot target) => target?.GetIsEmpty() ?? false;


    //Comparison의 반환값 :
    //  마이너스 : 왼쪽이 작다
    //  0       : 같다
    //  플러스   : 왼쪽이 크다
    public void Sort(System.Comparison<ItemSlot> Method)
    {
        //배열 자체를 정렬할 수는 없다.
        //비교를 했을때 내용만 바꿔준다!
        //사다리 타기로 버블 정렬이 걸렸으니까!
        int totalLength = slots.Length;
        if (slots is null || totalLength <= 1) return;
        int width = slots.GetLength(1);

        //                 맨 마지막 친구는 돌 필요가 없음! (바로 앞 친구가 나를 봐줬을 것이기 때문!)
        int lastFinder = totalLength - 1;

        while (lastFinder > 0)
        {
            int currentFinder = -1; //어디까지 갔나? 를 체크하는 변수!
            for (int i = 0; i < lastFinder; i++)
            {
                ItemSlot left = GetSlot(i, width);
                ItemSlot right = GetSlot(i + 1, width);
                int comparisonResult = Method(left, right); //둘이 비교하고
                                                            //결과가 + (오른쪽이 더 큼) 왼쪽 오른쪽 아이템을 바꾼다!
                                                            //if (comparisonResult > 0) //왼쪽이 더 클 때 바꾼다 = 오름차순으로 정렬
                if (comparisonResult < 0) //왼쪽이 더 작을 때 바꾼다 = 내림차순으로 정렬
                {
                    currentFinder = i; //나 마지막에 바뀐 거야! 마지막으로 바뀐 위치를 갱신!
                    left.ExchangeItem(right);
                }
            }
            //지금 도는 동안에 lastFinder가 초기화될 필요가 있다!
            lastFinder = currentFinder;
        }

        //Sort함수의 반복문이 끝난 뒤! 한 번만 그래픽 업데이트를 할 것이다!
        foreach (ItemSlot currentSlot in GetAllSlot())
        {
            currentSlot?.NoticeChanged(); //너, 바뀐 거야!
        }
    }

    int ItemTypeComparison(ItemSlot left, ItemSlot right)
    {
        int result;
        if (ItemExistComparison(left, right, out result)) return result;

        ItemContainer leftItem = left.GetItem();
        ItemContainer rightItem = right.GetItem();

        //여기서 정할 수 있는 것은 "기본 정보"를 가지고만 비교할 수 있음!
        result = leftItem.CompareByType(rightItem);
        if (result != 0) return result;
        //아이템 자체가 똑같다면 들어있는 개수를 기준으로 내림차순!
        result = left.GetStack() - right.GetStack();
        return result;
    }

    int? ItemExistComparison(ItemSlot left, ItemSlot right)
    {
        if (left is null)
        {
            if (right is null) return 0;
            else return -1;
        }
        if (right is null) return 1;
        ItemContainer leftItem = left.GetItem();
        ItemContainer rightItem = right.GetItem();
        if (!leftItem)
        {
            if (!rightItem) return 0;
            else return -1;
        }
        if (!rightItem) return 1;

        return null;
    }

    bool ItemExistComparison(ItemSlot left, ItemSlot right, out int result)
    {
        int? calculated = ItemExistComparison(left, right); //원래 함수를 실행하고
        result = calculated ?? 0; //결과를 저장하는데 결과가 없으면 0
        return calculated.HasValue;//값이 나왔는지 여부를 반환
    }

    public void SortByType() => Sort(ItemTypeComparison);

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
        if (!wantItem) return 0;
        int result = 0;
        foreach (ItemSlot currentSlot in FindFirstItem(wantItem))
        {
            result += currentSlot.GetStack();
        }
        return result;
    }

    public int CountItem(ItemContainer wantItem, out List<ItemSlot> returnSlots)
    {
        returnSlots = new();
        if (!wantItem) return 0;

        int result = 0;
        //해당 아이템을 가지고 있는 슬롯들을 모두 찾아와서
        foreach (ItemSlot currentSlot in FindFirstItem(wantItem))
        {
            //리스트에 넣어주고
            returnSlots.Add(currentSlot);
            //개수에다가 지금 보고 있는 슬롯의 개수를 더해준다!
            result += currentSlot.GetStack();
        }
        return result;
    }

    //예외처리 해놓은 거겠지?
    public ItemSlot GetSlot(int index, int width) => slots[index / width, index % width];
    public ItemSlot GetSlot(int index)
    {
        if (slots is null || index < 0 || slots.Length == 0 || slots.Length <= index) return null;
        int width = slots.GetLength(1);
        //               행 (몫)      열 (나머지)     
        return slots[index / width, index % width];
        //1차원 배열을 가지고 있다!
        //return slots[index];
        //2차원 배열이면 어떻게 해야 할까?
        //2차원 배열에서 "다음행"으로 이동은 몇을 더하는 행동?
        //0 1 2
        //3 4 5
        //6 7 8
        //다음행으로 이동 = 현재 위치에서 열의 길이를 더하기!
        //7이라는 숫자는 몇행 몇열일까?
        //              2    1
        //3이라는 숫자는 1    0
        //7 / 3 = 2 ..  1
        //3 / 3 = 1 ..  0
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
        int width = slots.GetLength(1);
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
        if (pred is null) yield break;
        foreach (ItemSlot currentSlot in GetAllSlot())
        {
            if (pred(currentSlot)) yield return currentSlot;
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
        if (wantRow < 0 || wantColumn < 0) return null;
        if (wantRow >= slots.GetLength(0)) return null;
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

    //아이템 병합
    public void MergeItem(ItemContainer wantItem)
    {
        if (!wantItem) return; //아이템이 없다?
        if (wantItem.maxStack <= 1) return; //이거.. 못합치는데?
        //종합 개수!
        int totalCount = CountItem(wantItem, out List<ItemSlot> containSlots);
        //  들어있는 슬롯이 없거나    슬롯이 다해서 1개밖에 없거나
        if (containSlots is null || containSlots.Count <= 1) return;
        //모든 슬롯을 돌아주면서
        for(int i = 0; i < containSlots.Count; i++)
        {
            ItemSlot currentSlot = containSlots[i];
            if (currentSlot.GetIsMax()) continue; //꽉 찬 슬롯은 병합할 필요가 없으니까 패스!
        }
    }

    public void ExchangeItem(int startRow, int startColumn, int targetRow, int targetColumn)
    {
        ExchangeItem(startRow, startColumn, this, targetRow, targetColumn);
    }
    public void ExchangeItem(int startRow, int startColumn, ItemSlot targetSlot)
    {
        if (targetSlot is null) return; //대상이 없습니다!
        //일단 내 거!
        ItemSlot first = FindItem(startRow, startColumn);
        if (first is null) return; //슬롯이 없는데?
        first.ExchangeItem(targetSlot);
        first.NoticeChanged();
        targetSlot.NoticeChanged();
    }

    public void ExchangeItem(int startRow, int startColumn, Inventory targetInventory, int targetRow, int targetColumn)
    {
        if (!targetInventory) return; //인벤토리가 없는데?
        ExchangeItem(startRow, startColumn, targetInventory.FindItem(targetRow, targetColumn));
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
