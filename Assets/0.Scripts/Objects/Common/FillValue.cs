using UnityEngine;

public delegate void FillValueChangeEvent(in FillValue value);

[System.Serializable]
public struct FillValue
{
	[SerializeField] int _current;

	public event FillValueChangeEvent OnChanged;
	public int Current
	{
		readonly get => _current;
		set
		{
			_current = Mathf.Clamp(value, Min, Max);
			OnChanged?.Invoke(this);
		}
	}
	[SerializeField] int _max;
	public int Max
	{
		readonly get => _max;
		set
		{
			_max = Mathf.Max(value, Min);
			Current = Current;
		}
	}
	[SerializeField] int _min;
	public int Min
	{
		readonly get => _min;
		set
		{
			_min = Mathf.Min(value, Max);
			Current = Current;
		}
	}

	public readonly float Percent => (float)Current / Max;

	public readonly bool IsEmpty => _current <= Min;
	public readonly bool IsMax => _current >= Max;
	public readonly bool IsUnderZero => _current <= 0;

	public FillValue(int current, int max, int min = 0)
	{
		_max = max;
		_min = min;
		_current = Mathf.Clamp(current, _min, _max);
		OnChanged = null;
	}
	public FillValue(int max)
	{
		_current = _max = max;
		_min = 0;
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
	public int   SetCurrent(int value)	    => Current  = value;
	public int   SetFull()					=> Current  = Max;
	public int   SetEmpty()					=> Current  = Min;
	public int	 SetPercent(float value)    => Current  = Mathf.CeilToInt(Mathf.Lerp(Min, Max, Mathf.Clamp(value, 0.0f, 1.0f)));
	public void  SetMax(int value)		    => Max = value;
	public void  SetMin(int value)		    => Min = value;
}
