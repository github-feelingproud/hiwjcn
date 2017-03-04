using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lib.core;
using Lib.model;
using Model.Post;
using Dapper;

namespace Dal.Post
{
    /// <summary>
    /// 文章数据库链接层
    /// </summary>
    public class PostDal : EFRepository<PostModel>
    {

        /// <summary>
        /// 添加阅读次数
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int AddReadCountByID(int id)
        {
            int count = 0;
            PrepareConnection(con =>
            {
                var sql = "update wp_posts set read_count=(read_count+1) where post_id=@pid";
                count = con.Execute(sql, new { pid = id });
            });
            return count;
        }

    }
}
