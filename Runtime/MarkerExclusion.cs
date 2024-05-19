using UnityEngine;

namespace VRChatExpressionParametersOptimizer.Runtime
{
    public class MarkerExclusion : ScriptableObject
    {
        public bool Applies;
        public string Regex;
        public string Comment;
    }
}
