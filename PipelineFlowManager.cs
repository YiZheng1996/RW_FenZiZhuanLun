using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace GJCS25004_分子筛转轮动态测试系统大屏
{
    public class PipelineFlowManager
    {

    #region 私有字段

    // 流水线控制器
    private readonly FlowingLineController _flowingLineController;

    // 查找UI元素的函数
    private readonly Func<string, object> _findNameFunc;

    // 流水线集合
    private readonly Dictionary<string, Line> _lines = new Dictionary<string, Line>();

    // 动画ID集合
    private readonly List<string> _animationIds = new List<string>();

    // 电动蝶阀状态
    private readonly Dictionary<string, bool> _valveStates = new Dictionary<string, bool>
    {
        { "DMP201电动蝶阀", false },
        { "DMP501电动蝶阀", false },
        { "DMP701电动蝶阀", false }
    };

    // 风机状态
    private readonly Dictionary<string, bool> _fanStates = new Dictionary<string, bool>
    {
        { "VFD101变频器1正转", false },
        { "VFD102变频器2正转", false }
    };

    // 流水线速度配置
    private readonly Dictionary<string, int> _lineSpeedConfigs = new Dictionary<string, int>
    {
        { "liquidline1", 10 },
        { "liquidline2", 10 },
        { "liquidline4", 10 },
        { "liquidline5", 10 },
        { "liquidline6", 10 },
        { "liquidline7", 10 },
        { "liquidline8", 10 },
        { "liquidline9", -10 },
        { "liquidline10", -10 },
        { "liquidlin12", 10 }, // 注意：原名中有拼写错误
        { "liquidline13", 10 },
        { "liquidline14", -10 },
        { "liquidline15", -10 },
        { "liquidline16", -10 },
        { "liquidline17", -10 },
        { "liquidline18", 10 },
        { "liquidline19", 10 },
    };

    // 电动蝶阀控制的流水线
    private readonly Dictionary<string, List<string>> _valveControlledLines = new Dictionary<string, List<string>>
    {
        {
            "DMP201电动蝶阀", new List<string>
            {
                 "liquidline19","liquidline4", "liquidline5", "liquidline7", "liquidline8",
                 "liquidlin12", "liquidline2", "liquidline13", "liquidline6","liquidline9", "liquidline10", 
                 "liquidline14", "liquidline15", "liquidline16", "liquidline17",
                 "liquidline18",  "liquidline20", "liquidline1", "liquidlin11"
            }
        },

    };

    // 风机控制的流水线
    private readonly Dictionary<string, List<string>> _fanControlledLines = new Dictionary<string, List<string>>
    {
        {
            "VFD101变频器1正转", new List<string>
            {
                "liquidline", "liquidlin11", "liquidline4","liquidline5","liquidline7","liquidline8","liquidline9","liquidlin12","liquidline10","liquidline19","liquidlin12" // 第一个风机控制的流水线(1-3条)
            }
        },
        {
            "VFD102变频器2正转", new List<string>
            {
                // 剩余所有流水线
                "liquidline2",
                "liquidline13", "liquidline6", "liquidline14",
                 "liquidline15","liquidline16","liquidline17","liquidline18","liquidline1"
            }
        }
    };

    #endregion

    #region 构造函数

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="flowingLineController">流水线控制器</param>
    /// <param name="findNameFunc">查找UI元素的函数</param>
    public PipelineFlowManager(FlowingLineController flowingLineController, Func<string, object> findNameFunc)
    {

            _flowingLineController =
            flowingLineController ?? throw new ArgumentNullException(nameof(flowingLineController));
        _findNameFunc = findNameFunc ?? throw new ArgumentNullException(nameof(findNameFunc));

        InitializePipeLines();
    }

    #endregion
        
    #region 公共方法

    /// <summary>
    /// 更新设备状态并刷新所有流水动画
    /// </summary>
    /// <param name="deviceName">设备名称</param>
    /// <param name="isOn">开关状态</param>
    public void UpdateDeviceState(string deviceName, bool isOn)
    {
        // 更新设备状态
        if (_valveStates.ContainsKey(deviceName))
        {
            _valveStates[deviceName] = isOn;
        }
        else if (_fanStates.ContainsKey(deviceName))
        {
            _fanStates[deviceName] = isOn;
        }
        else
        {
            Console.WriteLine($"警告: 未识别的设备 '{deviceName}'");
            return;
        }

        // 刷新所有流水动画
        RefreshFlows();
    }
        
    /// <summary>
    /// 更新所有设备状态
    /// </summary>
    public void UpdateAllDeviceStates(
        bool dmp201State, bool dmp501State, bool dmp701State,
        bool vfd101State, bool vfd102State)
    {
        // 更新电动蝶阀状态
        _valveStates["DMP201电动蝶阀"] = dmp201State;
        _valveStates["DMP501电动蝶阀"] = dmp501State;
        _valveStates["DMP701电动蝶阀"] = dmp701State;

        // 更新风机状态
        _fanStates["VFD101变频器1正转"] = vfd101State;
        _fanStates["VFD102变频器2正转"] = vfd102State;

        // 刷新所有流水动画
        RefreshFlows();
    }
    /// <summary>
    /// 开始指定的流水动画
    /// </summary>
    /// <param name="lineName"></param>
    public void StratFlows(string lineName)
        {
            _flowingLineController.Start( lineName );
        }
        public void SoptFlows( string lineName )
        {
            _flowingLineController.Stop( lineName );
        }
        /// <summary>
        /// 停止所有流水动画
        /// </summary>
        public void StopAllFlows()
    {
        foreach (var id in _animationIds)
        {
            _flowingLineController.Stop(id);
        }

    }

    #endregion

    #region 私有方法

    /// <summary>
    /// 初始化所有流水线
    /// </summary>
    private void InitializePipeLines()
    {
        foreach (var lineName in _lineSpeedConfigs.Keys)
        {
            try
            {
                var line = (Line)_findNameFunc(lineName);
                if (line != null)
                {
                    _lines[lineName] = line;
                    _animationIds.Add( lineName );
                }
                else
                {
                    Console.WriteLine($"警告: 找不到流水线 '{lineName}'");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"初始化流水线出错 '{lineName}': {ex.Message}");
            }
        }
       
        }

    
    /// <summary>
    /// 刷新所有流水动画
    /// </summary>
    private void RefreshFlows()
    {
        // 先停止所有动画
        StopAllFlows();

        // 获取有效的流水线
        HashSet<string> activeLines = GetActiveLines();

        // 启动所有有效的流水线
        foreach (string lineName in activeLines)
        {
               
                if (_lines.ContainsKey(lineName))
            {
                   
                Line line = _lines[lineName];
                int speed = _lineSpeedConfigs[lineName];

                _flowingLineController.Start( lineName );
                
            }
        }
    }

    /// <summary>
    /// 获取当前应该激活的所有流水线
    /// </summary>
    private HashSet<string> GetActiveLines()
    {
        // 根据电动蝶阀状态获取有效的流水线
        HashSet<string> valveActiveLines = new HashSet<string>();
        foreach (var valveEntry in _valveStates)
        {
            if (valveEntry.Value && _valveControlledLines.ContainsKey(valveEntry.Key))
            {
                foreach (string lineName in _valveControlledLines[valveEntry.Key])
                {
                    valveActiveLines.Add(lineName);
                }
            }
        }

        // 根据风机状态获取有效的流水线
        HashSet<string> fanActiveLines = new HashSet<string>();
        foreach (var fanEntry in _fanStates)
        {
            if (fanEntry.Value && _fanControlledLines.ContainsKey(fanEntry.Key))
            {
                foreach (string lineName in _fanControlledLines[fanEntry.Key])
                {
                    fanActiveLines.Add(lineName);
                }
            }
        }

        // 找出同时满足电动蝶阀和风机条件的流水线
        HashSet<string> result = new HashSet<string>();
        foreach (string lineName in valveActiveLines)
        {
            if (fanActiveLines.Contains(lineName))
            {
                result.Add(lineName);
            }
        }

        return result;
    }

    #endregion

    }
}
