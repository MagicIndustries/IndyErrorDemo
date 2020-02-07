using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class GenesisObject
{
    public string genesis_txn;
}

public class IndyRaw : MonoBehaviour
{
	private static int _nextCommandHandle = 0;
	// Handle to the C++ DLL
	public IntPtr libraryHandle;
 
#if UNITY_EDITOR_OSX
	const string LIB_PATH = "/indy.dylib";
#elif UNITY_EDITOR_LINUX
	const string LIB_PATH = "/NativeScript.so";
#elif UNITY_EDITOR_WIN
	const string LIB_PATH = "/NativeScript.dll";
#endif
	
	private static void CallbackMethod(int xcommand_handle, int err)
	{
		Debug.Log("CallbackExecuted");
		Debug.Log(string.Format("TaskCompletingNoValueCallbackMethod exit code: {0}", err.ToString()));
	}
	
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void CallBackDelegate(int xcommand_handle, int err);
	private static CallBackDelegate Callback = CallbackMethod;

	public delegate void SetProtocolVersionDelegate(
        int command_handle, 
        int version, 
        CallBackDelegate cb
    );
	
	[DllImport("indy", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.Cdecl)]
	internal static extern int indy_set_protocol_version(int command_handle, int protocol_version, CallBackDelegate cb);

	[DllImport("indy", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.Cdecl)]
	internal static extern int indy_set_default_logger(ref int loglevel);
	
	void Awake()
	{
		Environment.SetEnvironmentVariable("RUST_LOG", "trace");
#if UNITY_EDITOR
		// Open native library
		libraryHandle = OpenLibrary(Application.dataPath + LIB_PATH);
#endif
		
	}
	
	void Start()
	{
		const string poolConfigName = "poolname";
//		var data = new GenesisObject {
//			genesis_txn = "/Users/tobytremayne/work/DigitalSoulApp/Assets/StreamingAssets/pool.txn"
//		};
		var commandHandle = GetNextCommandHandle();
		
		// execute the rust command
		indy_set_protocol_version(
          	commandHandle,
          	2,
          	Callback
        );
		Debug.Log("did the thing");
	}
	
	private static int GetNextCommandHandle()
	{
		return Interlocked.Increment(ref _nextCommandHandle);
	}
	
	void OnApplicationQuit()
	{
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
 
	
	private GenesisObject data;
	Task _task;
	
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
 
	[DllImport("indy")]
	static extern void Init(
		IntPtr gameObjectNew,
		IntPtr gameObjectGetTransform,
		IntPtr transformSetPosition);
 
	[DllImport("indy")]
	static extern void MonoBehaviourUpdate();
 
#endif
}