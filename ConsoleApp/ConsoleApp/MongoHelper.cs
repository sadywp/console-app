using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace ConsoleApp
{
    public class MongoHelper
    {
        // <summary>
        /// 数据库连接
        /// </summary>
        private const string conn = "mongodb://127.0.0.1:27017";
        private const string connremote = "mongodb://120.77.16.167:27017";
        /// <summary>
        /// 指定的数据库
        /// </summary>
        private const string dbName = "education_module";
        /// <summary>
        /// 指定的表
        /// </summary>
        private const string tbName = "news_models";
        /// <summary>
        /// 查询数据库,检查是否存在指定ID的对象
        /// </summary>
        /// <param name="key">对象的ID值</param>
        /// <returns>存在则返回指定的对象,否则返回Null</returns>
        public static List<Information> FindNews(string type,ref string msg)
        {
            int sourceType = 1;
            if (type == "boluo") sourceType = 1;
            else sourceType = 2;
            try
            {
                var client = new MongoClient(conn);
                var database = client.GetDatabase(dbName);
                var collection = database.GetCollection<Information>(tbName);
                var data = collection.AsQueryable().Where(p => p.source_type == sourceType).ToList();

                msg = "数据库查询成功 " + data .Count+ " 条";
                return data;

            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return null;
            }
            

        }
        public static void AddTest()
        {
            var data = new user();
            data.name = "tttttt";
            data.laset = "tttttttttttttttddddddddd";
            var client = new MongoClient(conn);
            var database = client.GetDatabase(dbName);
            var collection = database.GetCollection<user>(tbName);
            collection.InsertOne(data);

        }
        public static void AddNews(List<Information> models,string type,ref string msg)
        {
            string msg_query = "";
            var addModels = new List<Information>();
            var oldModels = FindNews(type, ref msg_query);
            if (oldModels != null && oldModels.Count > 0)
            {
                foreach (var item in models)
                {
                    var itemModel = oldModels.Where(p => p.title == item.title).FirstOrDefault();
                    if (itemModel == null) addModels.Add(item);
                }
            }
            else
            {
                msg = msg_query;
                return;
            }
            if (addModels == null || addModels.Count <= 0)
            {
                msg = msg_query + " 数据库入库成功"  + "0 条";
                return;
            }
            try
            {
                var client = new MongoClient(conn);
                var database = client.GetDatabase(dbName);
                var collection = database.GetCollection<Information>(tbName);
                collection.InsertMany(addModels);

                msg = msg_query + " 数据库入库成功" + addModels.Count+"条";
            }
            catch (Exception ex)
            {

                msg = ex.Message;
            }
           
        }

        public static void ExportData()
        {
            string msg = "";
            var data = FindNews("boluo", ref msg);
            var client = new MongoClient(connremote);
            var database = client.GetDatabase(dbName);
            var collection = database.GetCollection<Information>(tbName);
            collection.InsertMany(data);

        }
    }
    public class user
    {
        public string name { get; set; }
        public string laset{get;set;}
    }
}
