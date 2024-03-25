using System.Windows;
using System.Windows.Media;

namespace DialogGraph;

// https://learn.microsoft.com/en-us/dotnet/desktop/wpf/graphics-multimedia/custom-animations-overview?view=netframeworkdesktop-4.8#use-per-frame-callback
public static class CustomAnimator
{
    // private Action<double> callback;
    // private Duration duration;
    // private double totalMillisecondsDuration;
    // private double? startTime = null;
    // private double timePassed;
    //
    // public event Action Completed;

    private static HashSet<BaseCustomAnimation> animations = new HashSet<BaseCustomAnimation>(64);
    private static bool isAnimatorRunning;
    public static TimeSpan RenderingTime { get; private set; }

    static CustomAnimator()
    {
        CompositionTarget.Rendering += OnRendering;
    }
    
    public static bool SubscribeAnimation(BaseCustomAnimation animation)
    {
        animation.InitializeStartRenderTime(RenderingTime);
        var result = animations.Add(animation);
        if (result == false) return false;
        
        if (isAnimatorRunning == false)
        {
            //CompositionTarget.Rendering += OnRendering;
            isAnimatorRunning = true;
        }

        return true;
    }

    public static bool UnsubscribeAnimation(BaseCustomAnimation animation)
    {
        var result = animations.Remove(animation);
        if (result == false) return false;

        if (animations.Count == 0)
        {
            //CompositionTarget.Rendering -= OnRendering;
            isAnimatorRunning = false;
        }

        return true;
    }

    private static void OnRendering(object sender, EventArgs e)
    {
        RenderingTime = ((RenderingEventArgs)e).RenderingTime;
        if (isAnimatorRunning == false) return;
        foreach (var animation in animations)
        {
            animation.CalculateAnimation(sender, (RenderingEventArgs)e);
        }
    }
    
    // public CustomCallbackAnimator(Action<double> callback, Duration duration)
    // {
    //     this.callback = callback;
    //     this.duration = duration;
    //     this.totalMillisecondsDuration = duration.TimeSpan.TotalMilliseconds;
    // }
    //
    // public void BeginAnimation()
    // {
    //     CompositionTarget.Rendering += OnRendering;
    // }
    //
    // public void EndAnimation()
    // {
    //     CompositionTarget.Rendering -= OnRendering;
    // }
    //
    // private void OnRendering(object sender, EventArgs e)
    // {
    //     RenderingEventArgs renderingArgs = (RenderingEventArgs)e;
    //     double totalRenderTime = renderingArgs.RenderingTime.TotalMilliseconds;
    //
    //     if (startTime == null)
    //     {
    //         startTime = totalRenderTime;
    //         return;
    //     }
    //
    //     timePassed = totalRenderTime - startTime.Value;
    //     
    //     //Console.WriteLine("OnRendering timePassed: " + timePassed);
    //     //Console.WriteLine("OnRendering TotalMilliseconds: " + totalMillisecondsDuration);
    //     
    //     double progress;
    //     if (timePassed >= totalMillisecondsDuration)
    //     {
    //         progress = 1;
    //         callback(progress);
    //         Completed?.Invoke();
    //         EndAnimation();
    //         return;
    //     }
    //
    //     progress = timePassed / totalMillisecondsDuration;
    //     callback(progress);
    // }
}

