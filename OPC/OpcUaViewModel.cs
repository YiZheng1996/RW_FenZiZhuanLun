using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GJCS25004_分子筛转轮动态测试系统大屏.OPC
{
    public class OpcUaViewModel : INotifyPropertyChanged
    {
        #region Private Fields
        private readonly IOpcUaClient _opcUaClient;
        private string _endpointUrl = "opc.tcp://192.168.0.80:49320/SharpNodeSettings/OpcUaServer";
        private string _statusMessage = "Disconnected";
        private string _nodeId = "ns=2;s=Device1/Temperature";
        private string _nodeValue = string.Empty;
        private bool _isConnected;
        private ObservableCollection<OpcUaNodeViewModel> _browseResults = new ObservableCollection<OpcUaNodeViewModel>();
        #endregion
        #region INI File Reader
        [DllImport( "kernel32" )]
        private static extern int GetPrivateProfileString( string section , string key , string defaultValue , StringBuilder returnedString , int size , string filePath );

        [DllImport( "kernel32" )]
        private static extern long WritePrivateProfileString( string section , string key , string val , string filePath );

        /// <summary>
        /// 从INI文件读取值
        /// </summary>
        /// <param name="section">节名称</param>
        /// <param name="key">键名称</param>
        /// <param name="defaultValue">默认值</param>
        /// <param name="filePath">INI文件路径</param>
        /// <returns>读取的值</returns>
        private static string ReadIniValue( string section , string key , string defaultValue , string filePath )
        {
            StringBuilder sb = new StringBuilder( 255 );
            GetPrivateProfileString( section , key , defaultValue , sb , 255 , filePath );
            return sb.ToString();
        }

        /// <summary>
        /// 写入值到INI文件
        /// </summary>
        /// <param name="section">节名称</param>
        /// <param name="key">键名称</param>
        /// <param name="value">要写入的值</param>
        /// <param name="filePath">INI文件路径</param>
        /// <returns>是否成功</returns>
        private static bool WriteIniValue( string section , string key , string value , string filePath )
        {
            return WritePrivateProfileString( section , key , value , filePath ) != 0;
        }
        #endregion

        #region Properties
        /// <summary>
        /// 获取或设置OPC UA服务器端点URL
        /// </summary>
        public string EndpointUrl
        {
            get => _endpointUrl;
            set
            {
                _endpointUrl = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 获取或设置连接状态消息
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 获取或设置为读/写的节点ID
        /// </summary>
        public string NodeId
        {
            get => _nodeId;
            set
            {
                _nodeId = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 获取或设置节点值
        /// </summary>
        public string NodeValue
        {
            get => _nodeValue;
            set
            {
                _nodeValue = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 获取或设置客户端是否已连接
        /// </summary>
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                _isConnected = value;
                OnPropertyChanged();
                ConnectCommand.RaiseCanExecuteChanged();
                DisconnectCommand.RaiseCanExecuteChanged();
                ReadNodeCommand.RaiseCanExecuteChanged();
                WriteNodeCommand.RaiseCanExecuteChanged();
                BrowseNodeCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// 获取浏览结果的集合
        /// </summary>
        public ObservableCollection<OpcUaNodeViewModel> BrowseResults
        {
            get => _browseResults;
            private set
            {
                _browseResults = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Commands
        /// <summary>
        /// 获取连接到OPC UA服务器的命令
        /// </summary>
        public DelegateCommand ConnectCommand { get; }

        /// <summary>
        /// 获取断开与OPC UA服务器的连接的命令
        /// </summary>
        public DelegateCommand DisconnectCommand { get; }

        /// <summary>
        /// 获取读取节点值的命令
        /// </summary>
        public DelegateCommand ReadNodeCommand { get; }

        /// <summary>
        /// 获取用于写入节点值的命令
        /// </summary>
        public DelegateCommand WriteNodeCommand { get; }

        /// <summary>
        ///获取浏览节点的命令
        /// </summary>
        public DelegateCommand BrowseNodeCommand { get; }
        #endregion

        #region Constructor
        /// <summary>
        /// 初始化OpcUaViewModel类的新实例
        /// </summary>
        /// <param name="opcUaClient">OPC UA客户端服务</param>
        public OpcUaViewModel( IOpcUaClient opcUaClient )
        {
            _opcUaClient = opcUaClient ?? throw new ArgumentNullException( nameof( opcUaClient ) );

            // 初始化命令
            ConnectCommand = new DelegateCommand( ConnectAsync , ( ) => !IsConnected );
            DisconnectCommand = new DelegateCommand( Disconnect , ( ) => IsConnected );
            ReadNodeCommand = new DelegateCommand( ReadNode , ( ) => IsConnected );
            WriteNodeCommand = new DelegateCommand( WriteNode , ( ) => IsConnected );
            BrowseNodeCommand = new DelegateCommand( BrowseNode , ( ) => IsConnected );

            // 订阅连接状态更改
            _opcUaClient.ConnectionStatusChanged += OpcUaClient_ConnectionStatusChanged;
        }
        #endregion

        #region Command Methods
        /// <summary>
        /// 连接OPC UA服务器
        /// </summary>
        private async void ConnectAsync( )
        {
            try
            {
                StatusMessage = "Connecting...";
                bool success = await _opcUaClient.ConnectAsync( EndpointUrl );

                if (success)
                {
                    StatusMessage = "Connected";
                    IsConnected = true;
                }
                else
                {
                    StatusMessage = "Connection failed";
                    IsConnected = false;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Connection error: {ex.Message}";
                IsConnected = false;
            }
        }

        /// <summary>
        /// 与OPC UA服务器断开连接
        /// </summary>
        private void Disconnect( )
        {
            try
            {
                _opcUaClient.Disconnect();
                StatusMessage = "Disconnected";
                IsConnected = false;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Disconnect error: {ex.Message}";
            }
        }

        /// <summary>
        /// 读取节点值
        /// </summary>
        private void ReadNode( )
        {
            try
            {
                DataValue dataValue = _opcUaClient.ReadNode( new NodeId( NodeId ) );
                NodeValue = dataValue.WrappedValue.Value?.ToString() ?? "null";
                StatusMessage = "Node read successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Read error: {ex.Message}";
                NodeValue = string.Empty;
            }
        }

        /// <summary>
        /// 将值写入节点
        /// </summary>
        private void WriteNode( )
        {
            try
            {
                //注意：这是一个简单的例子。在实际应用中，
                ////您需要为节点确定正确的数据类型
                //并适当地转换字符串值。

                //在这个例子中，我们假设节点是一个字符串
                bool success = _opcUaClient.WriteNode( NodeId , NodeValue );

                if (success)
                {
                    StatusMessage = "Node written successfully";
                }
                else
                {
                    StatusMessage = "Write operation failed";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Write error: {ex.Message}";
            }
        }

        /// <summary>
        ///浏览节点查找引用
        /// </summary>
        private void BrowseNode( )
        {
            try
            {
                BrowseResults.Clear();

                var references = _opcUaClient.BrowseNodeReference( NodeId );

                foreach (var reference in references)
                {
                    BrowseResults.Add( new OpcUaNodeViewModel
                    {
                        NodeId = reference.NodeId.ToString() ,
                        BrowseName = reference.BrowseName.ToString() ,
                        DisplayName = reference.DisplayName.Text ,
                        NodeClass = reference.NodeClass.ToString()
                    } );
                }

                StatusMessage = $"Browse completed, found {references.Count} references";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Browse error: {ex.Message}";
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// 处理连接状态更改
        /// </summary>
        private void OpcUaClient_ConnectionStatusChanged( object sender , OpcUaConnectionStatusEventArgs e )
        {
            IsConnected = e.IsConnected;

            if (e.IsConnected)
            {
                StatusMessage = $"Connected to {e.EndpointUrl}";
            }
            else
            {
                StatusMessage = e.ErrorMessage != null
                    ? $"Connection error: {e.ErrorMessage}"
                    : "Disconnected";
            }
        }
        #endregion

        #region INotifyPropertyChanged
        /// <summary>
        /// 属性更改时引发的事件
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 引发PropertyChanged事件
        /// </summary>
        /// <param name="propertyName">已更改的属性的名称</param>
        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this , new PropertyChangedEventArgs( propertyName ) );
        }
        #endregion
    }

    /// <summary>
    /// OPC UA节点的ViewModel
    /// </summary>
    public class OpcUaNodeViewModel
    {
        /// <summary>
        /// 获取或设置节点ID
        /// </summary>
        public string NodeId { get; set; }

        /// <summary>
        /// 获取或设置浏览名称
        /// </summary>
        public string BrowseName { get; set; }

        /// <summary>
        /// 获取或设置显示名称
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 获取或设置节点类
        /// </summary>
        public string NodeClass { get; set; }
    }

    /// <summary>
    /// 将执行委托给方法的iccommand的实现
    /// </summary>
    public class DelegateCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        /// <summary>
        /// 初始化DelegateCommand类的新实例
        /// </summary>
        /// <param name="execute">执行逻辑</param>
        /// <param name="canExecute">执行状态逻辑</param>
        public DelegateCommand( Action execute , Func<bool> canExecute = null )
        {
            _execute = execute ?? throw new ArgumentNullException( nameof( execute ) );
            _canExecute = canExecute;
        }

        /// <summary>
        /// 当命令的执行状态改变时引发的事件
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// 引发CanExecuteChanged事件
        /// </summary>
        public void RaiseCanExecuteChanged( )
        {
            CanExecuteChanged?.Invoke( this , EventArgs.Empty );
        }

        /// <summary>
        /// 定义确定命令是否可以在其当前状态下执行的方法
        /// </summary>
        /// <param name="parameter">命令使用的数据</param>
        /// <returns>如果该命令可以执行，则为True；否则,假</returns>
        public bool CanExecute( object parameter )
        {
            return _canExecute == null || _canExecute();
        }

        /// <summary>
        /// 定义在调用命令时要调用的方法
        /// </summary>
        /// <param name="parameter">命令使用的数据</param>
        public void Execute( object parameter )
        {
            _execute();
        }
    }
}
