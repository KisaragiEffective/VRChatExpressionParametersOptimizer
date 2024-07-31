using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using nadena.dev.ndmf;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRChatExpressionParametersOptimizer.Runtime;
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
            if (!ctx.AvatarRootObject.TryGetComponent<ExpressionParameterOptimizerSetting>(out var setting))
            {
                Debug.Log($"skipped: attach {nameof(ExpressionParameterOptimizerSetting)} to enable");
                return;
            }
            
            Process0(ctx.AvatarDescriptor, setting);
        }

        // TODO: RemoveOrphanに分割
        private static void Process0(VRCAvatarDescriptor ad, ExpressionParameterOptimizerSetting setting)
        {
            // nothing to do (expecting custom plugins set this flag accordingly)
            if (!ad.customizeAnimationLayers)
            {
                return;
            }

            var layerCollections = new List<IEnumerable<VRCAvatarDescriptor.CustomAnimLayer>> { ad.baseAnimationLayers, ad.specialAnimationLayers, };

            var enabledExclusionPatterns = setting.Exclusions
                .Where(ex => ex.Applies)
                .Select(TryConstructExclusionPattern)
                .ToList();

            var excludedParameterNames = ad.expressionParameters.parameters
                .Where(IsExcludedParameterByPreComputedPatterns)
                .Select(p => p.name)
                .ToList();
            
            if (excludedParameterNames.Count == ad.expressionParameters.parameters.Length)
            {
                // everything is excluded from optimizer. nothing to do.
                Debug.Log("early return: all parameters are excluded from optimizer");
                return;
            }
            
            var parametersToBeLeft = layerCollections.SelectMany(toChain => toChain)
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
                .Chain(excludedParameterNames)
                .ToHashSet();

            Debug.Log("actually used parameters\n" + string.Join("\n", parametersToBeLeft));

            var clonedExParams = Object.Instantiate(ad.expressionParameters);
            clonedExParams.name += "(slimed down)";
            clonedExParams.parameters = clonedExParams.parameters.Where(x => parametersToBeLeft.Contains(x.name)).ToArray();
            Debug.Log($"slimed down: {CountBits(ad.expressionParameters.parameters)} bits -> {CountBits(clonedExParams.parameters)} bits");
            
            RecordAndAssign(ref ad.expressionParameters, clonedExParams);
            return;

            bool IsExcludedParameterByPreComputedPatterns(VRCExpressionParameters.Parameter p)
            {
                return enabledExclusionPatterns.Any(matcher =>
                {
                    try
                    {
                        return matcher.Item1.Match(p.name).Success;
                    }
                    catch (RegexMatchTimeoutException e)
                    {
                        ErrorReport.ReportError(new TooLongRegexMatchingReport(e, matcher.Item2));
                        return true;
                    }
                });
            }

            (Regex, int) TryConstructExclusionPattern(MarkerExclusion ex, int nth)
            {
                try
                {
                    // フリーズ防止
                    var second = new TimeSpan(0, 0, 0, 1);
                    return (new Regex(ex.ExclusionNamePattern, RegexOptions.None, second), nth);
                }
                catch (ArgumentNullException)
                {
                    ErrorReport.ReportError(new AbsentExclusionPatternReport(setting, nth));
                    return (NeverMatch, nth);
                }
                catch (ArgumentException e)
                {
                    ErrorReport.ReportError(new InvalidRegexPatternReport(setting, e, nth));
                    return (NeverMatch, nth);
                }
            }
        }

        private static void RecordAndAssign<TDest, TSource>(ref TDest lhs, TSource source) 
            where TSource: TDest
            where TDest: Object
        {
            ObjectRegistry.RegisterReplacedObject(lhs, source);
            lhs = source;
        }

        private static readonly Regex NeverMatch = new(@"^\b$");
    }

    internal class TooLongRegexMatchingReport : IError
    {
        private readonly RegexMatchTimeoutException _e;
        private readonly int _nth;

        public TooLongRegexMatchingReport(RegexMatchTimeoutException regexMatchTimeoutException, int nth)
        {
            this._e = regexMatchTimeoutException;
            this._nth = nth;
        }

        public ErrorSeverity Severity => ErrorSeverity.Error;
        public VisualElement CreateVisualElement(ErrorReport report)
        {
            return new VisualElement();
        }

        public string ToMessage() =>
            $"[VEPO-0201] Regex {_e.Input} [on #{_nth}] was timed-out after {_e.MatchTimeout.TotalMilliseconds} milliseconds.";

        public void AddReference(ObjectReference obj)
        {
        }
    }

    internal class InvalidRegexPatternReport : IError
    {
        private readonly ExpressionParameterOptimizerSetting _setting;
        private readonly ArgumentException _e;
        private readonly int _invalid;

        public InvalidRegexPatternReport(ExpressionParameterOptimizerSetting setting, ArgumentException argumentException, int invalidIndex)
        {
            this._setting = setting;
            this._e = argumentException;
            this._invalid = invalidIndex;
        }

        public ErrorSeverity Severity => ErrorSeverity.Error;
        public VisualElement CreateVisualElement(ErrorReport report)
        {
            return new VisualElement();
        }

        public string ToMessage() => 
            $"[VEPO-0102] Pattern must be valid Regex, but exclusion pattern #{_invalid} is invalid: {_e.Message}.\n" +
                "This exclusion is ignored. Please refer Microsoft's manual to how to fix.";

        public void AddReference(ObjectReference obj)
        {
        }
    }

    internal class AbsentExclusionPatternReport : IError
    {
        private readonly ExpressionParameterOptimizerSetting setting;
        private readonly int invalidIndex;

        public AbsentExclusionPatternReport(ExpressionParameterOptimizerSetting setting, int i)
        {
            this.setting = setting;
            this.invalidIndex = i;
        }

        public ErrorSeverity Severity => ErrorSeverity.Error;
        
        public VisualElement CreateVisualElement(ErrorReport report)
        {
            return new VisualElement();
        }

        public string ToMessage() =>
            $"[VEPO-0101] Pattern must not be absent, but exclusion pattern #{invalidIndex} is absent. This exclusion is ignored.";

        public void AddReference(ObjectReference obj)
        {
        }
    }
}
