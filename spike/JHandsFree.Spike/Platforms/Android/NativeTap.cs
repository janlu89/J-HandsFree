using Android.OS;
using Android.Views;
using AndroidWebView = Android.Webkit.WebView;

namespace JHandsFree.Spike;

// A touch dispatched to the WebView reaches the page as a trusted event, unlike a click raised by a script.
public static class NativeTap
{
    private const long TapDurationMs = 60;

    public static bool Tap(AndroidWebView view, float x, float y)
    {
        var downTime = SystemClock.UptimeMillis();
        var down = MotionEvent.Obtain(downTime, downTime, MotionEventActions.Down, x, y, 0);
        var up = MotionEvent.Obtain(downTime, downTime + TapDurationMs, MotionEventActions.Up, x, y, 0);
        if (down is null || up is null)
        {
            return false;
        }

        down.SetSource(InputSourceType.Touchscreen);
        up.SetSource(InputSourceType.Touchscreen);
        var handled = view.DispatchTouchEvent(down) & view.DispatchTouchEvent(up);
        down.Recycle();
        up.Recycle();
        return handled;
    }
}
