# =============================================================
#  分子筛转轮大屏  —  2.5D 配色自动替换脚本
#  使用方法：
#    1. 将此脚本放在项目根目录（和 MainWindow.xaml 同级）
#    2. 右键 → 用 PowerShell 运行
#    3. 脚本会自动备份原文件为 MainWindow.xaml.bak
# =============================================================

$filePath = ".\MainWindow.xaml"
$backupPath = ".\MainWindow.xaml.bak"

# ── 备份原文件 ──────────────────────────────────────────────
if (!(Test-Path $backupPath)) {
    Copy-Item $filePath $backupPath
    Write-Host "✅ 已备份原文件到 MainWindow.xaml.bak" -ForegroundColor Green
} else {
    Write-Host "⚠️  备份文件已存在，跳过备份" -ForegroundColor Yellow
}

$content = Get-Content $filePath -Raw -Encoding UTF8

# ── 替换1：主背景 RadialGradientBrush ───────────────────────
Write-Host "🔄 替换主背景色..." -ForegroundColor Cyan
$content = $content -replace '#FF0F1923', '#FF060D16'
$content = $content -replace '#FF132535', '#FF0A1A2C'
$content = $content -replace '"#0A1520"', '"#FF030810"'

# ── 替换2：管道槽 LinearGradientBrush ───────────────────────
Write-Host "🔄 替换管道槽背景渐变..." -ForegroundColor Cyan

# 匹配并替换 管道槽三段式渐变（#1A3550 → #254A68 → #1A3550）
# 使用多行替换
$pipeSlotPattern = @'
<LinearGradientBrush StartPoint="0.5,0" EndPoint="0.5,1">

                            <GradientStop Color="#1A3550" Offset="0.1"/>

                            <GradientStop Color="#254A68" Offset="0.5"/>

                            <GradientStop Color="#1A3550" Offset="0.8"/>

                        </LinearGradientBrush>
'@

$pipeSlotReplacement = @'
<LinearGradientBrush StartPoint="0.5,0" EndPoint="0.5,1">
                            <GradientStop Color="#FF010A14" Offset="0"/>
                            <GradientStop Color="#FF072030" Offset="0.12"/>
                            <GradientStop Color="#FF00A8C0" Offset="0.30"/>
                            <GradientStop Color="#FF005868" Offset="0.55"/>
                            <GradientStop Color="#FF082030" Offset="0.78"/>
                            <GradientStop Color="#FF010A14" Offset="1"/>
                        </LinearGradientBrush>
'@

$beforeCount = ([regex]::Matches($content, [regex]::Escape($pipeSlotPattern))).Count
$content = $content.Replace($pipeSlotPattern, $pipeSlotReplacement)
Write-Host "   替换管道槽渐变 $beforeCount 处" -ForegroundColor Gray

# ── 替换3：管道流动 Line 颜色 ────────────────────────────────
Write-Host "🔄 替换管道流动线颜色..." -ForegroundColor Cyan
$content = $content -replace 'Stroke="#3A7CAF"', 'Stroke="#00E8FF"'

# ── 替换4：管道流动线透明度（仅Line元素上的Opacity="0.5"）────
Write-Host "🔄 替换管道流动线透明度..." -ForegroundColor Cyan
# 精确匹配 Line 元素上的 Opacity="0.5"，避免误改其他元素
$content = $content -replace '(StrokeEndLineCap="[^"]*"\s+Opacity=)"0\.5"', '$1"0.85"'

# ── 替换5：TextBox 样式属性（交给 Styles3D.xaml 的全局Style接管）
Write-Host "🔄 清理 TextBox 内联颜色属性（交由全局Style接管）..." -ForegroundColor Cyan
$content = $content -replace '\s*Background="#E61E3C5A"', ''
$content = $content -replace '\s*BorderBrush="#664A8AB8"', ''
$content = $content -replace '\s*Foreground="#A0B8CC"', ''

# ── 替换6：Path 导向管道颜色 ─────────────────────────────────
Write-Host "🔄 替换 Path 管道导向颜色..." -ForegroundColor Cyan
$content = $content -replace 'Color="#4A6A8A"', 'Color="#006A80"'
$content = $content -replace 'Color="#6A8AAA"', 'Color="#00B0C8"'
$content = $content -replace 'Stroke="#6A7A8A"', 'Stroke="#00788A"'

# ── 替换7：标题文字颜色 ──────────────────────────────────────
Write-Host "🔄 替换标题文字颜色..." -ForegroundColor Cyan
$content = $content -replace 'Foreground="#E8ECF1"', 'Foreground="#00E5FF"'

# ── 替换8：标题 Border 添加背景 ──────────────────────────────
Write-Host "🔄 升级标题栏 Border..." -ForegroundColor Cyan
$oldTitleBorder = '<Border 
            Height="40" 
            VerticalAlignment="Top">'
$newTitleBorder = '<Border 
            Height="40" 
            VerticalAlignment="Top"
            Effect="{StaticResource CardDropShadow}">'
$content = $content.Replace($oldTitleBorder, $newTitleBorder)

# ── 替换9：为标题 TextBlock 加发光 Effect ────────────────────
$oldTitleText = '                       FontSize="12" 
                       HorizontalAlignment="Center" 
                       VerticalAlignment="Center" 
                       Foreground="#00E5FF"/>'
$newTitleText = '                       FontSize="12"
                       FontWeight="SemiBold"
                       HorizontalAlignment="Center" 
                       VerticalAlignment="Center" 
                       Foreground="#00E5FF">
                    <TextBlock.Effect>
                        <DropShadowEffect Color="#00C8FF" BlurRadius="8" ShadowDepth="0" Opacity="0.70"/>
                    </TextBlock.Effect>
                </TextBlock>'
$content = $content.Replace($oldTitleText, $newTitleText)

# ── 替换10：FanControl 添加发光效果 ─────────────────────────
Write-Host "🔄 为 FanControl 添加发光效果..." -ForegroundColor Cyan
# fanControl
$content = $content -replace '(<local:FanControl x:Name="fanControl"(?!\s+Effect))', '$1
                  Effect="{StaticResource CyanGlowEffect}"'
# fanControl1
$content = $content -replace '(<local:FanControl x:Name="fanControl1"(?!\s+Effect))', '$1
                  Effect="{StaticResource CyanGlowEffect}"'

# ── 写回文件 ─────────────────────────────────────────────────
[System.IO.File]::WriteAllText($filePath, $content, [System.Text.Encoding]::UTF8)

Write-Host ""
Write-Host "═══════════════════════════════════════════════" -ForegroundColor Green
Write-Host "✅ 所有替换完成！" -ForegroundColor Green
Write-Host "   原文件备份：MainWindow.xaml.bak" -ForegroundColor Green
Write-Host "   如需回滚：将 .bak 文件改回 .xaml 即可" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════" -ForegroundColor Green
Write-Host ""
Write-Host "📋 下一步操作：" -ForegroundColor Yellow
Write-Host "   1. 将 Styles3D.xaml 复制到项目根目录（和 App.xaml 同级）" -ForegroundColor White
Write-Host "   2. 在 VS 中右键项目 → 添加现有项 → 选择 Styles3D.xaml" -ForegroundColor White
Write-Host "   3. 替换 App.xaml 内容（已提供新版本）" -ForegroundColor White
Write-Host "   4. 编译运行查看效果" -ForegroundColor White
Write-Host ""
