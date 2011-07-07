// Dbg is a utility to log debug output and is designed to work within the Unity3D editor's Console
//
// By using [Conditional("UNITY_EDITOR")], it allows debug log calls to be "stripped" away 
// when building to the target platform.
// 
// Dbg is also designed around logging to a channel so that user can selectively watch
// the channel that they are interested in.
// 
// Example Usage:
//
// on a Start() or Awake() method call
//     Dbg.SetDebugChannel(Dbg.eChannelType.PLAYER, true);  // this will turn on the PLAYER channel
//
// now one can call:
//     Dbg.Log(Dbg.eChannelType.PLAYER, "display something that can help you debug" );
//     Dbg.LogMethodCall(Dbg.eChannelType.PLAYER);          // to log the method you are in
//     Dbg.Assert( needsToBeTrue, "ok, we got an assert");  // this will pause the editor if needsToBeTrue is false
// 
// original author: joseph.laurino@paradessagames.com
// 
// <feel free to use and modify this code>

using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.Reflection;

using ConditionalAttribute = System.Diagnostics.ConditionalAttribute;

public sealed class DbgEventArgs: System.EventArgs {
	public DbgEventArgs(string msg) {
		Msg = msg;
	}
	
	public string Msg;
}

public class Dbg {
	
	// add debug channels for your app
	public enum eChannelType
	{
		GAME_MANAGER,
		DATABASE,
		GAME_OBJECT,
		HUD,
		RENDERER,
		SOUND,
		INPUT,
		CAMERA,
		PLAYER,
		NUM_DEBUG_CHANNELS
	}
	
	// this can be a bit vector
	private static bool[] DEBUG_CHANNELS = new bool[(int)eChannelType.NUM_DEBUG_CHANNELS];
	
	// these events are provided in case you want to set-up something like 
	// a file logger or print the logs onscreen... WATCH OUT FOR RECURSION, 
	// make sure that the event handler does not use ZDebug to log something 
	// else while in your event handler
	public static event System.EventHandler<DbgEventArgs> OnSetDebugChannel;
	public static event System.EventHandler<DbgEventArgs> OnLog;
	public static event System.EventHandler<DbgEventArgs> OnLogMethodCall;
	public static event System.EventHandler<DbgEventArgs> OnAssert;
	
	// making this into a true singleton -- set to private so that no instance can be created
	private Dbg() {
	}
	
	[Conditional("UNITY_EDITOR")]
	public static void SetDebugChannel(eChannelType channel, bool state)
	{
		DEBUG_CHANNELS[(int)channel] = state;
		string s = channel.ToString() + ": CHANNEL IS " + state.ToString() + "\n";
		UnityEngine.Debug.Log(s);
		
		if( OnSetDebugChannel != null ) {
			OnSetDebugChannel(null, new DbgEventArgs(s));
		}
	}
	
	[Conditional("UNITY_EDITOR")]
	public static void Log(eChannelType channel, string message)
	{
		if( DEBUG_CHANNELS[(int)channel] == true ) 
		{
			string s = channel.ToString() + ": " + message + "\n";
			UnityEngine.Debug.Log(s);
			
			if( OnLog != null ) {
				OnLog(null, new DbgEventArgs(s));
			}
		}
	}
	
	[Conditional("UNITY_EDITOR")]
	public static void LogMethodCall(eChannelType channel)
	{
		if( DEBUG_CHANNELS[(int)channel] == true ) 
		{
			StackFrame sf = new StackFrame(1);
			MethodBase mb = sf.GetMethod();
			System.Type t = mb.DeclaringType;
			
			string s = System.String.Format(channel.ToString() + ": " + "METHODCALL {0}::{1} \n", t.Name, mb.Name);
			UnityEngine.Debug.Log(s);
			
			if( OnLogMethodCall != null ) {
				OnLogMethodCall(null, new DbgEventArgs(s));
			}
		}
	}
	
	[Conditional("UNITY_EDITOR")]
	public static void Assert(bool assertVal, string message)
	{
		if( assertVal != true ) 
		{			
			StackFrame sf = new StackFrame(1);
			MethodBase mb = sf.GetMethod();
			System.Type t = mb.DeclaringType;
			
			string s = message + System.String.Format("\nMETHODCALL {0}::{1}\n", t.Name, mb.Name);
			
			UnityEngine.Debug.Log(s);
			
			if( OnAssert != null ) {
				OnAssert(null, new DbgEventArgs(s));
			}
			
			// then pause the editor
			UnityEngine.Debug.Break();
		}
	}
}