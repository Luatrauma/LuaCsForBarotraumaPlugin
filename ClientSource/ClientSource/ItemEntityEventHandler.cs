using Barotrauma;
using Barotrauma.Networking;
using Barotrauma.Plugins;

namespace ExampleMod;

public class ItemEntityEventHandler : ICustomSerializable<Item>
{
    public void ClientWrite(Item entity, IWriteMessage msg, NetEntityEvent.IData? extraData)
    {
        // do nothing
    }

    public void ClientRead(Item entity, IReadMessage msg, float sendingTime)
    {
        Plugin.EntityEventType eventType = ((Plugin.EntityEventType)msg.ReadByte());
        switch (eventType)
        {
            case Plugin.EntityEventType.SendHealthAndIdentifier:
                Identifier id = msg.ReadIdentifier();
                float condition = msg.ReadSingle();
                Plugin.DebugConsole.NewMessage($"Received item with identifier: {id} and condition: {condition}");
                break;
            default:
                throw new Exception($"Unknown entity event type: {eventType}");
        }
    }
}