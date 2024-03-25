using DialogGraph;

namespace Sandbox;

public class TestAppInitializer : AppInitializer
{
    public static void PrepareTestInitializer()
    {
        AppInitializer.Instance = new TestAppInitializer();
    }
    
    protected override void CreateIOCContainer()
    {
        TestIOCContainer.CreateIOCContainer();
    }
}