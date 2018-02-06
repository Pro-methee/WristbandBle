package com.breakfirst.bfsensorscavyband;

import android.os.Handler;
import android.util.Log;

import com.unity3d.player.UnityPlayer;

/**
 * Created by Mhyshka on 17-Jan-18.
 */

public class BleSensorsInterop implements BleScannerResultManager.IBleDeviceManagerDelegate, BleSensorBase.IBleSensorDelegate{

    private static String TAG = BleSensorsInterop.class.toString();

    private static final int VERSION_CODE = 2;

    private BleScannerResultManager mScannerResultManager = null;
    private BleScanner mScanner = null;
    private BleSensorManager mSensorManager = null;
    private BleScanTimeoutTask mScanTimeoutTask = null;
    private Handler mHandler;
    private BleSupport mBleSupport;

    private static BleSensorsInterop s_instance = null;
    public static BleSensorsInterop getInstance()
    {
        Log.e(TAG, "Get Instance");
        if(s_instance == null)
            s_instance = new BleSensorsInterop();

        return s_instance;
    }

    public BleSensorsInterop() {
        Log.e(TAG, "Constructor start");
        mSensorManager = new BleSensorManager(UnityPlayer.currentActivity);

        mScannerResultManager = new BleScannerResultManager();
        mScanner = new BleScanner(UnityPlayer.currentActivity, mScannerResultManager);
        mHandler = new Handler();
        mScanTimeoutTask = new BleScanTimeoutTask(mHandler, mScannerResultManager);
        mScannerResultManager.setDelegate(this);
        mBleSupport = new BleSupport(UnityPlayer.currentActivity);
        Log.e(TAG, "Constructor end");
    }

    public int getVersionCode()
    {
        return VERSION_CODE;
    }

    public BleSupport getBleSupport()
    {
        return mBleSupport;
    }

    public BleSensorCavyband getSensorForAddress(String a_bleAddress)
    {
        BleSensorCavyband sensor = mSensorManager.tryGetBleSensorForAddress(a_bleAddress);
        return sensor;
    }

    public void reset()
    {
        mSensorManager.reset();
        mScanner.stopScan();
        mScannerResultManager.reset();
        mHandler.removeCallbacks(mScanTimeoutTask);
        //UnityPlayer.UnitySendMessage("BfSensorsNativeReceiver", "ResetManager", "");
    }

    //region Scan
    public void startScanningForSensors()
    {
        mScanner.startScan();
        mScanTimeoutTask.run();
    }

    public void stopScanningForSensors()
    {

        mScanner.stopScan();
        mHandler.removeCallbacks(mScanTimeoutTask);
    }
    //endregion

    @Override
    public void onNewDeviceFound(BleScanStatus a_scanStatus) {
        BleSensorCavyband cavyband = mSensorManager.tryGetBleSensorForAddress(a_scanStatus.getScanResult().getDevice().getAddress());

        if(cavyband == null) {
            cavyband = new BleSensorCavyband(a_scanStatus.getScanResult().getDevice(), mHandler, UnityPlayer.currentActivity);
            mSensorManager.addBleSensor(cavyband);
            cavyband.setSensorDeletage(this);
            cavyband.tryConnect();
        }
    }

    @Override
    public void onDeviceFound(BleScanStatus a_scanStatus) {

    }

    @Override
    public void onDeviceLost(BleScanStatus a_scanStatus) {

    }


    @Override
    public void onServiceExplorationComplete(BleSensorBase a_sensor) {
        UnityPlayer.UnitySendMessage("BfSensorsNativeReceiver", "NewSensorConnected", a_sensor.getBleAddress());
    }

    @Override
    public void onSensorConnected(BleSensorBase a_sensor) {
        if(a_sensor.getIsExplored())
        {
            UnityPlayer.UnitySendMessage("BfSensorsNativeReceiver", "SensorReconnected", a_sensor.getBleAddress());
        }
    }

    @Override
    public void onSensorDisconnected(BleSensorBase a_sensor) {
        UnityPlayer.UnitySendMessage("BfSensorsNativeReceiver", "SensorDisconnected", a_sensor.getBleAddress());
    }

    @Override
    public void onDataReceived(BleSensorBase a_sensor, byte[] a_data) {
        UnityPlayer.UnitySendMessage("BfSensorsNativeReceiver", "OnSensorDataReceived", a_sensor.getBleAddress());
    }
}