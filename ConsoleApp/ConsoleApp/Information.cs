using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
   public class Information
    {
       public ObjectId id { get; set; }
        /// <summary>
        /// 编号
        /// </summary>
        public string newsid { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// 发布日期
        /// </summary>
        public DateTime date { get; set; }
        /// <summary>
        /// 点击量
        /// </summary>
        public string click_count { get; set; }
        /// <summary>
        /// 摘要
        /// </summary>
        public string summary { get; set; }
        /// <summary>
        /// 公告详细地址
        /// </summary>
        public string href { get; set; }
        /// <summary>
        /// 公告来源 1：博罗教育网公告 2：惠州教育网公告
        /// </summary>
        public int source_type { get; set; }

        public Details details { get; set; }
        public string detailshtml { get; set; }

        public DateTime create_at { get; set; }

        public DateTime update_at { get; set; }
    }
   public class Details
   {
       public string title{get;set;}
       public string info { get; set; }
       public string content { get; set; }
   }
}
