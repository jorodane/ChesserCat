using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public delegate void SwitchValueChangeEvent(int index, string data);

public class UI_SwitchField : MonoBehaviour
{
	public event SwitchValueChangeEvent OnSwitchValueChanged;

	public TextMeshProUGUI switchedText;

	public string[] datas;
	protected string	_selectedData;
	public string	SelectedData => _selectedData;
	protected int		_selectedIndex;
	public int SelectedIndex => _selectedIndex;

	void OnEnable()
	{
		SetIndex(_selectedIndex);
	}

	public void SetContent(IEnumerable<string> newContents)
	{
		if(newContents is null) datas = null;
		else datas = newContents.ToArray(); 
		SetIndex(0);
	}

	public virtual string SetIndex(int index)
	{
		if (datas is null || datas.Length == 0)
		{
			_selectedData = string.Empty;
			UpdateText();
			return _selectedData;
		}
		_selectedIndex = index < 0 ? (index + datas.Length) % datas.Length : index % datas.Length;
		_selectedData = datas[_selectedIndex];
		UpdateText();
		OnSwitchValueChanged?.Invoke(_selectedIndex, _selectedData);
		return _selectedData;
    }

    public string SetIndex(string fromName)
	{
		if (datas is null) return null;
		if(string.IsNullOrEmpty(fromName)) SetIndex(0);
		for(int i = 0; i < datas.Length; i++)
		{
			if(datas[i] == fromName)
			{
				return SetIndex(i);
			}
		}
		return SetIndex(0);
	}

	public void NextIndex() => SetIndex(_selectedIndex + 1);
	public void PrevIndex() => SetIndex(_selectedIndex - 1);

	public void UpdateText()
	{
		if (!switchedText) return;
		string showingText = string.IsNullOrEmpty(_selectedData) ? "None" : _selectedData;
		switchedText.SetText(showingText.Translate());
	}

}
