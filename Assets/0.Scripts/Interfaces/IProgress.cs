using UnityEngine;

//IProgress는 Integer로 갈 수도 있고, Floating Point로 갈 수도 있고
//몬스터한테 물어보기
//자료형을.. 자유롭게 쓸 수 있다구요? => 제네릭 메소드!
//제네릭 클래스
//     interface는 패턴이지 실제 있는 무언가가 아닙니다. 사실 클래스가 맞아요 ㅎㅎ
//     C#에서는 규칙으로 등록해놓은 형태!
public interface IProgress<T>
{
	public T Current { get; }
	public T Max { get; }

	public float Progress { get; }

	public void SetComplete(ScreenType openScreen, ScreenChangeType changeType);

	public T Set(T newCurrent);
	public T Set(T newCurrent, T newMax);

	public T AddCurrent(T value);
	public T AddMax(T value);
}