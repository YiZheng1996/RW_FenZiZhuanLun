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
    /// <summary>
    /// 电动蝶阀页面控制器 - 整合流水动画与UI控件
    /// </summary>
    public class ValveFlowController
    {
        // 数据提供者
        private readonly object _dataProvider;

        // 流水管理器
        private readonly PipelineFlowManager _flowManager;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dataProvider">数据提供者</param>
        /// <param name="flowingLineController">流水线控制器</param>
        /// <param name="findNameFunc">查找UI元素的函数</param>
        public ValveFlowController( object dataProvider , FlowingLineController flowingLineController , Func<string , object> findNameFunc )
        {
            FanControl.UpdateFlowsFromCurrentValveStatesHandler+= UpdateFlowsFromCurrentValveStates;
            _dataProvider = dataProvider ?? throw new ArgumentNullException( nameof( dataProvider ) );
            _flowManager = new PipelineFlowManager( flowingLineController , findNameFunc );

            // 监听数据提供者的属性变化
            if (_dataProvider is INotifyPropertyChanged notifyPropertyChanged)
            {
                notifyPropertyChanged.PropertyChanged += OnDataProviderPropertyChanged;
            }

            // 初始化流水动画状态
            UpdateFlowsFromCurrentValveStates();
        }

       

        /// <summary>
        /// 处理数据提供者属性变化事件
        /// </summary>
        private void OnDataProviderPropertyChanged( object sender , PropertyChangedEventArgs e )
        {
            // 检查是否是我们关心的电动蝶阀属性
            if (e.PropertyName == "DMP201电动蝶阀" ||
                e.PropertyName == "DMP501电动蝶阀" ||
                e.PropertyName == "DMP701电动蝶阀")
            {
                // 获取属性当前值
                bool isOn = GetBoolPropertyValue( _dataProvider , e.PropertyName );

                // 更新流水动画 (使用新的UpdateDeviceState方法)
                _flowManager.UpdateDeviceState( e.PropertyName , isOn );
            }
            // 检查是否是风机属性
            else if (e.PropertyName == "VFD101变频器1正转" ||
                     e.PropertyName == "VFD102变频器2正转")
            {
                // 获取属性当前值
                bool isOn = GetBoolPropertyValue( _dataProvider , e.PropertyName );

                // 更新流水动画
                _flowManager.UpdateDeviceState( e.PropertyName , isOn );
            }
            else if (e.PropertyName == "VOC1启动" || e.PropertyName == "VOC2启动" || e.PropertyName == "VOC3启动")
            {
                // 获取当前改变的属性值
                bool isOn = GetBoolPropertyValue( _dataProvider , e.PropertyName );

                if (isOn)
                {
                    // 如果当前属性为true，直接启动流
                    _flowManager.StratFlows( "liquidline20" );
                }
                else
                {
                    // 当前属性为false，需要检查其他两个VOC属性
                    // 确定其他两个属性的名称
                    List<string> otherVocProperties = new List<string> { "VOC1启动" , "VOC2启动" , "VOC3启动" };
                    otherVocProperties.Remove( e.PropertyName ); // 移除当前属性

                    // 获取其他两个VOC属性的值
                    bool otherVoc1 = GetBoolPropertyValue( _dataProvider , otherVocProperties [ 0 ] );
                    bool otherVoc2 = GetBoolPropertyValue( _dataProvider , otherVocProperties [ 1 ] );

                    // 只有当所有VOC都为false时才停止流
                    if (!otherVoc1 && !otherVoc2)
                    {
                        _flowManager.SoptFlows( "liquidline20" );
                    }
                    // 否则其他VOC仍有启动的，保持流动
                }
            }
            //获取VOC点位信息

        }

        /// <summary>
        /// 获取布尔属性值的辅助方法
        /// </summary>
        private bool GetBoolPropertyValue( object obj , string propertyName )
        {
            try
            {
                var propertyInfo = obj.GetType().GetProperty( propertyName );
                if (propertyInfo != null)
                {

                    if (propertyName== "VFD101变频器1正转"|| propertyName == "VFD102变频器2正转")
                    {
                        var item= (float) propertyInfo.GetValue( obj );
                        if (item==0)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }

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
        /// 根据当前阀门状态更新所有流水动画
        /// </summary>
        private void UpdateFlowsFromCurrentValveStates( )
        {
            // 获取电动蝶阀状态
            bool dmp201State = GetBoolPropertyValue( _dataProvider , "DMP201电动蝶阀" );
            bool dmp501State = GetBoolPropertyValue( _dataProvider , "DMP501电动蝶阀" );
            bool dmp701State = GetBoolPropertyValue( _dataProvider , "DMP701电动蝶阀" );

            // 获取风机状态
            bool vfd101State = GetBoolPropertyValue( _dataProvider , "VFD101变频器1正转" );
            bool vfd102State = GetBoolPropertyValue( _dataProvider , "VFD102变频器2正转" );

            // 更新所有设备状态 (使用新的UpdateAllDeviceStates方法)
            _flowManager.UpdateAllDeviceStates(
                dmp201State , dmp501State , dmp701State ,
                vfd101State , vfd102State
            );
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Cleanup( )
        {
            if (_dataProvider is INotifyPropertyChanged notifyPropertyChanged)
            {
                notifyPropertyChanged.PropertyChanged -= OnDataProviderPropertyChanged;
            }

            _flowManager.StopAllFlows();
        }
    }
}
