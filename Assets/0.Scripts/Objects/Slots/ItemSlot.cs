using UnityEngine;

//대리자를 만든다!    아이템슬롯이바뀌는걸알려줌(바뀐 슬롯)
public delegate void ItemSlotChangeEvent(ItemSlot changedSlot);

public class ItemSlot
{
	//이 칸에 들어있는 아이템의 정보
	[SerializeField] ItemContainer item;
	//이 칸 만의 정보
	[SerializeField] int currentStack;
    //아이템 슬롯이 바뀌었을 때 일어날 수 있는 이벤트!
    public event ItemSlotChangeEvent OnItemSlotChanged;

    public void NoticeChanged() => OnItemSlotChanged?.Invoke(this);

    public virtual bool Containable(ItemContainer wantItem)
	{
        if (!wantItem)                  return false;    //아이템이 없는데 남는게 어딨어
        // 내가 아이템이 있는데   내 아이템이랑 들어온 아이템이 달라!
        // 내가 아이템이 없으면 상대가 아이템이 있을 때 달라도 들어올 수 있어야 하니까!
        if (item && item != wantItem)   return false;
        // 여기 더 이상 자리가 없는데?
        if (GetIsMax())                 return false;

        return true;
	}
	public ItemContainer GetItem()	=> item;
	public int GetStack()			=> currentStack;
	//                                 조건 ?      맞으면                    : 아니면
	public bool GetIsMax()          => item ? currentStack >= item.maxStack : false;
    public bool GetIsEmpty()        => !item || currentStack <= 0;

    public int Clear()
    {
        item = null; //일단 아이템을 비움!
        int removed = currentStack; //비우기 전에 몇개 있었는지 저장하고
        currentStack = 0; //스택을 비움!
        return removed; //얼마나 비웠는지 리턴할 수 있다!
    }

    //internal : 내부적인! => 나랑 같은 프로젝트에 있는 대상은 모두 쓸 수 있음!
    //반환값 : 추가했더니 못 추가하고 넘겨버린 것!
    public int AddItem(ItemContainer wantItem, int amount)
	{
        if (amount <= 0) return 0;  //준게 없는데 남는게 어딨어
        if (!Containable(wantItem)) return amount; //이것 못 넣겠는데요?
        //아이템은 걍 넣고
        item = wantItem;
        //넣을 수 있는 만큼만 넣어야 해요!
        //                            최대값 - 현재값  추가값 - 추가가능한값
        //최대값 - 현재값   추가값    =  추가가능한값   추가못한값
        //100         98        5   =       2           3
        //                                    가능              가져온
        //                                     10                2     => 2
        //                                     4                 10    => 4
        int stackable = Mathf.Min(item.maxStack - currentStack, amount);
        currentStack += stackable;
		return amount - stackable; //추가하려는값 - 추가한값
	}

    //개수를 지정해주지 않은 경우 반환값 : 몇 개나 지웠는가?
    public int RemoveItem(ItemContainer wantItem)
    {
        //제거하지 않아도 되는 순간?
        //아이템 없잖아!
        if(!wantItem) return 0;
        //나.. 빈털터리야..
        if(GetIsEmpty()) return 0;
        //그건 내가 가지고 있지 않아!
        if (item != wantItem) return 0;
        //슬롯 싹 비우고 개수만 보내줌!
        return Clear();
    }

    public int RemoveItem(ItemContainer wantItem, int amount)
    {
        //제거하지 않아도 되는 순간?
        //지울게 없는데 여기는 왜 온거니?
        if (amount <= 0) return 0;  
        //아이템 없잖아!
        if (!wantItem) return 0;
        //나.. 빈털터리야..
        if (GetIsEmpty()) return amount;
        //그건 내가 가지고 있지 않아!
        if (item != wantItem) return amount;
        //가진것보다 많이 요구하는 경우     요구량 - 지운개수
        if(amount >= currentStack) return amount - Clear();
        //현재 개수에서 원하는 만큼만 빼준다!
        currentStack -= amount;
        //이제 더 지우지 않아도 돼. 내가 다 처리했어.
        return 0;
    }

    public void ExchangeItem(ItemSlot wantSlot)
    {
        //아이템과 현재 스택을 상대와 서로 공유하기!
        if (wantSlot is null) return;
        //값을 서로 바꾸려고 했을 때!
        //프로그래머 2명이 자리를 바꾸는 데에 필요한 의자의 개수는 3개다
        //[a][b][ ]
        //[a][b][b]
        //[a][a][b]
        //[b][a][b]
        //[b][a][ ]
        ItemContainer wasItem = item;
        int wasStack = currentStack;
        //아이템을 가져와보기!
        item = wantSlot.item;
        currentStack = wantSlot.currentStack;
        //제가 원래 가지고 있었던 것으로 상대방을 갱신!
        wantSlot.item = wasItem;
        wantSlot.currentStack = wasStack;
    }

    public void LeftClick(ItemSlot wantSlot)
    {
        if (wantSlot is null) return;
        ExchangeItem(wantSlot);
        NoticeChanged();
        wantSlot.NoticeChanged();
    }
}
