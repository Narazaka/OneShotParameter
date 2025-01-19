using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using nadena.dev.ndmf;
using nadena.dev.modular_avatar.core;
using Narazaka.VRChat.AvatarParametersUtil;
using System.Linq;
using UnityEditor.Animations;
using VRC.SDK3.Avatars.Components;
using UnityEditor;

[assembly: ExportsPlugin(typeof(Narazaka.VRChat.OneShotParameter.Editor.OneShotParameterPlugin))]

namespace Narazaka.VRChat.OneShotParameter.Editor
{
    public class OneShotParameterPlugin : Plugin<OneShotParameterPlugin>
    {
        public override string DisplayName => "net.narazaka.vrchat.one_shot_parameter";
        public override string QualifiedName => "One Shot Parameter";

        protected override void Configure()
        {
            InPhase(BuildPhase.Generating).BeforePlugin("nadena.dev.modular-avatar").Run("OneShotParameter", ctx =>
            {
                var oneShotParameters = ctx.AvatarRootObject.GetComponentsInChildren<OneShotParameter>(true);
                if (oneShotParameters.Length == 0) return;
                var parameterByName = ParameterInfo.ForContext(ctx).GetParametersForObject(ctx.AvatarRootObject).ToDistinctSubParameters().ToDictionary(p => p.EffectiveName);
                var parameterNames = oneShotParameters.Select(d => d.ParameterName).Distinct().ToArray();
                if (parameterNames.Length != oneShotParameters.Length)
                {
                    throw new System.InvalidOperationException("Duplicate parameter names");
                }
                var notFoundParameters = parameterNames.Where(p => !parameterByName.ContainsKey(p)).ToArray();
                if (notFoundParameters.Length > 0)
                {
                    throw new System.InvalidOperationException($"Parameters {string.Join(", ", notFoundParameters)} not found");
                }
                var invalidTypeParameters = parameterNames.Where(p => parameterByName[p].ParameterType != AnimatorControllerParameterType.Bool && parameterByName[p].ParameterType != AnimatorControllerParameterType.Int).ToArray();
                if (invalidTypeParameters.Length > 0)
                {
                    throw new System.InvalidOperationException($"Parameters {string.Join(", ", invalidTypeParameters)} are not bool or int");
                }

                var nopClip = MakeEmptyAnimationClip(1f / 60);
                var oneSecClip = MakeEmptyAnimationClip(1f);
                var animator = new AnimatorController();
                foreach (var parameterName in parameterNames)
                {
                    if (parameterByName.TryGetValue(parameterName, out var parameter))
                    {
                        animator.AddParameter(parameter.ToAnimatorControllerParameter());
                    }
                }

                foreach (var oneShotParameter in oneShotParameters)
                {
                    var parameter = parameterByName[oneShotParameter.ParameterName];
                    var layer = AddLastLayer(animator, $"One Shot Parameter for {oneShotParameter.ParameterName}");

                    layer.stateMachine.entryPosition = new Vector3(0, 0, 0);
                    layer.stateMachine.anyStatePosition = new Vector3(0, -100, 0);
                    layer.stateMachine.exitPosition = new Vector3(0, 400, 0);
                    var idleState = AddConfiguredState(layer.stateMachine, "idle", nopClip, new Vector3(0, 100, 0));
                    layer.stateMachine.defaultState = idleState;
                    var waitState = AddConfiguredState(layer.stateMachine, "wait", oneSecClip, new Vector3(0, 200, 0));
                    var resetState = AddConfiguredState(layer.stateMachine, "reset", nopClip, new Vector3(0, 300, 0));
                    resetState.behaviours = new StateMachineBehaviour[]
                    {
                        new VRCAvatarParameterDriver
                        {
                            parameters = new List<VRC.SDKBase.VRC_AvatarParameterDriver.Parameter>
                            {
                                new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter
                                {
                                    type = VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Set,
                                    name = oneShotParameter.ParameterName,
                                    value = oneShotParameter.ParameterDefaultValue,
                                },
                            },
                            localOnly = oneShotParameter.LocalOnly,
                        },
                    };

                    var toWait = idleState.AddTransition(waitState);
                    toWait.hasExitTime = false;
                    toWait.exitTime = 0f;
                    toWait.hasFixedDuration = true;
                    toWait.duration = 0f;
                    AddCondition(toWait, oneShotParameter, parameter, false);
                    var toResetByCondition = waitState.AddTransition(resetState);
                    toResetByCondition.hasExitTime = false;
                    toResetByCondition.exitTime = 0f;
                    toResetByCondition.hasFixedDuration = true;
                    toResetByCondition.duration = 0f;
                    AddCondition(toResetByCondition, oneShotParameter, parameter, true);
                    var toResetByTime = waitState.AddTransition(resetState);
                    toResetByTime.hasExitTime = true;
                    toResetByTime.exitTime = oneShotParameter.Duration;
                    toResetByTime.hasFixedDuration = true;
                    toResetByTime.duration = 0f;
                    var toExit = resetState.AddExitTransition();
                    toExit.hasExitTime = true;
                    toExit.exitTime = 0f;
                    toExit.hasFixedDuration = true;
                    toExit.duration = 0f;
                    Object.DestroyImmediate(oneShotParameter);
                }

                var mergeAnimator = ctx.AvatarRootObject.AddComponent<ModularAvatarMergeAnimator>();
                mergeAnimator.animator = animator;
                mergeAnimator.matchAvatarWriteDefaults = true;
            });
        }

        static AnimationClip MakeEmptyAnimationClip(float duration)
        {
            var clip = new AnimationClip();
            clip.frameRate = 60;
            clip.SetCurve("__OneShotParameter_Empty__", typeof(GameObject), "m_IsActive", AnimationCurve.Constant(0, duration, 0));
            return clip;
        }

        static AnimatorControllerLayer AddLastLayer(AnimatorController controller, string name)
        {
            controller.AddLayer(name);
            var layer = controller.layers[controller.layers.Length - 1];
            layer.defaultWeight = 1f;
            return layer;
        }

        static AnimatorState AddConfiguredState(AnimatorStateMachine stateMachine, string name, Motion motion, Vector3 position)
        {
            var state = stateMachine.AddState(name, position);
            state.writeDefaultValues = false;
            state.motion = motion;
            return state;
        }

        static void AddCondition(AnimatorStateTransition transition, OneShotParameter oneShotParameter, ProvidedParameter parameter, bool toDefault)
        {
            var condition = Condition(oneShotParameter, parameter, toDefault);
            transition.AddCondition(condition.mode, condition.threshold, condition.parameter);
        }

        static AnimatorCondition Condition(OneShotParameter oneShotParameter, ProvidedParameter parameter, bool toDefault)
        {
            switch (parameter.ParameterType)
            {
                case AnimatorControllerParameterType.Bool:
                    var mode = oneShotParameter.ParameterDefaultValue >= 0.5f ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot;
                    if (!toDefault) mode = mode == AnimatorConditionMode.If ? AnimatorConditionMode.IfNot : AnimatorConditionMode.If;
                    return new AnimatorCondition
                    {
                        mode = mode,
                        threshold = 0f,
                        parameter = oneShotParameter.ParameterName,
                    };
                case AnimatorControllerParameterType.Int:
                    return new AnimatorCondition
                    {
                        mode = toDefault ? AnimatorConditionMode.Equals : AnimatorConditionMode.NotEqual,
                        threshold = Mathf.Round(oneShotParameter.ParameterDefaultValue),
                        parameter = oneShotParameter.ParameterName,
                    };
                default:
                    throw new System.InvalidOperationException("Invalid parameter type");
            }
        }
    }
}
