using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RareDisease.Data.Entity
{
    public class OperationLog
    {
        /// <summary>
        /// 编号
        /// </summary>
        [Key]
        public string Guid { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [JsonProperty("action")]
        public string Action { get; set; }


        /// <summary>
        /// 描述
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }


        /// <summary>
        /// 创建人
        /// </summary>
        /// 
        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        [JsonProperty("createdOn")]
        public DateTime CreatedOn { get; set; }
    }

    public class OperationLogOutPut
    {
   
        /// <summary>
        /// 描述
        /// </summary>
        [JsonProperty("action")]
        public string Action { get; set; }


        /// <summary>
        /// 创建人
        /// </summary>
        /// 
        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        [JsonProperty("createdOn")]
        public string CreatedOn { get; set; }
    }
}
