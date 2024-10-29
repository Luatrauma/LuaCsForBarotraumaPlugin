using Barotrauma;

namespace ClientSource
{
    public class MyContentFile : GenericPrefabFile<MyPrefab>
    {
        public MyContentFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path) { }

        protected override bool MatchesSingular(Identifier identifier) => identifier == "MyPrefab";
        protected override bool MatchesPlural(Identifier identifier) => identifier == "MyPrefabs";
        protected override MyPrefab CreatePrefab(ContentXElement element) => new(this, element);
        protected override PrefabCollection<MyPrefab> Prefabs => MyPrefab.Prefabs;
    }
}