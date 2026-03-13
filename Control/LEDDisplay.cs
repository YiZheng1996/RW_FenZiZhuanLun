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
    ///     <MyNamespace:LEDDisplay/>
    ///
    /// </summary>
    public class LEDDisplay : System.Windows.Controls.Control
    {
        static LEDDisplay( )
        {
            DefaultStyleKeyProperty.OverrideMetadata( typeof( LEDDisplay ) ,
                new FrameworkPropertyMetadata( typeof( LEDDisplay ) ) );
        }

        // Label text property
        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register( "LabelText" , typeof( string ) , typeof( LEDDisplay ) ,
                new PropertyMetadata( "位置(mm)" ) );

        public string LabelText
        {
            get { return (string) GetValue( LabelTextProperty ); }
            set { SetValue( LabelTextProperty , value ); }
        }

        // Display value property
        public static readonly DependencyProperty DisplayValueProperty =
            DependencyProperty.Register( "DisplayValue" , typeof( double ) , typeof( LEDDisplay ) ,
                new PropertyMetadata( 0.0 , OnDisplayValueChanged ) );

        public double DisplayValue
        {
            get { return (double) GetValue( DisplayValueProperty ); }
            set { SetValue( DisplayValueProperty , value ); }
        }

        // Formatted text property
        public static readonly DependencyProperty FormattedTextProperty =
            DependencyProperty.Register( "FormattedText" , typeof( string ) , typeof( LEDDisplay ) ,
                new PropertyMetadata( "0.0" ) );

        public string FormattedText
        {
            get { return (string) GetValue( FormattedTextProperty ); }
            private set { SetValue( FormattedTextProperty , value ); }
        }

        // Scale factor property
        public static readonly DependencyProperty ScaleFactorProperty =
            DependencyProperty.Register( "ScaleFactor" , typeof( double ) , typeof( LEDDisplay ) ,
                new PropertyMetadata( 1.0 ) );

        public double ScaleFactor
        {
            get { return (double) GetValue( ScaleFactorProperty ); }
            set { SetValue( ScaleFactorProperty , value ); }
        }

        // Base font size property (will be scaled by ScaleFactor)
        public static readonly DependencyProperty BaseFontSizeProperty =
            DependencyProperty.Register( "BaseFontSize" , typeof( double ) , typeof( LEDDisplay ) ,
                new PropertyMetadata( 28.0 ) );

        public double BaseFontSize
        {
            get { return (double) GetValue( BaseFontSizeProperty ); }
            set { SetValue( BaseFontSizeProperty , value ); }
        }

        // Label font size property (will be scaled by ScaleFactor)
        public static readonly DependencyProperty LabelFontSizeProperty =
            DependencyProperty.Register( "LabelFontSize" , typeof( double ) , typeof( LEDDisplay ) ,
                new PropertyMetadata( 14.0 ) );

        public double LabelFontSize
        {
            get { return (double) GetValue( LabelFontSizeProperty ); }
            set { SetValue( LabelFontSizeProperty , value ); }
        }

        // Label-to-display ratio property
        public static readonly DependencyProperty LabelToDisplayRatioProperty =
            DependencyProperty.Register( "LabelToDisplayRatio" , typeof( double ) , typeof( LEDDisplay ) ,
                new PropertyMetadata( 0.4 ) );

        public double LabelToDisplayRatio
        {
            get { return (double) GetValue( LabelToDisplayRatioProperty ); }
            set { SetValue( LabelToDisplayRatioProperty , value ); }
        }

        // Is editable property
        public static readonly DependencyProperty IsEditableProperty =
            DependencyProperty.Register( "IsEditable" , typeof( bool ) , typeof( LEDDisplay ) ,
                new PropertyMetadata( false ) );

        public bool IsEditable
        {
            get { return (bool) GetValue( IsEditableProperty ); }
            set { SetValue( IsEditableProperty , value ); }
        }

        // Label background property
        public static readonly DependencyProperty LabelBackgroundProperty =
            DependencyProperty.Register( "LabelBackground" , typeof( Brush ) , typeof( LEDDisplay ) ,
                new PropertyMetadata( new SolidColorBrush( Color.FromRgb( 173 , 216 , 230 ) ) ) );

        public Brush LabelBackground
        {
            get { return (Brush) GetValue( LabelBackgroundProperty ); }
            set { SetValue( LabelBackgroundProperty , value ); }
        }

        // Label border property
        public static readonly DependencyProperty LabelBorderBrushProperty =
            DependencyProperty.Register( "LabelBorderBrush" , typeof( Brush ) , typeof( LEDDisplay ) ,
                new PropertyMetadata( new SolidColorBrush( Colors.Blue ) ) );

        public Brush LabelBorderBrush
        {
            get { return (Brush) GetValue( LabelBorderBrushProperty ); }
            set { SetValue( LabelBorderBrushProperty , value ); }
        }

        // Display background property
        public static readonly DependencyProperty DisplayBackgroundProperty =
            DependencyProperty.Register( "DisplayBackground" , typeof( Brush ) , typeof( LEDDisplay ) ,
                new PropertyMetadata( new SolidColorBrush( Colors.Black ) ) );

        public Brush DisplayBackground
        {
            get { return (Brush) GetValue( DisplayBackgroundProperty ); }
            set { SetValue( DisplayBackgroundProperty , value ); }
        }

        // Display text color property
        public static readonly DependencyProperty DisplayForegroundProperty =
            DependencyProperty.Register( "DisplayForeground" , typeof( Brush ) , typeof( LEDDisplay ) ,
                new PropertyMetadata( new SolidColorBrush( Color.FromRgb( 0 , 255 , 0 ) ) ) );

        public Brush DisplayForeground
        {
            get { return (Brush) GetValue( DisplayForegroundProperty ); }
            set { SetValue( DisplayForegroundProperty , value ); }
        }

        // Format string property
        public static readonly DependencyProperty FormatStringProperty =
            DependencyProperty.Register( "FormatString" , typeof( string ) , typeof( LEDDisplay ) ,
                new PropertyMetadata( "0.0" , OnFormatStringChanged ) );

        public string FormatString
        {
            get { return (string) GetValue( FormatStringProperty ); }
            set { SetValue( FormatStringProperty , value ); }
        }

        // Event handler for value changes
        private static void OnDisplayValueChanged( DependencyObject d , DependencyPropertyChangedEventArgs e )
        {
            LEDDisplay display = d as LEDDisplay;
            if (display != null)
            {
                display.UpdateFormattedText();
            }
        }

        // Event handler for format string changes
        private static void OnFormatStringChanged( DependencyObject d , DependencyPropertyChangedEventArgs e )
        {
            LEDDisplay display = d as LEDDisplay;
            if (display != null)
            {
                display.UpdateFormattedText();
            }
        }

        // Update the formatted text based on the current value and format string
        private void UpdateFormattedText( )
        {
            FormattedText = DisplayValue.ToString( FormatString );
        }

        // Override OnMouseDown for editing functionality
        protected override void OnMouseDown( MouseButtonEventArgs e )
        {
            base.OnMouseDown( e );
            if (IsEditable)
            {
                EditValue();
            }
        }

        // Method to handle editing the display value
        private void EditValue( )
        {
            // This is a simple implementation - in a real app you might want a more sophisticated input method
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter new value:" ,
                "Edit Display Value" ,
                DisplayValue.ToString() );

            if (!string.IsNullOrEmpty( input ) && double.TryParse( input , out double newValue ))
            {
                DisplayValue = newValue;
            }
        }

        // Add support for automatic scaling when control size changes
        public override void OnApplyTemplate( )
        {
            base.OnApplyTemplate();
            SizeChanged += LEDDisplay_SizeChanged;
        }

        private void LEDDisplay_SizeChanged( object sender , SizeChangedEventArgs e )
        {
            // Calculate a scale factor based on the control's width
            // (assuming a base width of 250px for the reference scale)
            double widthFactor = ActualWidth / 250.0;
            double heightFactor = ActualHeight / 120.0;

            // Use the smaller factor to ensure everything fits
            ScaleFactor = Math.Min( widthFactor , heightFactor );
        }
    }
}
