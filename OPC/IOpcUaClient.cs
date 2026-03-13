using Opc.Ua.Client;
using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GJCS25004_分子筛转轮动态测试系统大屏.OPC
{
    public interface IOpcUaClient
    {
        /// <summary>
        /// OPC UA连接状态变化时触发的事件
        /// </summary>
        event EventHandler<OpcUaConnectionStatusEventArgs> ConnectionStatusChanged;

        /// <summary>
        /// 获取客户端是否已连接到OPC UA服务器
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 连接到OPC UA服务器
        /// </summary>
        /// <param name="endpointUrl">OPC UA服务器的端点URL</param>
        /// <returns>表示连接操作的任务</returns>
        Task<bool> ConnectAsync( string endpointUrl );

        /// <summary>
        /// 从OPC UA服务器断开连接
        /// </summary>
        void Disconnect( );

        /// <summary>
        /// 从OPC UA服务器读取节点值
        /// </summary>
        /// <typeparam name="T">节点值的预期类型</typeparam>
        /// <param name="nodeId">要读取的节点ID</param>
        /// <returns>节点的值</returns>
        T ReadNode<T>( string nodeId );

        /// <summary>
        /// 从OPC UA服务器读取节点值
        /// </summary>
        /// <param name="nodeId">要读取的节点ID</param>
        /// <returns>节点的原始数据值</returns>
        DataValue ReadNode( NodeId nodeId );

        /// <summary>
        /// 从OPC UA服务器读取多个节点值
        /// </summary>
        /// <typeparam name="T">节点值的预期类型</typeparam>
        /// <param name="nodeIds">要读取的节点ID集合</param>
        /// <returns>从节点读取的值列表</returns>
        List<T> ReadNodes<T>( string [ ] nodeIds );

        /// <summary>
        /// 向OPC UA服务器的节点写入值
        /// </summary>
        /// <typeparam name="T">要写入的值的类型</typeparam>
        /// <param name="nodeId">要写入的节点ID</param>
        /// <param name="value">要写入的值</param>
        /// <returns>如果写入操作成功，则为True，否则为False</returns>
        bool WriteNode<T>( string nodeId , T value );

        /// <summary>
        /// 向OPC UA服务器的多个节点写入值
        /// </summary>
        /// <param name="nodeIds">要写入的节点ID集合</param>
        /// <param name="values">要写入的值集合</param>
        /// <returns>如果所有写入操作都成功，则为True，否则为False</returns>
        bool WriteNodes( string [ ] nodeIds , object [ ] values );

        /// <summary>
        /// 添加订阅以监控节点的变化
        /// </summary>
        /// <param name="key">用于标识此订阅的唯一键</param>
        /// <param name="nodeId">要监控的节点ID</param>
        /// <param name="callback">当值变化时要调用的回调</param>
        void AddSubscription( string key , string nodeId , Action<string , MonitoredItem , MonitoredItemNotificationEventArgs> callback );

        /// <summary>
        /// 添加订阅以监控多个节点的变化
        /// </summary>
        /// <param name="key">用于标识这些订阅的唯一键</param>
        /// <param name="nodeIds">要监控的节点ID集合</param>
        /// <param name="callback">当值变化时要调用的回调</param>
        void AddSubscription( string key , string [ ] nodeIds , Action<string , MonitoredItem , MonitoredItemNotificationEventArgs> callback );

        /// <summary>
        /// 移除订阅
        /// </summary>
        /// <param name="key">标识要移除的订阅的键</param>
        void RemoveSubscription( string key );

        /// <summary>
        /// 浏览节点的引用
        /// </summary>
        /// <param name="nodeId">要浏览的节点ID</param>
        /// <returns>引用描述的集合</returns>
        ReferenceDescriptionCollection BrowseNodeReference( string nodeId );

        /// <summary>
        /// 读取节点的属性
        /// </summary>
        /// <param name="nodeId">要读取属性的节点ID</param>
        /// <returns>节点属性数组</returns>
        OpcNodeAttribute [ ] ReadNodeAttributes( string nodeId );
    }

    /// <summary>
    /// OPC UA连接状态事件参数
    /// </summary>
    public class OpcUaConnectionStatusEventArgs : EventArgs
    {
        /// <summary>
        /// 获取或设置连接是否已建立
        /// </summary>
        public bool IsConnected { get; set; }

        /// <summary>
        /// 获取或设置连接失败时的错误消息
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 获取或设置已连接或尝试连接的端点URL
        /// </summary>
        public string EndpointUrl { get; set; }
    }

    /// <summary>
    /// OPC节点属性类
    /// </summary>
    public class OpcNodeAttribute
    {
        /// <summary>
        /// 获取或设置属性的名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 获取或设置属性的数据类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 获取或设置属性的状态码
        /// </summary>
        public StatusCode StatusCode { get; set; }

        /// <summary>
        /// 获取或设置属性的值
        /// </summary>
        public object Value { get; set; }
    }
}

