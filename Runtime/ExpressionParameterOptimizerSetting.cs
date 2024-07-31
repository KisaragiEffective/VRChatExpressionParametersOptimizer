using UnityEngine;
using VRC.SDKBase;

namespace VRChatExpressionParametersOptimizer.Runtime
{
    [DisallowMultipleComponent]
    public class ExpressionParameterOptimizerSetting : MonoBehaviour, IEditorOnly
    {
        public MarkerExclusion[] Exclusions;
    }
}
