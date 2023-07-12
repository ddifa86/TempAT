using Mozart.Extensions;
using Mozart.SeePlan.Aleatorik.Data;
 
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Mozart.SeePlan.Aleatorik
{
    public partial class ATDataGenLogic
    {
        public static ATDataGenLogic Instance
        {
            get
            {
                return ServiceLocator.Resolve<ATDataGenLogic>();
            }
        }

        public void Initialize(ATStage stage)
        {
            ATInputControl.Instance.Initialize(stage);
            // Interface 함수 호출 구조?

        }

        public void SetItemSiteBufferAttribute(ATItemSiteBuffer isb, ATBuffer firstBuffer)
        {
            ATInputControl.Instance.SetItemSiteBufferAttribute(isb, firstBuffer);
        }

        public void Done(ATStage stage)
        {
            ATInputControl.Instance.Done(stage);

            foreach (var bomDetails in ATInputData.Boms.GetBomDetails())
            {
                //bom Detail의 Key가 Stage에 속하지 않는다면 기록하지 않도록 수정
                if (stage.Buffers.ContainsKey(bomDetails.Key) == false)
                    continue;

                foreach (var detail in bomDetails.Value)
                {
                    foreach (var sop in detail.Bom.SoItemSites.Keys)
                    {
                        if (string.IsNullOrEmpty(ATOption.Instance.DemandItems) == false
                                && ATOption.Instance.DemandItems.Contains(sop.ItemID) == false)
                            continue;

                        OutputWriter.Instance.WriteBomNetwork(detail, sop);
                    }
                }
            }

            foreach (var siteitem in ATInputData.ItemSiteBuffers.GetItemSiteBuffers()) 
            {
                if (stage.Buffers.ContainsKey(siteitem.BufferID) == false)
                    continue;

                foreach (var pair in siteitem.AltItemSiteBuffers)
                {
                    // so별 누적 wip 수량
                    foreach (var altdetail in pair.Value)
                        OutputWriter.Instance.WriteItemSiteBufferAltInfo(siteitem, altdetail, pair.Key);
                }
            }


        }
    }
}
