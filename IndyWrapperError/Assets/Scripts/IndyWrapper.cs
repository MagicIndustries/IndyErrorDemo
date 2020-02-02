using System;
using UnityEngine;
using UnityEditor;
using Hyperledger.Indy.PoolApi;
using System.Threading.Tasks;

[InitializeOnLoad]
public class IndyWrapper : MonoBehaviour {

    string poolname = "poolnew2";
    string data = "{ \"genesis_txn\" : \"/Users/tobytremayne/work/DigitalSoulsDemo/Assets/StreamingAssets/pool.txn\" }";

    Task _task;
    const int PROTOCOL_VERSION = 2;

    void Start() {
        Pool.SetProtocolVersionAsync(2);
        InitPool();
    }

    private async void InitPool() {
        await CreatePool();
        Debug.Log("Pool Created");
    }

    public async Task CreatePool() {
        try {
            _task = Pool.CreatePoolLedgerConfigAsync(poolname, data);
            await _task;
            _task.Dispose();
        } catch (Exception e) {
            Debug.Log("Pool Config Already Exists" + e);
        }
    }

    private void OnDisable() {
        Pool.ExtClose();
        Debug.Log("OnDisable");
        _task.Dispose();
        _task = null;
        poolname = null;
        data = null;
    }

}
