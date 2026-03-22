using System;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.LuaCs.Data;
using Barotrauma.LuaCs;
using Barotrauma.Networking;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Xna.Framework;
using OneOf;

namespace Barotrauma.LuaCs.Data;

public partial class SettingEntry<T> : SettingBase, ISettingBase<T>, INetworkSyncVar where T : IEquatable<T>, IConvertible
{
    public class Factory : ISettingBase.IFactory<ISettingBase<T>>
    {
        public ISettingBase<T> CreateInstance(IConfigInfo configInfo, Func<OneOf<string, XElement, object>, bool> valueChangePredicate)
        {
            Guard.IsNotNull(configInfo, nameof(configInfo));
            return new SettingEntry<T>(configInfo, valueChangePredicate);
        }
    }
    
    public SettingEntry(IConfigInfo configInfo, 
        Func<OneOf<string, XElement, object>, bool> valueChangePredicate) 
        : base(configInfo)
    {
        if (!( 
                typeof(T).IsEnum || 
                typeof(T).IsPrimitive || 
                typeof(T) == typeof(string)))
        {
            ThrowHelper.ThrowArgumentException($"{nameof(ISettingBase<T>)}: The type of {nameof(T)} is not an allowed type.");
        }
        ValueChangePredicate = valueChangePredicate;
        
        try
        {
            Value = (T)Convert.ChangeType(ConfigInfo.Element.GetAttributeString("Value", null), typeof(T));
            DefaultValue = Value;
        }
        catch (Exception e) when (e is InvalidCastException or ArgumentNullException)
        {
            Value = default(T);
            DefaultValue = default(T);
        }
    }

    protected Func<OneOf<string, XElement, object>, bool> ValueChangePredicate;
    public T Value { get; protected set; }
    
    public T DefaultValue { get; protected set; }

    public virtual bool TrySetValue(T value)
    {
#if CLIENT
        if (SyncType is NetSync.ServerAuthority && NetworkingService is not null 
                                                && GameMain.IsMultiplayer
                                                && !GameMain.Client.HasPermission(this.WritePermissions))
        {
            return false;
        }
#endif
        
        if (!TrySetValueInternal(value))
        {
            return false;
        }
        OnValueChanged?.Invoke(this);
#if CLIENT
        if (GameMain.IsMultiplayer && SyncType is NetSync.ClientOneWay or NetSync.TwoWay)
        {
            NetworkingService?.SendNetVar(this);
        }
#elif SERVER
        if (GameMain.IsMultiplayer && SyncType is NetSync.TwoWay or NetSync.ServerAuthority)
        {
            NetworkingService?.SendNetVar(this);
        }
#endif
        return true;
    }

    private bool TrySetValueInternal(T value)
    {
        if (value is null)
        {
            return false;
        }

        if (ValueChangePredicate != null && !ValueChangePredicate(value))
        {
            return false;
        }
        
        Value = value;
        return true;
    }

    protected override void OnDispose()
    {
        ValueChangePredicate = null;
    }

    public override Type GetValueType() => typeof(T);

    public override string GetStringValue() => Value.ToString();
    
    public override string GetDefaultStringValue() => DefaultValue.ToString();

    public override bool TrySetValue(OneOf<string, XElement> value)
    {
        bool isFailed = false;
        var typeConvertedValue = value.Match<T>(
            (string val) =>
            {
                try
                {
                    return (T)Convert.ChangeType(val, typeof(T));
                }
                catch (Exception e)
                {
                    // ignored
                    isFailed = true;
                    return default(T);
                }
            },
            (XElement val) =>
            {
                try
                {
                    return (T)Convert.ChangeType(val.GetAttributeString("Value", null), typeof(T));
                }
                catch (Exception e)
                {
                    isFailed = true;
                    return default(T);
                }
            });

        return !isFailed && TrySetValue(typeConvertedValue);
    }

    public override event Action<ISettingBase> OnValueChanged;

    public override OneOf<string, XElement> GetSerializableValue() => Value.ToString();
    
    // -- Networking
    protected IEntityNetworkingService NetworkingService;
    public Guid InstanceId => NetworkingService?.GetNetworkIdForInstance(this) ?? Guid.Empty;
    public void SetNetworkOwner(IEntityNetworkingService networkingService)
    {
        NetworkingService = networkingService;
        if (NetworkingService is null)
        {
            return;
        }
        NetworkingService.RegisterNetVar(this);
    }

    public NetSync SyncType => ConfigInfo?.NetSync ?? NetSync.None;
    // needs to be added IConfigInfo
    public ClientPermissions WritePermissions => ClientPermissions.ManageSettings;
    
    public void ReadNetMessage(IReadMessage message)
    {
        if (SyncType == NetSync.None || NetworkingService is null)
        {
            return;
        }
        
        try
        {
            if (typeof(T).IsEnum)
            {
                TrySetValueInternal((T)(object)message.ReadInt32());
            }
            
            // No...there's no better way to do this...
            var typeCode = Type.GetTypeCode(typeof(T));
            switch (typeCode)
            {
                 case TypeCode.Boolean:
                     TrySetValueInternal((T)Convert.ChangeType(message.ReadBoolean(), typeCode));
                     return;
                 case TypeCode.Byte:
                     TrySetValueInternal((T)Convert.ChangeType(message.ReadByte(), typeCode));
                     return;
                 // SByte not supported by interface
                 case TypeCode.SByte:
                     TrySetValueInternal((T)Convert.ChangeType(message.ReadInt16(), typeCode));
                     return;
                 case TypeCode.Int16:
                     TrySetValueInternal((T)Convert.ChangeType(message.ReadInt16(), typeCode));
                     return;
                 case TypeCode.Char:
                 case TypeCode.UInt16:
                     TrySetValueInternal((T)Convert.ChangeType(message.ReadUInt16(), typeCode));
                     return;
                 case TypeCode.Int32:
                     TrySetValueInternal((T)Convert.ChangeType(message.ReadInt32(), typeCode));
                     return;
                 case TypeCode.UInt32:
                     TrySetValueInternal((T)Convert.ChangeType(message.ReadUInt32(), typeCode));
                     return;
                 case TypeCode.Int64:
                     TrySetValueInternal((T)Convert.ChangeType(message.ReadInt64(), typeCode));
                     return;
                 case TypeCode.UInt64:
                     TrySetValueInternal((T)Convert.ChangeType(message.ReadUInt64(), typeCode));
                     return;
                 case TypeCode.Single:
                     TrySetValueInternal((T)Convert.ChangeType(message.ReadSingle(), typeCode));
                     return;
                 case TypeCode.Double:
                     TrySetValueInternal((T)Convert.ChangeType(message.ReadDouble(), typeCode));
                     return;
                 case TypeCode.String:
                     TrySetValueInternal((T)Convert.ChangeType(message.ReadString(), typeCode));
                     return;
                 case TypeCode.Decimal: 
                 default:
                     ThrowHelper.ThrowNotSupportedException($"{nameof(SettingEntry<T>)}: The type {typeof(T).Name} is not supported.");
                     break;
            }
        }
        catch (Exception e)
        {
            // Suppress unless we're testing.
#if DEBUG
            throw;
#endif
        }
    }

    public void WriteNetMessage(IWriteMessage message)
    {
        if (SyncType == NetSync.None || NetworkingService is null)
        {
            return;
        }
        
        try
        {
            if (typeof(T).IsEnum)
            {
                message.WriteInt32((int)((IConvertible)Value));
            }
            
            // No...there's no better way to do this...
            var typeCode = Type.GetTypeCode(typeof(T));
            switch (typeCode)
            {
                 case TypeCode.Boolean:
                     message.WriteBoolean((bool)Convert.ChangeType(Value, typeCode)!);
                     return;
                 case TypeCode.Byte:
                     message.WriteByte((byte)Convert.ChangeType(Value, typeCode)!);
                     return;
                 // SByte not supported by interface
                 case TypeCode.SByte:
                     message.WriteInt16((short)Convert.ChangeType(Value, typeCode)!);
                     return;
                 case TypeCode.Int16:
                     message.WriteInt16((short)Convert.ChangeType(Value, typeCode)!);
                     return;
                 case TypeCode.Char:
                 case TypeCode.UInt16:
                     message.WriteUInt16((ushort)Convert.ChangeType(Value, typeCode)!);
                     return;
                 case TypeCode.Int32:
                     message.WriteInt32((int)Convert.ChangeType(Value, typeCode)!);
                     return;
                 case TypeCode.UInt32:
                     message.WriteUInt32((uint)Convert.ChangeType(Value, typeCode)!);
                     return;
                 case TypeCode.Int64:
                     message.WriteInt64((long)Convert.ChangeType(Value, typeCode)!);
                     return;
                 case TypeCode.UInt64:
                     message.WriteUInt64((ulong)Convert.ChangeType(Value, typeCode)!);
                     return;
                 case TypeCode.Single:
                     message.WriteSingle((float)Convert.ChangeType(Value, typeCode)!);
                     return;
                 case TypeCode.Double:
                     message.WriteDouble((double)Convert.ChangeType(Value, typeCode)!);
                     return;
                 case TypeCode.String:
                     message.WriteString((string)Convert.ChangeType(Value, typeCode)!);
                     return;
                 case TypeCode.Decimal: 
                 default:
                     ThrowHelper.ThrowNotSupportedException($"{nameof(SettingEntry<T>)}: The type {typeof(T).Name} is not supported.");
                     break;
            }
        }
        catch (Exception e)
        {
            // Suppress unless we're testing.
#if DEBUG
            throw;
#endif
        }
    }

#if CLIENT
    public override void AddDisplayComponent(GUILayoutGroup layoutGroup, Vector2 relativeSize, Action<string> onSerializedValue)
    {
        switch (Type.GetTypeCode(typeof(T)))
        {
            case TypeCode.Boolean:
                new GUITickBox(new RectTransform(relativeSize, layoutGroup.RectTransform), "")
                {
                    Selected = (bool)Convert.ChangeType(this.Value, TypeCode.Boolean),
                    OnSelected = (box) =>
                    {
                        onSerializedValue?.Invoke(box.Selected.ToString());
                        return true;
                    }
                };
                break;
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.Int16:
            case TypeCode.Char:
            case TypeCode.UInt16:
            case TypeCode.Int32:
            case TypeCode.UInt32:
            case TypeCode.Int64:
            case TypeCode.UInt64:
                new GUINumberInput(new RectTransform(relativeSize, layoutGroup.RectTransform), NumberType.Int)
                {
                    IntValue = (int)Convert.ChangeType(this.Value, TypeCode.Int32)!,
                    OnValueChanged = (num) =>
                    {
                        onSerializedValue?.Invoke(num.IntValue.ToString());
                    }
                };
                break;
            case TypeCode.Single:
            case TypeCode.Double:
                new GUINumberInput(new RectTransform(relativeSize, layoutGroup.RectTransform), NumberType.Float)
                {
                    FloatValue = (float)Convert.ChangeType(this.Value, TypeCode.Single)!,
                    OnValueChanged = (num) =>
                    {
                        onSerializedValue?.Invoke(num.FloatValue.ToString());
                    }
                };
                break;
            case TypeCode.String:
                default:
                base.AddDisplayComponent(layoutGroup, relativeSize, onSerializedValue);
                break;
        }
    }
#endif
}
