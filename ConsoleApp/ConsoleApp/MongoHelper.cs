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
        public static List<Information> FindNews()
        {
            var client = new MongoClient(conn);
            var database = client.GetDatabase(dbName);
            var collection = database.GetCollection<Information>(tbName);
            var data = collection.AsQueryable().ToList();
            return data;

        }

        public static void AddNews(List<Information> models)
        {
            var addModels = new List<Information>();
            var oldModels = FindNews();
            addModels = models.Except(oldModels).ToList();
            if (addModels == null || addModels.Count <= 0)
                return;

            var client = new MongoClient(conn);
            var database = client.GetDatabase(dbName);
            var collection = database.GetCollection<Information>(tbName);
            collection.InsertMany(addModels);
        }
    }
}
