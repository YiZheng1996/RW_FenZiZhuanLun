using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GJCS25004_分子筛转轮动态测试系统大屏.OPC
{
    public static class OpcUaClientProvider
    {
        private static IOpcUaClient _client;
        private static readonly object _lock = new object();

        /// <summary>
        /// 获取OPC UA客户端实例
        /// </summary>
        public static IOpcUaClient Client
        {
            get
            {
                if (_client == null)
                {
                    lock (_lock)
                    {
                        if (_client == null)
                        {
                            _client = new OpcUaClient();
                        }
                    }
                }
                return _client;
            }
        }
    }
}
