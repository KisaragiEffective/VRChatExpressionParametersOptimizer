#if KISARAGI_VRCHAT_EXPARAM_OPTIMIZER_NDMF && UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using nadena.dev.ndmf;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using Object = UnityEngine.Object;
using static VRChatExpressionParametersOptimizer.Editor.RecursiveWalker;

[assembly: ExportsPlugin(typeof(VRChatExpressionParametersOptimizer.Editor.NDMF.OptimizingPlugin))]
namespace VRChatExpressionParametersOptimizer.Editor.NDMF
{
    public class OptimizingPlugin: Plugin<OptimizingPlugin>
    {
        protected override void Configure()
        {
            InPhase(BuildPhase.Optimizing)
                .Run("io.github.kisaragieffective.VRChatExpressionParametersOptimizer", Process);
        }

        private static void Process(BuildContext ctx)
        {
            Process0(ctx.AvatarDescriptor);
        }

        private static void Process0(VRCAvatarDescriptor ad)
        {
            // nothing to do (expecting custom plugins set this flag accordingly)
            if (!ad.customizeAnimationLayers)
            {
                return;
            }

            var layerCollections = new List<IEnumerable<VRCAvatarDescriptor.CustomAnimLayer>> { ad.baseAnimationLayers, ad.specialAnimationLayers, };

            var actuallyReferencedVariable = layerCollections.SelectMany(toChain => toChain)
                .Where(x => x.animatorController != null)
                // 今我々が居るのはUnityEditorの空間なのでこの仮定は成り立ってほしい。そうでなければさっさと死ぬに限る。
                .Select(x => x.animatorController as AnimatorController ?? throw new Exception())
                .SelectMany(x => x.layers)
                .Select(x =>
                {
                    Debug.Log($"checking {x.name}");
                    return x.stateMachine;
                })
                .SelectMany(GatherTransitions)
                .SelectMany(trans => trans.conditions)
                .Select(cond => cond.parameter)
                .Chain(ReferencedParametersFromMenu(ad).Select(x => x.name))
                .ToHashSet();

            Debug.Log("actually used parameters\n" + string.Join("\n", actuallyReferencedVariable));

            var clonedExParams = Object.Instantiate(ad.expressionParameters);
            clonedExParams.name += "(slimed down)";
            clonedExParams.parameters = clonedExParams.parameters.Where(x => actuallyReferencedVariable.Contains(x.name)).ToArray();
            Debug.Log($"slimed down: {CountBits(ad.expressionParameters.parameters)} bits -> {CountBits(clonedExParams.parameters)} bits");
            
            RecordAndAssign(ref ad.expressionParameters, clonedExParams);
        }

        private static void RecordAndAssign<TDest, TSource>(ref TDest lhs, TSource source) 
            where TSource: TDest
            where TDest: Object
        {
            ObjectRegistry.RegisterReplacedObject(lhs, source);
            lhs = source;
        }
    }
}
#endif
