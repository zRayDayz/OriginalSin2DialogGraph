using System.Windows;
using System.Windows.Media;

namespace DialogGraph;

// This class was created to reduce the large number of memory allocations in standard WPF double animation
// The class can use a new "toValue" to adjust its animation at runtime, when the WPF double animation cannot do so (with its "To" property) because it is used as a Freezable object
public class CustomDoubleAnimation : BaseCustomAnimation
{
    private DependencyObject dependencyPropertyOwner;
    private DependencyProperty dependencyProperty;
    private double fromValue;
    private double toValue;
    private Duration duration;
    
    private double totalMillisecondsDuration;
    private double? startTime = null;
    private double timePassed;
    double? lastFromValue = null;
    
    public event Action Completed;
    
    public CustomDoubleAnimation(DependencyObject dependencyPropertyOwner, DependencyProperty dependencyPropertyForUpdate, double fromValue, double toValue, Duration duration)
    {
        if (dependencyPropertyForUpdate.PropertyType != typeof(double)) throw new ArgumentException($"Wrong 'dependencyProperty' type. Expected: {typeof(double).FullName}. Got: {dependencyPropertyForUpdate.PropertyType.FullName}");
        
        this.dependencyPropertyOwner = dependencyPropertyOwner;
        this.dependencyProperty = dependencyPropertyForUpdate;
        this.fromValue = fromValue;
        this.toValue = toValue;
        this.duration = duration;
        this.totalMillisecondsDuration = duration.TimeSpan.TotalMilliseconds;
    }

    public override void InitializeStartRenderTime(TimeSpan startRenderTime)
    {
        startTime = startRenderTime.TotalMilliseconds;
    }

    public override bool BeginAnimation()
    {
        return CustomAnimator.SubscribeAnimation(this);
    }

    public bool EndAnimation()
    {
        Completed?.Invoke();
        return CustomAnimator.UnsubscribeAnimation(this);
    }

    public void ExtendAnimation(double toValue, bool resetDurationTime = true)
    {
        // The new "fromValue" will basically jump during the next interpolation method call, this is not smooth unlike "HandoffBehavior.Compose" in the standard WPF animation, but it works fast
        this.fromValue = (double)dependencyPropertyOwner.GetValue(dependencyProperty);
        this.toValue = toValue;
        if (resetDurationTime)
        {
            var durationMilliseconds = duration.TimeSpan.TotalMilliseconds;
            totalMillisecondsDuration = timePassed + durationMilliseconds;
        }
    }

    public override void CalculateAnimation(object sender, RenderingEventArgs renderingArgs)
    {
        double totalRenderTime = renderingArgs.RenderingTime.TotalMilliseconds;
        timePassed = totalRenderTime - startTime.Value;

        double progress;
        if (timePassed >= totalMillisecondsDuration)
        {
            progress = 1;
            InterpolateValueAndSetProperty(progress);
            EndAnimation();
            return;
        }

        progress = timePassed / totalMillisecondsDuration;
        InterpolateValueAndSetProperty(progress);
    }
    
    private void InterpolateValueAndSetProperty(double progress)
    {
        var currentFromValue = (double)dependencyPropertyOwner.GetValue(dependencyProperty);
        // Animation turns off if changes were made to dependencyProperty from outside. The same logic as in the default WPF animation
        if (lastFromValue != null && currentFromValue.Equals(lastFromValue) == false)
        {
            EndAnimation();
        }
        
        var currentValue = fromValue + (toValue - fromValue) * progress;

        dependencyPropertyOwner.SetValue(dependencyProperty, currentValue);
        lastFromValue = currentValue;
    }
}