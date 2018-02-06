package com.breakfirst.bfsensorscavyband;

import android.app.Activity;
import android.app.AlertDialog;
import android.app.Dialog;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothManager;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.location.LocationManager;
import android.os.Build;
import android.provider.Settings;

/**
 * Created by Mhyshka on 23-Jan-18.
 */

public class BleSupport {
    private static final String TAG = BleSupport.class.getName();

    private final static int REQUEST_ENABLE_BT = 1;
    private final static int REQUEST_ENABLE_LOCATION = 2;
    private static final int PERMISSION_REQUEST_COARSE_LOCATION = 2;

    BluetoothAdapter mBluetoothAdapter;

    private Activity mActivity;

    public BleSupport(Activity a_activity)
    {
        mActivity = a_activity;
    }

    public boolean isBluetoothSupported()
    {
        if(!mActivity.getPackageManager().hasSystemFeature(PackageManager.FEATURE_BLUETOOTH_LE))
            return false;

        final BluetoothManager bluetoothManager = (BluetoothManager) mActivity.getSystemService(Context.BLUETOOTH_SERVICE);
        mBluetoothAdapter = bluetoothManager.getAdapter();

        // Checks if Bluetooth is supported on the device.
        if (mBluetoothAdapter == null) {
            return false;
        }

        return true;
    }

    public boolean isBluetoothEnabled()
    {
        return mBluetoothAdapter != null && mBluetoothAdapter.isEnabled();
    }

    public boolean requestBluetoothActivation()
    {
        if (mBluetoothAdapter == null || !mBluetoothAdapter.isEnabled())
        {
            Intent enableBtIntent = new Intent(BluetoothAdapter.ACTION_REQUEST_ENABLE);
            mActivity.startActivityForResult(enableBtIntent, REQUEST_ENABLE_BT);
            return false;
        }
        else
        {
            return true;
        }
    }

    public boolean isLocationServiceEnabled()
    {
        final LocationManager locationManager = (LocationManager) mActivity.getSystemService(Context.LOCATION_SERVICE);

        if(!locationManager.isProviderEnabled(LocationManager.GPS_PROVIDER) ||
                !locationManager.isProviderEnabled(LocationManager.NETWORK_PROVIDER))
            return false;

        return true;
    }

    public boolean requestLocationActivation()
    {
        return requestLocationActivation("Location Services Not Active", "Please enable Location Services and GPS", "OK");
    }

    public boolean requestLocationActivation(String a_title, String a_description, String a_okayButton)
    {
        if(!isLocationServiceEnabled())
        {
            // Build the alert dialog
            AlertDialog.Builder builder = new AlertDialog.Builder(mActivity);
            builder.setTitle(a_title);
            builder.setMessage(a_description);
            builder.setPositiveButton(a_okayButton, new DialogInterface.OnClickListener() {
                public void onClick(DialogInterface dialogInterface, int i) {
                    // Show location settings when the user acknowledges the alert dialog
                    Intent intent = new Intent(Settings.ACTION_LOCATION_SOURCE_SETTINGS);
                    mActivity.startActivity(intent);
                }
            });
            Dialog alertDialog = builder.create();
            alertDialog.setCanceledOnTouchOutside(false);
            alertDialog.show();
            return true;
        }
        else
            return false;
    }

    public boolean isLocationPermissionGranted()
    {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M)
        {
            return mActivity.checkSelfPermission( android.Manifest.permission.ACCESS_COARSE_LOCATION) == PackageManager.PERMISSION_GRANTED;
        }
        else
            return true;
    }

    public boolean requestLocationPermission()
    {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M)
        {
            if (mActivity.checkSelfPermission( android.Manifest.permission.ACCESS_COARSE_LOCATION) != PackageManager.PERMISSION_GRANTED)
            {
                mActivity.requestPermissions(new String[]{android.Manifest.permission.ACCESS_COARSE_LOCATION}, PERMISSION_REQUEST_COARSE_LOCATION);
                return true;
            }
        }

        return false;
    }
}
