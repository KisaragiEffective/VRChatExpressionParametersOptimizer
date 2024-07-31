using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;

namespace VRChatExpressionParametersOptimizer.Runtime
{
    public class MarkerExclusion : MonoBehaviour, IEditorOnly
    {
        public bool Applies;
        [FormerlySerializedAs("Regex")]
        public string ExclusionNamePattern;
        public string Comment;
    }
}
