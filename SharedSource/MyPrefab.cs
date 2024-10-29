using Barotrauma;

namespace ClientSource
{
    public class MyPrefab : Prefab
    {

        public static readonly PrefabCollection<MyPrefab> Prefabs = new();

        public int MyProperty { get; set; }

        public MyPrefab(ContentFile file, ContentXElement element) : base(file, element.GetAttributeIdentifier("identifier", Identifier.Empty))
        {
            MyProperty = element.GetAttributeInt("myproperty", 0);
        }

        public override void Dispose()
        {
            // do nothing
        }
    }
}