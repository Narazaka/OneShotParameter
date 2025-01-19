using UnityEngine;
using VRC.SDKBase;

namespace Narazaka.VRChat.OneShotParameter
{
    public class OneShotParameter : MonoBehaviour, IEditorOnly
    {
        [SerializeField] public string ParameterName;
        [SerializeField] public float ParameterDefaultValue;
        [SerializeField] public float Duration = 1f;
        [SerializeField] public bool LocalOnly = true;
    }
}
