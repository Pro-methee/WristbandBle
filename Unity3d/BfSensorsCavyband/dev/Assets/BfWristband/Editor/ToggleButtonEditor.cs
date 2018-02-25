using UnityEditor;

namespace BfWristband.Api
{
    [CustomEditor(typeof(ToggleButton))]
    public class MenuButtonEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            ToggleButton targetButton = (ToggleButton)target;

            base.DrawDefaultInspector();
            // Show default inspector property editor
            //DrawDefaultInspector();
        }
    }
}


