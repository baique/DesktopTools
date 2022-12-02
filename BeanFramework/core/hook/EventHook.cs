namespace BeanFramework.core.hook
{
    /// <summary>
    /// 事件信息
    /// </summary>
    /// <typeparam name="IN">事件入参</typeparam>
    /// <typeparam name="OUT">事件返回</typeparam>
    public interface EventHook<IN, OUT>
    {
        /// <summary>
        /// 事件响应条件
        /// </summary>
        /// <param name="param">事件参数</param>
        /// <returns>是否响应</returns>
        bool Match(IN param)
        {
            return true;
        }

        /// <summary>
        /// 响应事件
        /// </summary>
        /// <param name="param">事件参数</param>
        /// <returns>响应结果</returns>
        OUT Trigger(IN param);
    }
}
