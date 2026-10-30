using System;

namespace FraxinusEngine.Rendering.Ultra
{
    /// <summary>
    /// 渲染上下文作用域过滤器
    /// 用于控制InjectPass在不同作用域中的注入行为
    /// </summary>
    [Flags]
    public enum ContextScopeFilter
    {
        /// <summary>无作用域</summary>
        None = 0,
        
        /// <summary>主作用域（默认的全分辨率渲染）</summary>
        Main = 1 << 0,
        
        /// <summary>子作用域（低分辨率或特殊渲染分支）</summary>
        Sub = 1 << 1,
        
        /// <summary>所有作用域</summary>
        All = Main | Sub
    }
    
    /// <summary>
    /// 作用域类型（非标志枚举）
    /// 用于标识节点所属的作用域
    /// </summary>
    public enum ContextScope
    {
        /// <summary>主作用域</summary>
        Main,
        
        /// <summary>子作用域</summary>
        Sub
    }
}
