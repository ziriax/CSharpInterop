#define KEEP_CALLBACK_ALIVE

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ManagedApp;

public class Counter : IDisposable
{
    private nint _ptr;

#if KEEP_CALLBACK_ALIVE
    // Must keep a reference to the changed callback, it cannot be garbage collected!
    private ChangedCallback? _changed;
#else
    private WeakReference<ChangedCallback>? _changed;
#endif

    public Counter()
    {
        _ptr = NativeLib.counter_create();
    }

    public delegate void ChangedCallback(int value);

    public bool IsValid => _ptr != nint.Zero;

    public void AssertValid()
    {
        Debug.Assert(IsValid);

#if !KEEP_CALLBACK_ALIVE
        Debug.Assert(_changed == null || _changed.TryGetTarget(out _));
#endif
    }

    public ChangedCallback OnChanged
    {
        set
        {
            AssertValid();

#if KEEP_CALLBACK_ALIVE
            _changed = value;
#else
            _changed = new WeakReference<ChangedCallback>(value);
#endif

            NativeLib.counter_on_change(_ptr, value);
        }
    }

    public void Increment()
    {
        AssertValid();
        NativeLib.counter_increment(_ptr);
    }

    public void Dispose()
    {
        if (IsValid)
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
            _changed = null;
        }
    }

    ~Counter()
    {
        // Warning: this can be called on any thread!
        ReleaseUnmanagedResources();
    }

    private void ReleaseUnmanagedResources()
    {
        if (_ptr != nint.Zero)
        {
            NativeLib.counter_delete(_ptr);
            _ptr = nint.Zero;
        }
    }

    private static class NativeLib
    {
        private const string DllName = "NativeLib_x64";

        [DllImport(DllName)]
        public static extern nint counter_create();

        [DllImport(DllName)]
        public static extern void counter_delete(nint counter);

        [DllImport(DllName)]
        public static extern void counter_increment(nint counter);

        [DllImport(DllName)]
        public static extern void counter_on_change(nint counter, ChangedCallback callback);
    }
}