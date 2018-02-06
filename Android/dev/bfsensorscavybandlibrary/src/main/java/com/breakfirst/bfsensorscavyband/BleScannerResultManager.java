package com.breakfirst.bfsensorscavyband;

import android.bluetooth.BluetoothDevice;
import android.bluetooth.le.ScanResult;
import android.util.Log;

import java.util.ArrayList;
import java.util.Date;
import java.util.HashMap;

/**
 * Created by Mhyshka on 16-Jan-18.
 */

public class BleScannerResultManager {
    private static String TAG = BleScannerResultManager.class.toString();

    private HashMap<String, BleScanStatus> mDevicesByAddress = new HashMap<String, BleScanStatus>();

    protected IBleDeviceManagerDelegate mDelegate = null;
    public void setDelegate(IBleDeviceManagerDelegate a_delegate)
    {
        mDelegate = a_delegate;
    }

    public BleScannerResultManager(){
    }

    public void reset()
    {
        ArrayList<BleScanStatus> devicesToRemove = new ArrayList<BleScanStatus>();
        for(BleScanStatus each : mDevicesByAddress.values())
        {
            devicesToRemove.add(each);
        }

        for(BleScanStatus each : devicesToRemove)
        {
            lostDevice(each.getScanResult().getDevice().getAddress());
        }
    }

    public void deviceDetected(ScanResult a_result)
    {
        BleScanStatus scanStatus;
        String address = a_result.getDevice().getAddress();
        //Known device
        if(mDevicesByAddress.containsKey(address))
        {
            scanStatus = mDevicesByAddress.get(address);
            //reconnect event
            if(!scanStatus.getIsAdvertising()) {

                Log.e(TAG, "Known device found : " + address);

                if (mDelegate != null)
                    mDelegate.onDeviceFound(scanStatus);
            }
        }
        //New device
        else
        {
            scanStatus = new BleScanStatus(a_result);

            Log.e(TAG, "New device found : " + address);
            mDevicesByAddress.put(address, scanStatus);

            if(mDelegate != null)
                mDelegate.onNewDeviceFound(scanStatus);
        }

        scanStatus.setIsAdvertising(true);
        scanStatus.updateScanTick(a_result);
    }

    public void lostDevice(String a_bleAddress)
    {
        Log.e(TAG, "Device lost : " + a_bleAddress);

        BleScanStatus scanStatus = mDevicesByAddress.get(a_bleAddress);
        if(scanStatus != null)
        {
            scanStatus.setIsAdvertising(false);

            if(mDelegate != null)
                mDelegate.onDeviceLost(scanStatus);
        }
    }

    public BluetoothDevice tryGetBleDeviceForAddress(String a_address)
    {
        if(mDevicesByAddress.containsKey(a_address))
        {
            BluetoothDevice device = mDevicesByAddress.get(a_address).getScanResult().getDevice();
            return device;
        }

        return null;
    }

    protected long msToTimeout = 5000;
    public void updateAndRemoveLostDevices()
    {
        long tickNow = new Date().getTime();
        ArrayList<BleScanStatus> devicesToRemove = new ArrayList<BleScanStatus>();
        for(BleScanStatus each : mDevicesByAddress.values())
        {
            if(each.getIsAdvertising())
            {
                if(tickNow - each.getScanTick() >= msToTimeout)
                {
                    devicesToRemove.add(each);
                    Log.e(TAG, "Device timedout : " + each.getScanResult().getDevice().getAddress());
                }
            }
        }

        for(BleScanStatus each : devicesToRemove)
        {
            lostDevice(each.getScanResult().getDevice().getAddress());
        }
    }

    public interface IBleDeviceManagerDelegate {
        void onNewDeviceFound(BleScanStatus a_scanStatus);
        void onDeviceFound(BleScanStatus a_scanStatus);
        void onDeviceLost(BleScanStatus a_scanStatus);
    }
}
