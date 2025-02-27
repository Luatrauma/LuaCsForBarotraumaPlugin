using Barotrauma;
using Barotrauma.Networking;
using Barotrauma.Plugins;
using Microsoft.Xna.Framework;

namespace ExampleMod;

public partial class Plugin
{
    public partial void InitProjectSpecific()
    {
        GameNetwork.RegisterHandlers(this);
    }

    public readonly struct EntityExtraData(EntityEventType eventType) : NetEntityEvent.IData
    {
        public readonly EntityEventType EventType = eventType;
    }

    [NetworkHandler<NetworkHeaders>(NetworkHeaders.RequestEcho)]
    private void HandleEchoRequest(EchoRequestData data, Client client)
    {
        DebugConsole.NewMessage($"Received FooData with value: {data.TestIdentifier} and {data.TestInt}");
        Random random = new Random();
        var respData = new EchoRequestResponse(SerializableDateTime.LocalNow, Option.Some(new Vector2(random.NextSingle(), random.NextSingle())));
        GameNetwork.SendToClient(client, NetworkHeaders.RespondEcho, respData);
    }

    [NetworkHandler<NetworkHeaders>(NetworkHeaders.RequestEntityEvents)]
    private void OnClientRequestEntityEvents(ClientRequestEntityEventData data, Client client)
    {
        // if (!Client.CanAccessSomething()) { return; }
        foreach (Item item in Item.ItemList)
        {
            if (data.Identifier.Contains(item.Prefab.Identifier))
            {
                GameNetwork.CreateEntityEvent(item, new EntityExtraData(EntityEventType.SendHealthAndIdentifier));
            }
        }
    }
}