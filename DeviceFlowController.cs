using GJCS25004_分子筛转轮动态测试系统大屏.Control;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace GJCS25004_分子筛转轮动态测试系统大屏
{
    public class DeviceManager
    {
        #region 私有字段

        // 数据提供者
        private readonly object _dataProvider;

        // 流水动画系统
        private readonly PipelineFlowManager _pipelineSystem;

        // 监控的属性名称
        private readonly string [ ] _monitoredProperties = {
        "DMP201电动蝶阀", "DMP501电动蝶阀", "DMP701电动蝶阀",
        "VFD101变频器1正转", "VFD102变频器2正转"
    };

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public DeviceManager( object dataProvider , FlowingLineController flowingLineController , Func<string , object> findNameFunc )
        {
            _dataProvider = dataProvider ?? throw new ArgumentNullException( nameof( dataProvider ) );
            _pipelineSystem = new PipelineFlowManager( flowingLineController , findNameFunc );

            // 监听属性变化
            if (_dataProvider is INotifyPropertyChanged notifyPropertyChanged)
            {
                notifyPropertyChanged.PropertyChanged += OnPropertyChanged;
            }

            // 初始化流水动画状态
            InitializeFlowStates();
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 设置数据绑定
        /// </summary>
        public void SetupDataBinding( ToggleImageControl control , string propertyName )
        {
            Binding binding = new Binding( propertyName );
            binding.Source = _dataProvider;
            binding.Mode = BindingMode.TwoWay;
            control.SetBinding( ToggleImageControl.IsToggledProperty , binding );
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Cleanup( )
        {
            // 移除事件监听
            if (_dataProvider is INotifyPropertyChanged notifyPropertyChanged)
            {
                notifyPropertyChanged.PropertyChanged -= OnPropertyChanged;
            }

            // 停止所有动画
            _pipelineSystem.StopAllFlows();
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 处理属性变化
        /// </summary>
        private void OnPropertyChanged( object sender , PropertyChangedEventArgs e )
        {
            if (Array.IndexOf( _monitoredProperties , e.PropertyName ) >= 0)
            {
                bool isOn = GetPropertyValue( _dataProvider , e.PropertyName );
                _pipelineSystem.UpdateDeviceState( e.PropertyName , isOn );
            }
        }

        /// <summary>
        /// 获取属性值
        /// </summary>
        private bool GetPropertyValue( object obj , string propertyName )
        {
            try
            {
                var propertyInfo = obj.GetType().GetProperty( propertyName );
                if (propertyInfo != null)
                {
                    return (bool) propertyInfo.GetValue( obj );
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine( $"获取属性值出错: {ex.Message}" );
                return false;
            }
        }

        /// <summary>
        /// 初始化流水动画状态
        /// </summary>
        private void InitializeFlowStates( )
        {
            // 获取所有设备状态
            bool dmp201State = GetPropertyValue( _dataProvider , "DMP201电动蝶阀" );
            bool dmp501State = GetPropertyValue( _dataProvider , "DMP501电动蝶阀" );
            bool dmp701State = GetPropertyValue( _dataProvider , "DMP701电动蝶阀" );
            bool vfd101State = GetPropertyValue( _dataProvider , "VFD101变频器1正转" );
            bool vfd102State = GetPropertyValue( _dataProvider , "VFD102变频器2正转" );

            // 更新所有设备状态
            _pipelineSystem.UpdateAllDeviceStates(
                dmp201State , dmp501State , dmp701State ,
                vfd101State , vfd102State
            );
        }

        #endregion
    }
}
