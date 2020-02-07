using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class TestThreads : MonoBehaviour
{
    private static int _nextCommandHandle = 0;
    // Handle to the C++ DLL
    public IntPtr libraryHandle;
 
#if UNITY_EDITOR_OSX
    const string LIB_PATH = "/libthreadtestlib.dylib";
#elif UNITY_EDITOR_LINUX
	const string LIB_PATH = "/libthreadtestlib.so";
#elif UNITY_EDITOR_WIN
	const string LIB_PATH = "/libthreadtestlib.dll";
#endif
    
	// setup callback delegate and instance
    private static void CallbackMethod(int command_handle)
    {
        Debug.Log("Callback Executed");
        Debug.Log(string.Format("CallbackMethod command handle returns: {0}", command_handle.ToString()));
    }
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void CallBackDelegate(int xcommand_handle);
    private static CallBackDelegate Callback = CallbackMethod;

	// import the rust function
    [DllImport("libthreadtestlib", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int command_test(int command_handle, CallBackDelegate cb);
    
    // Start is called before the first frame update
    void Start()
    {
	    // grab a new handle
        var commandHandle = GetNextCommandHandle();
        // call the rust function
        command_test(
            commandHandle,
            Callback
        );
        Debug.Log("Thread Test Start() complete");
    }

    private static int GetNextCommandHandle()
	{
		return Interlocked.Increment(ref _nextCommandHandle);
	}
	
	void OnApplicationQuit()
	{
		// attempt to close / drop everything
#if UNITY_EDITOR
		CloseLibrary(libraryHandle);
		libraryHandle = IntPtr.Zero;
		Debug.Log("did close");
#endif
	}
	
	#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
 
	[DllImport("__Internal")]
	public static extern IntPtr dlopen(
		string path,
		int flag);
 
	[DllImport("__Internal")]
	public static extern IntPtr dlsym(
		IntPtr handle,
		string symbolName);
 
	[DllImport("__Internal")]
	public static extern int dlclose(
		IntPtr handle);
 
	public static IntPtr OpenLibrary(string path)
	{
		IntPtr handle = dlopen(path, 0);
		if (handle == IntPtr.Zero)
		{
			throw new Exception("Couldn't open native library: " + path);
		}
		return handle;
	}
 
	public static void CloseLibrary(IntPtr libraryHandle)
	{
		dlclose(libraryHandle);
	}
 
	public static T GetDelegate<T>(
		IntPtr libraryHandle,
		string functionName) where T : class
	{
		IntPtr symbol = dlsym(libraryHandle, functionName);
		if (symbol == IntPtr.Zero)
		{
			throw new Exception("Couldn't get function: " + functionName);
		}
		return Marshal.GetDelegateForFunctionPointer(
			symbol,
			typeof(T)) as T;
	}
 
 
#elif UNITY_EDITOR_WIN
 
	[DllImport("kernel32")]
	public static extern IntPtr LoadLibrary(
		string path);
 
	[DllImport("kernel32")]
	public static extern IntPtr GetProcAddress(
		IntPtr libraryHandle,
		string symbolName);
 
	[DllImport("kernel32")]
	public static extern bool FreeLibrary(
		IntPtr libraryHandle);
 
	public static IntPtr OpenLibrary(string path)
	{
		IntPtr handle = LoadLibrary(path);
		if (handle == IntPtr.Zero)
		{
			throw new Exception("Couldn't open native library: " + path);
		}
		return handle;
	}
 
	public static void CloseLibrary(IntPtr libraryHandle)
	{
		FreeLibrary(libraryHandle);
	}
 
	public static T GetDelegate<T>(
		IntPtr libraryHandle,
		string functionName) where T : class
	{
		IntPtr symbol = GetProcAddress(libraryHandle, functionName);
		if (symbol == IntPtr.Zero)
		{
			throw new Exception("Couldn't get function: " + functionName);
		}
		return Marshal.GetDelegateForFunctionPointer(
			symbol,
			typeof(T)) as T;
	}
 
#else
 
	[DllImport("libthreadtestlib")]
	static extern void Init(
		IntPtr gameObjectNew,
		IntPtr gameObjectGetTransform,
		IntPtr transformSetPosition);
 
	[DllImport("libthreadtestlib")]
	static extern void MonoBehaviourUpdate();
 
#endif
}
