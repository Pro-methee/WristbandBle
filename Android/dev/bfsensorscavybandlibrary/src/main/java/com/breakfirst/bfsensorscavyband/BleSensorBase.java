package com.breakfirst.bfsensorscavyband;


import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothProfile;
import android.content.Context;
import android.os.Handler;
import android.util.Log;

/**
 * Created by Mhyshka on 16-Jan-18.
 */

public abstract class BleSensorBase implements BleGattWrapper.IBleGattConnectionDelegate, BleGattWrapper.IBleGattExplorationDelegate, BleGattWrapper.IBleGattMessageDelegate{
    static String TAG = BleSensorBase.class.toString();

    //region Properties
    protected BleGattWrapper mGattWrapper = null;

    protected boolean mIsConnected = false;
    public boolean getIsConnected()
    {
        return mIsConnected;
    }

    protected boolean mIsExplored = false;
    public boolean getIsExplored()
    {
        return mIsExplored;
    }


    public abstract String getDisplayName();
    public String getBleAddress()
    {
        return mGattWrapper.getBluetoothDevice().getAddress();
    }

    protected BleConnectTask mConnectTask;

    protected IBleSensorDelegate mSensorDelegate = null;
    public void setSensorDeletage(IBleSensorDelegate a_delegate)
    {
        mSensorDelegate = a_delegate;
    }
    //endregion


    //region Connect/Disconnect Gatt
    public void tryConnect()
    {
        if(!mConnectTask.getIsRunning())
            mConnectTask.run();
    }

    public boolean connectGatt(Context a_context)
    {
        return mGattWrapper.connect(a_context);
    }

    public void disconnectGatt() {

        mGattWrapper.close();
        mGattWrapper = null;

        mConnectTask.cancelTask();
    }
    //endregion

    //Region Constructor & Destructor
    public BleSensorBase(BluetoothDevice a_device, Handler a_handler, Context a_context)
    {
        mConnectTask = new BleConnectTask(a_handler, a_context, this);
        mGattWrapper = new BleGattWrapper(a_device);
        mGattWrapper.setConnectionDelegate(this);
        mGattWrapper.setExploraitonDelegate(this);
        mGattWrapper.setMessageDelegate(this);
    }
    //endregion


    //region Gatt Delegate
    @Override
    public void onConnectionStatusChanged(int a_connectionStatus){
        Log.e(TAG, "Connection status changed : " + a_connectionStatus);
        if(a_connectionStatus == BluetoothProfile.STATE_CONNECTED) {
            OnConnectionSuccess();
        }
        else {
            OnConnectionLost();
        }
    }


    void OnConnectionSuccess()
    {
        mIsConnected = true;
        mConnectTask.cancelTask();
        if(!mIsExplored)
        {
            mGattWrapper.getBluetoothGatt().discoverServices();
        }

        if(mSensorDelegate != null)
            mSensorDelegate.onSensorConnected(this);
    }

    void OnConnectionLost()
    {
        if(mIsConnected) {
            tryConnect();
            mIsConnected = false;

            if (mSensorDelegate != null)
                mSensorDelegate.onSensorDisconnected(this);
        }
    }

    @Override
    public void onServiceExplorationComplete() {

        mIsExplored = true;

        if(mSensorDelegate != null)
            mSensorDelegate.onServiceExplorationComplete(this);
    }
    //endregion

    public interface IBleSensorDelegate {
        void onServiceExplorationComplete(BleSensorBase a_device);
        void onSensorConnected(BleSensorBase a_device);
        void onSensorDisconnected(BleSensorBase a_device);
        void onDataReceived(BleSensorBase a_sensor, byte[] a_data);
    }
}

