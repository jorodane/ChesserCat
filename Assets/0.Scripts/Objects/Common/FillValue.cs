using UnityEngine;

public delegate void FillValueChangeEvent(FillValue value);

public struct FillValue
{
	int _current, _min, _max;

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
	public int Min => _min;
	public int Max => _max;
	public float Percent => (float)Current / Max;

	public bool IsEmpty => _current <= Min;
	public bool IsMax => _current >= Max;
	public bool IsUnderZero => _current <= 0;

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

	public int   IncreaseCurrent(int value) => Current += value;
	public int   DecreaseCurrent(int value) => Current -= value;
	public int   SetCurrent(int value)	    => Current  = value;
	public int   SetFull()					=> Current  = Max;
	public int   SetEmpty()					=> Current  = Min;
	public int	 SetPercent(float value)    => Current  = Mathf.CeilToInt(Mathf.Lerp(Min, Max, Mathf.Clamp(value, 0.0f, 1.0f)));
										    
	public void  SetMax(int value)		  { _max = value; Current = Current; }
	public void  SetMin(int value)		  { _min = value; Current = Current; }
}
