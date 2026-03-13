using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GJCS25004_分子筛转轮动态测试系统大屏
{
    
    public partial class MolecularSieveDataProvider : OpcDataProvider
    {
        public delegate void RunnerStratStop();

        public static RunnerStratStop Strat;
        public static RunnerStratStop Stop;


        // 使用 ObservableProperty 特性来自动实现属性通知
        [ObservableProperty]
        private float _fT301流量计1数据;

        [ObservableProperty]
        private float _fT401流量计2数据;

        [ObservableProperty]
        private float _fT701流量计3数据;

        [ObservableProperty]
        private float _fT501流量计4数据;

        // 温度传感器数据点
        [ObservableProperty]
        private float _tT601温度传感1数据;

        [ObservableProperty]
        private float _tT603温度传感2数据;

        [ObservableProperty]
        private float _tT602温度传感3数据;

        [ObservableProperty]
        private float _tT301温度1数据;

        [ObservableProperty]
        private float _rH301湿度1数据;

        [ObservableProperty]
        private float _tT701温度2数据;

        [ObservableProperty]
        private float _rH701湿度2数据;

        [ObservableProperty]
        private float _tT501温度3数据;

        [ObservableProperty]
        private float _rH501湿度3数据;

        [ObservableProperty]
        private float _tT401温度4数据;

        [ObservableProperty]
        private float _rH401湿度4数据;

        // 压力传感器数据点
        [ObservableProperty]
        private float _pT301压力1数据;

        [ObservableProperty]
        private float _pT501压力2数据;

        [ObservableProperty]
        private float _pT701压力3数据;

        // 环境数据点
        [ObservableProperty]
        private float _tT101环境温度;

        [ObservableProperty]
        private float _rH101环境湿度;

        [ObservableProperty]
        private float _tT201管口温度;

        [ObservableProperty]
        private float _rH201管口湿度;

        [ObservableProperty]
        private float _pT101环境压力;

        [ObservableProperty]
        private float _pT201管口压力;

        // 加热数据点
        [ObservableProperty]
        private float _伴热温度PV1;

        [ObservableProperty]
        private float _伴热温度PV2;

        [ObservableProperty]
        private float _伴热温度PV3;

        [ObservableProperty]
        private float _汽化温度PV1;

        [ObservableProperty]
        private float _汽化温度PV2;

        [ObservableProperty]
        private float _汽化温度PV3;

        // 气体数据点
        [ObservableProperty]
        private float _可燃性气体数据1;

        [ObservableProperty]
        private float _可燃性气体数据2;

        [ObservableProperty]
        private float _可燃性气体数据3;

        // 差压数据点
        [ObservableProperty]
        private float _pDT802差压;

        [ObservableProperty]
        private float _pDT803差压;

        [ObservableProperty]
        private float _pDT701差压;

        [ObservableProperty]
        private float _pDT801差压;

        [ObservableProperty]
        private float _pDT501差压;

        [ObservableProperty]
        private float _pDT102差压;

        // 加热器温度数据点
        [ObservableProperty]
        private float _tT105加热器出口温度;

        [ObservableProperty]
        private float _tT105加热器内部温度;

        [ObservableProperty]
        private float _tT202加热器出口温度;

        [ObservableProperty]
        private float _tT202加热器内部温度;

        [ObservableProperty]
        private bool _dMP201电动蝶阀;
        [ObservableProperty]
        private bool _dMP501电动蝶阀;
        [ObservableProperty]
        private bool _dMP701电动蝶阀;

        [ObservableProperty]
        private float _vFD101变频器1正转;
        [ObservableProperty]
        private float _vFD102变频器2正转;
        [ObservableProperty]
        private float _a转轮实际转速;

        [ObservableProperty]
        private bool _vOC1启动;
        [ObservableProperty]
        private bool _vOC2启动;
        [ObservableProperty]
        private bool _vOC3启动;

        [ObservableProperty]
        private bool _vOC启动;
        [ObservableProperty]
        private double _s可燃性气体数据1;
        /// <summary>
        /// 初始化分子筛转轮系统OPC数据提供器
        /// </summary>opc.tcp://192.168.0.80:49320
        /// <param name="serverUrl">OPC UA服务器地址</param>
        //public MolecularSieveDataProvider( string serverUrl = "opc.tcp://192.168.0.80:49320" ) //现场使用地址
        public MolecularSieveDataProvider( string serverUrl = "opc.tcp://127.0.0.1:49320") //本地测试使用
            : base( serverUrl , true )
        {
            // 注册所有点位
            RegisterDataPoint( "ns=2;s=CH1.PLC.Canshu.Para00" , 0.0f , "FT301流量计1数据" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.Canshu.Para01" , 0.0f , "FT401流量计2数据" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.Canshu.Para02" , 0.0f , "FT701流量计3数据" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.Canshu.Para03" , 0.0f , "FT501流量计4数据" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.Canshu.Para04" , 0.0f , "TT601温度传感1数据" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.Canshu.Para05" , 0.0f , "TT603温度传感2数据" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.Canshu.Para06" , 0.0f , "TT602温度传感3数据" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.Canshu.Para07" , 0.0f , "TT301温度1数据" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.Canshu.Para08" , 0.0f , "RH301湿度1数据" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.Canshu.Para09" , 0.0f , "TT701温度2数据" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.Canshu.Para10" , 0.0f , "RH701湿度2数据" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.Canshu.Para11" , 0.0f , "TT501温度3数据" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.Canshu.Para12" , 0.0f , "RH501湿度3数据" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.Canshu.Para13" , 0.0f , "TT401温度4数据" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.Canshu.Para14" , 0.0f , "RH401湿度4数据" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.Canshu.Para15" , 0.0f , "PT301压力1数据" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.Canshu.Para16" , 0.0f , "PT501压力2数据" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.Canshu.Para17" , 0.0f , "PT701压力3数据" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.Canshu.Para18" , 0.0f , "TT101环境温度" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.Canshu.Para19" , 0.0f , "RH101环境湿度" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.Canshu.Para20" , 0.0f , "TT201管口温度" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.Canshu.Para21" , 0.0f , "RH201管口湿度" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.Canshu.Para22" , 0.0f , "PT101环境压力" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.Canshu.Para23" , 0.0f , "PT201管口压力" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.Canshu.Para24" , 0.0f , "伴热温度PV1" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.Canshu.Para25" , 0.0f , "伴热温度PV2" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.Canshu.Para26" , 0.0f , "伴热温度PV3" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.Canshu.Para27" , 0.0f , "汽化温度PV1" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.Canshu.Para28" , 0.0f , "汽化温度PV2" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.Canshu.Para29" , 0.0f , "汽化温度PV3" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.Canshu.Para30" , 0.0f , "S可燃性气体数据1" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.Canshu.Para31" , 0.0f , "可燃性气体数据2" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.Canshu.Para32" , 0.0f , "可燃性气体数据3" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.AI.AI00" , 0.0f , "PDT802差压" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.AI.AI03" , 0.0f , "PDT803差压" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.AI.AI06" , 0.0f , "PDT701差压" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.AI.AI09" , 0.0f , "PDT801差压" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.AI.AI12" , 0.0f , "PDT501差压" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.AI.AI15" , 0.0f , "PDT102差压" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.AI.AI24" , 0.0f , "TT105加热器出口温度" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.AI.AI27" , 0.0f , "TT105加热器内部温度" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.AI.AI30" , 0.0f , "TT202加热器出口温度" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.AI.AI33" , 0.0f , "TT202加热器内部温度" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.DO.DO06" , 0.0f , "DMP201电动蝶阀" );
            RegisterDataPoint( "ns=2;s=CH1.PLC.DO.DO18" , 0.0f , "DMP501电动蝶阀" );
            RegisterDataPoint( "ns=2;s=CH1.PLC.DO.DO16" , 0.0f , "DMP701电动蝶阀" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.DO.DO08" , 0.0f , "VFD101变频器1正转" );
            RegisterDataPoint( "ns=2;s=CH1.PLC.DO.DO12" , 0.0f , "VFD102变频器2正转" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.AI.AI66" , 0.0f , "A转轮实际转速" );

            RegisterDataPoint( "ns=2;s=CH1.PLC.DI.DI39" , 0.0f , "VOC1启动" );
            RegisterDataPoint( "ns=2;s=CH1.PLC.DI.DI40" , 0.0f , "VOC2启动" );
            RegisterDataPoint( "ns=2;s=CH1.PLC.DI.DI41" , 0.0f , "VOC3启动" );
        }

        partial void OnA转轮实际转速Changed(float value)
        {
            // 检查当前是否需要切换到UI线程
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                // 如果当前不在UI线程，使用Dispatcher调度到UI线程
                Application.Current.Dispatcher.BeginInvoke( new Action( ( ) =>
                {
                    // 在UI线程上执行操作
                    if (value > 0)
                    {
                        Strat?.Invoke();
                    }
                    else
                    {
                        Stop?.Invoke();
                    }
                } ) );
            }
            else
            {
                // 已经在UI线程上，直接执行
                if (value > 0)
                {
                    Strat?.Invoke();
                }
                else
                {
                    Stop?.Invoke();
                }
            }
        }
    }
    public static class MolecularSieveDataAccessor
    {
        private static readonly Lazy<MolecularSieveDataProvider> _instance =
            new Lazy<MolecularSieveDataProvider>( ( ) => new MolecularSieveDataProvider() );

        /// <summary>
        /// 获取全局唯一的 MolecularSieveDataProvider 实例
        /// </summary>
        public static MolecularSieveDataProvider Instance => _instance.Value;

       
    }
}
