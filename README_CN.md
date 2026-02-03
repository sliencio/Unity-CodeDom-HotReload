# Unity 运行时热重载系统

一个轻量级的Unity运行时热重载系统，允许你在不停止Play模式的情况下修改和重载C#脚本。**零依赖，开箱即用！**

## ✨ 核心特性

- 🔥 **运行时热重载**：在游戏运行时修改代码，无需重启
- 🔄 **MonoBehaviour自动替换**：智能替换GameObject上的组件
- 💾 **状态完整保存**：重载时自动保存并恢复所有字段数据
- 🎯 **可视化方法执行**：直接从编辑器执行方法，支持多种参数类型
- 🛠️ **零依赖**：使用.NET Framework内置的CSharpCodeProvider，无需第三方库
- 📝 **拖放式界面**：简单直观的可视化编辑器
- ⚡ **轻量级**：无需安装任何NuGet包，保持项目简洁

## 📋 系统要求

- Unity 2022.3 或更高版本
- **API Compatibility Level 必须设置为 .NET Framework**
- **无需任何第三方库！**

## 🚀 快速开始

### 通过Git URL安装（推荐）

1. 打开Unity Package Manager（`Window > Package Manager`）
2. 点击左上角的 `+` 按钮
3. 选择 `Add package from git URL...`
4. 输入以下URL：
   ```
   https://github.com/yourusername/Unity-CodeDom-HotReload.git
   ```
5. 点击 `Add`

### 通过本地包安装

1. 克隆或下载本仓库
2. 在Unity Package Manager中，点击 `+` 按钮
3. 选择 `Add package from disk...`
4. 导航到克隆的仓库并选择 `package.json`

### 配置API Compatibility Level

**重要**：在Unity菜单中设置：
1. 打开 `Edit > Project Settings > Player`
2. 展开 `Other Settings`
3. 将 `Api Compatibility Level` 设置为 `.NET Framework`

就这么简单！无需安装任何第三方依赖包。.NET Framework自带`System.CodeDom.dll`支持。

## 📖 使用教程

### 打开热重载窗口

菜单：`Tools > C# Hot Reload Window`

### 添加脚本

1. 拖放MonoScript或GameObject到窗口
2. 脚本自动添加到热重载列表
3. 可选：配置需要执行的方法

### 热重载流程

1. **进入Play模式**
2. **修改代码**（添加方法、修改逻辑等）
3. **点击"🔄 重新编译并重载"**
4. **立即生效**！

系统会自动：
- ✅ 编译新代码
- ✅ 替换GameObject上的组件
- ✅ 保留所有字段数据
- ✅ 使新方法立即可用

### 执行方法

1. 在脚本配置中点击"+ 添加方法调用"
2. 输入方法名
3. 添加参数（支持：String, Int, Float, Bool, Vector3）
4. 点击"▶ 执行"

## ⚠️ 重要说明

### 调试限制

**热重载的代码无法使用断点！**

原因：
- 调试器附加到原始程序集
- 动态编译的代码没有调试符号
- 源代码与运行时代码映射关系丢失

**解决方案**：
- 需要调试时：停止Play模式 → 修改代码 → 重新运行
- 快速迭代时：使用热重载

### C#版本支持

本系统使用.NET Framework内置的CSharpCodeProvider，支持：
- ✅ C# 7.3 及以下的所有特性
- ✅ 标准的类、方法、属性定义
- ✅ LINQ、泛型、委托等常用特性
- ⚠️ 不支持C# 8.0+的新特性（如init访问器、记录类型、switch表达式等）

对于大多数Unity开发场景，这已经完全足够！

### MonoBehaviour重要限制

**⚠️ MonoBehaviour热重载存在已知问题，不推荐使用！**

由于Unity的MonoBehaviour架构限制，热重载MonoBehaviour组件时会遇到以下问题：
- ❌ 新增方法可能无法调用
- ❌ 组件引用经常丢失
- ❌ GameObject引用在序列化后可能失效
- ❌ 复杂序列化数据无法完全恢复
- ❌ 组件替换不稳定

**💡 强烈建议：**
- ✅ **对MonoBehaviour使用传统开发流程**（停止Play模式 → 修改代码 → 重新运行）
- ✅ **热重载系统专注于纯C#类**，效果稳定可靠
- ✅ **将核心逻辑提取到纯C#类中**，MonoBehaviour只做简单调用

## 🎯 使用场景

### ✅ 强烈推荐：纯C#类热重载

```csharp
// ✅ 完美支持！热重载效果稳定可靠
public class GameLogic
{
    public int score;
    
    public void AddScore(int points)
    {
        score += points;
        Debug.Log($"Score: {score}");
    }
    
    // 可以随时添加新方法，立即生效！
    public void ResetScore()
    {
        score = 0;
    }
}
```

**适用场景：**
- ✅ 游戏逻辑类
- ✅ 数据管理类
- ✅ 工具类和辅助类
- ✅ 算法和计算类
- ✅ 快速原型验证
- ✅ 迭代开发

### ⚠️ 不推荐：MonoBehaviour热重载

```csharp
// ⚠️ 不推荐热重载！请使用传统开发流程
public class Player : MonoBehaviour
{
    // MonoBehaviour热重载不稳定
    // 建议：停止Play → 修改代码 → 重新运行
}
```

**原因：**
- ❌ 新增方法可能无法调用
- ❌ 组件引用经常丢失
- ❌ 序列化数据可能损坏

### ❌ 不适合使用热重载

- 需要断点调试
- 复杂bug排查
- 性能分析
- 生产环境构建
- MonoBehaviour组件

## 🔧 技术实现

### 编译器

使用.NET Framework内置的`System.CodeDom.Compiler.CSharpCodeProvider`：
- ✅ 无需第三方依赖
- ✅ Unity自带支持
- ✅ 稳定可靠
- ✅ 零配置

### 编译选项

系统自动配置以下编译参数：
```csharp
compilerParameters = new CompilerParameters
{
    GenerateExecutable = false,
    GenerateInMemory = true,
    IncludeDebugInformation = false
};
```

### 自动引用管理

系统自动添加所有必要的程序集引用：
- .NET Framework核心库
- Unity引擎程序集
- 项目中所有已加载的程序集

## 🐛 已知问题

1. **断点不可用**（设计限制）
2. **MonoBehaviour热重载不稳定**（Unity架构限制，不推荐使用）
3. **不支持C# 8.0+新特性**（编译器限制）

**关于MonoBehaviour：**
由于Unity的MonoBehaviour组件系统的复杂性，热重载MonoBehaviour时会遇到各种问题。我们强烈建议：
- 对MonoBehaviour使用传统开发流程
- 将核心逻辑提取到纯C#类中
- 热重载系统专注于纯C#类的快速迭代

## 🤝 贡献指南

欢迎提交Pull Request！

### 开发环境

1. Fork本仓库
2. 创建特性分支：`git checkout -b feature/AmazingFeature`
3. 提交更改：`git commit -m 'Add some AmazingFeature'`
4. 推送分支：`git push origin feature/AmazingFeature`
5. 提交Pull Request

## 📄 开源协议

本项目采用 **MIT License** 开源协议。

这意味着你可以：
- ✅ 商业使用
- ✅ 修改代码
- ✅ 分发
- ✅ 私有使用

详见 [LICENSE](LICENSE) 文件。

## 🙏 致谢

- Microsoft .NET Framework - 提供强大的CodeDom编译器
- Unity Technologies - 优秀的游戏引擎

## 📞 获取帮助

- 📧 提交Issue：[GitHub Issues](https://github.com/yourusername/unity-hot-reload/issues)
- 💬 讨论区：[GitHub Discussions](https://github.com/yourusername/unity-hot-reload/discussions)

## 🗺️ 路线图

- [x] 基础热重载功能
- [x] MonoBehaviour组件替换
- [x] 可视化方法执行
- [ ] 支持更多参数类型
- [ ] 改进错误提示
- [ ] 添加性能监控
- [ ] 支持Assembly Definition

## 📊 性能说明

- 编译时间：通常 1-2秒（取决于代码复杂度）
- 内存占用：每个热重载的程序集约 1-3MB
- 建议：不要在一次热重载中包含过多脚本

## 💡 最佳实践

### 1. 代码组织（重要！）

```csharp
// ✅✅✅ 强烈推荐：纯C#类 - 热重载完美支持
public class GameLogic
{
    private int score;
    
    public void UpdateScore(int points)
    {
        score += points;
    }
    
    // 可以随时添加新方法，热重载立即生效！
    public int GetScore() => score;
}

// ❌❌❌ 不推荐热重载：MonoBehaviour - 请使用传统流程
public class Player : MonoBehaviour
{
    private GameLogic gameLogic = new GameLogic(); // 使用纯C#类
    
    void Update()
    {
        // MonoBehaviour只做简单调用
        if (Input.GetKeyDown(KeyCode.Space))
        {
            gameLogic.UpdateScore(10); // 核心逻辑在纯C#类中
        }
    }
}
```

**架构建议：**
- ✅ 将游戏逻辑提取到纯C#类
- ✅ MonoBehaviour只负责Unity生命周期和简单调用
- ✅ 热重载专注于纯C#类的快速迭代
- ❌ 不要热重载MonoBehaviour组件

### 2. 字段管理

```csharp
// ✅ 简单类型会自动保存恢复
public int health;
public string playerName;

// ⚠️ 引用类型可能丢失
public GameObject target; // 可能丢失
public Player otherPlayer; // 可能丢失
```

### 3. 方法设计

```csharp
// ✅ 推荐：无状态方法
public int Calculate(int a, int b) 
{
    return a + b;
}

// ✅ 推荐：明确的参数
public void SetPosition(float x, float y, float z) { }
```

### 4. C#版本兼容

```csharp
// ✅ 支持（C# 7.3及以下）
public string Name { get; set; }
if (obj is Type t) { ... }
var result = (a > b) ? a : b;

// ❌ 不支持（C# 8.0+）
public string Name { get; init; }  // init访问器
var result = obj switch { ... };   // switch表达式
record Person(string Name);        // 记录类型
```

## 📸 效果展示

![热重载效果](pic.png)

## 🎓 使用示例

查看 [`CodeDomExample.cs`](Assets/Scripts/Utils/HotReload/CodeDomExample.cs) 了解详细的使用示例。

## 📝 更新日志

### v2.0.0 (2026-02-03)
- 🎉 切换到CodeDom实现
- ✨ 移除所有第三方依赖
- ✨ 零配置，开箱即用
- ✨ 更轻量，更稳定

### v1.0.0 (2026-02-02)
- 🎉 首次发布
- ✨ 支持运行时热重载
- ✨ MonoBehaviour自动替换
- ✨ 可视化方法执行界面

---

**Star ⭐ 本项目如果它对你有帮助！**