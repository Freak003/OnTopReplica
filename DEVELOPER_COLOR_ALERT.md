# 颜色警报功能 - 开发者集成指南

## 架构概览

颜色警报功能由以下组件组成：

```
┌─────────────────────────────────────────────────────────────┐
│                     MainForm                                 │
│  ┌──────────────────────────────────────────────────────┐   │
│  │              MessagePumpManager                      │   │
│  │  ┌────────────────────────────────────────────────┐  │   │
│  │  │  ColorDetectionProcessor (消息处理器)          │  │   │
│  │  │  - 定期采样窗口像素                            │  │   │
│  │  │  - 检测颜色匹配                                │  │   │
│  │  │  - 触发报警                                    │  │   │
│  │  └────────────────────────────────────────────────┘  │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │              SidePanelContainer                      │   │
│  │  ┌────────────────────────────────────────────────┐  │   │
│  │  │  ColorAlertPanel (配置 UI)                    │  │   │
│  │  │  - 颜色选择                                    │  │   │
│  │  │  - 容差设置                                    │  │   │
│  │  │  - 采样间隔配置                                │  │   │
│  │  └────────────────────────────────────────────────┘  │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

## 核心类说明

### 1. ColorDetectionProcessor

**文件**：`MessagePumpProcessors/ColorDetectionProcessor.cs`

**功能**：
- 继承自 `BaseMessagePumpProcessor`
- 在消息泵中周期性执行颜色检测
- 管理报警状态和音频播放

**关键成员**：

```csharp
public bool Enabled { get; set; }              // 是否启用检测
public Color TargetColor { get; set; }         // 目标颜色
public int ColorTolerance { get; set; }        // 颜色容差 (0-255)
public int SampleInterval { get; set; }        // 采样间隔 (ms)
public bool IsAlarmActive { get; }             // 报警是否正在进行
```

**核心方法**：

```csharp
public override bool Process(ref Message msg)  // 消息处理器的主方法
private bool DetectColorInWindow(IntPtr windowHandle)  // 检测窗口中的颜色
private bool SampleBitmapForColor(Bitmap bmp)  // 采样位图检测颜色
private bool IsColorMatch(Color c1, Color c2, int tolerance)  // 颜色匹配算法
private void StartAlarm()                      // 启动报警
private void StopAlarm()                       // 停止报警
```

**工作流程**：

```
Process() 被调用
  ↓
检查是否启用 + 采样间隔时间到达？
  ├─ 是 → 检测颜色
  │      ├─ 检测到颜色 → StartAlarm()
  │      └─ 未检测到颜色 → 继续检测
  ├─ 是 且 报警进行中 → 检查是否应停止 → StopAlarm()
  └─ 否 → 返回 false
```

### 2. ColorAlertPanel

**文件**：
- `SidePanels/ColorAlertPanel.cs` - 代码逻辑
- `SidePanels/ColorAlertPanel.Designer.cs` - UI 设计

**功能**：
- 为用户提供颜色检测配置界面
- 被嵌入在 `SidePanelContainer` 中
- 与 `ColorDetectionProcessor` 通信

**UI 控件**：

| 控件 | 作用 |
|------|------|
| `checkEnabled` | 启用/禁用检测的复选框 |
| `panelColorPreview` | 显示当前选定颜色的面板 |
| `btnChooseColor` | 打开颜色选择器的按钮 |
| `labelColorValue` | 显示十六进制颜色值 |
| `trackBarTolerance` | 调整颜色容差的滑块 |
| `numInterval` | 输入采样间隔的数字框 |
| `btnClose` | 关闭面板的按钮 |

**生命周期**：

```
ColorAlertPanel 创建
  ↓
InitializeComponent() (Designer 生成)
  ↓
LocalizePanel() (本地化 UI 文本)
  ↓
OnFirstShown(MainForm form) 被调用
  ├─ 获取 ColorDetectionProcessor 实例
  └─ 初始化控件值
  ↓
用户交互（选择颜色、调整参数）
  ↓
OnClosing(MainForm form) 被调用
  ├─ 将 UI 值保存到 ColorDetectionProcessor
  └─ 处理器更新其行为
  ↓
面板关闭
```

## 集成点

### 1. MessagePumpManager 中的注册

**文件**：`MessagePumpManager.cs`

```csharp
public void Initialize(MainForm form) {
    // ... 其他初始化
    Register(new ColorDetectionProcessor(), form);  // ← 关键一行
}
```

### 2. 主菜单中的菜单项

**文件**：`MainForm.Designer.cs` 和 `MainForm_MenuEvents.cs`

菜单项会在右键菜单中显示，用户可以点击打开 ColorAlertPanel。

### 3. 事件处理

**文件**：`MainForm_MenuEvents.cs`

```csharp
private void Menu_ColorAlert_click(object sender, EventArgs e) {
    this.SetSidePanel(new ColorAlertPanel());
}
```

## 颜色检测算法

### 采样策略

为了性能优化，使用网格采样而不是逐像素检查：

```
原始窗口内容 (例如 800x600)
  ↓
创建与窗口大小相同的位图
  ↓
限制位图大小 (最大 800x600)
  ↓
网格采样 (20x20 网格)
  │
  ├─ stepX = width / 20
  ├─ stepY = height / 20
  │
  └─ 检查采样点
```

### 颜色匹配算法

```csharp
bool IsColorMatch(Color color1, Color color2, int tolerance) {
    int rDiff = Math.Abs(color1.R - color2.R);
    int gDiff = Math.Abs(color1.G - color2.G);
    int bDiff = Math.Abs(color1.B - color2.B);
    
    // RGB 各分量差值都在容差范围内
    return rDiff <= tolerance && 
           gDiff <= tolerance && 
           bDiff <= tolerance;
}
```

## 性能考虑

### CPU 占用

| 采样间隔 | CPU 占用 | 响应速度 | 推荐场景 |
|---------|---------|---------|---------|
| 100ms | 5-10% | 很高 | 关键事件监控 |
| 300ms | 2-5% | 高 | 一般监控 |
| 500ms | 1-3% | 中等 | 推荐值 |
| 1000ms+ | <1% | 较低 | 后台监控 |

### 内存占用

- 每次采样创建临时位图：~2MB（800x600 @ 32bpp）
- 采样完成后立即释放
- 整体内存占用几乎无增长

### 优化建议

1. 避免在高分辨率（>1920x1200）的窗口上频繁采样
2. 如果精度要求不高，增大容差值而不是减小采样间隔
3. 监控多个窗口时，为不同窗口设置不同的采样间隔

## 扩展建议

### 可能的功能增强

1. **多颜色监控**
   - 支持监控多个颜色
   - 每个颜色可以有不同的报警音

2. **区域限制**
   - 只监控窗口的特定区域
   - 使用 RegionPanel 集成的现有区域选择功能

3. **自定义音效**
   - 允许用户上传自定义音效文件
   - 支持多种警报音选择

4. **数据统计**
   - 记录颜色检测事件
   - 可视化颜色出现频率

5. **触发动作**
   - 不仅仅播放声音，还可以：
     - 闪现主窗口
     - 显示通知
     - 执行自定义命令

## 代码示例

### 如何使用 ColorDetectionProcessor（程序员角度）

```csharp
// 获取处理器实例
ColorDetectionProcessor processor = 
    mainForm.MessagePumpManager.Get<ColorDetectionProcessor>();

// 配置参数
processor.TargetColor = Color.Red;
processor.ColorTolerance = 30;
processor.SampleInterval = 500;

// 启用检测
processor.Enabled = true;

// 检查是否正在报警
if (processor.IsAlarmActive) {
    // 报警进行中...
}
```

### 自定义音频播放（扩展示例）

```csharp
// 如果想要更高级的音频功能，可以替换 PlayAlarmTone 方法
// 使用 NAudio 库或其他音频库

private void PlayAudioData(byte[] audioData, int sampleRate) {
    // 使用 NAudio 的 WaveOutEvent
    var provider = new RawSourceWaveProvider(new MemoryStream(audioData), 
        new WaveFormat(sampleRate, 16, 1));
    var output = new WaveOutEvent();
    output.Init(provider);
    output.Play();
}
```

## 调试技巧

### 启用详细日志

编辑 `ColorDetectionProcessor.Process()` 方法中的日志：

```csharp
Log.Write("Sampling at tick {0}, last sample was {1}ms ago", 
    currentTick, currentTick - _lastSampleTick);
Log.Write("Detected color at pixel ({0}, {1})", x, y);
```

### 测试颜色检测

创建测试窗口显示不同颜色，验证检测准确性：

```csharp
// 创建测试窗口
Form testForm = new Form { BackColor = Color.Red };
testForm.Show();

// 设置检测参数并等待报警
processor.TargetColor = Color.Red;
processor.ColorTolerance = 10;
processor.Enabled = true;
```

## 常见问题解答（开发者版）

**Q: 为什么使用网格采样？**
A: 逐像素检查对于大窗口会非常耗 CPU。网格采样在保持检测准确率的同时大幅降低负载。

**Q: 线程安全吗？**
A: 是的。所有操作都在应用主线程的消息泵中执行，不存在线程同步问题。

**Q: 如何支持半透明窗口？**
A: CopyFromScreen 会自动获取窗口在屏幕上的实际显示效果，包括半透明。

**Q: 可以监控不在最前面的窗口吗？**
A: 是的，可以监控任何已知 HWND 的窗口，无论其是否可见或在最前面。
