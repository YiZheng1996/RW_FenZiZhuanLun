using GJCS25004_分子筛转轮动态测试系统大屏.OPC;
using Opc.Ua.Client;
using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace GJCS25004_分子筛转轮动态测试系统大屏
{
    public class OpcUaManager
    {
        #region 私有字段
        private readonly IOpcUaClient _opcUaClient;
        private readonly DispatcherTimer _dataUpdateTimer;
        private readonly Dictionary<string , Action<string>> _dataCallbacks = new Dictionary<string , Action<string>>();
        private string _lastErrorMessage = string.Empty;
        #endregion

        #region 事件
        /// <summary>
        /// 连接状态变化事件
        /// </summary>
        public event EventHandler<OpcConnectionStatusChangedEventArgs> ConnectionStatusChanged;

        /// <summary>
        /// 日志消息事件
        /// </summary>
        public event EventHandler<OpcLogMessageEventArgs> LogMessageReceived;
        #endregion

        #region 属性
        /// <summary>
        /// 是否已连接
        /// </summary>
        public bool IsConnected => _opcUaClient.IsConnected;

        /// <summary>
        /// 最后一次错误消息
        /// </summary>
        public string LastErrorMessage => _lastErrorMessage;

        /// <summary>
        /// 浏览结果集合
        /// </summary>
        public ObservableCollection<NodeReferenceInfo> BrowseResults { get; } = new ObservableCollection<NodeReferenceInfo>();
        #endregion

        #region 构造函数
        /// <summary>
        /// 初始化OPC UA管理器
        /// </summary>
        public OpcUaManager( )
        {
            // 创建OPC UA客户端实例
            _opcUaClient = new OpcUaClient();

            // 注册连接状态变化事件
            _opcUaClient.ConnectionStatusChanged += OpcUaClient_ConnectionStatusChanged;

            // 创建数据更新定时器
            _dataUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds( 1 )
            };
            _dataUpdateTimer.Tick += DataUpdateTimer_Tick;
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 连接到OPC UA服务器
        /// </summary>
        /// <param name="serverUrl">服务器地址</param>
        /// <returns>连接结果</returns>
        public async Task<bool> ConnectAsync( string serverUrl )
        {
            try
            {
                LogInfo( $"正在连接到服务器 {serverUrl}..." );
                bool connected = await _opcUaClient.ConnectAsync( serverUrl );

                if (connected)
                {
                    LogInfo( $"已成功连接到服务器 {serverUrl}" );
                    return true;
                }
                else
                {
                    _lastErrorMessage = "连接失败，未返回具体错误信息";
                    LogError( _lastErrorMessage );
                    return false;
                }
            }
            catch (Exception ex)
            {
                _lastErrorMessage = ex.Message;
                LogError( $"连接异常: {ex.Message}" );
                return false;
            }
        }

        /// <summary>
        /// 断开与OPC UA服务器的连接
        /// </summary>
        public void Disconnect( )
        {
            try
            {
                // 停止定时器
                _dataUpdateTimer.Stop();

                // 如果已连接，则断开连接
                if (_opcUaClient.IsConnected)
                {
                    // 移除所有订阅
                    _opcUaClient.RemoveSubscription( "DataMonitor" );

                    // 断开连接
                    _opcUaClient.Disconnect();
                    LogInfo( "已断开与服务器的连接" );
                }
            }
            catch (Exception ex)
            {
                _lastErrorMessage = ex.Message;
                LogError( $"断开连接异常: {ex.Message}" );
            }
        }

        /// <summary>
        /// 注册数据点回调
        /// </summary>
        /// <param name="nodeId">节点ID</param>
        /// <param name="callback">数据变化回调</param>
        public void RegisterDataPointCallback( string nodeId , Action<string> callback )
        {
            if (string.IsNullOrEmpty( nodeId ) || callback == null)
                return;

            _dataCallbacks [ nodeId ] = callback;
        }

        /// <summary>
        /// 设置订阅数据点
        /// </summary>
        /// <param name="nodeIds">要订阅的节点ID数组</param>
        /// <returns>是否成功设置订阅</returns>
        public bool SetupSubscriptions( string [ ] nodeIds )
        {
            try
            {
                if (!IsConnected)
                {
                    LogError( "未连接到服务器，无法设置订阅" );
                    return false;
                }

                // 移除已有的订阅
                _opcUaClient.RemoveSubscription( "DataMonitor" );

                // 添加新的订阅
                _opcUaClient.AddSubscription( "DataMonitor" , nodeIds , DataChangedCallback );

                // 启动定时器
                _dataUpdateTimer.Start();

                LogInfo( $"已设置 {nodeIds.Length} 个数据点的订阅" );
                return true;
            }
            catch (Exception ex)
            {
                _lastErrorMessage = ex.Message;
                LogError( $"设置订阅异常: {ex.Message}" );
                return false;
            }
        }

        /// <summary>
        /// 读取单个节点的值
        /// </summary>
        /// <param name="nodeId">节点ID</param>
        /// <returns>节点值</returns>
        public string ReadNodeValue( string nodeId )
        {
            try
            {
                if (!IsConnected)
                {
                    LogError( "未连接到服务器，无法读取节点" );
                    return "Error: Not connected";
                }

                DataValue value = _opcUaClient.ReadNode( new NodeId( nodeId ) );
                string result = value.WrappedValue.Value?.ToString() ?? "null";

                LogInfo( $"读取节点 {nodeId} 成功，值为 {result}" );
                return result;
            }
            catch (Exception ex)
            {
                _lastErrorMessage = ex.Message;
                LogError( $"读取节点 {nodeId} 异常: {ex.Message}" );
                return $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// 写入单个节点的值
        /// </summary>
        /// <param name="nodeId">节点ID</param>
        /// <param name="value">要写入的值</param>
        /// <returns>是否成功写入</returns>
        public bool WriteNodeValue( string nodeId , string value )
        {
            try
            {
                if (!IsConnected)
                {
                    LogError( "未连接到服务器，无法写入节点" );
                    return false;
                }

                bool success = _opcUaClient.WriteNode( nodeId , value );

                if (success)
                {
                    LogInfo( $"写入节点 {nodeId} 成功，值为 {value}" );
                    return true;
                }
                else
                {
                    LogError( $"写入节点 {nodeId} 失败" );
                    return false;
                }
            }
            catch (Exception ex)
            {
                _lastErrorMessage = ex.Message;
                LogError( $"写入节点 {nodeId} 异常: {ex.Message}" );
                return false;
            }
        }

        /// <summary>
        /// 浏览节点的引用
        /// </summary>
        /// <param name="nodeId">节点ID</param>
        /// <returns>是否成功浏览</returns>
        public bool BrowseNode( string nodeId )
        {
            try
            {
                if (!IsConnected)
                {
                    LogError( "未连接到服务器，无法浏览节点" );
                    return false;
                }

                var references = _opcUaClient.BrowseNodeReference( nodeId );

                // 清空当前结果
                Application.Current.Dispatcher.Invoke( ( ) => BrowseResults.Clear() );

                // 添加新结果
                foreach (var reference in references)
                {
                    Application.Current.Dispatcher.Invoke( ( ) =>
                    {
                        BrowseResults.Add( new NodeReferenceInfo
                        {
                            NodeId = reference.NodeId.ToString() ,
                            BrowseName = reference.BrowseName.ToString() ,
                            DisplayName = reference.DisplayName.Text ,
                            NodeClass = reference.NodeClass.ToString()
                        } );
                    } );
                }

                LogInfo( $"浏览节点 {nodeId} 成功，找到 {references.Count} 个引用" );
                return true;
            }
            catch (Exception ex)
            {
                _lastErrorMessage = ex.Message;
                LogError( $"浏览节点 {nodeId} 异常: {ex.Message}" );
                return false;
            }
        }
        #endregion

        #region 私有方法
        private void OpcUaClient_ConnectionStatusChanged( object sender , OpcUaConnectionStatusEventArgs e )
        {
            // 触发连接状态变化事件
            ConnectionStatusChanged?.Invoke( this , new OpcConnectionStatusChangedEventArgs
            {
                IsConnected = e.IsConnected ,
                ServerUrl = e.EndpointUrl ,
                ErrorMessage = e.ErrorMessage
            } );
        }

        private void DataChangedCallback( string key , MonitoredItem monitoredItem , MonitoredItemNotificationEventArgs args )
        {
            try
            {
                var notification = args.NotificationValue as MonitoredItemNotification;
                if (notification != null)
                {
                    string nodeId = monitoredItem.StartNodeId.ToString();
                    string value = notification.Value.WrappedValue.Value?.ToString() ?? "null";

                    // 如果注册了回调，则调用回调
                    if (_dataCallbacks.TryGetValue( nodeId , out var callback ))
                    {
                        Application.Current.Dispatcher.Invoke( ( ) => callback( value ) );
                    }
                }
            }
            catch (Exception ex)
            {
                LogError( $"处理数据变更回调异常: {ex.Message}" );
            }
        }

        private void DataUpdateTimer_Tick( object sender , EventArgs e )
        {
            // 这里可以添加定时读取一些不通过订阅更新的数据的逻辑
            // 对于简单示例，我们不在这里实现具体功能
        }

        private void LogInfo( string message )
        {
            LogMessageReceived?.Invoke( this , new OpcLogMessageEventArgs
            {
                Message = message ,
                Severity = LogSeverity.Info ,
                Timestamp = DateTime.Now
            } );
        }

        private void LogError( string message )
        {
            LogMessageReceived?.Invoke( this , new OpcLogMessageEventArgs
            {
                Message = message ,
                Severity = LogSeverity.Error ,
                Timestamp = DateTime.Now
            } );
        }
        #endregion
    }

    /// <summary>
    /// 节点引用信息类
    /// </summary>
    public class NodeReferenceInfo
    {
        public string NodeId { get; set; }
        public string BrowseName { get; set; }
        public string DisplayName { get; set; }
        public string NodeClass { get; set; }
    }

    /// <summary>
    /// OPC连接状态变化事件参数
    /// </summary>
    public class OpcConnectionStatusChangedEventArgs : EventArgs
    {
        public bool IsConnected { get; set; }
        public string ServerUrl { get; set; }
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// 日志消息严重级别
    /// </summary>
    public enum LogSeverity
    {
        Info,
        Warning,
        Error
    }

    /// <summary>
    /// OPC日志消息事件参数
    /// </summary>
    public class OpcLogMessageEventArgs : EventArgs
    {
        public string Message { get; set; }
        public LogSeverity Severity { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
