using System;
using System.Collections.Generic;
using UnityEngine;


namespace YFramework.HFSM
{
    /// <summary>
    /// 状态转换，包含触发条件和目标状态
    /// </summary>
    public class Transition
    {
        public string FromState { get; }
        public string ToState { get; }

        //AnyState转换，任意状态都能触发
        public bool IsAnyState => FromState == null;

        private readonly List<ITransitionCondition> _conditions = new();
        private readonly List<Func<bool>> _funcConditions = new();

        public Transition(string fromState, string toState)
        {
            FromState = fromState;
            ToState = toState;
        }

        public Transition AddCondition(ITransitionCondition condition)
        {
            _conditions.Add(condition);
            return this;
        }

        /// <summary>
        /// 添加lambda条件
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public Transition AddCondition(Func<bool> condition)
        {
            _funcConditions.Add(condition);
            return this;
        }

        /// <summary>
        /// 所有条件全都满足才触发转换
        /// </summary>
        /// <returns></returns>
        public bool CanTransition()
        {
            foreach (var condition in _conditions)
            {
                if (!condition.Evaluate()) return false;
            }

            foreach (var func in _funcConditions)
            {
                if (!func()) return false;
            }

            return true;
        }
    }
}
