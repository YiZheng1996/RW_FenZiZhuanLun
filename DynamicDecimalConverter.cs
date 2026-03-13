using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace GJCS25004_分子筛转轮动态测试系统大屏
{
    public class DynamicDecimalConverter : IValueConverter
    {
        public object Convert( object value , Type targetType , object parameter , CultureInfo culture )
        {
            if (value == null)
                return string.Empty;

            // 获取浮点数值
            if (!float.TryParse( value.ToString() , out float floatValue ))
                return value.ToString();

            // 获取绝对值以确定范围
            float absValue = Math.Abs( floatValue );

            // 根据数值范围应用不同的格式化
            if (absValue < 10)
                return floatValue.ToString( "F2" ); // 10以内，保留2位小数
            else if (absValue < 100)
                return floatValue.ToString( "F1" ); // 10-100之间，保留1位小数
            else
                return floatValue.ToString( "F0" ); // 100以上，不显示小数

            // 注意：F0与N0的区别是N0会添加千位分隔符
        }

        public object ConvertBack( object value , Type targetType , object parameter , CultureInfo culture )
        {
            if (string.IsNullOrEmpty( value?.ToString() ))
                return 0f;

            if (float.TryParse( value.ToString() , out float result ))
                return result;

            return 0f;
        }
    }
}
