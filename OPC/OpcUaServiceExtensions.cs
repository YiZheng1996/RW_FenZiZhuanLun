using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GJCS25004_分子筛转轮动态测试系统大屏.OPC
{
    public static class OpcUaServiceExtensions
    {
        /// <summary>
        /// 将OPC UA客户端服务添加到服务集合
        /// </summary>
        /// <param name="services">要向其中添加服务的服务集合</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddOpcUaClient( this IServiceCollection services )
        {
            // Register the OPC UA client as a singleton
            services.AddSingleton<IOpcUaClient , OpcUaClient>();

            return services;
        }
    }
}
