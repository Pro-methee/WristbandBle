package com.breakfirst.bfsensorscavyband;

import android.bluetooth.le.ScanResult;

import java.util.Date;

/**
 * Created by Mhyshka on 17-Jan-18.
 */

public class BleScanStatus {
    //region Properties
    protected boolean mIsAdvertising = false;
    public boolean getIsAdvertising()
    {
        return mIsAdvertising;
    }

    public void setIsAdvertising(boolean a_isLost)
    {
        mIsAdvertising = a_isLost;
    }

    protected long mScanTick = 0L;
    public long getScanTick()
    {
        return mScanTick;
    }

    protected ScanResult mScanResult = null;
    public ScanResult getScanResult()
    {
        return mScanResult;
    }
    //endregion


    public BleScanStatus(ScanResult a_scanResult)
    {
        mScanResult = a_scanResult;
        mScanTick = new Date().getTime();
    }

    public void updateScanTick(ScanResult a_scanResult)
    {
        mScanResult = a_scanResult;
        mIsAdvertising = true;
        mScanTick = new Date().getTime();
    }
}
