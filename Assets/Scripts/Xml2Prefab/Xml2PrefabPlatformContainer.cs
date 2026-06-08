using UnityEditor;
using UnityEngine;

namespace Xml2Prefab
{
    public class Xml2PrefabPlatformContainer : MonoBehaviour
    {
        [SerializeField] private string _name;
        [SerializeField] private float _x;
        [SerializeField] private float _y;
        [SerializeField] private float _w;
        [SerializeField] private float _h;
        [SerializeField] private bool _sticky;
        [SerializeField] private string _transformations;
        [SerializeField] private ChoiceContainer _choice;

        public string Name => _name;
        public float X => transform.localPosition.x;
        public float Y => transform.localPosition.y;
        public float W => _w;
        public float H => _h;
        public bool Sticky => _sticky;
        public string Transformations => _transformations;
        public ChoiceContainer Choice
        {
            get => _choice;
            set => _choice = value;
        }

        public void Init(string n, float x, float y, float w, float h, bool s, string transformations, ChoiceContainer choice)
        {
            _name = n;
            _x = x;
            _y = y;
            _w = w;
            _h = h;
            _transformations = transformations;
            _choice = choice;
            _sticky = s;
        }

        public void ChangeHW(float h, float w)
        {
            _h = h;
            _w = w;
        }

        private void OnDrawGizmos()
        {
            Vector3 topLeft = transform.position;

            Vector3 right = transform.right * _w;
            Vector3 down = -transform.up * _h;

            Vector3 center = topLeft + right * 0.5f + down * 0.5f;
            Vector3 size = new Vector3(_w, _h, 0.01f);

            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(center, transform.rotation, Vector3.one);

            Gizmos.color = new Color(0f, 0f, 1f, 0.2f);
            Gizmos.DrawCube(Vector3.zero, size);

            Gizmos.matrix = oldMatrix;
        }
        

#if UNITY_EDITOR

        [CustomEditor(typeof(Xml2PrefabPlatformContainer))]
        public class Xml2PrefabPlatformContainerEditor : Editor
        {
            private void OnSceneGUI()
            {
                Xml2PrefabPlatformContainer platform = (Xml2PrefabPlatformContainer)target;

                Transform t = platform.transform;

                Vector3 topLeft = t.position;

                float w = platform.W;
                float h = platform.H;

                Vector3 topRight = topLeft + Vector3.right * w;
                Vector3 bottomLeft = topLeft + Vector3.down * h;
                Vector3 bottomRight = topLeft + Vector3.right * w + Vector3.down * h;

                Vector3 topHandle = (topLeft + topRight) * 0.5f;
                Vector3 rightHandle = (topRight + bottomRight) * 0.5f;
                Vector3 bottomHandle = (bottomLeft + bottomRight) * 0.5f;
                Vector3 leftHandle = (topLeft + bottomLeft) * 0.5f;

                Handles.color = Color.cyan;

                EditorGUI.BeginChangeCheck();

                float handleSize = HandleUtility.GetHandleSize(topLeft) * 0.08f;

                Vector3 newTopHandle = Handles.FreeMoveHandle(
                    topHandle,
                    handleSize,
                    Vector3.zero,
                    Handles.SphereHandleCap
                );

                Vector3 newRightHandle = Handles.FreeMoveHandle(
                    rightHandle,
                    handleSize,
                    Vector3.zero,
                    Handles.SphereHandleCap
                );

                Vector3 newBottomHandle = Handles.FreeMoveHandle(
                    bottomHandle,
                    handleSize,
                    Vector3.zero,
                    Handles.SphereHandleCap
                );

                Vector3 newLeftHandle = Handles.FreeMoveHandle(
                    leftHandle,
                    handleSize,
                    Vector3.zero,
                    Handles.SphereHandleCap
                );

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(platform, "Resize Platform");
                    Undo.RecordObject(t, "Move Platform");

                    Vector3 newPosition = topLeft;
                    float newW = w;
                    float newH = h;

                    if (newTopHandle != topHandle)
                    {
                        float deltaY = newTopHandle.y - topHandle.y;

                        newPosition.y += deltaY;
                        newH += deltaY;
                    }

                    if (newRightHandle != rightHandle)
                    {
                        float deltaX = newRightHandle.x - rightHandle.x;

                        newW += deltaX;
                    }

                    if (newBottomHandle != bottomHandle)
                    {
                        float deltaY = newBottomHandle.y - bottomHandle.y;

                        newH -= deltaY;
                    }

                    if (newLeftHandle != leftHandle)
                    {
                        float deltaX = newLeftHandle.x - leftHandle.x;

                        newPosition.x += deltaX;
                        newW -= deltaX;
                    }

                    var snap = EditorSnapSettings.move;

                    newW = Snap(Mathf.Max(0f, newW), snap.x);
                    newH = Snap(Mathf.Max(0f, newH), snap.y);

                    platform.ChangeHW(newH, newW);
                    t.position = newPosition;

                    EditorUtility.SetDirty(platform);
                    EditorUtility.SetDirty(t);
                }

                Handles.color = Color.yellow;

                Handles.DrawLine(topLeft, topRight);
                Handles.DrawLine(topRight, bottomRight);
                Handles.DrawLine(bottomRight, bottomLeft);
                Handles.DrawLine(bottomLeft, topLeft);

                Handles.Label(
                    topLeft + Vector3.up * 0.25f,
                    $"{platform.Name}\nW: {platform.W:0.###}, H: {platform.H:0.###}"
                );

                float Snap(float value, float snap)
                {
                    return Mathf.Round(value / snap) * snap;
                }
            }
        }
#endif

    }
}
