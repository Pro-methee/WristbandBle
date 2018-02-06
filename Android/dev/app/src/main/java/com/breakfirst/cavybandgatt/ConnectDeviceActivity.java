package com.breakfirst.cavybandgatt;

import android.content.Intent;
import android.os.Bundle;
import android.os.Handler;
import android.support.v7.app.AppCompatActivity;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.LinearLayout;
import android.widget.ProgressBar;
import android.widget.Toast;

import com.breakfirst.bfsensorscavyband.BleScanStatus;
import com.breakfirst.bfsensorscavyband.BleScanTimeoutTask;
import com.breakfirst.bfsensorscavyband.BleScanner;
import com.breakfirst.bfsensorscavyband.BleScannerResultManager;
import com.breakfirst.bfsensorscavyband.BleSensorBase;
import com.breakfirst.bfsensorscavyband.BleSensorCavyband;
import com.breakfirst.bfsensorscavyband.BleSensorManager;

import java.util.HashMap;

public class ConnectDeviceActivity extends AppCompatActivity implements BleScannerResultManager.IBleDeviceManagerDelegate, BleSensorBase.IBleSensorDelegate
{
    private static final String TAG = ConnectDeviceActivity.class.getName();

    private BleScannerResultManager mScannerResultManager = null;
    private BleScanner mScanner = null;
    private BleSensorManager mSensorManager = null;
    private BleScanTimeoutTask mScanTimeoutTask = null;
    private Handler mHandler;

    private final static int REQUEST_ENABLE_BT = 1;

    private static int DEVICE_WIDGET_ID = R.layout.device_row_layout;
    private static int DEVICE_LIST_ID = R.id.devices_list;

    private LinearLayout mSensorListLayout = null;

    private HashMap<Integer, SensorWidget> mSensorsWidgets = new HashMap<Integer, SensorWidget>();

    private Button mSearch;

    private int mLoaderIndicatorId = R.id.loading_indicator;
    private ProgressBar mLoaderIndicator;


    // --------------------------------------------------------------------------------------------
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_connect_device);
        getViewReferences();

        mSensorManager = new BleSensorManager(this);
        mScannerResultManager = new BleScannerResultManager();
        mScanner = new BleScanner(this, mScannerResultManager);
        mHandler = new Handler();
        mScanTimeoutTask = new BleScanTimeoutTask(mHandler, mScannerResultManager);
        mScannerResultManager.setDelegate(this);

        Log.i(TAG,"CREATE");
    }

    @Override
    protected void onStart() {
        super.onStart();
        Log.i(TAG,"START");
    }

    @Override
    protected void onResume() {
        super.onResume();

        Log.i(TAG, "Checking...");
        mScanner.startScan();
        mScanTimeoutTask.run();
        mLoaderIndicator.setVisibility(View.VISIBLE);

        Toast.makeText(ConnectDeviceActivity.this, "Scanning...", Toast.LENGTH_LONG).show();
    }


    @Override
    protected void onPause() {
        super.onPause();
        mScanner.stopScan();
        mHandler.removeCallbacks(mScanTimeoutTask);
        mLoaderIndicator.setVisibility(View.GONE);
    }

    @Override
    protected void onDestroy()
    {
        mScannerResultManager.setDelegate(null);
        mScannerResultManager.reset();
        mSensorManager.setSensorManagerCallback(null);
        mSensorManager.reset();
        super.onDestroy();
    }


    protected void getViewReferences()
    {
        mSearch = (Button) findViewById(R.id.search_again);
        mSearch.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                reset();
                Log.i(TAG,"Checking...");
                mScanner.startScan();
                mScanTimeoutTask.run();
                Toast.makeText(ConnectDeviceActivity.this, "Scanning...", Toast.LENGTH_LONG).show();
            }
        });

        mSensorListLayout = (LinearLayout) findViewById(DEVICE_LIST_ID);
        mLoaderIndicator = (ProgressBar) findViewById(mLoaderIndicatorId);
    }


    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        // Check which request we're responding to
        if (requestCode == REQUEST_ENABLE_BT){
            // Make sure the request was successful
            if (resultCode == RESULT_OK)
            {
                mScanner.startScan();
                mScanTimeoutTask.run();
            }
            else if(resultCode == RESULT_CANCELED)
            {
                finish();
            }
            else
            {
                mScanner.stopScan();
                mHandler.removeCallbacks(mScanTimeoutTask);
            }
        }
        super.onActivityResult(requestCode, resultCode, data);
    }


    // -------------------------------------------------------------------------------------------
    public void reset() {
        Log.i(TAG,"RESET");
        mScannerResultManager.reset();
        mSensorManager.reset();

        mSensorListLayout.removeAllViews();
        mSensorsWidgets.clear();
    }


    @Override
    public void onNewDeviceFound(BleScanStatus a_scanStatus) {
        BleSensorCavyband cavyband = mSensorManager.tryGetBleSensorForAddress(a_scanStatus.getScanResult().getDevice().getAddress());

        if(cavyband == null) {
            cavyband = new BleSensorCavyband(a_scanStatus.getScanResult().getDevice(), mHandler, this);
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
        Log.e(TAG,"newSensor : " + a_sensor.getBleAddress());
    }

    @Override
    public void onSensorConnected(BleSensorBase a_sensor) {
        if(a_sensor.getIsExplored())
        {
            Log.e(TAG,"onSensorReconnected : " + a_sensor.getBleAddress());
        }
    }

    @Override
    public void onSensorDisconnected(BleSensorBase a_sensor) {
        Log.e(TAG,"onSensorDisconnected : " + a_sensor.getBleAddress());
    }

    @Override
    public void onDataReceived(BleSensorBase a_sensor, byte[] a_data) {
        Log.e(TAG,"onDataReceived");
    }
}
