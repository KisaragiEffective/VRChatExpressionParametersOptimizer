using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

[assembly: InternalsVisibleTo("VRChatExpressionParametersOptimizer.Editor.NDMF")]

namespace VRChatExpressionParametersOptimizer.Editor
{
    internal static class RecursiveWalker
    {
        internal static int CountBits(IEnumerable<VRCExpressionParameters.Parameter> parameters) =>
            parameters.Aggregate(0, (bits, param) => bits + VRCExpressionParameters.TypeCost(param.valueType));
        
        internal static IEnumerable<AnimatorTransitionBase> GatherTransitions(AnimatorStateMachine asm)
        {
            var transitions = new List<IEnumerable<AnimatorTransitionBase>> { asm.entryTransitions, asm.anyStateTransitions, };
            Debug.Log($"VEPO - {asm.name}: checking {asm.stateMachines.Length} state machines");
            if (asm.stateMachines.Length > 0)
            {
                var op = asm.stateMachines.Select(y =>
                    GatherTransitions(y.stateMachine));
                transitions.AddRange(op);
            }
            else
            {
                transitions.AddRange(asm.stateMachines.Select(y => asm.GetStateMachineTransitions(y.stateMachine)));
                transitions.AddRange(asm.states.Select(x => x.state).Select(x => x.transitions));
            }
                    
            Debug.Log($"VEPO - {asm.name}: checking {transitions.Count} transitions");

            return transitions.SelectMany(op => op);
        }

        internal static IEnumerable<VRCExpressionsMenu.Control.Parameter> ReferencedParametersFromMenu(VRCAvatarDescriptor ad)
        {
            return Walk(ad.expressionsMenu);

            IEnumerable<VRCExpressionsMenu.Control.Parameter> Walk(VRCExpressionsMenu menu)
            {
                return menu.controls.SelectMany(x => new[] { x.parameter }
                    .Chain(x.subParameters ?? Array.Empty<VRCExpressionsMenu.Control.Parameter>())
                    .Chain(x.subMenu != null ? Walk(x.subMenu) : Array.Empty<VRCExpressionsMenu.Control.Parameter>()));
            }
        }
    }
}