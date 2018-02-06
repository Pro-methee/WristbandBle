package com.breakfirst.bfsensorscavyband;

import android.content.Context;
import android.os.Handler;

/**
 * Created by Mhyshka on 17-Jan-18.
 */

public class BleConnectTask implements Runnable {
    protected Handler mHandler;
    protected Context mContext;
    protected BleSensorBase mSensor;
    protected boolean mIsRunning = false;
    public boolean getIsRunning()
    {
        return mIsRunning;
    }

    private static final long INTERVAL = 1000;

    public BleConnectTask(Handler a_handler, Context a_context, BleSensorBase a_sensor)
    {
        mHandler = a_handler;
        mContext = a_context;
        mSensor = a_sensor;
    }

    @Override
    public void run() {
        mIsRunning = true;
        try {
            if(mSensor.connectGatt(mContext))
            {

            }
            else
            {
                //Retry
                mHandler.postDelayed(this, INTERVAL);
            }
        }
        finally {

        }
    }

    public void cancelTask()
    {
        mIsRunning = false;
        mHandler.removeCallbacks(this);
    }
}