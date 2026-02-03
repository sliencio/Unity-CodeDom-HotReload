using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CSharp;
using UnityEngine;

namespace Utils.HotReload
{
    /// <summary>
    /// 使用CodeDom实现的C#热重载系统（单例模式）
    /// 支持运行时动态编译和重载C#脚本
    /// </summary>
    public class CodeDomHotReload
    {
        private static CodeDomHotReload _instance;
        public static CodeDomHotReload Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CodeDomHotReload();
                }
                return _instance;
            }
        }

        // 存储已编译的程序集
        private Dictionary<string, Assembly> compiledAssemblies = new Dictionary<string, Assembly>();
        
        // 存储已注册的实例
        private Dictionary<string, object> registeredInstances = new Dictionary<string, object>();
        
        // 存储实例数据（用于状态保存）
        private Dictionary<string, Dictionary<string, object>> instanceData = new Dictionary<string, Dictionary<string, object>>();

        private CodeDomHotReload()
        {
            Debug.Log("[CodeDomHotReload] 热重载系统已初始化");
        }

        /// <summary>
        /// 编译并重载源代码
        /// </summary>
        public bool CompileAndReloadSource(string sourceCode, string assemblyName)
        {
            try
            {
                Assembly assembly = CompileCode(sourceCode, out string errors);
                
                if (assembly == null)
                {
                    Debug.LogError($"[CodeDomHotReload] 编译失败:\n{errors}");
                    return false;
                }

                compiledAssemblies[assemblyName] = assembly;
                Debug.Log($"[CodeDomHotReload] 编译成功: {assemblyName}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CodeDomHotReload] 编译异常: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// 编译并重载脚本文件
        /// </summary>
        public bool CompileAndReloadScript(string scriptPath)
        {
            try
            {
                if (!File.Exists(scriptPath))
                {
                    Debug.LogError($"[CodeDomHotReload] 文件不存在: {scriptPath}");
                    return false;
                }

                string sourceCode = File.ReadAllText(scriptPath);
                
                // 编译代码
                Assembly assembly = CompileCode(sourceCode, out string errors);
                
                if (assembly == null)
                {
                    Debug.LogError($"[CodeDomHotReload] 编译失败:\n{errors}");
                    return false;
                }

                // 使用程序集的实际名称作为key存储
                string assemblyName = assembly.GetName().Name;
                compiledAssemblies[assemblyName] = assembly;
                
                // 同时也用脚本文件名存储一份，方便查找
                string scriptName = Path.GetFileNameWithoutExtension(scriptPath);
                compiledAssemblies[scriptName] = assembly;
                
                Debug.Log($"[CodeDomHotReload] 编译成功: {scriptPath} -> {assemblyName}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CodeDomHotReload] 读取文件失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 编译C#源代码
        /// </summary>
        private Assembly CompileCode(string sourceCode, out string errors)
        {
            errors = string.Empty;

            try
            {
                // 创建C#代码提供器
                CSharpCodeProvider codeProvider = new CSharpCodeProvider();

                // 配置编译参数
                CompilerParameters compilerParameters = new CompilerParameters
                {
                    GenerateExecutable = false,
                    GenerateInMemory = true,
                    IncludeDebugInformation = false
                };

                // 添加所有必要的程序集引用
                AddAssemblyReferences(compilerParameters);

                // 编译代码
                CompilerResults results = codeProvider.CompileAssemblyFromSource(compilerParameters, sourceCode);

                // 检查编译错误
                if (results.Errors.HasErrors)
                {
                    errors = GetCompilerErrors(results);
                    return null;
                }

                return results.CompiledAssembly;
            }
            catch (Exception ex)
            {
                errors = $"编译异常: {ex.Message}\n{ex.StackTrace}";
                return null;
            }
        }

        /// <summary>
        /// 添加程序集引用
        /// </summary>
        private void AddAssemblyReferences(CompilerParameters parameters)
        {
            // 添加.NET Framework核心库
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Core.dll");
            parameters.ReferencedAssemblies.Add("mscorlib.dll");

            // 添加Unity引擎程序集
            parameters.ReferencedAssemblies.Add(typeof(UnityEngine.Object).Assembly.Location);
            parameters.ReferencedAssemblies.Add(typeof(UnityEngine.UI.Text).Assembly.Location);

            // 添加当前域中所有已加载的程序集
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    if (!asm.IsDynamic && !string.IsNullOrEmpty(asm.Location))
                    {
                        parameters.ReferencedAssemblies.Add(asm.Location);
                    }
                }
                catch
                {
                    // 忽略无法访问的程序集
                }
            }
        }

        /// <summary>
        /// 获取编译错误信息
        /// </summary>
        private string GetCompilerErrors(CompilerResults results)
        {
            System.Text.StringBuilder errorBuilder = new System.Text.StringBuilder();
            
            foreach (CompilerError error in results.Errors)
            {
                if (error.IsWarning)
                    continue;

                errorBuilder.AppendLine($"错误 {error.ErrorNumber} 在 行{error.Line}: {error.ErrorText}");
            }

            return errorBuilder.ToString();
        }

        /// <summary>
        /// 创建并注册实例
        /// </summary>
        public T CreateAndRegister<T>(string instanceName, string typeName, string assemblyName = null)
        {
            try
            {
                Assembly assembly = null;
                
                if (string.IsNullOrEmpty(assemblyName))
                {
                    // 优先在已编译的程序集中查找（这些是热重载的新版本）
                    foreach (var kvp in compiledAssemblies)
                    {
                        Type type = kvp.Value.GetTypes().FirstOrDefault(t => t.Name == typeName);
                        if (type != null)
                        {
                            assembly = kvp.Value;
                            Debug.Log($"[CodeDomHotReload] 在已编译程序集中找到类型: {kvp.Key}");
                            break;
                        }
                    }
                    
                    // 如果还是没找到，尝试在所有已加载的程序集中查找
                    if (assembly == null)
                    {
                        foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            try
                            {
                                Type type = asm.GetTypes().FirstOrDefault(t => t.Name == typeName);
                                if (type != null)
                                {
                                    assembly = asm;
                                    Debug.Log($"[CodeDomHotReload] 在系统程序集中找到类型: {asm.GetName().Name}");
                                    break;
                                }
                            }
                            catch
                            {
                                // 忽略无法访问的程序集
                            }
                        }
                    }
                }
                else
                {
                    compiledAssemblies.TryGetValue(assemblyName, out assembly);
                }

                if (assembly == null)
                {
                    Debug.LogError($"[CodeDomHotReload] 找不到包含类型 '{typeName}' 的程序集");
                    Debug.LogError($"[CodeDomHotReload] 已编译的程序集: {string.Join(", ", compiledAssemblies.Keys)}");
                    return default(T);
                }

                Type targetType = assembly.GetTypes().FirstOrDefault(t => t.Name == typeName);
                if (targetType == null)
                {
                    Debug.LogError($"[CodeDomHotReload] 找不到类型: {typeName}");
                    return default(T);
                }

                // 强制重新创建实例（即使已存在）
                object instance = Activator.CreateInstance(targetType);
                registeredInstances[instanceName] = instance;
                
                Debug.Log($"[CodeDomHotReload] 实例已创建并注册: {instanceName} (Type: {targetType.FullName}, Assembly: {assembly.GetName().Name})");
                return (T)instance;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CodeDomHotReload] 创建实例失败: {ex.Message}\n{ex.StackTrace}");
                return default(T);
            }
        }

        /// <summary>
        /// 注册已有实例
        /// </summary>
        public void RegisterInstance(string instanceName, object instance)
        {
            registeredInstances[instanceName] = instance;
            Debug.Log($"[CodeDomHotReload] 实例已注册: {instanceName}");
        }

        /// <summary>
        /// 获取已注册的实例
        /// </summary>
        public T GetInstance<T>(string instanceName)
        {
            if (registeredInstances.TryGetValue(instanceName, out object instance))
            {
                return (T)instance;
            }
            return default(T);
        }

        /// <summary>
        /// 保存实例数据
        /// </summary>
        public void SaveInstanceData()
        {
            instanceData.Clear();
            
            foreach (var kvp in registeredInstances)
            {
                var data = new Dictionary<string, object>();
                var type = kvp.Value.GetType();
                
                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    try
                    {
                        data[field.Name] = field.GetValue(kvp.Value);
                    }
                    catch { }
                }
                
                instanceData[kvp.Key] = data;
            }
            
            Debug.Log($"[CodeDomHotReload] 已保存 {instanceData.Count} 个实例的数据");
        }

        /// <summary>
        /// 恢复实例数据
        /// </summary>
        public void RestoreInstanceData()
        {
            foreach (var kvp in instanceData)
            {
                if (registeredInstances.TryGetValue(kvp.Key, out object instance))
                {
                    var type = instance.GetType();
                    
                    foreach (var fieldData in kvp.Value)
                    {
                        try
                        {
                            var field = type.GetField(fieldData.Key, BindingFlags.Public | BindingFlags.Instance);
                            field?.SetValue(instance, fieldData.Value);
                        }
                        catch { }
                    }
                }
            }
            
            Debug.Log($"[CodeDomHotReload] 已恢复 {instanceData.Count} 个实例的数据");
        }

        /// <summary>
        /// 获取实例信息
        /// </summary>
        public string GetInstancesInfo()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"已注册实例数量: {registeredInstances.Count}");
            
            foreach (var kvp in registeredInstances)
            {
                sb.AppendLine($"- {kvp.Key}: {kvp.Value.GetType().Name}");
            }
            
            return sb.ToString();
        }

        /// <summary>
        /// 获取所有已编译的程序集
        /// </summary>
        public IEnumerable<Assembly> GetCompiledAssemblies()
        {
            return compiledAssemblies.Values;
        }

        /// <summary>
        /// 清除所有数据
        /// </summary>
        public void Clear()
        {
            compiledAssemblies.Clear();
            registeredInstances.Clear();
            instanceData.Clear();
            Debug.Log("[CodeDomHotReload] 所有数据已清除");
        }
    }
}