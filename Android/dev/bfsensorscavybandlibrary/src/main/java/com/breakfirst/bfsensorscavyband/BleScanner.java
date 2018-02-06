package com.breakfirst.bfsensorscavyband;

import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothManager;
import android.bluetooth.le.BluetoothLeScanner;
import android.bluetooth.le.ScanCallback;
import android.bluetooth.le.ScanResult;
import android.content.Context;

import android.util.Log;

import java.util.List;

/**
 * Created by Mhyshka on 16-Jan-18.
 */

public class BleScanner extends ScanCallback {

    private final static String TAG = BleScannerResultManager.class.getSimpleName();

    private static BleScannerResultManager s_instance = null;
    private BleScannerResultManager mScannerResultManager;

    private BluetoothManager mBluetoothManager;
    private BluetoothAdapter mBluetoothAdapter;
    private BluetoothLeScanner mBleScanner;

    private Context mContext;

    public BleScanner(Context a_context, BleScannerResultManager a_deviceManager){
        mScannerResultManager = a_deviceManager;
        mContext = a_context;

        if (mBluetoothManager == null) {
            mBluetoothManager = (BluetoothManager) mContext.getSystemService(Context.BLUETOOTH_SERVICE);
            if (mBluetoothManager == null) {
                Log.i(TAG, "Unable to initialize BluetoothManager.");
            }
        }

        mBluetoothAdapter = mBluetoothManager.getAdapter();
        if (mBluetoothAdapter == null) {
            Log.i(TAG, "Unable to obtain a BluetoothAdapter.");
        }

        mBleScanner = mBluetoothAdapter.getBluetoothLeScanner();
        if(mBleScanner == null){
            Log.e(TAG,"Unable to obtain BleScannerResultManager");
        }
    }


    protected boolean mIsScanning = false;
    public boolean getIsScanning()
    {
        return mIsScanning;
    }

    public void startScan()
    {
        Log.e(TAG, "Start scan...");
        if(mBleScanner != null)
        {
            if (!mIsScanning)
            {
                Log.e(TAG, "Scanning...");
                mBleScanner.startScan(this);
                mIsScanning = true;
            }
        }
    }

    public void stopScan()
    {
        if(mBleScanner != null) {
            if (mIsScanning) {
                mBleScanner.stopScan(this);
                mIsScanning = false;
            }
        }
    }


    @Override
    public void onScanFailed(int a_errorCode)
    {
        Log.e(TAG, "Scan Failed : " + a_errorCode);
        mIsScanning = false;
    }

    @Override
    public void onBatchScanResults (List<ScanResult> results)
    {
        //Log.e(TAG, "onBatchScanResults : " + results.size());
        for(ScanResult each : results)
        {
            onScanResult(-1, each);
        }
    }

    @Override
    public void onScanResult (int callbackType,
                              ScanResult result)
    {
        if(result != null && result.getScanRecord() != null) {

            String bytesStr = "";
            for (Byte each : result.getScanRecord().getBytes())
                bytesStr += String.format("%02x", each);

            //Log.e(TAG, "Bytes : " + result.getScanRecord().getBytes().length +"\n" +bytesStr);
            if(bytesStr.startsWith("0201050302021808ff"))
            {
                BluetoothDevice device = result.getDevice();
                onDeviceFound(result);
            }
        }
    }

    protected void onDeviceFound(ScanResult a_result)
    {
        mScannerResultManager.deviceDetected(a_result);
    }
}
