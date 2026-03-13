using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace GJCS25004_分子筛转轮动态测试系统大屏
{
   public class FlowingLineController
    {
        private Dictionary<string , Storyboard> _animations = new Dictionary<string , Storyboard>();
        /// <summary>
        /// 为线条添加流动效果
        /// </summary>
        /// <param name="line">要添加动画的线条</param>
        /// <param name="duration">动画持续时间(秒)</param>
        /// <param name="speed">流动速度(正值向左,负值向右)</param>
        /// <param name="autoStart">是否自动开始动画</param>
        /// <returns>动画的唯一标识符</returns>
        public string AddFlowingEffect( Line line , double duration = 2 , double speed = 10 , bool autoStart = true )
        {
            // 创建唯一ID
            string animationId = line.Name;

            // 创建一个Storyboard来控制动画
            Storyboard flowStoryboard = new Storyboard();

            // 创建一个动画来移动虚线模式
            DoubleAnimation dashOffsetAnimation = new DoubleAnimation
            {
                From = 0 ,
                To = speed , // 正值向左流动，负值向右流动
                Duration = new Duration( TimeSpan.FromSeconds( duration ) ) ,
                RepeatBehavior = RepeatBehavior.Forever
            };

            // 设置动画目标属性
            Storyboard.SetTarget( dashOffsetAnimation , line );
            Storyboard.SetTargetProperty( dashOffsetAnimation , new PropertyPath( "StrokeDashOffset" ) );

            // 将动画添加到Storyboard
            flowStoryboard.Children.Add( dashOffsetAnimation );

            // 保存动画以便后续控制
            _animations [ animationId ] = flowStoryboard;

            // 如果需要，自动启动动画
            if (animationId== "liquidline")
            {
                flowStoryboard.Begin();
            }

            return animationId;
        }

        /// <summary>
        /// 开始指定的流动动画
        /// </summary>
        /// <param name="animationId">动画ID</param>
        public void Start( string animationId )
        {
            if (_animations.ContainsKey( animationId ))
            {
                _animations [ animationId ].Begin();
            }
        }

        /// <summary>
        /// 停止指定的流动动画
        /// </summary>
        /// <param name="animationId">动画ID</param>
        public void Stop( string animationId )
        {
            if (_animations.ContainsKey( animationId ))
            {
                _animations [ animationId ].Stop();
            }
        }

        /// <summary>
        /// 暂停指定的流动动画
        /// </summary>
        /// <param name="animationId">动画ID</param>
        public void Pause( string animationId )
        {
            if (_animations.ContainsKey( animationId ))
            {
                _animations [ animationId ].Pause();
            }
        }

        /// <summary>
        /// 恢复指定的流动动画
        /// </summary>
        /// <param name="animationId">动画ID</param>
        public void Resume( string animationId )
        {
            if (_animations.ContainsKey( animationId ))
            {
                _animations [ animationId ].Resume();
            }
        }

        /// <summary>
        /// 开始所有流动动画
        /// </summary>
        public void StartAll( )
        {
            foreach (var storyboard in _animations.Values)
            {
                storyboard.Begin();
            }
        }

        /// <summary>
        /// 停止所有流动动画
        /// </summary>
        public void StopAll( )
        {
            foreach (var storyboard in _animations.Values)
            {
                storyboard.Stop();
            }
        }

        /// <summary>
        /// 暂停所有流动动画
        /// </summary>
        public void PauseAll( )
        {
            foreach (var storyboard in _animations.Values)
            {
                storyboard.Pause();
            }
        }

        /// <summary>
        /// 恢复所有流动动画
        /// </summary>
        public void ResumeAll( )
        {
            foreach (var storyboard in _animations.Values)
            {
                storyboard.Resume();
            }
        }
    }
}
