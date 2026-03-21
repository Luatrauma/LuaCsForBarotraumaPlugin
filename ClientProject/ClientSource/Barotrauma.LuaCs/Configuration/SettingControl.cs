using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.LuaCs.Data;
using Microsoft.Xna.Framework.Input;
using OneOf;

namespace Barotrauma.LuaCs.Configuration;

public class SettingControl : SettingBase, ISettingControl
{
    public SettingControl(IConfigInfo configInfo) : base(configInfo)
    {
    }

    protected override void OnDispose()
    {
        OnValueChanged = null;
    }

    public override Type GetValueType() => typeof(KeyOrMouse);
    public override string GetStringValue() => Value.ToString();

    public override string GetDefaultStringValue() => new KeyOrMouse(Keys.NumLock).ToString();

    public override bool TrySetValue(OneOf<string, XElement> value)
    {
        var newVal = value.Match<KeyOrMouse>(
            (string v) => GetKeyOrMouse(v), 
            (XElement e) => e.GetAttributeKeyOrMouse("Value", null));
        
        if (newVal is null)
        {
            return false;
        }

        Value = newVal;
        OnValueChanged?.Invoke(this);
        return true;

        KeyOrMouse GetKeyOrMouse(string strValue)
        {
            strValue ??= string.Empty;
            if (Enum.TryParse(strValue, true, out Microsoft.Xna.Framework.Input.Keys key))
            {
                return key;
            }
            else if (Enum.TryParse(strValue, out MouseButton mouseButton))
            {
                return mouseButton;
            }
            else if (int.TryParse(strValue, NumberStyles.Any, CultureInfo.InvariantCulture, out int mouseButtonInt) &&
                     Enum.GetValues<MouseButton>().Contains((MouseButton)mouseButtonInt))
            {
                return (MouseButton)mouseButtonInt;
            }
            else if (string.Equals(strValue, "LeftMouse", StringComparison.OrdinalIgnoreCase))
            {
                return !PlayerInput.MouseButtonsSwapped() ? MouseButton.PrimaryMouse : MouseButton.SecondaryMouse;
            }
            else if (string.Equals(strValue, "RightMouse", StringComparison.OrdinalIgnoreCase))
            {
                return !PlayerInput.MouseButtonsSwapped() ? MouseButton.SecondaryMouse : MouseButton.PrimaryMouse;
            }

            return null;
        }

    }

    public override event Action<ISettingBase> OnValueChanged;
    public override OneOf<string, XElement> GetSerializableValue() => Value.ToString();
    public KeyOrMouse Value { get; private set; } = new KeyOrMouse(Keys.NumLock);
    
    public bool TrySetValue(KeyOrMouse value)
    {
        Value = value;
        OnValueChanged?.Invoke(this);
        return true;
    }

    public bool IsDown()
    {
        if (this.Value is null)
            return false;
        switch (this.Value.MouseButton)
        {   
            case MouseButton.None:
                return Barotrauma.PlayerInput.KeyDown(this.Value.Key);
            case MouseButton.PrimaryMouse:
                return Barotrauma.PlayerInput.PrimaryMouseButtonHeld();
            case MouseButton.SecondaryMouse:
                return Barotrauma.PlayerInput.SecondaryMouseButtonHeld();
            case MouseButton.MiddleMouse:
                return Barotrauma.PlayerInput.MidButtonHeld();
            case MouseButton.MouseButton4:
                return Barotrauma.PlayerInput.Mouse4ButtonHeld();
            case MouseButton.MouseButton5:
                return Barotrauma.PlayerInput.Mouse5ButtonHeld();
            case MouseButton.MouseWheelUp:
                return Barotrauma.PlayerInput.MouseWheelUpClicked();
            case MouseButton.MouseWheelDown:
                return Barotrauma.PlayerInput.MouseWheelDownClicked();
        }
        return false;
    }
    
    public bool IsHit()
    {
        if (this.Value is null)
            return false;
        switch (this.Value.MouseButton)
        {   
            case MouseButton.None:
                return Barotrauma.PlayerInput.KeyHit(this.Value.Key);
            case MouseButton.PrimaryMouse:
                return Barotrauma.PlayerInput.PrimaryMouseButtonClicked();
            case MouseButton.SecondaryMouse:
                return Barotrauma.PlayerInput.SecondaryMouseButtonClicked();
            case MouseButton.MiddleMouse:
                return Barotrauma.PlayerInput.MidButtonClicked();
            case MouseButton.MouseButton4:
                return Barotrauma.PlayerInput.Mouse4ButtonClicked();
            case MouseButton.MouseButton5:
                return Barotrauma.PlayerInput.Mouse5ButtonClicked();
            case MouseButton.MouseWheelUp:
                return Barotrauma.PlayerInput.MouseWheelUpClicked();
            case MouseButton.MouseWheelDown:
                return Barotrauma.PlayerInput.MouseWheelDownClicked();
        }
        return false;
    }
}
