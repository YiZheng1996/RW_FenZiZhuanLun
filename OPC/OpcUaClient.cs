using Opc.Ua.Client;
using Opc.Ua;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GJCS25004_分子筛转轮动态测试系统大屏.OPC
{
    /// <summary>
    /// OPC UA客户端的实现类
    /// </summary>
    public class OpcUaClient : IOpcUaClient
    {
        #region 私有字段
        private OpcUaHelper.OpcUaClient _innerClient;
        private string _currentEndpointUrl;
        private bool _isConnected;
        private readonly ConcurrentDictionary<string , Subscription> _subscriptions = new ConcurrentDictionary<string , Subscription>();
        #endregion

        #region 事件
        /// <summary>
        /// 连接状态变化时触发的事件
        /// </summary>
        public event EventHandler<OpcUaConnectionStatusEventArgs> ConnectionStatusChanged;
        #endregion

        #region 属性
        /// <summary>
        /// 获取客户端是否已连接到OPC UA服务器
        /// </summary>
        public bool IsConnected => _isConnected;
        #endregion

        #region 构造函数
        /// <summary>
        /// 初始化OpcUaClient类的新实例
        /// </summary>
        public OpcUaClient( )
        {
            _innerClient = new OpcUaHelper.OpcUaClient();
            _isConnected = false;
        }
        #endregion

        #region 连接方法
        /// <summary>
        /// 连接到OPC UA服务器
        /// </summary>
        /// <param name="endpointUrl">OPC UA服务器的端点URL</param>
        /// <returns>表示连接操作的任务</returns>
        public async Task<bool> ConnectAsync( string endpointUrl )
        {
            try
            {
                _currentEndpointUrl = endpointUrl;

                // 创建连接配置，明确指定使用匿名登录
                _innerClient.UserIdentity = new UserIdentity( new AnonymousIdentityToken() ); // 不提供用户名和密码即为匿名

                // 实际建立连接

                await  _innerClient.ConnectServer( endpointUrl );
                _isConnected = true;

                OnConnectionStatusChanged( new OpcUaConnectionStatusEventArgs
                {
                    IsConnected = true ,
                    EndpointUrl = endpointUrl
                } );

                return true;
            }
            catch (Exception ex)
            {
                _isConnected = false;

                OnConnectionStatusChanged( new OpcUaConnectionStatusEventArgs
                {
                    IsConnected = false ,
                    EndpointUrl = endpointUrl ,
                    ErrorMessage = ex.Message
                } );

                return false;
            }
        }

        /// <summary>
        /// 从OPC UA服务器断开连接
        /// </summary>
        public void Disconnect( )
        {
            if (_isConnected)
            {
                // 首先移除所有订阅
                foreach (var key in _subscriptions.Keys.ToList())
                {
                    RemoveSubscription( key );
                }

                _innerClient.Disconnect();
                _isConnected = false;

                OnConnectionStatusChanged( new OpcUaConnectionStatusEventArgs
                {
                    IsConnected = false ,
                    EndpointUrl = _currentEndpointUrl
                } );
            }
        }
        #endregion

        #region 读取方法
        /// <summary>
        /// 从OPC UA服务器读取节点值
        /// </summary>
        /// <typeparam name="T">节点值的预期类型</typeparam>
        /// <param name="nodeId">要读取的节点ID</param>
        /// <returns>节点的值</returns>
        public T ReadNode<T>( string nodeId )
        {
            CheckConnection();
            return _innerClient.ReadNode<T>( nodeId );
        }

        /// <summary>
        /// 从OPC UA服务器读取节点值
        /// </summary>
        /// <param name="nodeId">要读取的节点ID</param>
        /// <returns>节点的原始数据值</returns>
        public DataValue ReadNode( NodeId nodeId )
        {
            CheckConnection();
            return _innerClient.ReadNode( nodeId );
        }

        /// <summary>
        /// 从OPC UA服务器读取多个节点值
        /// </summary>
        /// <typeparam name="T">节点值的预期类型</typeparam>
        /// <param name="nodeIds">要读取的节点ID集合</param>
        /// <returns>从节点读取的值列表</returns>
        public List<T> ReadNodes<T>( string [ ] nodeIds )
        {
            CheckConnection();
            return _innerClient.ReadNodes<T>( nodeIds );
        }
        #endregion

        #region 写入方法
        /// <summary>
        /// 向OPC UA服务器的节点写入值
        /// </summary>
        /// <typeparam name="T">要写入的值的类型</typeparam>
        /// <param name="nodeId">要写入的节点ID</param>
        /// <param name="value">要写入的值</param>
        /// <returns>如果写入操作成功，则为True，否则为False</returns>
        public bool WriteNode<T>( string nodeId , T value )
        {
            CheckConnection();
            _innerClient.WriteNode( nodeId , value );
            return true; // 如果没有异常发生
        }

        /// <summary>
        /// 向OPC UA服务器的多个节点写入值
        /// </summary>
        /// <param name="nodeIds">要写入的节点ID集合</param>
        /// <param name="values">要写入的值集合</param>
        /// <returns>如果所有写入操作都成功，则为True，否则为False</returns>
        public bool WriteNodes( string [ ] nodeIds , object [ ] values )
        {
            CheckConnection();
            return _innerClient.WriteNodes( nodeIds , values );
        }
        #endregion

        #region 订阅方法
        /// <summary>
        /// 添加订阅以监控节点的变化
        /// </summary>
        /// <param name="key">用于标识此订阅的唯一键</param>
        /// <param name="nodeId">要监控的节点ID</param>
        /// <param name="callback">当值变化时要调用的回调</param>
        public void AddSubscription( string key , string nodeId , Action<string , MonitoredItem , MonitoredItemNotificationEventArgs> callback )
        {
            CheckConnection();

            // 移除具有相同键的任何现有订阅
            RemoveSubscription( key );
            // 添加新订阅
            _innerClient.AddSubscription( key , nodeId , callback );

            // 存储订阅引用
            var subscription = new Subscription
            {
                Key = key ,
                NodeIds = new [ ] { nodeId } ,
                Callback = callback
            };

            _subscriptions [ key ] = subscription;
        }

        /// <summary>
        /// 添加订阅以监控多个节点的变化
        /// </summary>
        /// <param name="key">用于标识这些订阅的唯一键</param>
        /// <param name="nodeIds">要监控的节点ID集合</param>
        /// <param name="callback">当值变化时要调用的回调</param>
        public void AddSubscription( string key , string [ ] nodeIds , Action<string , MonitoredItem , MonitoredItemNotificationEventArgs> callback )
        {
            CheckConnection();

            // 移除具有相同键的任何现有订阅
            RemoveSubscription( key );

            // 添加新订阅
            foreach (var nodeId in nodeIds)
            {
                string keys = nodeId; // 为每个节点创建唯一的key
                AddSubscription( keys , nodeId , callback );
            }

            // 存储订阅引用
            var subscription = new Subscription
            {
                Key = key ,
                NodeIds = nodeIds ,
                Callback = callback
            };

            _subscriptions [ key ] = subscription;
        }

        /// <summary>
        /// 移除订阅
        /// </summary>
        /// <param name="key">标识要移除的订阅的键</param>
        public void RemoveSubscription( string key )
        {
            if (_isConnected && _subscriptions.TryRemove( key , out _ ))
            {
                _innerClient.RemoveSubscription( key );
            }
        }
        #endregion

        #region 浏览方法
        /// <summary>
        /// 浏览节点的引用
        /// </summary>
        /// <param name="nodeId">要浏览的节点ID</param>
        /// <returns>引用描述的集合</returns>
        public ReferenceDescriptionCollection BrowseNodeReference( string nodeId )
        {
            CheckConnection();
            return _innerClient.BrowseNodeReference( nodeId );
        }

        /// <summary>
        /// 读取节点的属性
        /// </summary>
        /// <param name="nodeId">要读取属性的节点ID</param>
        /// <returns>节点属性数组</returns>
        public OpcNodeAttribute [ ] ReadNodeAttributes( string nodeId )
        {
            CheckConnection();
            var attributes = _innerClient.ReadNoteAttributes( nodeId );

            // 从内部格式转换为我们的公共接口格式
            return attributes.Select( attr => new OpcNodeAttribute
            {
                Name = attr.Name ,
                Type = attr.Type ,
                StatusCode = attr.StatusCode ,
                Value = attr.Value
            } ).ToArray();
        }
        #endregion

        #region 辅助方法
        /// <summary>
        /// 触发ConnectionStatusChanged事件
        /// </summary>
        /// <param name="args">事件参数</param>
        protected virtual void OnConnectionStatusChanged( OpcUaConnectionStatusEventArgs args )
        {
            ConnectionStatusChanged?.Invoke( this , args );
        }

        /// <summary>
        /// 检查客户端是否已连接，如果未连接则抛出异常
        /// </summary>
        private void CheckConnection( )
        {
            if (!_isConnected)
            {
                throw new InvalidOperationException( "OPC UA客户端未连接。请先连接到服务器。" );
            }
        }
        #endregion

        #region 内部类
        /// <summary>
        /// 用于跟踪订阅的内部类
        /// </summary>
        private class Subscription
        {
            public string Key { get; set; }
            public string [ ] NodeIds { get; set; }
            public Action<string , MonitoredItem , MonitoredItemNotificationEventArgs> Callback { get; set; }
        }
        #endregion
    }
}
