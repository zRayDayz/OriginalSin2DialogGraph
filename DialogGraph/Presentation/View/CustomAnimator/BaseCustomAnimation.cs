using System.Windows.Media;

namespace DialogGraph;

public abstract class BaseCustomAnimation
{
    public abstract bool BeginAnimation();
    public abstract void CalculateAnimation(object sender, RenderingEventArgs e);

    public abstract void InitializeStartRenderTime(TimeSpan startRenderTime);
}
