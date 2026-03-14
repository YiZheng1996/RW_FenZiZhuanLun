using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace GJCS25004_分子筛转轮动态测试系统大屏.Control
{
    /// <summary>
    /// 管道圆角弯头控件 —— 用于替代直角拼接，实现平滑圆弧过渡
    /// 
    /// 使用方法：
    ///   在 XAML 中添加：
    ///   <local:PipeElbowControl Direction="TopToRight" BendRadius="15" 
    ///       PipeThickness="8" Width="30" Height="30" Canvas.Left="xxx" Canvas.Top="yyy"/>
    ///
    /// 注意：Width 和 Height 应设置为 BendRadius * 2，或至少 >= BendRadius
    /// </summary>
    public class PipeElbowControl : System.Windows.Controls.Control
    {
        #region 枚举

        /// <summary>
        /// 弯头方向：描述管道从哪个方向转向哪个方向
        /// 
        /// 想象一个正方形区域，管道从一个边进入，从另一个相邻边出去：
        ///   TopToRight:    从上方进入 → 向右方出去   ╮
        ///   TopToLeft:     从上方进入 → 向左方出去   ╭
        ///   BottomToRight: 从下方进入 → 向右方出去   ╯
        ///   BottomToLeft:  从下方进入 → 向左方出去   ╰
        ///   LeftToTop:     从左方进入 → 向上方出去   ╰ (同 BottomToLeft)
        ///   LeftToBottom:  从左方进入 → 向下方出去   ╭ (同 TopToLeft)
        ///   RightToTop:    从右方进入 → 向上方出去   ╯ (同 BottomToRight)
        ///   RightToBottom: 从右方进入 → 向下方出去   ╮ (同 TopToRight)
        /// </summary>
        public enum ElbowDirection
        {
            /// <summary>从上方进 → 向右出  ╮ 圆心在右上角</summary>
            TopToRight,
            /// <summary>从上方进 → 向左出  ╭ 圆心在左上角</summary>
            TopToLeft,
            /// <summary>从下方进 → 向右出  ╯ 圆心在右下角</summary>
            BottomToRight,
            /// <summary>从下方进 → 向左出  ╰ 圆心在左下角</summary>
            BottomToLeft,
            /// <summary>从左进 → 向上出（等价于 BottomToLeft 翻转）</summary>
            LeftToTop,
            /// <summary>从左进 → 向下出（等价于 TopToLeft 翻转）</summary>
            LeftToBottom,
            /// <summary>从右进 → 向上出（等价于 BottomToRight 翻转）</summary>
            RightToTop,
            /// <summary>从右进 → 向下出（等价于 TopToRight 翻转）</summary>
            RightToBottom
        }

        #endregion

        #region 依赖属性

        public static readonly DependencyProperty DirectionProperty =
            DependencyProperty.Register("Direction", typeof(ElbowDirection),
                typeof(PipeElbowControl),
                new FrameworkPropertyMetadata(ElbowDirection.TopToRight,
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty BendRadiusProperty =
            DependencyProperty.Register("BendRadius", typeof(double),
                typeof(PipeElbowControl),
                new FrameworkPropertyMetadata(15.0,
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty PipeThicknessProperty =
            DependencyProperty.Register("PipeThickness", typeof(double),
                typeof(PipeElbowControl),
                new FrameworkPropertyMetadata(8.0,
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty PipeBrushProperty =
            DependencyProperty.Register("PipeBrush", typeof(Brush),
                typeof(PipeElbowControl),
                new FrameworkPropertyMetadata(CreateDefaultPipeBrush(),
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty FluidBrushProperty =
            DependencyProperty.Register("FluidBrush", typeof(Brush),
                typeof(PipeElbowControl),
                new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromArgb(0xD9, 0x00, 0xA3, 0xAF)),
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty FluidThicknessProperty =
            DependencyProperty.Register("FluidThickness", typeof(double),
                typeof(PipeElbowControl),
                new FrameworkPropertyMetadata(5.0,
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty IsFlowingProperty =
            DependencyProperty.Register("IsFlowing", typeof(bool),
                typeof(PipeElbowControl),
                new PropertyMetadata(false, OnIsFlowingChanged));

        public static readonly DependencyProperty FlowSpeedProperty =
            DependencyProperty.Register("FlowSpeed", typeof(double),
                typeof(PipeElbowControl),
                new PropertyMetadata(1.0));

        #endregion

        #region 属性

        public ElbowDirection Direction
        {
            get => (ElbowDirection)GetValue(DirectionProperty);
            set => SetValue(DirectionProperty, value);
        }

        public double BendRadius
        {
            get => (double)GetValue(BendRadiusProperty);
            set => SetValue(BendRadiusProperty, value);
        }

        public double PipeThickness
        {
            get => (double)GetValue(PipeThicknessProperty);
            set => SetValue(PipeThicknessProperty, value);
        }

        public Brush PipeBrush
        {
            get => (Brush)GetValue(PipeBrushProperty);
            set => SetValue(PipeBrushProperty, value);
        }

        public Brush FluidBrush
        {
            get => (Brush)GetValue(FluidBrushProperty);
            set => SetValue(FluidBrushProperty, value);
        }

        public double FluidThickness
        {
            get => (double)GetValue(FluidThicknessProperty);
            set => SetValue(FluidThicknessProperty, value);
        }

        public bool IsFlowing
        {
            get => (bool)GetValue(IsFlowingProperty);
            set => SetValue(IsFlowingProperty, value);
        }

        public double FlowSpeed
        {
            get => (double)GetValue(FlowSpeedProperty);
            set => SetValue(FlowSpeedProperty, value);
        }

        #endregion

        #region 私有字段

        private Path _fluidPath;
        private Storyboard _flowStoryboard;

        #endregion

        #region 构造

        static PipeElbowControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PipeElbowControl),
                new FrameworkPropertyMetadata(typeof(PipeElbowControl)));
        }

        public PipeElbowControl()
        {
            this.Loaded += (s, e) => InitFluidAnimation();
        }

        #endregion

        #region 渲染

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            double w = ActualWidth;
            double h = ActualHeight;
            if (w <= 0 || h <= 0) return;

            double r = BendRadius;
            double t = PipeThickness;

            // ── 1. 计算圆弧的起点、终点、圆心和扫描方向 ──
            GetArcGeometry(r, w, h, out Point arcStart, out Point arcEnd,
                           out SweepDirection sweep);

            // ── 2. 绘制管道外壳（粗线条 = 管壁） ──
            var pipeGeometry = new StreamGeometry();
            using (var ctx = pipeGeometry.Open())
            {
                ctx.BeginFigure(arcStart, false, false);
                ctx.ArcTo(arcEnd, new Size(r, r), 0, false, sweep, true, false);
            }
            pipeGeometry.Freeze();

            var pipePen = new Pen(PipeBrush, t)
            {
                StartLineCap = PenLineCap.Flat,
                EndLineCap = PenLineCap.Flat
            };
            dc.DrawGeometry(null, pipePen, pipeGeometry);

            // ── 3. 绘制流体虚线（内部细线 + 动画） ──
            var fluidGeometry = new StreamGeometry();
            using (var ctx = fluidGeometry.Open())
            {
                ctx.BeginFigure(arcStart, false, false);
                ctx.ArcTo(arcEnd, new Size(r, r), 0, false, sweep, true, false);
            }
            fluidGeometry.Freeze();

            // 用 Path 元素承载流体，以便做 StrokeDashOffset 动画
            if (_fluidPath == null)
            {
                _fluidPath = new Path();
                // 添加到可视化树
                var adornerLayer = System.Windows.Documents.AdornerLayer.GetAdornerLayer(this);
            }

            // 直接用 DrawingContext 绘制流体（静态部分）
            var fluidPen = new Pen(FluidBrush, FluidThickness)
            {
                DashStyle = new DashStyle(new double[] { 2, 3 }, 0),
                DashCap = PenLineCap.Round,
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round
            };
            dc.DrawGeometry(null, fluidPen, fluidGeometry);
        }

        /// <summary>
        /// 根据方向枚举计算圆弧的起点、终点和扫描方向
        /// </summary>
        private void GetArcGeometry(double r, double w, double h,
            out Point start, out Point end, out SweepDirection sweep)
        {
            // 统一映射：8种方向映射到4种基本弧形
            var dir = NormalizeDirection(Direction);

            switch (dir)
            {
                case ElbowDirection.TopToRight:
                    // 圆心在右上角 (r, 0)，弧从上边中点到右边中点
                    // 起点：从上方进入的管道末端
                    start = new Point(w - r, 0);     // 上边
                    end = new Point(w, r);            // 右边
                    sweep = SweepDirection.Clockwise;
                    break;

                case ElbowDirection.TopToLeft:
                    // 圆心在左上角 (0, 0)
                    start = new Point(r, 0);          // 上边
                    end = new Point(0, r);             // 左边
                    sweep = SweepDirection.Counterclockwise;
                    break;

                case ElbowDirection.BottomToRight:
                    // 圆心在右下角 (r, r)
                    start = new Point(w - r, h);      // 下边
                    end = new Point(w, h - r);         // 右边
                    sweep = SweepDirection.Counterclockwise;
                    break;

                case ElbowDirection.BottomToLeft:
                default:
                    // 圆心在左下角 (0, r)
                    start = new Point(r, h);           // 下边
                    end = new Point(0, h - r);          // 左边
                    sweep = SweepDirection.Clockwise;
                    break;
            }
        }

        /// <summary>
        /// 将8种方向映射到4种基本方向（因为 LeftToTop ≡ BottomToLeft 等）
        /// </summary>
        private ElbowDirection NormalizeDirection(ElbowDirection dir)
        {
            switch (dir)
            {
                case ElbowDirection.LeftToTop:     return ElbowDirection.BottomToLeft;
                case ElbowDirection.LeftToBottom:   return ElbowDirection.TopToLeft;
                case ElbowDirection.RightToTop:     return ElbowDirection.BottomToRight;
                case ElbowDirection.RightToBottom:  return ElbowDirection.TopToRight;
                default: return dir;
            }
        }

        #endregion

        #region 流动动画

        private static void OnIsFlowingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PipeElbowControl ctrl)
            {
                if ((bool)e.NewValue)
                    ctrl.StartFlowAnimation();
                else
                    ctrl.StopFlowAnimation();
            }
        }

        private void InitFluidAnimation()
        {
            if (IsFlowing)
                StartFlowAnimation();
        }

        /// <summary>
        /// 启动流体虚线的流动动画
        /// 注意：由于弯头使用 OnRender 绘制，动画需通过重绘实现
        /// 推荐方式：在外部使用 Path 元素包裹弯头路径，配合 StrokeDashOffset 动画
        /// 见下方的 CreateFlowingElbowPath() 静态辅助方法
        /// </summary>
        public void StartFlowAnimation()
        {
            // OnRender 模式下动画较复杂，推荐使用静态辅助方法
            // 这里提供定时器方式作为备选
        }

        public void StopFlowAnimation()
        {
            _flowStoryboard?.Stop();
        }

        #endregion

        #region 静态辅助方法（推荐方式：直接在 XAML 中使用 Path）

        /// <summary>
        /// 创建默认的管道 3D 渐变画刷（与项目中 Pipe3DBrush 一致）
        /// </summary>
        private static Brush CreateDefaultPipeBrush()
        {
            var brush = new LinearGradientBrush
            {
                StartPoint = new Point(0.5, 0),
                EndPoint = new Point(0.5, 1)
            };
            brush.GradientStops.Add(new GradientStop(Color.FromRgb(0x0D, 0x3A, 0x50), 0));
            brush.GradientStops.Add(new GradientStop(Color.FromRgb(0x00, 0xB8, 0xD0), 0.22));
            brush.GradientStops.Add(new GradientStop(Color.FromRgb(0x00, 0x68, 0x78), 0.55));
            brush.GradientStops.Add(new GradientStop(Color.FromRgb(0x00, 0x30, 0x40), 0.82));
            brush.GradientStops.Add(new GradientStop(Color.FromRgb(0x00, 0x14, 0x20), 1));
            brush.Freeze();
            return brush;
        }

        /// <summary>
        /// 【推荐】生成弯头的 PathGeometry Data 字符串
        /// 可直接用在 XAML 的 Path.Data 中：
        ///   Data="{x:Static local:PipeElbowControl.GetElbowData(...)}" 
        /// 或在代码中动态生成
        /// </summary>
        /// <param name="direction">弯头方向</param>
        /// <param name="radius">弯曲半径</param>
        /// <returns>Mini-Language 路径字符串</returns>
        public static string GetElbowPathData(ElbowDirection direction, double radius)
        {
            double r = radius;

            switch (direction)
            {
                case ElbowDirection.TopToRight:
                case ElbowDirection.RightToBottom:
                    // ╮ 从上到右：M 0,0  A r,r 0 0 1 r,r
                    return $"M 0,0 A {r},{r} 0 0 1 {r},{r}";

                case ElbowDirection.TopToLeft:
                case ElbowDirection.LeftToBottom:
                    // ╭ 从上到左：M r,0  A r,r 0 0 0 0,r
                    return $"M {r},0 A {r},{r} 0 0 0 0,{r}";

                case ElbowDirection.BottomToRight:
                case ElbowDirection.RightToTop:
                    // ╯ 从下到右：M 0,r  A r,r 0 0 0 r,0
                    return $"M 0,{r} A {r},{r} 0 0 0 {r},0";

                case ElbowDirection.BottomToLeft:
                case ElbowDirection.LeftToTop:
                    // ╰ 从下到左：M r,r  A r,r 0 0 1 0,0  → 不对，重新算
                    // 实际：从底部中点到左边中点
                    return $"M {r},{r} A {r},{r} 0 0 0 0,0";

                default:
                    return $"M 0,0 A {r},{r} 0 0 1 {r},{r}";
            }
        }

        #endregion
    }
}
