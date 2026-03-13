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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GJCS25004_分子筛转轮动态测试系统大屏.Control
{
    /// <summary>
    /// 按照步骤 1a 或 1b 操作，然后执行步骤 2 以在 XAML 文件中使用此自定义控件。
    ///
    /// 步骤 1a) 在当前项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:GJCS25004_分子筛转轮动态测试系统大屏.Control"
    ///
    ///
    /// 步骤 1b) 在其他项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:GJCS25004_分子筛转轮动态测试系统大屏.Control;assembly=GJCS25004_分子筛转轮动态测试系统大屏.Control"
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
    ///     <MyNamespace:ToggleImageControl/>
    ///
    /// </summary>
    public class ToggleImageControl : System.Windows.Controls.Control
    {
        static ToggleImageControl( )
        {
            DefaultStyleKeyProperty.OverrideMetadata( typeof( ToggleImageControl ) , new FrameworkPropertyMetadata( typeof( ToggleImageControl ) ) );
        }
        #region 依赖属性

        /// <summary>
        /// 控件开关状态的依赖属性
        /// </summary>
        public static readonly DependencyProperty IsToggledProperty =
            DependencyProperty.Register(
                "IsToggled" ,
                typeof( bool ) ,
                typeof( ToggleImageControl ) ,
                new FrameworkPropertyMetadata(
                    false ,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault ,
                    OnIsToggledChanged ) );

        /// <summary>
        /// 开关开启状态显示的图片资源
        /// </summary>
        public static readonly DependencyProperty OnImageSourceProperty =
            DependencyProperty.Register(
                "OnImageSource" ,
                typeof( ImageSource ) ,
                typeof( ToggleImageControl ) ,
                new PropertyMetadata( null ) );

        /// <summary>
        /// 开关关闭状态显示的图片资源
        /// </summary>
        public static readonly DependencyProperty OffImageSourceProperty =
            DependencyProperty.Register(
                "OffImageSource" ,
                typeof( ImageSource ) ,
                typeof( ToggleImageControl ) ,
                new PropertyMetadata( null ) );

        /// <summary>
        /// 当前显示的图片资源
        /// </summary>
        public static readonly DependencyProperty CurrentImageSourceProperty =
            DependencyProperty.Register(
                "CurrentImageSource" ,
                typeof( ImageSource ) ,
                typeof( ToggleImageControl ) ,
                new PropertyMetadata( null ) );
       
        #endregion
        #region 属性包装器

        /// <summary>
        /// 控件开关状态
        /// </summary>
        public bool IsToggled
        {
            get { return (bool) GetValue( IsToggledProperty ); }
            set { SetValue( IsToggledProperty , value ); }
        }

        /// <summary>
        /// 开关开启状态显示的图片
        /// </summary>
        public ImageSource OnImageSource
        {
            get { return (ImageSource) GetValue( OnImageSourceProperty ); }
            set { SetValue( OnImageSourceProperty , value ); }
        }

        /// <summary>
        /// 开关关闭状态显示的图片
        /// </summary>
        public ImageSource OffImageSource
        {
            get { return (ImageSource) GetValue( OffImageSourceProperty ); }
            set { SetValue( OffImageSourceProperty , value ); }
        }

        /// <summary>
        /// 当前显示的图片
        /// </summary>
        public ImageSource CurrentImageSource
        {
            get { return (ImageSource) GetValue( CurrentImageSourceProperty ); }
            private set { SetValue( CurrentImageSourceProperty , value ); }
        }

        #endregion
        #region 事件

        /// <summary>
        /// 状态改变的事件
        /// </summary>
        public event EventHandler<ToggleChangedEventArgs> ToggleChanged;

        #endregion
        #region 方法

        /// <summary>
        /// 在构造函数中初始化
        /// </summary>
        public ToggleImageControl( )
        {
            // 添加点击事件处理
            this.PreviewMouseLeftButtonDown += ToggleImageControl_PreviewMouseLeftButtonDown;

            // 初始化当前图片
            UpdateCurrentImage();
        }

        /// <summary>
        /// 处理鼠标点击事件
        /// </summary>
        private void ToggleImageControl_PreviewMouseLeftButtonDown( object sender , System.Windows.Input.MouseButtonEventArgs e )
        {
            // 切换状态
            IsToggled = !IsToggled;

            // 触发状态变更事件
            OnToggleChanged();
        }

        /// <summary>
        /// 触发ToggleChanged事件
        /// </summary>
        protected virtual void OnToggleChanged( )
        {
            ToggleChanged?.Invoke( this , new ToggleChangedEventArgs( IsToggled ) );
        }

        /// <summary>
        /// 当IsToggled属性改变时的回调
        /// </summary>
        private static void OnIsToggledChanged( DependencyObject d , DependencyPropertyChangedEventArgs e )
        {
            if (d is ToggleImageControl control)
            {
                control.UpdateCurrentImage();
            }
        }

        /// <summary>
        /// 根据开关状态更新当前显示的图片
        /// </summary>
        public void UpdateCurrentImage( )
        {
            CurrentImageSource = IsToggled ? OnImageSource : OffImageSource;
        }

        #endregion
    }

    /// <summary>
    /// 开关状态变化的事件参数
    /// </summary>
    public class ToggleChangedEventArgs : EventArgs
    {
        public bool IsToggled { get; private set; }

        public ToggleChangedEventArgs( bool isToggled )
        {
            IsToggled = isToggled;
        }
    }
}
