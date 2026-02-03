using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Editor
{
    /// <summary>
    /// 热重载配置数据
    /// </summary>
    [Serializable]
    public class HotReloadConfig : ScriptableObject
    {
        [SerializeField]
        public List<ScriptEntry> scripts = new List<ScriptEntry>();
    }

    [Serializable]
    public class ScriptEntry
    {
        public string scriptName;
        public string scriptPath;
        public MonoScript monoScript;
        public GameObject targetGameObject; // 如果是MonoBehaviour
        public string typeName; // 完整类型名称
        public bool isMonoBehaviour;
        public List<MethodCall> methodCalls = new List<MethodCall>();
        public bool isFoldout = true;
    }

    [Serializable]
    public class MethodCall
    {
        public string methodName;
        public List<MethodParameter> parameters = new List<MethodParameter>();
    }

    [Serializable]
    public class MethodParameter
    {
        public string parameterName;
        public ParameterType parameterType;
        public string stringValue;
        public int intValue;
        public float floatValue;
        public bool boolValue;
        public Vector3 vector3Value;
    }

    public enum ParameterType
    {
        String,
        Int,
        Float,
        Bool,
        Vector3
    }
}