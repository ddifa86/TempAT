using Mozart.Extensions;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.SeePlan.Aleatorik.Data;

namespace Mozart.SeePlan.Aleatorik
{
    public partial class ATDataGenModule
    {
        public static ATDataGenModule Instance
        {
            get
            {
                return ServiceLocator.Resolve<ATDataGenModule>();
            }
        }


        internal Dictionary<string, ABomNetwork> RootInfos = new Dictionary<string, ABomNetwork>();

        public void Initialize()
        {
            ATElapsedTimeChecker.Instance.ResetTimer("ON_INITIALIZE0");
            try
            {
                foreach (var stage in ATExecutionContext.Instance.Stages)
                {
                    // Stage에 정보를 설정할 일이 있을까..?
                    ATDataGenModule.Instance.Initialize(stage);

                    ATDataGenModule.Instance.GenerateABomNet(stage);
                }
                
                ATDataGenModule.Instance.InitItemSiteBufferInfo();

                foreach (var stage in ATExecutionContext.Instance.Stages)
                {
                    ATDataGenModule.Instance.Done(stage);
                }

            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("ON_INITIALIZE0");
            }
        }

        public void Initialize(ATStage stage)
        {
            ATDataGenLogic.Instance.Initialize(stage);
        }

        /// <summary>
        /// Bom의 연결정보 생성 작업.
        /// </summary>
        public void GenerateABomNet(ATStage stage)
        {
            var lastBuffer = stage.BufferRoute.LastOper as ATBuffer;
            
            if (stage.BufferRoute.LastOper == null)
                return;

            List<ATBomDetail> bomDetails = ATInputData.Boms.GetBomDetails(lastBuffer.BufferID);

            if (bomDetails == null)
                bomDetails = new List<ATBomDetail>();

            // 추후 변경 작업 진행.
            // Stage 별 BomPath 구성 작업
            foreach (var bomDetail in bomDetails)
            {
                string rootItemBufferKey = bomDetail.ToItemSiteBuffer.Key;

                // 중복 방지.
                if (RootInfos.ContainsKey(rootItemBufferKey) == true)
                    continue;

                //if (string.IsNullOrEmpty(ATOption.Instance.DemandItems) == false
                //      && ATOption.Instance.DemandItems.Contains(bomDetail.ToItemID) == false)
                //    continue;

                // Root 생성.
                ABomNetwork root = new ABomNetwork(null, bomDetail.ToItemSiteBuffer);



                //
                ATInputData.ItemSiteBuffers.AddItemSiteBufferNode(root);

                // 하위 BufferBom 생성
                CreateABomNet(root);

                //
                RootInfos.Add(rootItemBufferKey, root);
            }

        }

        private void CreateABomNet(ABomNetwork parent)
        {
            foreach (var bompair in parent.ItemSiteBuffer.PrevBoms)
            {
                if (bompair.Key.BomType == BomType.Assembly)
                {
                    parent.RootItemSiteBuffer.PrevAssyItemSiteBuffers.Add(parent.ItemSiteBuffer);
                }

                foreach (var detail in bompair.Value)
                {
                    var nodes = ATInputData.ItemSiteBuffers.GetABomNetwork(true, parent.RootKey);

                    var node = nodes.Where(x => x.Key == detail.FromItemSiteBuffer.Key).FirstOrDefault();

                    if (node != null)
                    {
                        parent.DuplicatePrevBomDetails.Add(detail);
                        node.AddNextBomDetail(detail);
                        continue;
                    }

                    node = new ABomNetwork(parent, detail.FromItemSiteBuffer);

                    // 관련된 BomDetial 등록 작업
                    node.AddNextBomDetail(detail);

                    // 데이터 등록 작업
                    //ItemSiteBufferHelper.ItemSiteBufferNodes.Add(node);
                    ATInputData.ItemSiteBuffers.AddItemSiteBufferNode(node);

                    // AtItemBufferBom 트리 구성 작업
                    parent.Child.Add(node);

                    CreateABomNet(node);
                }
            }
        }

        public void InitItemSiteBufferInfo()
        {
            ///// ItemBuffer - Bom 간의 유효성 여부 체크
            ///// In ~ Out 까지 Path가 모두 연결되는지 여부 체크

            var stage = ATExecutionContext.Instance.Stages.First();
            var inputOper = stage.BufferRoute.FirstOper as ATBuffer;
            var buffers = ATInputData.ItemSiteBuffers.GetBuffers().OrderBy(x => x.Sequence).ToList();

            foreach (var buffer in buffers)
            {
                var isbs = ATInputData.ItemSiteBuffers.GetItemSiteBuffers(buffer.OperID);
                var inputBuffer = buffer.Stage.BufferRoute.FirstOper as ATBuffer;

                foreach (var isb in isbs)
                {
                    isb.SetPrevAttributeInfos(inputBuffer);

                    ATDataGenLogic.Instance.SetItemSiteBufferAttribute(isb, inputBuffer);
                }
            }

            buffers.Reverse();

            foreach (var buffer in buffers)
            {
                var isbs = ATInputData.ItemSiteBuffers.GetItemSiteBuffers(buffer.OperID);

                foreach (var isb in isbs)
                {
                    isb.SetNextAttributeInfos();
                }
            }

        }

        public void Done(ATStage stage)
        {
            ATDataGenLogic.Instance.Done(stage);

            foreach (var rootISB in this.RootInfos.Values)
            {
                if (rootISB.ItemSiteBuffer.IsUsablePath != BomPathType.N)
                    continue;

                OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.InvalidBomPath,
                       null, rootISB.ItemID + "," + rootISB.SiteID + "," + rootISB.BufferID , string.Empty, string.Empty) ;
            }
        }
    }
}
