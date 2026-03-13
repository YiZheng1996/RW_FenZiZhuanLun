using GJCS25004_分子筛转轮动态测试系统大屏.OPC;
using Opc.Ua.Client;
using Opc.Ua;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GJCS25004_分子筛转轮动态测试系统大屏
{
    /// <summary>
    /// OPC数据提供器，负责自动连接和更新点位数据
    /// </summary>
    public class OpcDataProvider : ObservableObject,INotifyPropertyChanged
    {
        #region 私有字段
        private readonly IOpcUaClient _opcUaClient;
        private readonly DispatcherTimer _reconnectTimer;
        private readonly Dictionary<string , object> _dataPoints = new Dictionary<string , object>();
        private readonly string _serverUrl;
        private bool _isConnected;
        private string _connectionStatus = "未连接";
        #endregion

        #region 属性和事件
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 是否已连接
        /// </summary>
        public bool IsConnected
        {
            get => _isConnected;
            private set
            {
                if (_isConnected != value)
                {
                    _isConnected = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 连接状态描述
        /// </summary>
        public string ConnectionStatus
        {
            get => _connectionStatus;
            private set
            {
                if (_connectionStatus != value)
                {
                    _connectionStatus = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region 构造函数
        /// <summary>
        /// 初始化OPC数据提供器
        /// </summary>
        /// <param name="serverUrl">OPC UA服务器地址</param>
        /// <param name="autoConnect">是否自动连接</param>
        public OpcDataProvider( string serverUrl , bool autoConnect = true )
        {
            _serverUrl = serverUrl;
            _opcUaClient = new OpcUaClient();
            _opcUaClient.ConnectionStatusChanged += OpcUaClient_ConnectionStatusChanged;

            // 创建重连定时器
            _reconnectTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds( 10 ) // 每10秒尝试重连
            };
            _reconnectTimer.Tick += ReconnectTimer_Tick;

            // 自动连接
            if (autoConnect)
            {
                Task.Run( ( ) => ConnectAsync() );
            }
        }
        #endregion
        private Dictionary<string , string> _nodeIdToPropertyMap = new Dictionary<string , string>();
        #region 公共方法
        /// <summary>
        /// 注册一个点位数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="nodeId">节点ID</param>
        /// <param name="defaultValue">默认值</param>
        /// <param name="propertyName">属性名称</param>
        public void RegisterDataPoint<T>( string nodeId , T defaultValue , [CallerMemberName] string propertyName = null )
        {
            try
            {
                // 存储默认值
                _dataPoints [ nodeId ] = defaultValue;

                // 注册属性变更通知
                if (!string.IsNullOrEmpty( propertyName ))
                {
                    PropertyChanged?.Invoke( this , new PropertyChangedEventArgs( propertyName ) );
                }

                // 已连接则订阅

                SubscribeToNode( nodeId );
                _nodeIdToPropertyMap [ nodeId ] = propertyName;
            }
            catch (Exception e)
            {
                
            }
           
            
        }

        /// <summary>
        /// 获取点位数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="nodeId">节点ID</param>
        /// <returns>点位值</returns>
        public T GetDataPoint<T>( string nodeId )
        {
            if (_dataPoints.TryGetValue( nodeId , out object value ))
            {
                if (value is T typedValue)
                {
                    return typedValue;
                }

                try
                {
                    // 尝试转换类型
                    return (T) Convert.ChangeType( value , typeof( T ) );
                }
                catch
                {
                    // 转换失败返回默认值
                    return default;
                }
            }

            return default;
        }

        /// <summary>
        /// 写入点位数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="nodeId">节点ID</param>
        /// <param name="value">要写入的值</param>
        /// <returns>是否成功</returns>
        public bool WriteDataPoint<T>( string nodeId , T value )
        {
            try
            {
                if (!IsConnected)
                {
                    return false;
                }

                // 写入到OPC服务器
                bool success = _opcUaClient.WriteNode( nodeId , value );
                if (success)
                {
                    // 更新本地缓存
                    _dataPoints [ nodeId ] = value;

                    // 触发属性变更
                    PropertyChanged?.Invoke( this , new PropertyChangedEventArgs( nodeId ) );
                }

                return success;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 连接到OPC UA服务器
        /// </summary>
        private async Task ConnectAsync( )
        {
            try
            {
                ConnectionStatus = "正在连接...";
                bool connected = await _opcUaClient.ConnectAsync( _serverUrl );

                if (connected)
                {
                    // 连接成功，停止重连定时器
                    _reconnectTimer.Stop();

                    // 订阅所有已注册的点位
                    SubscribeToAllNodes();
                }
            }
            catch (Exception ex)
            {
                ConnectionStatus = $"连接失败: {ex.Message}";

                // 连接失败，启动重连定时器
                if (!_reconnectTimer.IsEnabled)
                {
                    _reconnectTimer.Start();
                }
            }
        }

        /// <summary>
        /// 订阅所有已注册的点位
        /// </summary>
        private void SubscribeToAllNodes( )
        {
            try
            {
                if (!IsConnected || _dataPoints.Count == 0)
                {
                    return;
                }

                // 获取所有节点ID
                string [ ] nodeIds = new string [ _dataPoints.Count ];
                int i = 0;
                foreach (var nodeId in _dataPoints.Keys)
                {
                    nodeIds [ i++ ] = nodeId;
                }

                // 设置订阅
                _opcUaClient.AddSubscription( "DataPoints" , nodeIds , NodeValueChanged );
            }
            catch (Exception ex)
            {
                ConnectionStatus = $"订阅失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 订阅单个节点
        /// </summary>
        private void SubscribeToNode( string nodeId )
        {
            try
            {
                if (!IsConnected)
                {
                    return;
                }

                // 设置订阅
                _opcUaClient.AddSubscription( $"Node_{nodeId}" , nodeId , NodeValueChanged );
            }
            catch
            {
                // 忽略单个节点订阅失败
            }
        }

        /// <summary>
        /// 节点值变更回调
        /// </summary>
        private void NodeValueChanged( string key , MonitoredItem monitoredItem , MonitoredItemNotificationEventArgs args )
        {
            try
            {
                var notification = args.NotificationValue as MonitoredItemNotification;
                if (notification != null)
                {
                    string nodeId = monitoredItem.StartNodeId.ToString();
                    object value = notification.Value.WrappedValue.Value;

                    // 1. 先更新 _dataPoints 字典
                    if (_dataPoints.ContainsKey( nodeId ) && value != null)
                    {
                        Type targetType = _dataPoints [ nodeId ]?.GetType();
                        if (targetType != null && value.GetType() != targetType)
                        {
                            try
                            {
                                value = Convert.ChangeType( value , targetType );
                            }
                            catch
                            {
                                // 转换失败，使用原始值
                            }
                        }
                        _dataPoints [ nodeId ] = value;

                        // 2. 使用映射找到对应的属性名，并更新属性值
                        if (_nodeIdToPropertyMap.TryGetValue( nodeId , out string propertyName ))
                        {
                            // 使用反射获取属性并设置值
                            var property = this.GetType().GetProperty( propertyName );
                            if (property != null && property.CanWrite)
                            {
                                try
                                {
                                    // 这一步很关键 - 实际更新属性值
                                    object convertedValue = Convert.ChangeType( value , property.PropertyType );
                                    property.SetValue( this , convertedValue );

                                    // 3. 通知 UI 更新
                                    System.Windows.Application.Current.Dispatcher.Invoke( ( ) =>
                                    {
                                        PropertyChanged?.Invoke( this , new PropertyChangedEventArgs( propertyName ) );
                                        Console.WriteLine( $"已更新属性 {propertyName} 为 {convertedValue}" );
                                    } );
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine( $"更新属性 {propertyName} 失败: {ex.Message}" );
                                }
                            }
                            else
                            {
                                Console.WriteLine( $"找不到属性: {propertyName}" );
                            }
                        }
                        else
                        {
                            Console.WriteLine( $"找不到节点 {nodeId} 对应的属性" );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine( $"NodeValueChanged 错误: {ex.Message}" );
            }
        }
        /// <summary>
        /// 重连定时器回调
        /// </summary>
        private void ReconnectTimer_Tick( object sender , EventArgs e )
        {
            if (!IsConnected)
            {
                Task.Run( ( ) => ConnectAsync() );
            }
            else
            {
                _reconnectTimer.Stop();
            }
        }

        /// <summary>
        /// OPC UA连接状态变更回调
        /// </summary>
        private void OpcUaClient_ConnectionStatusChanged( object sender , OpcUaConnectionStatusEventArgs e )
        {
            IsConnected = e.IsConnected;

            if (e.IsConnected)
            {
                ConnectionStatus = $"已连接到 {e.EndpointUrl}";

                // 连接成功后订阅所有点位
                SubscribeToAllNodes();
            }
            else
            {
                ConnectionStatus = e.ErrorMessage != null
                    ? $"连接错误: {e.ErrorMessage}"
                    : "已断开连接";

                // 断开连接后启动重连定时器
                if (!_reconnectTimer.IsEnabled)
                {
                    _reconnectTimer.Start();
                }
            }
        }

        /// <summary>
        /// 触发属性变更事件
        /// </summary>
        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this , new PropertyChangedEventArgs( propertyName ) );
        }
        #endregion
    }
}
