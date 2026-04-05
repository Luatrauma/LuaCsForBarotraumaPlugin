using Barotrauma;
using Barotrauma.LuaCs;
using Barotrauma.Plugins;
using System.Xml.Linq;

internal class StatusEffectHookAction : IStatusEffectAction
{
    private readonly IEventService _eventService;
    private readonly XElement _element;
    private StatusEffect _statusEffect;
    private readonly string _hookIdentifier;

    public StatusEffectHookAction(XElement element, StatusEffect statusEffect, IEventService eventService)
    {
        _element = element;
        _eventService = eventService;
        _statusEffect = statusEffect;
        _hookIdentifier = element.GetAttributeString("name", "");
    }

    public void Apply(StatusEffectParams effectParams)
    {
        _eventService.Call(_hookIdentifier, _statusEffect, effectParams.DeltaTime, effectParams.Entity, effectParams.Targets, effectParams.WorldPosition!, _element);
    }
}

internal class StatusEffectService : ISystem, IStatusEffectActionFactory
{
    public bool IsDisposed { get; private set; }

    private readonly IEventService _eventService;


    public StatusEffectService(IEventService eventService)
    {
        _eventService = eventService;

        Plugin.StatusEffectService.RegisterAction(this);
    }

    public void Dispose()
    {
        IsDisposed = true;
    }

    public FluentResults.Result Reset()
    {
        return FluentResults.Result.Ok();
    }

    public bool SupportsElement(XElement element)
    {
        Identifier name = element.NameAsIdentifier();
        return name == "hook" || name == "luahook";
    }

    public IStatusEffectAction Create(StatusEffect statusEffect, XElement element)
    {
        return new StatusEffectHookAction(element, statusEffect, _eventService);
    }
}
