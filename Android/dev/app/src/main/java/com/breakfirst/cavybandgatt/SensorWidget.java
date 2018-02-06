package com.breakfirst.cavybandgatt;

import android.view.View;
import android.widget.CheckBox;
import android.widget.TextView;

import com.breakfirst.cavybandgatt.R;

/**
 * Created by Alexandre Pinto on 12/10/2016.
 */
public class SensorWidget
{
    protected View mRootView = null;
    protected CheckBox mCheckbox = null;
    protected TextView mAccelerationLabel = null;
    protected TextView mRotationLabel = null;

    private static int mDeviceCheckboxId = R.id.device_check_box;

    public SensorWidget(View a_rootView, String a_displayName)
    {
        mRootView = a_rootView;

        mCheckbox = (CheckBox)mRootView.findViewById(mDeviceCheckboxId);
        mAccelerationLabel = (TextView)mRootView.findViewById(R.id.device_acceleration_text);
        mRotationLabel = (TextView)mRootView.findViewById(R.id.device_rotation_text);

      /*  setAccelerationNotAvailable();
        setRotationNotAvailable();*/

        mCheckbox.setText(a_displayName);
        mCheckbox.setChecked(false);

        a_rootView.invalidate();
    }

   /* public void setAcceleration(Vector3 a_acceleration)
    {
        mAccelerationLabel.setText("Acceleration : " + a_acceleration.toString());
    }

    public void setRotation(Quaternion a_rotation)
    {
        mRotationLabel.setText("Rotation : " + a_rotation.toString());
    }*/

    public void setAccelerationNotAvailable()
    {
        mAccelerationLabel.setText("Acceleration : Not available.");
    }

    public void setRotationNotAvailable()
    {
        mRotationLabel.setText("Rotation : Not available.");
    }

    public void setChecked(boolean a_value)
    {
        mCheckbox.setChecked(a_value);
    }
}
