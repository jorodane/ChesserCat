using UnityEngine;

public interface IOpenable
{
	//ISP => Interface Segragation Principle => 인터페이스 분리 원칙
	public bool IsOpen { get; }
	public void Open();
	public void Close();
	public void Toggle();
}
