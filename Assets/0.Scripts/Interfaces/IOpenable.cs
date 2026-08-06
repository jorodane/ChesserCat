using UnityEngine;

public interface IOpenable
{
    //ISP => Interface Segragation Principle => 인터페이스 분리 원칙
    public bool IsOpen { get; }
    public bool IsNeedClose { get; }
    public void Open(bool isActiveByKey); //isOpenable만 있는 경우 : 숏컷
    public void Close(bool isActiveByKey); //isClosable : 봉인
    public bool Toggle(bool isActiveByKey); //isTogglable : 레버
    public void SetOpen(bool newOpen, bool isActiveByKey);
}