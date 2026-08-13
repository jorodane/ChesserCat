using UnityEngine;
using static UnityEngine.UI.Image;

public delegate void FillValueChangeEvent(in FillValue value, int delta, bool isAnimation);

[System.Serializable]
public struct FillValue
{
	[SerializeField] int _current;

	public event FillValueChangeEvent OnChanged;
	int Current
	{
		readonly get => _current;
		set => _current = Mathf.Clamp(value, 0, Max);
	}
	[SerializeField] int _max;
	public int Max
	{
		readonly get => _max;
		set
		{
			_max = Mathf.Max(value, 0);
			Current = Current;
		}
	}

	public readonly float Percent => (float)Current / Max;

	public readonly bool IsEmpty => _current <= 0;
	public readonly bool IsMax => _current >= Max;

	public FillValue(int current, int max)
	{
		_max = max;
		_current = Mathf.Clamp(current, 0, _max);
		OnChanged = null;
	}
	public FillValue(int max)
	{
		_current = _max = max;
		OnChanged = null;
	}

	public int	 IncreaseCurrent(int value)
	{
		int lastValue = Current;
		Current += value;
		return Current - lastValue;
	}
	public int   DecreaseCurrent(int value)
	{
		int lastValue = Current;
		Current -= value;
		return lastValue - Current;
	}
	public int   GetCurrent()	            => Current;
    public int   SetCurrent(int value, bool isAnimation)
    {
        int origin = Current;
        Current = value;
        OnChanged?.Invoke(this, Current - origin, isAnimation);
        return Current;
    }
	public int   SetFull()					=> Current  = Max;
	public int   SetEmpty()					=> Current  = 0;
	public int	 SetPercent(float value)    => Current  = Mathf.CeilToInt(Mathf.Lerp(0, Max, Mathf.Clamp(value, 0.0f, 1.0f)));
	public void  SetMax(int value)		    => Max = value;
}
