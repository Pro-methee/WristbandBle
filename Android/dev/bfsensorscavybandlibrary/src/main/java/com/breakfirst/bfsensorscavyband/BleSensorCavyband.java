package com.breakfirst.bfsensorscavyband;

import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothGattCharacteristic;
import android.bluetooth.BluetoothGattService;
import android.content.Context;
import android.os.Handler;
import android.util.Log;

import java.util.UUID;

/**
 * Created by Alexandre Pinto on 10/10/2016.
 */
public class BleSensorCavyband extends BleSensorBase
{
    ////////////////////////////////
    ////// Cavyband UUID
    //////////////////////////////
    static String SERVICE_UUID_STR = "14839ac4-7d7e-415c-9a42-167340cf2339";
    public static UUID SERVICE_UUID = UUID.fromString(SERVICE_UUID_STR);

    static String OUTPUT_CHARACTERISTIC_UUID_STR = "0734594A-A8E7-4B1A-A6B1-CD5243059A57";
    public static UUID OUTPUT_CHARACTERISTIC_UUID = UUID.fromString(OUTPUT_CHARACTERISTIC_UUID_STR);
    //To read info from device

    static String INPUT_CHARACTERISTIC_UUID_STR = "8B00ACE7-EB0B-49B0-BBE9-9AEE0A26E1A3";
    public static UUID INPUT_CHARACTERISTIC_UUID = UUID.fromString(INPUT_CHARACTERISTIC_UUID_STR);
    //To send info to device

    protected BluetoothGattCharacteristic mOutputCharacteristic = null;
    protected BluetoothGattCharacteristic mInputCharacteristic = null;

    protected byte[] mLastDataReceived = null;
    public byte[] getLastDataReceived()
    {
        return mLastDataReceived;
    }

    @Override
    public String getDisplayName()
    {
        return "Cavyband-" + getBleAddress();
    }

    /*protected int mGameModeInterval = 20;
    public int getGameModeInterval()
    {
        return mGameModeInterval;
    }*/

    //region Constructor and Destructor
    public BleSensorCavyband(BluetoothDevice a_device, Handler a_handler, Context a_context)
    {
        super(a_device, a_handler, a_context);
    }
    //endregion


    public void sendCommand(String a_command)
    {
        mInputCharacteristic.setValue(a_command);
        mGattWrapper.getBluetoothGatt().writeCharacteristic(mInputCharacteristic);
        Log.e(TAG,"Sending command : " + a_command);
    }

    /// GAME MODE
   /* protected boolean mIsInGameMode = false;
    public void enterGameMode(int a_interval)
    {
        mInputCharacteristic.setValue("%OPR=3,4," + String.valueOf(mGameModeInterval) + "\\n");
        mBleDeviceWrapper.getBluetoothGatt().writeCharacteristic(mInputCharacteristic);
        Log.e(TAG,"Enter game mode command");
        mIsInGameMode = true;
        mGameModeInterval = a_interval;
    }

    public void exitGameMode()
    {
        mInputCharacteristic.setValue("%OPR=3,0\\n");
        mBleDeviceWrapper.getBluetoothGatt().writeCharacteristic(mInputCharacteristic);
        Log.e(TAG,"Exit game mode command");
        mIsInGameMode = false;
    }*/

    @Override
    public void onCharacteristicRead(UUID a_uuid, byte[] a_data) {
        onCharacteristicChanged(a_uuid, a_data);
    }

    //protected ByteArrayInputStream mByteInputStream = null;
    @Override
    public void onCharacteristicChanged(UUID a_uuid, byte[] a_data) {
        if(a_uuid.equals(mOutputCharacteristic.getUuid()))
        {
            mLastDataReceived = a_data;

            if(mSensorDelegate != null)
                mSensorDelegate.onDataReceived(this, a_data);
            /*try
            {
                mByteInputStream = new ByteArrayInputStream(a_data);
                String header = parseHeader(mByteInputStream);//parseHeader(mByteInputStream);
                Log.e(TAG, "Header : " + header + " / " + a_data.length);
                mByteInputStream.close();


                if(header.equals("d1"))
                {
                    if(mIsInGameMode)
                        exitGameMode();
                    else
                        enterGameMode(50);
                }
                else if(header.equals("a1"))
                {
                    parseMotionUpdateMessage(mByteInputStream);
                }
            }
            catch(Exception e)
            {
                Log.e(TAG,"Crashed in mByteInputSream : " + e.getMessage());
            }*/
        }
    }

    @Override
    public void onDescriptorRead(UUID a_uuid, byte[] a_data) {

    }


    @Override
    public void onServiceExplorationComplete() {

        BluetoothGattService service = mGattWrapper.getBluetoothGatt().getService(SERVICE_UUID);
        mOutputCharacteristic = service.getCharacteristic(OUTPUT_CHARACTERISTIC_UUID);
        mGattWrapper.getBluetoothGatt().setCharacteristicNotification(mOutputCharacteristic, true);

        mInputCharacteristic = service.getCharacteristic(INPUT_CHARACTERISTIC_UUID);

        super.onServiceExplorationComplete();
    }
    /*
    ///Message Parsing
    protected String parseHeader(ByteArrayInputStream a_byteArrayStream)
    {
        int curByte = mByteInputStream.read();//0x
        //String header2 = Integer.toHexString(curByte);

        curByte = mByteInputStream.read();//FF
        String header = Integer.toHexString(curByte);

        //Log.e(TAG, "Headers : " + header + " / " + header2);

        return header;
    }

    protected void parseMotionUpdateMessage(ByteArrayInputStream a_byteArrayStream)
    {
        ByteBuffer bb = ByteBuffer.allocate(2);
        bb.order(ByteOrder.BIG_ENDIAN);

        float rx ,ry, rz, rw;
        bb.put((byte)mByteInputStream.read());
        bb.put((byte)mByteInputStream.read());
        rw = bb.getShort(0)/16384f;

        bb.clear();
        bb.put((byte)mByteInputStream.read());
        bb.put((byte)mByteInputStream.read());
        rx = bb.getShort(0)/16384f;

        bb.clear();
        bb.put((byte)mByteInputStream.read());
        bb.put((byte)mByteInputStream.read());
        ry = bb.getShort(0)/16384f;

        bb.clear();
        bb.put((byte)mByteInputStream.read());
        bb.put((byte)mByteInputStream.read());
        rz = bb.getShort(0)/16384f;
        Log.e(TAG, "Rot : " + rx + " / " + ry + " / " + rz + " / " + rw);


        float ax,ay, az;
        bb.clear();
        bb.put((byte)mByteInputStream.read());
        bb.put((byte)mByteInputStream.read());
        ax = bb.getShort(0)/1000f;

        bb.clear();
        bb.put((byte)mByteInputStream.read());
        bb.put((byte)mByteInputStream.read());
        ay = bb.getShort(0)/1000f;

        bb.clear();
        bb.put((byte)mByteInputStream.read());
        bb.put((byte)mByteInputStream.read());
        az = bb.getShort(0)/1000f;
        //Log.e(TAG, "acc : " + ax + " / " + ay + " / " + az);
    }*/
}
