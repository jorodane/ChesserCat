using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public delegate void SwitchDrawableValueChangeEvent(int index, DrawableBase data);
public delegate void SwitchVariationValueChangeEvent(int index, string varitation);
public delegate DrawableBase GetDrawableFunction(string varitation);

public class UI_SwitchField_Drawable : UI_SwitchField
{
    public event SwitchDrawableValueChangeEvent OnSwitchDrawableValueChanged;
    public event SwitchVariationValueChangeEvent OnSwitchVariationValueChanged;
    public GetDrawableFunction OnGetDrawable;

    [SerializeField] UI_SwitchField variationField;

    DrawableBase _selectedDrawable;
    public DrawableBase SelectedDrawable => _selectedDrawable;

    public void Initialize()
    {
        if(variationField)
        {
            variationField.OnSwitchValueChanged -= RelayVariationValueChanged;
            variationField.OnSwitchValueChanged += RelayVariationValueChanged;
        }
    }

    void RelayVariationValueChanged(int index, string data)
    {
        OnSwitchVariationValueChanged?.Invoke(index, data);
    }

    public void SetContent(DrawableBase drawable)
    {
        _selectedDrawable = drawable;
        OnSwitchDrawableValueChanged?.Invoke(SelectedIndex, SelectedDrawable);
        if (variationField) variationField.SetContent(drawable ? drawable.GetVariationNames(true) : null);
    }

    public override string SetIndex(int index)
    {
        string result = base.SetIndex(index);
        _selectedIndex = index;
        SetContent(FindDrawable(result));
        return result;
    }

    public DrawableBase SetIndex(string fromName, string variationName)
    {
        SetIndex(fromName);
        if(variationField) variationField.SetIndex(variationName);
        return SelectedDrawable;
    }

    public DrawableBase FindDrawable(string data) => OnGetDrawable?.Invoke(data) ?? null;
}
