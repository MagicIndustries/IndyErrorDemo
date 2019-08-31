using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Runtime.InteropServices;
using Hyperledger.Indy.PoolApi;
using Hyperledger.Indy.Utils;
using System.Threading.Tasks;
using UnityEngine.TestTools;

//using NLog;
//using NLog.Config;
//using NLog.Targets;

[InitializeOnLoad]
public class IndyWrapper : MonoBehaviour {

   string poolname = "poolnew";
   string data = "{ \"genesis_txn\" : \"/Users/tobytremayne/work/DigitalSoulsDemo/Assets/StreamingAssets/pool.txn\" }";

   Task _task;
   const int PROTOCOL_VERSION = 2;

   // Start is called before the first frame update
   void Start() {
        //AssemblyInit();
        //Hyperledger.Indy.Utils.Logger.Init();
        Pool.SetProtocolVersionAsync(2);

        RunCode();

    }

   // Update is called once per frame
   void Update() {

   }

   public async void RunCode() {

       await CreatePool();
       Debug.Log("blah");

   }


   //public static void AssemblyInit() {
   //    //Initialization code goes here.
   //    var config = new LoggingConfiguration();

   //    // Add appender to print messages with Console.WriteLine
   //    //LoggerFactory.AddAppender((logger, logLevel, message) => Console.WriteLine(message));

   //    var fileTarget = new FileTarget("filelog");
   //    fileTarget.FileName = "nlog.log";
   //    config.AddTarget(fileTarget);

   //    // Step 2. Create targets
   //    //var consoleTarget = new ColoredConsoleTarget("target1") {
   //    //    Layout = @"${date:format=HH\:mm\:ss} ${level} ${message} ${exception}"
   //    //};
   //    //consoleTarget.DetectConsoleAvailable = false;
   //    //config.AddTarget(consoleTarget);

   //    //config.AddRuleForAllLevels(consoleTarget); // all to console
   //    config.AddRuleForAllLevels(fileTarget); // all to console

   //    // Step 4. Activate the configuration
   //    LogManager.Configuration = config;

   //    Hyperledger.Indy.Utils.Logger.Init();

   //}

   

   private void OnDisable() {
       Pool.ExtClose();
       Debug.Log("OnDisable");
       _task.Dispose();
       _task = null;
       poolname = null;
       data = null;
   }

   //static void Abort() {
   //    Debug.Log("ABORT");
   //    tsk
   //    Thread = null;
   //}

   //static void Restart() {
   //    Abort();
   //    ThreadStart();
   //}

   public async Task CreatePool() {

       try {
           _task = Pool.CreatePoolLedgerConfigAsync(poolname, data);
           await _task;
           _task.Dispose();
       } catch (Exception e) {
           Debug.Log("Pool Config Already Exists" + e);
       }


   }

   static IndyWrapper() {
       Debug.Log("IndyWrapper");
       EditorApplication.playModeStateChanged += PlayModeChanged;
   }

   private static void PlayModeChanged(PlayModeStateChange playModeStateChange) {
       Debug.Log("PlayModeChanged");
       switch (playModeStateChange) {
           case PlayModeStateChange.EnteredEditMode:
               Debug.Log("EnteredEditMode");
               EditorApplication.UnlockReloadAssemblies();
               break;
           case PlayModeStateChange.EnteredPlayMode:
               Debug.Log("EnteredPlayMode");
               EditorApplication.LockReloadAssemblies();
               break;
       }
   }
}
