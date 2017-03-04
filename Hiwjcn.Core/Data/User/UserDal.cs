using Dapper;
using Hiwjcn.Dal.User;
using Lib.data;
using Model.User;

namespace Dal.User
{
    /// <summary>
    /// 用户数据连接类
    /// </summary>
    public class UserDal : EFRepository<UserModel>, IUserDal
    {
        /// <summary>
        /// 修改数据库中的用户头像
        /// </summary>
        /// <param name="db"></param>
        /// <param name="uid"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public int UpdateUserMask(int uid, byte[] b)
        {
            int count = 0;
            PrepareConnection(con =>
            {
                var sql = "update wp_users set user_db_img=@img where user_id=@uid";
                count = con.Execute(sql, new { img = b, uid = uid });
            });
            return count;
        }

        /// <summary>
        /// 从数据库中读取用户头像
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public byte[] ReadUserImage(int uid)
        {
            byte[] b = null;
            PrepareConnection(con =>
            {
                var sql = "select user_db_img from wp_users where user_id=@uid limit 0,1";
                b = con.ExecuteScalar<byte[]>(sql, new { uid = uid });
            });
            return b;
        }

    }
}
