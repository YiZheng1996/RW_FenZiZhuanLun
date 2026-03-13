
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
    ///     <MyNamespace:FanControl/>
    ///
    /// </summary>
    public class FanControl : System.Windows.Controls.Control
    {
        public delegate void UpdateFlowsFromCurrentValveStates();
        public static UpdateFlowsFromCurrentValveStates UpdateFlowsFromCurrentValveStatesHandler;
        /// <summary>
        /// 转速设置
        /// </summary>
        private int RotationRate = 0;
        /// <summary>
        /// 动画 重复 次数
        /// </summary>
        private int RotationRepeatTime = 100000;
        static FanControl( )
        {
            DefaultStyleKeyProperty.OverrideMetadata( typeof( FanControl ) , new FrameworkPropertyMetadata( typeof( FanControl ) ) );

        }
        #region 控件列表
        /// <summary>
        /// 风扇 扇页元素
        /// </summary>
        Grid _GridFanJiShuiBeng = null;
        /// <summary>
        /// 风力大小
        /// </summary>
        TextBlock _FanValue = null;
        /// <summary>
        /// 动画标题
        /// </summary>
        TextBlock _JiShuiBengTitle = null;
        #endregion

        #region 控件绑定
        public override void OnApplyTemplate( )
        {
            base.OnApplyTemplate();

            _GridFanJiShuiBeng = GetTemplateChild( "GridFanJiShuiBeng" ) as Grid;
            _FanValue = GetTemplateChild( "FanValue" ) as TextBlock;
            _JiShuiBengTitle = GetTemplateChild( "JiShuiBengTitle" ) as TextBlock;

            _FanValue.SetBinding( TextBlock.TextProperty , new Binding( "FanValue" ) { Source = this } );
            _JiShuiBengTitle.SetBinding( TextBlock.TextProperty , new Binding( "JiShuiBengTitle" ) { Source = this } );

            // Remove this line
            // _GridFanJiShuiBeng.SetBinding(Grid.StyleProperty, new Binding("FanValue") { Source = this });

            // 确保在模板应用后再次调用 FanValueStoryboardPlay
            FanValueStoryboardPlay( new DependencyPropertyChangedEventArgs( _FanValueProperty , null , FanValue ) );
        }
        #endregion

        #region 依赖及属性

        /// <summary>
        /// 标题
        /// </summary>
        public string JiShuiBengTitle
        {
            get { return (string) GetValue( _JiShuiBengTitleProperty ); }
            set { SetValue( _JiShuiBengTitleProperty , value ); }
        }
        public static readonly DependencyProperty _JiShuiBengTitleProperty =
            DependencyProperty.Register( "JiShuiBengTitle" , typeof( string ) , typeof( FanControl ) , new PropertyMetadata( "" ) );
        /// <summary>
        /// 风力值
        /// </summary>
        public float FanValue
        {
            get { return (float) GetValue( _FanValueProperty ); }
            set { SetValue( _FanValueProperty , value ); }
        }
        public static readonly DependencyProperty _FanValueProperty =
            DependencyProperty.Register( "FanValue" , typeof( float ) , typeof( FanControl ) , new PropertyMetadata( 0f , FanValueCallBack ) );

        /// <summary>
        /// 旋转方向
        /// </summary>
        public bool IsClockwise
        {
            get { return (bool) GetValue( IsClockwiseProperty ); }
            set { SetValue( IsClockwiseProperty , value ); }
        }
        public static readonly DependencyProperty IsClockwiseProperty =
            DependencyProperty.Register( "IsClockwise" , typeof( bool ) , typeof( FanControl ) , new PropertyMetadata( true , FanValueCallBack ) );

        private static void FanValueCallBack( DependencyObject d , DependencyPropertyChangedEventArgs e )
        {
            (d as FanControl).FanValueStoryboardPlay( e );
        }
        private void FanValueStoryboardPlay( DependencyPropertyChangedEventArgs e )
        {
            if (_GridFanJiShuiBeng == null)
            {
                // 控件模板还没有应用，直接返回
                return;
            }
            float newV = float.Parse( e.NewValue.ToString() );
            // float oldV = float.Parse( e.OldValue.ToString() );
            ////转速设置
            RotationRepeatTime = 100000;
            if (newV == 1)
            {
                RotationRate = 35000;
            }
            else if (newV == 2)
            {
                RotationRate = 16000;
            }
            else if (newV == 3)
            {
                RotationRate = 6000;
            }
            else if (newV == 0)
            {
                RotationRepeatTime = 0;
            }
            //元素 转动动画
            RotateTransform rtGSB = new RotateTransform();
            rtGSB.CenterX = 0;
            rtGSB.CenterY = 0;
            if (_GridFanJiShuiBeng != null)
            {
                _GridFanJiShuiBeng.RenderTransform = rtGSB;
            }

            // 根据旋转方向设置动画
            double toValue = IsClockwise ? 3000 : -3000;
            DoubleAnimation GSB_DA = new DoubleAnimation( 0 , toValue , new Duration( TimeSpan.FromMilliseconds( RotationRate ) ) );
            GSB_DA.RepeatBehavior = new RepeatBehavior( RotationRepeatTime );
            rtGSB.BeginAnimation( RotateTransform.AngleProperty , GSB_DA );
            UpdateFlowsFromCurrentValveStatesHandler();
        }
        #endregion
    }

}
