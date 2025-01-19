using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Narazaka.VRChat.AvatarParametersUtil.Editor;
using nadena.dev.ndmf;
using nadena.dev.ndmf.localization;
using System;

namespace Narazaka.VRChat.OneShotParameter.Editor
{
    [CustomEditor(typeof(OneShotParameter))]
    public class OneShotParameterEditor : UnityEditor.Editor
    {
        SerializedProperty ParameterName;
        SerializedProperty ParameterDefaultValue;
        SerializedProperty Duration;
        SerializedProperty LocalOnly;
        AvatarParametersUtilEditor ParameterUtil;

        void OnEnable()
        {
            ParameterName = serializedObject.FindProperty("ParameterName");
            ParameterDefaultValue = serializedObject.FindProperty("ParameterDefaultValue");
            Duration = serializedObject.FindProperty("Duration");
            LocalOnly = serializedObject.FindProperty("LocalOnly");
            ParameterUtil = AvatarParametersUtilEditor.Get(serializedObject);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            var rect = EditorGUILayout.GetControlRect();
            ParameterUtil.ShowParameterNameField(rect, ParameterName, IsValidParameterType, new GUIContent(Lang.ParameterName));
            var parameter = ParameterUtil.GetParameter(ParameterName.stringValue);
            if (parameter != null && !IsValidParameterType(parameter))
            {
                EditorGUILayout.HelpBox(Lang.BoolOrInt, MessageType.Error);
            }
            rect = EditorGUILayout.GetControlRect();
            ParameterUtil.ShowParameterValueField(rect, ParameterName.stringValue, ParameterDefaultValue, new GUIContent(Lang.ParameterDefaultValue));
            EditorGUILayout.HelpBox(Lang.ParameterDefaultValueDescription, MessageType.Info);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(Duration, new GUIContent(Lang.Duration));
            EditorGUILayout.PropertyField(LocalOnly, new GUIContent(Lang.LocalOnly));

            serializedObject.ApplyModifiedProperties();
        }

        bool IsValidParameterType(ProvidedParameter parameter)
        {
            return parameter.ParameterType == AnimatorControllerParameterType.Bool || parameter.ParameterType == AnimatorControllerParameterType.Int;
        }

        static class Lang
        {
            static bool IsJa => LanguagePrefs.Language == "ja-jp";
            static string L(string ja, string en) => IsJa ? ja : en;
            public static string ParameterName => L("パラメータ名", "Parameter Name");
            public static string ParameterDefaultValue => L("パラメータ初期値", "Parameter Default Value");
            public static string ParameterDefaultValueDescription => L("この値から変化してからリセット時間が経過すると、リセットされて再びこの値になります。", "After the reset time has elapsed since the change from this value, it will be reset to this value again.");
            public static string Duration => L("リセット時間(秒)", "Reset Time(s)");
            public static string LocalOnly => L("ローカルのみで実行", "Local Only");
            public static string BoolOrInt => L("Bool か Int を指定して下さい", "Bool or Int allowed");
        }
    }
}
