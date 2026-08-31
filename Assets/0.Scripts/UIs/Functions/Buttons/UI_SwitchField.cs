using TMPro;
using UnityEngine;

public delegate void SwitchValueChangeEvent(int index, string data);

public class UI_SwitchField : MonoBehaviour
{
	public event SwitchValueChangeEvent OnSwitchValueChanged;

	public TextMeshProUGUI switchedText;

	public string[] datas;
	string	_selectedData;
	public string	SelectedData => _selectedData;
	int		_selectedIndex;
	public int SelectedIndex => _selectedIndex;

	void OnEnable()
	{
		SetIndex(_selectedIndex);
	}

	public void SetIndex(int index)
	{
		if (datas is null || datas.Length == 0)
		{
			_selectedData = string.Empty;
			UpdateText();
			return;
		}
		_selectedIndex = index < 0 ? (index + datas.Length) % datas.Length : index % datas.Length;
		_selectedData = datas[_selectedIndex];
		UpdateText();
		OnSwitchValueChanged?.Invoke(_selectedIndex, _selectedData);
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
