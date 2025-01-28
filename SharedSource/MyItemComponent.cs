using Barotrauma;
using Barotrauma.Items.Components;

namespace ExampleMod;

public class MyItemComponent(Item item, ContentXElement element) : ItemComponent(item, element)
{
    [Serialize("", IsPropertySaveable.Yes, description: "My custom property")]
    public int MyProperty { get; set; }
}