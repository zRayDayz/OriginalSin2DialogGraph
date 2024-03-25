namespace DialogGraph;

public class CustomHighlightAlgorithmFactoryProvider : ICustomHighlightAlgorithmFactoryProvider
{
    public CustomHighlightAlgorithmFactory CreateFactory()
    {
        return new CustomHighlightAlgorithmFactory();
    }
}