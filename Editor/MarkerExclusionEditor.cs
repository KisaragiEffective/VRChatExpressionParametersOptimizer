#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
using VRChatExpressionParametersOptimizer.Runtime;

namespace VRChatExpressionParametersOptimizer.Editor
{
    // [CustomEditor(typeof(VRChatExpressionParametersOptimizerMarker))]
    public class MarkerExclusionEditor: UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            var target = (this.target as VRChatExpressionParametersOptimizerMarker)!;
            var fold = new Foldout();
            fold.Add(new Label("除外設定"));
            return root;
        }
    }
}
#endif
