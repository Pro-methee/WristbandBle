package com.breakfirst.bfsensorscavyband;

import android.content.Context;
import android.util.Log;

import java.util.HashMap;

/**
 * Created by Alexandre Pinto on 10/10/2016.
 */
public class BleSensorManager
{
    private static String TAG = BleSensorManager.class.toString();

    private HashMap<String, BleSensorCavyband> mBleSensorsByAddress = new HashMap<String, BleSensorCavyband>();
    public int getSensorsCount()
    {
        return mBleSensorsByAddress.size();
    }

    private Context mContext = null;
    public BleSensorManager(Context a_context)
    {
        mContext = null;
    }

    public void reset()
    {
        for(BleSensorCavyband each : mBleSensorsByAddress.values())
        {
            each.disconnectGatt();
        }
        mBleSensorsByAddress.clear();
    }

    public void addBleSensor(BleSensorCavyband a_sensor)
    {
        Log.e(TAG, "Ble Sensor Added");
        mBleSensorsByAddress.put(a_sensor.getBleAddress(), a_sensor);

        if(mSensorManagerDelegate != null)
            mSensorManagerDelegate.onSensorAdded(a_sensor);
    }

    public void removeBleSensor(String a_bleAddress)
    {
        if(mBleSensorsByAddress.containsKey(a_bleAddress))
        {
            BleSensorCavyband removedSensor = mBleSensorsByAddress.remove(a_bleAddress);

            if(mSensorManagerDelegate != null)
                mSensorManagerDelegate.onSensorRemoved(removedSensor);

            removedSensor.disconnectGatt();
        }
    }

    public BleSensorCavyband tryGetBleSensorForAddress(String a_address)
    {
        if(mBleSensorsByAddress.containsKey(a_address)) {
            BleSensorCavyband sensor = mBleSensorsByAddress.get(a_address);
            return sensor;
        }
        return null;
    }

    private IBleSensorManagerCallback mSensorManagerDelegate;
    public void setSensorManagerCallback(IBleSensorManagerCallback a_sensorManagerCallback)
    {
        mSensorManagerDelegate = a_sensorManagerCallback;
    }

    public interface IBleSensorManagerCallback {
        void onSensorAdded(BleSensorCavyband a_sensor);
        void onSensorRemoved(BleSensorCavyband a_sensor);
    }
}
