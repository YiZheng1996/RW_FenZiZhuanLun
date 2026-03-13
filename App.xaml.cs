using GJCS25004_分子筛转轮动态测试系统大屏.OPC;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;

namespace GJCS25004_分子筛转轮动态测试系统大屏;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup( StartupEventArgs e )
    {
        base.OnStartup( e );

        // 应用程序启动时可以在这里进行一些初始化工作
        // 例如设置全局异常处理
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }

    private void CurrentDomain_UnhandledException( object sender , UnhandledExceptionEventArgs e )
    {
        // 处理未捕获的异常
        Exception ex = e.ExceptionObject as Exception;
        MessageBox.Show( $"发生未处理的异常：{ex?.Message}" , "错误" , MessageBoxButton.OK , MessageBoxImage.Error );
    }

    protected override void OnExit( ExitEventArgs e )
    {
        try
        {
            // 断开OPC UA连接
            if (OpcUaClientProvider.Client.IsConnected)
            {
                OpcUaClientProvider.Client.Disconnect();
            }
        }
        catch (Exception ex)
        {
            // 记录断开连接时的异常
            Console.WriteLine( $"断开OPC UA连接时发生异常: {ex.Message}" );
        }

        base.OnExit( e );
    }
}

