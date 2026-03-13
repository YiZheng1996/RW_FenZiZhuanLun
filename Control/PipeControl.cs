using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GJCS25004_分子筛转轮动态测试系统大屏
{
    /// <summary>
    /// 按照步骤 1a 或 1b 操作，然后执行步骤 2 以在 XAML 文件中使用此自定义控件。
    ///
    /// 步骤 1a) 在当前项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:GJCS25004_分子筛转轮动态测试系统大屏"
    ///
    ///
    /// 步骤 1b) 在其他项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:GJCS25004_分子筛转轮动态测试系统大屏;assembly=GJCS25004_分子筛转轮动态测试系统大屏"
    ///
    /// 您还需要添加一个从 XAML 文件所在的项目到此项目的项目引用，
    /// 并重新生成以避免编译错误:
    ///
    ///     在解决方案资源管理器中右击目标项目，然后依次单击
    ///     “添加引用”->“项目”->[浏览查找并选择此项目]
    ///
    ///
    /// 步骤 2)
    /// 继续操作并在 XAML 文件中使用控件。
    ///
    ///     <MyNamespace:Conduit/>
    ///
    /// </summary>
    ///
    public enum FlowDirection
    {
        LeftToRight,
        RightToLeft,
        TopToBottom,
        BottomToTop
    }
    public class PipeControl : System.Windows.Controls.Control
    {
        static PipeControl( )
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof( PipeControl ) , new FrameworkPropertyMetadata(typeof( PipeControl ) ));
        }
        #region 依赖属性

        public static readonly DependencyProperty PipeColorProperty =
            DependencyProperty.Register( "PipeColor" , typeof( Brush ) , typeof( PipeControl ) ,
                new PropertyMetadata( CreateDefaultPipeGradient() ) );

        public static readonly DependencyProperty FluidColorProperty =
            DependencyProperty.Register( "FluidColor" , typeof( Brush ) , typeof( PipeControl ) ,
                new PropertyMetadata( new SolidColorBrush( Colors.LightBlue ) ) );

        public static readonly DependencyProperty FlowDirectionProperty =
            DependencyProperty.Register( "FlowDirection" , typeof( FlowDirection ) , typeof( PipeControl ) ,
                new PropertyMetadata( FlowDirection.LeftToRight , OnFlowDirectionChanged ) );

        public static readonly DependencyProperty FlowSpeedProperty =
            DependencyProperty.Register( "FlowSpeed" , typeof( double ) , typeof( PipeControl ) ,
                new PropertyMetadata( 2.0 , OnFlowSpeedChanged ) );

        public static readonly DependencyProperty PipeThicknessProperty =
            DependencyProperty.Register( "PipeThickness" , typeof( double ) , typeof( PipeControl ) ,
                new PropertyMetadata( 20.0 ) );

        public static readonly DependencyProperty IsFlowingProperty =
            DependencyProperty.Register( "IsFlowing" , typeof( bool ) , typeof( PipeControl ) ,
                new PropertyMetadata( true , OnIsFlowingChanged ) );

        #endregion

        #region 属性

        public Brush PipeColor
        {
            get { return (Brush) GetValue( PipeColorProperty ); }
            set { SetValue( PipeColorProperty , value ); }
        }

        public Brush FluidColor
        {
            get { return (Brush) GetValue( FluidColorProperty ); }
            set { SetValue( FluidColorProperty , value ); }
        }

        public FlowDirection FlowDirection
        {
            get { return (FlowDirection) GetValue( FlowDirectionProperty ); }
            set { SetValue( FlowDirectionProperty , value ); }
        }

        public double FlowSpeed
        {
            get { return (double) GetValue( FlowSpeedProperty ); }
            set { SetValue( FlowSpeedProperty , value ); }
        }

        public double PipeThickness
        {
            get { return (double) GetValue( PipeThicknessProperty ); }
            set { SetValue( PipeThicknessProperty , value ); }
        }

        public bool IsFlowing
        {
            get { return (bool) GetValue( IsFlowingProperty ); }
            set { SetValue( IsFlowingProperty , value ); }
        }

        #endregion

        private Canvas _canvas;
        private Rectangle _pipe;
        private Path _fluid;
        private Storyboard _flowAnimation;

        public override void OnApplyTemplate( )
        {
            base.OnApplyTemplate();

            _canvas = new Canvas();
            _pipe = new Rectangle();
            _fluid = new Path();

            // 设置管道样式
            _pipe.Stroke = PipeColor;
            _pipe.Fill = PipeColor;
            _pipe.StrokeThickness = 2;
            _pipe.RadiusX = PipeThickness / 2;
            _pipe.RadiusY = PipeThickness / 2;

            // 将元素添加到画布
            _canvas.Children.Add( _pipe );
            _canvas.Children.Add( _fluid );

            // 将画布添加到控件内容
            this.AddVisualChild( _canvas );
            this.AddLogicalChild( _canvas );

            // 初始化流动动画
            InitializeFlowAnimation();

            // 如果启用了流动，开始动画
            if (IsFlowing)
            {
                StartFlowAnimation();
            }

            // 在尺寸变化时更新视觉效果
            this.SizeChanged += PipeControl_SizeChanged;
            UpdateVisuals();
        }

        private void PipeControl_SizeChanged( object sender , SizeChangedEventArgs e )
        {
            UpdateVisuals();
        }

        private void UpdateVisuals( )
        {
            if (_pipe == null || _fluid == null)
                return;

            // 设置管道尺寸
            _pipe.Width = this.ActualWidth;
            _pipe.Height = this.ActualHeight;

            // 创建流体路径
            StreamGeometry streamGeometry = new StreamGeometry();
            using (StreamGeometryContext ctx = streamGeometry.Open())
            {
                // 缩小气体流动的波浪高度
                double waveHeight = PipeThickness / 5;  // 将原来的值降低，确保气体流动不太大
                double waveWidth = PipeThickness;

                Point startPoint = new Point();

                switch (FlowDirection)
                {
                    case FlowDirection.LeftToRight:
                    case FlowDirection.RightToLeft:
                        startPoint = new Point( 0 , PipeThickness / 2 );
                        ctx.BeginFigure( startPoint , true , true );

                        for (double x = 0 ; x < ActualWidth + waveWidth ; x += waveWidth)
                        {
                            ctx.QuadraticBezierTo(
                                new Point( x + waveWidth / 2 , PipeThickness / 2 - waveHeight ) ,
                                new Point( x + waveWidth , PipeThickness / 2 ) ,
                                true , false );

                            if (x + waveWidth < ActualWidth + waveWidth)
                            {
                                ctx.QuadraticBezierTo(
                                    new Point( x + waveWidth * 1.5 , PipeThickness / 2 + waveHeight ) ,
                                    new Point( x + waveWidth * 2 , PipeThickness / 2 ) ,
                                    true , false );
                            }
                        }

                        ctx.LineTo( new Point( ActualWidth , ActualHeight ) , true , false );
                        ctx.LineTo( new Point( 0 , ActualHeight ) , true , false );
                        ctx.LineTo( startPoint , true , false );
                        break;

                    case FlowDirection.TopToBottom:
                    case FlowDirection.BottomToTop:
                        startPoint = new Point( PipeThickness / 2 , 0 );
                        ctx.BeginFigure( startPoint , true , true );

                        for (double y = 0 ; y < ActualHeight + waveWidth ; y += waveWidth)
                        {
                            ctx.QuadraticBezierTo(
                                new Point( PipeThickness / 2 - waveHeight , y + waveWidth / 2 ) ,
                                new Point( PipeThickness / 2 , y + waveWidth ) ,
                                true , false );

                            if (y + waveWidth < ActualHeight + waveWidth)
                            {
                                ctx.QuadraticBezierTo(
                                    new Point( PipeThickness / 2 + waveHeight , y + waveWidth * 1.5 ) ,
                                    new Point( PipeThickness / 2 , y + waveWidth * 2 ) ,
                                    true , false );
                            }
                        }

                        ctx.LineTo( new Point( ActualWidth , ActualHeight ) , true , false );
                        ctx.LineTo( new Point( ActualWidth , 0 ) , true , false );
                        ctx.LineTo( startPoint , true , false );
                        break;
                }
            }

            // 设置流体路径
            _fluid.Data = streamGeometry;
            _fluid.Fill = FluidColor;

            // 重新初始化动画
            InitializeFlowAnimation();

            if (IsFlowing)
            {
                StartFlowAnimation();
            }
        }

        private void InitializeFlowAnimation( )
        {
            if (_fluid == null)
                return;

            _flowAnimation = new Storyboard();

            TranslateTransform translateTransform = new TranslateTransform();
            _fluid.RenderTransform = translateTransform;

            DoubleAnimation animation = new DoubleAnimation();
            animation.Duration = TimeSpan.FromSeconds( 5 / FlowSpeed );
            animation.RepeatBehavior = RepeatBehavior.Forever;

            switch (FlowDirection)
            {
                case FlowDirection.LeftToRight:
                    animation.From = -PipeThickness;  // 缩小起始位置
                    animation.To = 0;
                    Storyboard.SetTargetProperty( _flowAnimation , new PropertyPath( "(UIElement.RenderTransform).(TranslateTransform.X)" ) );
                    break;

                case FlowDirection.RightToLeft:
                    animation.From = 0;
                    animation.To = -PipeThickness;  // 缩小结束位置
                    Storyboard.SetTargetProperty( _flowAnimation , new PropertyPath( "(UIElement.RenderTransform).(TranslateTransform.X)" ) );
                    break;

                case FlowDirection.TopToBottom:
                    animation.From = -PipeThickness;
                    animation.To = 0;
                    Storyboard.SetTargetProperty( _flowAnimation , new PropertyPath( "(UIElement.RenderTransform).(TranslateTransform.Y)" ) );
                    break;

                case FlowDirection.BottomToTop:
                    animation.From = 0;
                    animation.To = -PipeThickness;
                    Storyboard.SetTargetProperty( _flowAnimation , new PropertyPath( "(UIElement.RenderTransform).(TranslateTransform.Y)" ) );
                    break;
            }

            Storyboard.SetTarget( animation , _fluid );
            _flowAnimation.Children.Add( animation );
        }
        private void StartFlowAnimation( )
        {
            if (_flowAnimation != null)
            {
                _flowAnimation.Begin();
            }
        }

        private void StopFlowAnimation( )
        {
            if (_flowAnimation != null)
            {
                _flowAnimation.Stop();
            }
        }

        protected override Visual GetVisualChild( int index )
        {
            if (index != 0 || _canvas == null)
                throw new ArgumentOutOfRangeException( "index" );
            return _canvas;
        }

        protected override int VisualChildrenCount
        {
            get { return _canvas != null ? 1 : 0; }
        }

        private static void OnFlowDirectionChanged( DependencyObject d , DependencyPropertyChangedEventArgs e )
        {
            PipeControl control = d as PipeControl;
            if (control != null)
            {
                control.UpdateVisuals();
            }
        }

        private static void OnFlowSpeedChanged( DependencyObject d , DependencyPropertyChangedEventArgs e )
        {
            PipeControl control = d as PipeControl;
            if (control != null)
            {
                control.InitializeFlowAnimation();
                if (control.IsFlowing)
                {
                    control.StopFlowAnimation();
                    control.StartFlowAnimation();
                }
            }
        }

        private static void OnIsFlowingChanged( DependencyObject d , DependencyPropertyChangedEventArgs e )
        {
            PipeControl control = d as PipeControl;
            if (control != null)
            {
                bool isFlowing = (bool) e.NewValue;
                if (isFlowing)
                {
                    control.StartFlowAnimation();
                }
                else
                {
                    control.StopFlowAnimation();
                }
            }
        }

        private static LinearGradientBrush CreateDefaultPipeGradient( )
        {
            LinearGradientBrush gradientBrush = new LinearGradientBrush();
            gradientBrush.StartPoint = new Point( 0 , 0 );
            gradientBrush.EndPoint = new Point( 0 , 1 );
            gradientBrush.GradientStops.Add( new GradientStop( Colors.White , 0.0 ) );
            gradientBrush.GradientStops.Add( new GradientStop( Colors.LightGray , 0.5 ) );
            gradientBrush.GradientStops.Add( new GradientStop( Color.FromRgb( 180 , 180 , 180 ) , 1.0 ) );
            return gradientBrush;
        }
    }
}
