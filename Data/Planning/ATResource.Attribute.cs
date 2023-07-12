using Mozart.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public partial class ATResource
    {
        ///// <summary>
        ///// OBO 용 장비 정보
        ///// </summary>
        //internal ObyO.OboBucket ResourcePlan { get; set; }

        /// <summary>
        /// Planning 용 장비 정보
        /// </summary>
        public IBucket Bucket { get; set; }

        /// resource에 등록된 arrange의 모든 addresource를 확인할 필요가 있음.
        internal Dictionary<ATResource, List<ATOperation>> AddResources = new Dictionary<ATResource, List<ATOperation>>();
        internal Dictionary<ATOperation, ATResource> AddResourceInfo = new Dictionary<ATOperation, ATResource>();

        
    }
}
