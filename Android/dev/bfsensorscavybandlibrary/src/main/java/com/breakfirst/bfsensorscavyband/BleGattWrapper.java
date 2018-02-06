package com.breakfirst.bfsensorscavyband;

import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothGatt;
import android.bluetooth.BluetoothGattCallback;
import android.bluetooth.BluetoothGattCharacteristic;
import android.bluetooth.BluetoothGattDescriptor;
import android.bluetooth.BluetoothGattService;
import android.bluetooth.BluetoothProfile;
import android.content.Context;
import android.util.Log;

import java.util.Iterator;
import java.util.List;
import java.util.UUID;


/**
* Created by Mhyshka on 16-Jan-18.
*/
public class BleGattWrapper {

    public interface IBleGattConnectionDelegate {
        String getBleAddress();
        void onConnectionStatusChanged(int a_connectionStatus);
    }
    public interface IBleGattExplorationDelegate {
        void onServiceExplorationComplete();
    }

    public interface IBleGattMessageDelegate {
        void onCharacteristicRead(UUID a_uuid, byte[] a_data);
        void onCharacteristicChanged(UUID a_uuid, byte[] a_data);
        void onDescriptorRead(UUID a_uuid, byte[] a_data);
    }

    /// PROPERTIES
    static String TAG = BleGattWrapper.class.toString();
    protected boolean mIsServiceDiscovered = false;

    protected BluetoothGatt mGatt;
    public BluetoothGatt getBluetoothGatt()
    {
        return mGatt;
    }

    public int getConnectionStatus()
    {
        return mConnectionStatus;
    }
    protected int mConnectionStatus = BluetoothGatt.STATE_DISCONNECTED;

    private BluetoothDevice mBleDevice = null;
    public BluetoothDevice getBluetoothDevice()
    {
        return mBleDevice;
    }

    protected IBleGattConnectionDelegate mConnectionDelegate = null;
    public void setConnectionDelegate(IBleGattConnectionDelegate a_delegate)
    {
        mConnectionDelegate = a_delegate;
    }

    protected IBleGattExplorationDelegate mExplorationDelegate = null;
    public void setExploraitonDelegate(IBleGattExplorationDelegate a_delegate)
    {
        mExplorationDelegate = a_delegate;
    }

    protected IBleGattMessageDelegate mMessageDelegate = null;
    public void setMessageDelegate(IBleGattMessageDelegate a_delegate)
    {
        mMessageDelegate = a_delegate;
    }

    ///CONSTRUCTOR
    public BleGattWrapper(BluetoothDevice a_bluetoothDevice)
    {
        mBleDevice = a_bluetoothDevice;
    }

    ///STATE MANAGEMENT
    public boolean connect(Context a_context) {
        if(mGatt != null)
        {
            Log.d(TAG, "Trying to use an existing mGattWrapper for connection.");
            if(mGatt.connect())
            {
                mConnectionStatus = BluetoothGatt.STATE_CONNECTING;
                StateChanged();
                return true;
            }
            else
            {
                mConnectionStatus = BluetoothGatt.STATE_DISCONNECTED;
                Log.i(TAG, "Can't connect to Bluetooth Device : " + mGatt.getDevice().getName());
                StateChanged();
                return false;
            }
        }
        else
        {
            Log.d(TAG, "Trying to create a new connection.");
            mConnectionStatus = BluetoothGatt.STATE_CONNECTING;
            mGatt = mBleDevice.connectGatt(a_context, false, mGattCallback);
            StateChanged();
            Log.i(TAG, mGatt.getDevice().getName()+" Connecting...");
        }

        return true;
    }

    public void close()
    {
        if (mGatt == null) {
            return;
        }
        Log.w(TAG, "mGattWrapper closed");
        mGatt.close();
        mConnectionStatus = BluetoothGatt.STATE_DISCONNECTED;
        mGatt = null;
        mConnectionDelegate = null;
        mExplorationDelegate = null;
        mMessageDelegate = null;
    }

    protected void StateChanged()
    {
        if(mConnectionDelegate != null)
            mConnectionDelegate.onConnectionStatusChanged(mConnectionStatus);
    }
    ///

    protected BluetoothGattCallback mGattCallback = new BluetoothGattCallback() {
        // --------------------------------------------------------------------------------------------
        // ---------------------------- BluetoothGattCallback -----------------------------------------
        // Various callback methods defined by the BLE API. //
        @Override
        public void onConnectionStateChange(BluetoothGatt gatt, int status,
                                            int newState) {
            if (newState == BluetoothProfile.STATE_CONNECTED) {
                mConnectionStatus = newState;
                Log.d(TAG, mBleDevice.getName() + " - Connected to GATT server.");

            } else if (newState == BluetoothProfile.STATE_DISCONNECTED) {
                mConnectionStatus = newState;
                Log.d(TAG, mBleDevice.getName() + " - Disconnected from GATT server.");
            }

            //mConnectionStatus = newState;
            StateChanged();
        }

        @Override
        // New services discovered
        public void onServicesDiscovered(BluetoothGatt gatt, int status) {
            if (status == BluetoothGatt.GATT_SUCCESS) {
                if(mExplorationDelegate != null)
                    mExplorationDelegate.onServiceExplorationComplete();
                Log.d(TAG, "On Service Discovered");
            } else {
                Log.w(TAG, "onServicesDiscovered received: " + status);
            }
        }

        @Override
        // Result of a characteristic read operation
        public void onCharacteristicRead(BluetoothGatt gatt,
                                         BluetoothGattCharacteristic characteristic,
                                         int status) {
            //Log.d(TAG, "On characteristic Read");
            if (status == BluetoothGatt.GATT_SUCCESS){
            /*Log.d(TAG, "UUID : " + characteristic.getUuid());
            Log.d(TAG, "Value : " + characteristic.getValue());*/

                if(mMessageDelegate != null)
                    mMessageDelegate.onCharacteristicRead(characteristic.getUuid(), characteristic.getValue());
            }

            //tryExecuteNextCommand();
        }

        @Override
        public void onCharacteristicChanged(BluetoothGatt gatt,
                                            BluetoothGattCharacteristic characteristic) {
        /*Log.d(TAG, "On characteristic Changed");
        Log.d(TAG, "UUID : " + characteristic.getUuid());
        Log.d(TAG, "Value : " + characteristic.getValue());*/

            if(mMessageDelegate != null)
                mMessageDelegate.onCharacteristicChanged(characteristic.getUuid(), characteristic.getValue());
        }

        @Override
        public void onDescriptorRead(BluetoothGatt gatt, BluetoothGattDescriptor descriptor, int status)
        {
            //Log.d(TAG, "On Descriptor Read");
            if (status == BluetoothGatt.GATT_SUCCESS) {
            /*Log.d(TAG, "UUID : " + descriptor.getUuid());
            Log.d(TAG, "Value : " + descriptor.getValue());*/

                if(mMessageDelegate != null)
                    mMessageDelegate.onDescriptorRead(descriptor.getUuid(), descriptor.getValue());
            }

            //tryExecuteNextCommand();
        }
    };


    public boolean hasServiceUuid(UUID a_serviceUuid)
    {
        boolean didFoundService = false;

        List<BluetoothGattService> services = mGatt.getServices();
        for (BluetoothGattService each : services) {
            if(each.getUuid().equals(a_serviceUuid))
            {
                Log.e(TAG,"Found Service Target");
                didFoundService = true;
                break;
            }
        }

        return didFoundService;
    }


    public void exploreAllServices()
    {
        List<BluetoothGattService> services = mGatt.getServices();
        Log.d(TAG, "Explore All Services");
        for (Iterator<BluetoothGattService> iter = services.listIterator(); iter.hasNext();) {
            BluetoothGattService bgs = iter.next();
            exploreService(bgs, 0);
        }

        /*if(mExplorationDelegate != null)
            mExplorationDelegate.onServiceExplorationComplete();*/
    }

    public void exploreService(BluetoothGattService a_bgs, int a_depth)
    {
        String desc = "";
        for (int i = 0; i < a_depth; i++) {
            desc += "   ";
        }
        desc += "s : " + a_bgs.getUuid().toString();
        Log.d(TAG, desc);

        for(BluetoothGattCharacteristic each : a_bgs.getCharacteristics())
        {
            exploreCharacteristic(each, a_depth+1);
        }

        for(BluetoothGattService each : a_bgs.getIncludedServices())
        {
            exploreService(each, a_depth+1);
        }
    }

    public void exploreCharacteristic(BluetoothGattCharacteristic a_bgc, int a_depth)
    {
        String desc = "";
        for (int i = 0; i < a_depth; i++) {
            desc += "   ";
        }
        desc += "c : " + a_bgc.getUuid().toString();
        Log.d(TAG, desc);

        desc = "";
        for (int i = 0; i < a_depth; i++) {
            desc += "   ";
        }
        desc += "val : " + a_bgc.getValue();
        Log.d(TAG, desc);

        for(BluetoothGattDescriptor each : a_bgc.getDescriptors())
        {
            exploreDescriptor(each, a_depth+1);
        }
    }

    public void exploreDescriptor(BluetoothGattDescriptor a_bgd, int a_depth)
    {
        String desc = "";
        for (int i = 0; i < a_depth; i++) {
            desc += "   ";
        }
        desc += "d : " + a_bgd.getUuid().toString();
        Log.d(TAG, desc);


        desc = "";
        for (int i = 0; i < a_depth; i++) {
            desc += "   ";
        }
        desc += "val : " + a_bgd.getValue();
        Log.d(TAG, desc);
    }
}
