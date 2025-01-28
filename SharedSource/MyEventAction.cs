using Barotrauma;

namespace ExampleMod;

public class MyEventAction(ScriptedEvent parentEvent, ContentXElement element) : EventAction(parentEvent, element)
{
    private bool isFinished;

    public override bool IsFinished(ref string goToLabel)
        => isFinished;

    public override void Reset()
        => isFinished = false;

    public override void Update(float deltaTime)
    {
        // Do something
        isFinished = true;
    }
}