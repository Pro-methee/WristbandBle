package com.breakfirst.bfsensorscavyband;

import android.os.Handler;

/**
 * Created by Mhyshka on 17-Jan-18.
 */

public class BleScanTimeoutTask implements Runnable {
    protected Handler mHandler;
    protected BleScannerResultManager mDeviceManager;

    private static final long INTERVAL = 1000;

    public BleScanTimeoutTask(Handler a_handler, BleScannerResultManager a_deviceManager)
    {
        mHandler = a_handler;
        mDeviceManager = a_deviceManager;
    }

    @Override
    public void run() {
        try {
            mDeviceManager.updateAndRemoveLostDevices();
        } finally {
            // 100% guarantee that this always happens, even if
            // your update method throws an exception
            mHandler.postDelayed(this, INTERVAL);
        }
    }
}