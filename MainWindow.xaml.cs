using GJCS25004_分子筛转轮动态测试系统大屏.OPC;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using GJCS25004_分子筛转轮动态测试系统大屏.Control;
using static System.Net.Mime.MediaTypeNames;

namespace GJCS25004_分子筛转轮动态测试系统大屏;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MolecularSieveDataProvider _dataProvider;
    private DeviceManager _deviceFlowController;
    private  FlowingLineController flowingLineController = new FlowingLineController();
    private ValveFlowController _valveFlowController;
    private List<string> _animationIds = new List<string>();
    public MainWindow()
    {
        InitializeComponent();
        // 订阅SourceInitialized事件
        this.SourceInitialized += MainWindow_SourceInitialized;
        //创建具有不同边的圆柱
        MolecularSieveDataProvider.Stop += StopRotationAnimation;
        MolecularSieveDataProvider.Strat += StartRotationAnimation;
        rotation = new AxisAngleRotation3D( new Vector3D( 0 , 1 , 0 ) , 0 );
        _dataProvider = new MolecularSieveDataProvider();
        CreateVerticalStripedCylinder();
        //StartRotationAnimation();
        init();
    }
    // 添加一个角度属性用于绑定
    private void MainWindow_SourceInitialized( object sender , EventArgs e )
    {
        // 将窗口状态改为Normal
        this.WindowState = WindowState.Normal;

        // 获取工作区（不包括任务栏的区域）
        Rect workArea = SystemParameters.WorkArea;

        // 设置窗口位置和大小为工作区
        this.Left = workArea.Left;
        this.Top = workArea.Top;
        this.Width = workArea.Width;
        this.Height = workArea.Height;
    }
    private DoubleAnimation rotationAnimation;
    // 添加静态扇区的视觉对象，这些不会随圆柱旋转
    private ModelVisual3D fanSectorsVisual;
    // 添加圆柱主体的视觉对象，这部分会旋转
    private ModelVisual3D cylinderMainVisual;
   
  
    private void CreateVerticalStripedCylinder( )
    {
        // 创建两个模型组：一个用于旋转部分，一个用于静止部分
        Model3DGroup rotatingGroup = new Model3DGroup();
        Model3DGroup staticGroup = new Model3DGroup();

        // 确保我们清空视口，以避免任何之前的内容干扰
        viewport3D.Children.Clear();

        // 圆柱参数
        double radius = 1.0;
        double height = 0.6;

        // 条状网格参数
        int verticalSegments = 5;    // 垂直方向上的分段数量（高度分割）
        int horizontalSegments = 100; // 水平方向上的分段数量（围绕圆柱的条带数量）

        // 创建条形网格
        List<GeometryModel3D> stripModels = new List<GeometryModel3D>();

        double stripWidth = 2 * Math.PI / horizontalSegments;
        double stripHeight = height / verticalSegments;

        // 为每个条带创建一个独立的模型
        for (int h = 0 ; h < horizontalSegments ; h++)
        {
            // 计算条带的起始和结束角度
            double startAngle = h * stripWidth;
            double endAngle = (h + 1) * stripWidth;

            // 为当前条带创建网格
            MeshGeometry3D stripMesh = new MeshGeometry3D();

            // 为每个条带创建垂直分段
            for (int v = 0 ; v <= verticalSegments ; v++)
            {
                double y = -height / 2 + v * stripHeight;

                // 添加当前高度的两个点（起始角度和结束角度）
                Point3D startPoint = new Point3D(
                    radius * Math.Cos( startAngle ) ,
                    y ,
                    radius * Math.Sin( startAngle ) );

                Point3D endPoint = new Point3D(
                    radius * Math.Cos( endAngle ) ,
                    y ,
                    radius * Math.Sin( endAngle ) );

                stripMesh.Positions.Add( startPoint );
                stripMesh.Positions.Add( endPoint );
            }

            // 添加三角形（两个三角形构成一个矩形）
            for (int v = 0 ; v < verticalSegments ; v++)
            {
                int baseIndex = v * 2;

                // 第一个三角形
                stripMesh.TriangleIndices.Add( baseIndex );
                stripMesh.TriangleIndices.Add( baseIndex + 2 );
                stripMesh.TriangleIndices.Add( baseIndex + 1 );

                // 第二个三角形
                stripMesh.TriangleIndices.Add( baseIndex + 1 );
                stripMesh.TriangleIndices.Add( baseIndex + 2 );
                stripMesh.TriangleIndices.Add( baseIndex + 3 );
            }

            // 为每个条带创建一个随机颜色
            // 使用轻微的颜色变化，保持在同一色系中
            byte baseR = 30;
            byte baseG = 60;
            byte baseB = 100;

            Random rand = new Random( h );
            Color stripColor = Color.FromRgb(
                (byte) (baseR + rand.Next( -30 , 30 )) ,
                (byte) (baseG + rand.Next( -30 , 30 )) ,
                (byte) (baseB + rand.Next( -30 , 30 ))
            );

            Material stripMaterial = new DiffuseMaterial( new SolidColorBrush( stripColor ) );
            GeometryModel3D stripModel = new GeometryModel3D( stripMesh , stripMaterial );

            // 添加背面材质
            stripModel.BackMaterial = stripMaterial;

            // 将条带添加到旋转组
            rotatingGroup.Children.Add( stripModel );
        }

        // 创建底部盖子
        MeshGeometry3D bottomMesh = new MeshGeometry3D();

        // 底部中心点
        Point3D bottomCenter = new Point3D( 0 , -height / 2 , 0 );
        bottomMesh.Positions.Add( bottomCenter );

        // 圆周顶点
        int capSegments = 36;
        for (int i = 0 ; i <= capSegments ; i++)
        {
            double angle = i * 2 * Math.PI / capSegments;
            double x = radius * Math.Cos( angle );
            double z = radius * Math.Sin( angle );

            bottomMesh.Positions.Add( new Point3D( x , -height / 2 , z ) );
        }

        // 底部的三角形（注意顶点顺序）
        for (int i = 0 ; i < capSegments ; i++)
        {
            bottomMesh.TriangleIndices.Add( 0 );
            bottomMesh.TriangleIndices.Add( i + 2 );
            bottomMesh.TriangleIndices.Add( i + 1 );
        }

        // 为底部创建材质
        Material bottomMaterial = new DiffuseMaterial( new SolidColorBrush( Color.FromRgb(196, 18, 48) ) );  // 品牌红 #C41230
        GeometryModel3D bottomModel = new GeometryModel3D( bottomMesh , bottomMaterial );

        // 将底部添加到旋转组
        rotatingGroup.Children.Add( bottomModel );

        // 为顶部定义关键点
        // 1. 中心点
        Point3D centers = new Point3D( 0 , height / 2 , 0 );

        // 修改扇区定义部分，确保总和为360度
        // 定义特殊扇区的角度范围
        double sectorStartAngle = Math.PI;           // 扇区起始角度 (180度)
        double sectorMiddleAngle = Math.PI + Math.PI / 3; // 扇区中间角度 (240度)
        double sectorEndAngle = Math.PI + 2 * Math.PI / 3; // 扇区结束角度 (300度)
        double sectorThirdAngle = Math.PI * 3;  // 第三个扇区结束角度 (360度)

        // 添加间隙来解决闪烁问题
        double gap = 0.001;  // 减小间隙大小以减少扇区之间的视觉差距

        // 创建第一个扇形（从中心到起始角度到中间角度）
        MeshGeometry3D fan1Mesh = new MeshGeometry3D();

        // 添加中心点
        fan1Mesh.Positions.Add( centers );  // 索引0

        // 计算扇区边缘点
        Point3D startPoints = new Point3D(
            radius * Math.Cos( sectorStartAngle ) ,
            height / 2 ,
            radius * Math.Sin( sectorStartAngle )
        );

        Point3D middlePoint = new Point3D(
            radius * Math.Cos( sectorMiddleAngle - gap ) ,
            height / 2 ,
            radius * Math.Sin( sectorMiddleAngle - gap )
        );

        // 添加扇形边缘的点（弧形部分）
        int arcSegments = 12;  // 弧段数量，增加以获得更平滑的弧形

        // 先添加起始点
        fan1Mesh.Positions.Add( startPoints );  // 索引1

        // 然后添加第一段弧上的中间点
        for (int i = 1 ; i < arcSegments ; i++)
        {
            double angle = sectorStartAngle + (i * (sectorMiddleAngle - gap - sectorStartAngle) / arcSegments);
            double x = radius * Math.Cos( angle );
            double z = radius * Math.Sin( angle );

            fan1Mesh.Positions.Add( new Point3D( x , height / 2 , z ) );
        }

        // 添加最后一个点
        fan1Mesh.Positions.Add( middlePoint );

        // 为第一个扇形创建三角形（从中心点到相邻弧点）
        for (int i = 1 ; i < fan1Mesh.Positions.Count - 1 ; i++)
        {
            fan1Mesh.TriangleIndices.Add( 0 );  // 中心点
            fan1Mesh.TriangleIndices.Add( i );
            fan1Mesh.TriangleIndices.Add( i + 1 );
        }

        // 为第一个扇形创建材质
        Material fan1Material = new DiffuseMaterial( new SolidColorBrush( Color.FromRgb(232, 150, 58) ) );  // 脱附区 #E8963A
        GeometryModel3D fan1Model = new GeometryModel3D( fan1Mesh , fan1Material );
        fan1Model.BackMaterial = fan1Material;

        // 将第一个扇形添加到静态组
        staticGroup.Children.Add( fan1Model );

        // 创建第二个扇形（从中心到中间角度到结束角度）
        MeshGeometry3D fan2Mesh = new MeshGeometry3D();

        // 添加中心点
        fan2Mesh.Positions.Add( centers );  // 索引0

        // 添加中间点和结束点
        Point3D middlePoint2 = new Point3D(
            radius * Math.Cos( sectorMiddleAngle + gap ) ,
            height / 2 ,
            radius * Math.Sin( sectorMiddleAngle + gap )
        );

        Point3D endPoints = new Point3D(
            radius * Math.Cos( sectorEndAngle ) ,
            height / 2 ,
            radius * Math.Sin( sectorEndAngle )
        );

        // 添加起始点（中间点）
        fan2Mesh.Positions.Add( middlePoint2 );  // 索引1

        // 使用更多点来创建更精细的扇形
        int detailedArcSegments = 24;  // 增加弧段数量以获得更平滑的弧

        // 添加弧上的点
        for (int i = 1 ; i <= detailedArcSegments ; i++)
        {
            // 确保我们的点分布均匀
            double t = i / (double) detailedArcSegments;
            double angle = (sectorMiddleAngle + gap) * (1 - t) + sectorEndAngle * t;

            double x = radius * Math.Cos( angle );
            double z = radius * Math.Sin( angle );

            fan2Mesh.Positions.Add( new Point3D( x , height / 2 , z ) );
        }

        // 确保最后添加的点正好是结束点
        fan2Mesh.Positions.Add( endPoints );

        // 为第二个扇形创建三角形（从中心点到相邻弧点）
        for (int i = 1 ; i < fan2Mesh.Positions.Count - 1 ; i++)
        {
            fan2Mesh.TriangleIndices.Add( 0 );  // 中心点
            fan2Mesh.TriangleIndices.Add( i );
            fan2Mesh.TriangleIndices.Add( i + 1 );
        }

        // 为第二个扇形创建不同颜色的材质
        Material fan2Material = new DiffuseMaterial( new SolidColorBrush( Color.FromRgb(58, 124, 175) ) );  // 冷却区 #3A7CAF
        GeometryModel3D fan2Model = new GeometryModel3D( fan2Mesh , fan2Material );
        fan2Model.BackMaterial = fan2Material;

        // 将第二个扇形添加到静态组
        staticGroup.Children.Add( fan2Model );

        // 创建第三个扇形（从120度到360度，填补剩余部分）
        MeshGeometry3D fan3Mesh = new MeshGeometry3D();

        // 添加中心点
        fan3Mesh.Positions.Add( centers );  // 索引0

        // 添加起始点和结束点
        Point3D thirdStartPoint = new Point3D(
            radius * Math.Cos( sectorEndAngle + gap ) ,
            height / 2 ,
            radius * Math.Sin( sectorEndAngle + gap )
        );

        Point3D thirdEndPoint = new Point3D(
            radius * Math.Cos( sectorThirdAngle - gap ) ,
            height / 2 ,
            radius * Math.Sin( sectorThirdAngle - gap )
        );

        // 添加起始点
        fan3Mesh.Positions.Add( thirdStartPoint );  // 索引1

        // 使用更多点来创建更精细的扇形
        int thirdArcSegments = 48;  // 增加弧段数量以获得更平滑的弧，这个扇区更大所以需要更多点

        // 添加弧上的点
        for (int i = 1 ; i <= thirdArcSegments ; i++)
        {
            // 确保我们的点分布均匀
            double t = i / (double) thirdArcSegments;
            double angle = (sectorEndAngle + gap) * (1 - t) + (sectorThirdAngle - gap) * t;

            double x = radius * Math.Cos( angle );
            double z = radius * Math.Sin( angle );

            fan3Mesh.Positions.Add( new Point3D( x , height / 2 , z ) );
        }

        // 确保最后添加的点正好是结束点
        fan3Mesh.Positions.Add( thirdEndPoint );

        // 为第三个扇形创建三角形（从中心点到相邻弧点）
        for (int i = 1 ; i < fan3Mesh.Positions.Count - 1 ; i++)
        {
            fan3Mesh.TriangleIndices.Add( 0 );  // 中心点
            fan3Mesh.TriangleIndices.Add( i );
            fan3Mesh.TriangleIndices.Add( i + 1 );
        }

        // 为第三个扇形创建不同颜色的材质
        Material fan3Material = new DiffuseMaterial( new SolidColorBrush( Color.FromRgb(196, 18, 48) ) );  // 吸附区 品牌红 #C41230
        GeometryModel3D fan3Model = new GeometryModel3D( fan3Mesh , fan3Material );
        fan3Model.BackMaterial = fan3Material;

        // 将第三个扇形添加到静态组
        staticGroup.Children.Add( fan3Model );

        // 设置光源
        DirectionalLight directionalLight = new DirectionalLight( Colors.White , new Vector3D( -0.5 , -1 , -0.5 ) );
        DirectionalLight secondLight = new DirectionalLight( Colors.White , new Vector3D( 0.5 , -0.7 , 0.5 ) );
        AmbientLight ambientLight = new AmbientLight( Color.FromRgb( 80 , 80 , 80 ) );

        // 创建旋转部分的容器并应用旋转变换
        RotateTransform3D rotateTransform = new RotateTransform3D( rotation );
        cylinderMainVisual = new ModelVisual3D();
        cylinderMainVisual.Content = rotatingGroup;
        cylinderMainVisual.Transform = rotateTransform;

        // 创建静止部分的容器（不应用旋转变换）
        fanSectorsVisual = new ModelVisual3D();
        fanSectorsVisual.Content = staticGroup;

        // 添加光源视觉对象
        ModelVisual3D lightVisual1 = new ModelVisual3D();
        lightVisual1.Content = directionalLight;

        ModelVisual3D lightVisual2 = new ModelVisual3D();
        lightVisual2.Content = secondLight;

        ModelVisual3D ambientVisual = new ModelVisual3D();
        ambientVisual.Content = ambientLight;

        // 将所有视觉对象添加到视口
        viewport3D.Children.Add( cylinderMainVisual );  // 旋转部分
        viewport3D.Children.Add( fanSectorsVisual );    // 静止部分
        viewport3D.Children.Add( lightVisual1 );        // 主光源
        viewport3D.Children.Add( lightVisual2 );        // 辅助光源
        viewport3D.Children.Add( ambientVisual );       // 环境光
    }

    private void init()
    {

        #region 线条流动注册
        flowingLineController = new FlowingLineController();
        _deviceFlowController = new DeviceManager( _dataProvider , flowingLineController , FindName );
        _valveFlowController = new ValveFlowController( _dataProvider , flowingLineController , FindName );
        Line liquidLine0 = (Line) FindName( "liquidline" );
        string id0 = flowingLineController.AddFlowingEffect( liquidLine0 , duration: 2 , speed: -10 );
        _animationIds.Add( id0 );

        Line liquidLine1 = (Line) FindName( "liquidline4" );
        string id1 = flowingLineController.AddFlowingEffect( liquidLine1 , duration: 2 , speed: 10 );
        _animationIds.Add( id1 );

        Line liquidLine2 = (Line) FindName( "liquidline19" );
        string id2 = flowingLineController.AddFlowingEffect( liquidLine2 , duration: 2 , speed: 10 );
        _animationIds.Add( id2 );

        Line liquidLine3 = (Line) FindName( "liquidline5" );
        string id3 = flowingLineController.AddFlowingEffect( liquidLine3 , duration: 2 , speed: 10 );
        _animationIds.Add( id3 );

        Line liquidLine4 = (Line) FindName( "liquidline7" );
        string id4 = flowingLineController.AddFlowingEffect( liquidLine4 , duration: 2 , speed: 10 );
        _animationIds.Add( id4 );

        Line liquidLine5 = (Line) FindName( "liquidline8" );
        string id5 = flowingLineController.AddFlowingEffect( liquidLine5 , duration: 2 , speed: 10 );
        _animationIds.Add( id5 );

        Line liquidLine6 = (Line) FindName( "liquidline9" );
        string id6 = flowingLineController.AddFlowingEffect( liquidLine6 , duration: 2 , speed: -10 );
        _animationIds.Add( id6 );

        Line liquidLine7 = (Line) FindName( "liquidline10" );
        string id7 = flowingLineController.AddFlowingEffect( liquidLine7 , duration: 2 , speed: -10 );
        _animationIds.Add( id7 );

        

        Line liquidLine9 = (Line) FindName( "liquidlin12" );
        string id9 = flowingLineController.AddFlowingEffect( liquidLine9 , duration: 2 , speed: 10 );
        _animationIds.Add( id9 );

        Line liquidLine10 = (Line) FindName( "liquidline2" );
        string id10 = flowingLineController.AddFlowingEffect( liquidLine10 , duration: 2 , speed: 10 );
        _animationIds.Add( id10 );

        Line liquidLine11 = (Line) FindName( "liquidline13" );
        string id11 = flowingLineController.AddFlowingEffect( liquidLine11 , duration: 2 , speed: 10 );
        _animationIds.Add( id11 );

        Line liquidLine12 = (Line) FindName( "liquidline6" );
        string id12 = flowingLineController.AddFlowingEffect( liquidLine12 , duration: 2 , speed: 10 );
        _animationIds.Add( id12 );

        Line liquidLine13 = (Line) FindName( "liquidline14" );
        string id13 = flowingLineController.AddFlowingEffect( liquidLine13 , duration: 2 , speed: -10 );
        _animationIds.Add( id13 );

        Line liquidLine14 = (Line) FindName( "liquidline15" );
        string id14 = flowingLineController.AddFlowingEffect( liquidLine14 , duration: 2 , speed: -10 );
        _animationIds.Add( id14 );

        Line liquidLine15 = (Line) FindName( "liquidline16" );
        string id15 = flowingLineController.AddFlowingEffect( liquidLine15 , duration: 2 , speed: -10 );
        _animationIds.Add( id15 );

        Line liquidLine16 = (Line) FindName( "liquidline17" );
        string id16 = flowingLineController.AddFlowingEffect( liquidLine16 , duration: 2 , speed: -10 );
        _animationIds.Add( id16 );

        Line liquidLine17 = (Line) FindName( "liquidline18" );
        string id17 = flowingLineController.AddFlowingEffect( liquidLine17 , duration: 2 , speed: 10 );
        _animationIds.Add( id17 );

        Line liquidLine18 = (Line) FindName( "liquidline1" );
        string id18 = flowingLineController.AddFlowingEffect( liquidLine18 , duration: 2 , speed: 10 );
        _animationIds.Add( id18 );


        #endregion

        #region OPC注册
        

        SetupDataBinding( TT603 ,"TT601温度传感1数据" , "F1" );
        SetupDataBinding( TT601, "TT603温度传感2数据" , "F1" );

        SetupDataBinding( TT602 , "TT602温度传感3数据" , "F1" );

        SetupDataBinding( TT301 , "TT301温度1数据" , "F1" );
        SetupDataBinding( RH301 , "RH301湿度1数据" , "F1" );
        SetupDataBinding( TT701 , "TT701温度2数据" , "F1" );
        SetupDataBinding( RH701 , "RH701湿度2数据" , "F1" );

        SetupDataBinding( TT501 , "TT501温度3数据" , "F1" );

        SetupDataBinding( RH501 , "RH501湿度3数据" , "F1" );
        SetupDataBinding( TT401 , "TT401温度4数据" , "F1" );

        SetupDataBinding( RH401 , "RH401湿度4数据" , "F1" );

        SetupDataBinding( PT301 , "PT301压力1数据" , "F2" );

        SetupDataBinding( PT501 , "PT501压力2数据" , "F2" );

        SetupDataBinding( PT701 , "PT701压力3数据" , "F2" );



        SetupDataBinding( PDT802_ , "PDT802差压" , "F2" );

        SetupDataBinding( PDT803 , "PDT803差压" , "F2" );

        SetupDataBinding( PDT701 , "PDT701差压" , "F2" );

        SetupDataBinding( PDT801 , "PDT801差压" , "F2" );

        SetupDataBinding( PDT501 , "PDT501差压" , "F2" );

      

      

        SetupDataBinding( toggleControl , "DMP201电动蝶阀" );
        

        SetupDataBinding( fanControl , "VFD101变频器1正转" );
        SetupDataBinding( fanControl1 , "VFD102变频器2正转" );

        SetupRotationBinding( rotation , "A转轮实际转速" );


        #endregion
    }
    private void SetupDataBinding( TextBox textBlock , string propertyName , string format )
    {
        Binding binding = new Binding( propertyName );
        binding.Source = _dataProvider;
        binding.Mode = BindingMode.TwoWay;

        // 如果指定了具体格式，使用StringFormat
        if (!string.IsNullOrEmpty( format ))
        {
            binding.StringFormat = format;
        }
        // 否则使用动态小数点转换器
        else
        {
            binding.Converter = new DynamicDecimalConverter();
        }

        textBlock.SetBinding( TextBox.TextProperty , binding );
    }
    private void SetupDataBinding( ToggleImageControl toggleControl , string propertyName )
    {
        Binding binding = new Binding( propertyName );
        binding.Source = _dataProvider;
        binding.Mode = BindingMode.TwoWay;
        toggleControl.SetBinding( ToggleImageControl.IsToggledProperty , binding );
       

        // 强制更新当前图片
    }

    private void SetupDataBinding( FanControl toggleControl , string propertyName )
    {
        Binding binding = new Binding( propertyName );
        binding.Source = _dataProvider;
        binding.Mode = BindingMode.TwoWay;
        toggleControl.IsClockwise = false;
        toggleControl.SetBinding( FanControl._FanValueProperty , binding );
        // 强制更新当前图片
    }
    private void SetupRotationBinding( AxisAngleRotation3D rotation , string propertyName )
    {
        // 创建绑定到窗口的IsRotating属性
        Binding binding = new Binding( propertyName );
        binding.Source = this; // 假设在MainWindow类中
        binding.Mode = BindingMode.OneWay; // 单向绑定就足够了

        // 将绑定应用到AxisAngleRotation3D的Angle属性
        BindingOperations.SetBinding( rotation , AxisAngleRotation3D.AngleProperty , binding );
    }

    private void StartRotationAnimation( )
    {
        DoubleAnimation rotationAnimation = new DoubleAnimation
        {
            From = 360 ,
            To = 0 ,
            Duration = TimeSpan.FromSeconds( 100 ) ,
            RepeatBehavior = RepeatBehavior.Forever
        };

        rotation.BeginAnimation( AxisAngleRotation3D.AngleProperty , rotationAnimation );
    }
    private void StopRotationAnimation( )
    {
        rotation.BeginAnimation( AxisAngleRotation3D.AngleProperty , null );
    }


}