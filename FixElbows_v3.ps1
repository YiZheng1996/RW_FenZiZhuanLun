# ============================================================
#  分子筛转轮大屏 — 弯头坐标修正脚本 v3
#  通过像素扫描效果图反推，精确对齐真实管道位置
#  使用：放到项目根目录，右键用 PowerShell 运行
# ============================================================

$filePath   = ".\MainWindow.xaml"
$backupPath = ".\MainWindow.xaml.bak_elbows_v3"

if (!(Test-Path $filePath)) {
    Write-Host "❌ 找不到 MainWindow.xaml" -ForegroundColor Red
    Read-Host; exit 1
}

Copy-Item $filePath $backupPath
Write-Host "✅ 已备份到 $backupPath" -ForegroundColor Green

$content = Get-Content $filePath -Raw -Encoding UTF8

# ══════════════════════════════════════════════════════════════
#  核心思路：完整替换整个弯头 Canvas 块
#  旧块以 "管路弯头 Canvas（PipelineUpgrade" 为标记
# ══════════════════════════════════════════════════════════════

# 检查是否存在旧弯头块
if ($content -notmatch '管路弯头 Canvas') {
    Write-Host "⚠️  未找到旧弯头Canvas块，请确认已运行过 PipelineUpgrade_v2.ps1" -ForegroundColor Yellow
    Write-Host "   将直接插入新弯头块..." -ForegroundColor Yellow
}

# ── 新弯头 Canvas（坐标已通过像素扫描精确校准）──────────────
# 管道实测 Canvas 坐标：
#   左侧主竖管:  x=[102~107]
#   右侧上竖管:  x=[686~690]  y=[110~312]
#   右下竖管:    x=[479~483]  y=[309~388]
#   顶横管:      y=[104~108]
#   底横管:      y=[382~388]
#   脱附横管:    y=[251~259]  x=[162~435]
#   右下横管:    y=[309~318]  x=[478~693]
#
# 弯头尺寸：12×12（半径=6，与管道宽6px完整包裹）
# Canvas.Left/Top = 弯头左上角坐标

$newElbowCanvas = @'

            <!-- ════════════════════════════════════════════════
                 ★ 管路弯头 Canvas（FixElbows v3 精确校准版）
                 坐标通过像素扫描效果图反推，与实际管道精确对齐
                 Grid.RowSpan="2" 全区覆盖，ZIndex=20 置顶
            ════════════════════════════════════════════════ -->
            <Canvas Grid.RowSpan="2" Panel.ZIndex="20" IsHitTestVisible="False">

                <!-- ─────────────────────────────────────────
                     外框大矩形 4 个角
                     顶横管 y=[104~108]，左竖管 x=[102~107]
                     右竖管 x=[686~690]
                ───────────────────────────────────────────── -->

                <!-- 外框-左上角: 顶横管左端 ↔ 左竖管上端 -->
                <!-- 左竖管x_left=102, 顶横管y_top=104 -->
                <Path Canvas.Left="102" Canvas.Top="104"
                      Data="M 0,0 L 6,0 A 6,6 0 0,1 12,6 L 12,12 L 6,12 A 6,6 0 0,0 0,6 Z"
                      Effect="{StaticResource ElbowGlowEffect}"
                      Stroke="#5000E5FF" StrokeThickness="0.6">
                    <Path.Fill>
                        <LinearGradientBrush StartPoint="0.5,0" EndPoint="0.5,1">
                            <GradientStop Color="#FF061828" Offset="0"/>
                            <GradientStop Color="#FF006888" Offset="0.08"/>
                            <GradientStop Color="#FF00E5FF" Offset="0.25"/>
                            <GradientStop Color="#FF00A8C4" Offset="0.50"/>
                            <GradientStop Color="#FF005870" Offset="0.78"/>
                            <GradientStop Color="#FF021520" Offset="1"/>
                        </LinearGradientBrush>
                    </Path.Fill>
                </Path>

                <!-- 外框-右上角: 顶横管右端 ↔ 右侧上竖管上端 -->
                <!-- 右侧上竖管x_left=686, 顶横管y_top=104 -->
                <Path Canvas.Left="684" Canvas.Top="104"
                      Data="M 12,0 L 6,0 A 6,6 0 0,0 0,6 L 0,12 L 6,12 A 6,6 0 0,1 12,6 Z"
                      Effect="{StaticResource ElbowGlowEffect}"
                      Stroke="#5000E5FF" StrokeThickness="0.6">
                    <Path.Fill>
                        <LinearGradientBrush StartPoint="0.5,0" EndPoint="0.5,1">
                            <GradientStop Color="#FF061828" Offset="0"/>
                            <GradientStop Color="#FF006888" Offset="0.08"/>
                            <GradientStop Color="#FF00E5FF" Offset="0.25"/>
                            <GradientStop Color="#FF00A8C4" Offset="0.50"/>
                            <GradientStop Color="#FF005870" Offset="0.78"/>
                            <GradientStop Color="#FF021520" Offset="1"/>
                        </LinearGradientBrush>
                    </Path.Fill>
                </Path>

                <!-- 右侧中折角: 右侧上竖管下端 ↔ 右下横管左端 -->
                <!-- 右侧上竖管x_left=686, 右下横管y_top=309 -->
                <Path Canvas.Left="684" Canvas.Top="309"
                      Data="M 12,0 L 6,0 A 6,6 0 0,0 0,6 L 0,12 L 6,12 A 6,6 0 0,1 12,6 Z"
                      Effect="{StaticResource ElbowGlowEffect}"
                      Stroke="#5000E5FF" StrokeThickness="0.6">
                    <Path.Fill>
                        <LinearGradientBrush StartPoint="0.5,0" EndPoint="0.5,1">
                            <GradientStop Color="#FF061828" Offset="0"/>
                            <GradientStop Color="#FF006888" Offset="0.08"/>
                            <GradientStop Color="#FF00E5FF" Offset="0.25"/>
                            <GradientStop Color="#FF00A8C4" Offset="0.50"/>
                            <GradientStop Color="#FF005870" Offset="0.78"/>
                            <GradientStop Color="#FF021520" Offset="1"/>
                        </LinearGradientBrush>
                    </Path.Fill>
                </Path>

                <!-- 外框-右下角: 右下竖管下端 ↔ 底横管右端 -->
                <!-- 右下竖管x_left=479, 底横管y_top=382（弯头往上偏2px桥接） -->
                <Path Canvas.Left="479" Canvas.Top="380"
                      Data="M 12,12 L 6,12 A 6,6 0 0,1 0,6 L 0,0 L 6,0 A 6,6 0 0,0 12,6 Z"
                      Effect="{StaticResource ElbowGlowEffect}"
                      Stroke="#5000E5FF" StrokeThickness="0.6">
                    <Path.Fill>
                        <LinearGradientBrush StartPoint="0.5,0" EndPoint="0.5,1">
                            <GradientStop Color="#FF061828" Offset="0"/>
                            <GradientStop Color="#FF006888" Offset="0.08"/>
                            <GradientStop Color="#FF00E5FF" Offset="0.25"/>
                            <GradientStop Color="#FF00A8C4" Offset="0.50"/>
                            <GradientStop Color="#FF005870" Offset="0.78"/>
                            <GradientStop Color="#FF021520" Offset="1"/>
                        </LinearGradientBrush>
                    </Path.Fill>
                </Path>

                <!-- 外框-左下角: 左竖管下端 ↔ 底横管左端 -->
                <!-- 左竖管x_left=102, 底横管y_top=382（弯头Top=379桥接） -->
                <Path Canvas.Left="102" Canvas.Top="379"
                      Data="M 0,12 L 6,12 A 6,6 0 0,0 12,6 L 12,0 L 6,0 A 6,6 0 0,1 0,6 Z"
                      Effect="{StaticResource ElbowGlowEffect}"
                      Stroke="#5000E5FF" StrokeThickness="0.6">
                    <Path.Fill>
                        <LinearGradientBrush StartPoint="0.5,0" EndPoint="0.5,1">
                            <GradientStop Color="#FF061828" Offset="0"/>
                            <GradientStop Color="#FF006888" Offset="0.08"/>
                            <GradientStop Color="#FF00E5FF" Offset="0.25"/>
                            <GradientStop Color="#FF00A8C4" Offset="0.50"/>
                            <GradientStop Color="#FF005870" Offset="0.78"/>
                            <GradientStop Color="#FF021520" Offset="1"/>
                        </LinearGradientBrush>
                    </Path.Fill>
                </Path>

                <!-- ─────────────────────────────────────────
                     脱附回路相关弯头
                     脱附横管 y=[251~259]，左端 x=162，右端 x≈435
                     左侧竖管 x=[102~107]（穿过脱附管，属T型分叉，不是弯头）
                     脱附横管右端衔接的是阀门区域，不是竖管弯角
                ───────────────────────────────────────────── -->
                <!-- 注：脱附横管两端是T型分叉或接阀门，不放弯头 -->

            </Canvas>
'@

# ── 替换旧弯头Canvas块 ───────────────────────────────────────
# 匹配从注释开头到 </Canvas> 的整个旧块
$oldPattern = '(?s)<!-- ════+.*?管路弯头 Canvas.*?</Canvas>'

if ($content -match $oldPattern) {
    $content = [regex]::Replace($content, $oldPattern, $newElbowCanvas.TrimStart("`r`n"))
    Write-Host "✅ 旧弯头Canvas块已替换为精确坐标版本" -ForegroundColor Green
} else {
    # 没有旧块，则在 </Grid> 前插入
    $anchor = '    </Grid>'
    $lastIdx = $content.LastIndexOf($anchor)
    if ($lastIdx -ge 0) {
        $content = $content.Substring(0, $lastIdx) + $newElbowCanvas + "`r`n" + $content.Substring($lastIdx)
        Write-Host "✅ 新弯头Canvas块已插入Grid末尾" -ForegroundColor Green
    } else {
        Write-Host "❌ 无法找到插入位置，请手动处理" -ForegroundColor Red
    }
}

Set-Content $filePath $content -Encoding UTF8

Write-Host ""
Write-Host "════════════════════════════════════════" -ForegroundColor Green
Write-Host "  ✅ 完成！弯头坐标已精确校准" -ForegroundColor Green
Write-Host ""
Write-Host "  校准依据（Canvas坐标）：" -ForegroundColor White
Write-Host "  · 左侧主竖管:  x=[102~107]" -ForegroundColor Gray
Write-Host "  · 右侧上竖管:  x=[686~690]" -ForegroundColor Gray
Write-Host "  · 顶横管:      y=[104~108]" -ForegroundColor Gray
Write-Host "  · 底横管:      y=[382~388]" -ForegroundColor Gray
Write-Host "  · 右下横管:    y=[309~318]" -ForegroundColor Gray
Write-Host ""
Write-Host "  弯头位置：" -ForegroundColor White
Write-Host "  · 外框-左上:   Left=102, Top=104" -ForegroundColor Cyan
Write-Host "  · 外框-右上:   Left=684, Top=104" -ForegroundColor Cyan
Write-Host "  · 右侧-中折:   Left=684, Top=309" -ForegroundColor Cyan
Write-Host "  · 外框-右下:   Left=479, Top=380" -ForegroundColor Cyan
Write-Host "  · 外框-左下:   Left=102, Top=379" -ForegroundColor Cyan
Write-Host "════════════════════════════════════════" -ForegroundColor Green

Read-Host "`n按 Enter 退出"
