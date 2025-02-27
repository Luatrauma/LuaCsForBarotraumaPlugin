using Barotrauma;
using Barotrauma.Networking;
using Barotrauma.Plugins;

namespace ExampleMod;

public class ItemEntityEventHandler : ICustomSerializable<Item>
{
    public void ServerWrite(Item entity, IWriteMessage msg, Client client, NetEntityEvent.IData? extraData)
    {
        if (extraData is not Plugin.EntityExtraData data) { return; }

        msg.WriteByte((byte)data.EventType);
        switch (data.EventType)
        {
            case Plugin.EntityEventType.SendHealthAndIdentifier:
                msg.WriteIdentifier(entity.Prefab.Identifier);
                msg.WriteSingle(entity.Health);
                break;
            default:
                throw new NotImplementedException();
        }
    }

    public void ServerRead(Item entity, IReadMessage msg, Client client)
    {
        // do nothing
    }
}