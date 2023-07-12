using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.SeePlan.Aleatorik.Data;

namespace Mozart.SeePlan.Aleatorik
{
    [FeatureBind()]
    public class PeggingGroupKeyMethod
    {
        /// <summary>
        /// 제품 등급과 타겟 월이 동일한 타겟들을 패깅 그룹으로 설정
        /// </summary>
        /// <param name="pegPart"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(PeggingGroupKey))]
        public string GradeTargetMonth(APEPegPart pegPart)
        {
            string dueDate = pegPart.SampleTarget.TargetMonth; 
            string grade = pegPart.CurrentItem.Grade.ToString(); 
             
            return dueDate + "@" + grade;
        }

        /// <summary>
        /// 제품 등급과 타겟 주차가 동일한 타겟들을 패깅 그룹으로 설정
        /// </summary>
        /// <param name="pegPart"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(PeggingGroupKey))]
        public string GradeTargetWeek(APEPegPart pegPart)
        {
            string dueDate = pegPart.SampleTarget.TargetWeek;
            string grade = pegPart.CurrentItem.Grade.ToString();

            return dueDate + "@" + grade;
        }

        /// <summary>
        /// 개별 타겟을 패깅 그룹으로 설정 (타겟과 패깅 그룹이 1:1)
        /// </summary>
        /// <param name="pegPart"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(PeggingGroupKey))]
        public string TargetID(APEPegPart pegPart)
        {
            return pegPart.SampleTarget.TargetID;
        }

        /// <summary>
        /// 현재 버퍼의 모든 타겟을 하나의 패깅 그룹으로 설정
        /// </summary>
        /// <param name="pegPart"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(PeggingGroupKey))]
        public string BufferID(APEPegPart pegPart)
        {
            return pegPart.CurrentBuffer.BufferID;
        }
    }
}
