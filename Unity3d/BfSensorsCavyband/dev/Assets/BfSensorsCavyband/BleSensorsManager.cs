using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BfSensorsCavyband
{
    public class BleSensorsManager
    {
        public static bool ENABLE_LOG = false;
        private static int LIBRARY_VERSION_CODE = 3;

        private static BleSensorsManagerBase S_INSTANCE;
        public static BleSensorsManagerBase Instance
        {
            get
            {
                if (S_INSTANCE == null)
                {
#if !UNITY_EDITOR && UNITY_ANDROID
                    S_INSTANCE = new BleSensorsManagerAndroid();
#else
#endif
                }

                return S_INSTANCE;
            }
        }
    }
}