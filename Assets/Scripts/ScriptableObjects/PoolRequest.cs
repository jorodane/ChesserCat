using UnityEngine;

//                            기본 파일명                       메뉴 위치
[CreateAssetMenu(fileName = "PoolRequest", menuName = "PoolRequests/DefaultPoolRequest")]
public class PoolRequest : ScriptableObject
{
	public PoolSetting[] settings;
}
