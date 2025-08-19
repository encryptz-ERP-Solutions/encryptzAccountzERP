using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Core;
using Entities.Admin;
using Entities.Core;
using Infrastructure.Extensions;
using Microsoft.Data.SqlClient;
using Repository.Admin.Interface;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Repository.Admin
{
    public class UserRepository : IUserRepository
    {
        private readonly CoreSQLDbHelper _sqlHelper;

        public UserRepository(CoreSQLDbHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }
        public async Task<User> AddAsync(User user)
        {
            DataTable data = new DataTable();
            var query = @"IF not exists(SELECT Id FROM core.userM WHERE Email = @Email)
                            BEGIN 
                            Insert Into core.userM( userId, userName, userPassword, Email, panNo, adharCardNo, phoneNo, address, stateId, nationId, isActive )
                            Values(  @userId, @userName, @userPassword, @Email, @panNo, @adharCardNo, @phoneNo, @address, @stateId, @nationId, @isActive );
                            END
                            ELSE
                            BEGIN
                                set @userId=(SELECT top 1 userId FROM core.userM WHERE Email = @Email)
                            END
                             select * from core.userM where userId=@userId;";

            var parameters = GetSqlParameters(user);
            data = await _sqlHelper.ExecuteQueryAsync(query, parameters);
            if (data.Rows.Count == 0) return null;
            user = data.Rows[0].ToObjectFromDR<User>();
            return user;

        }
        public async Task DeleteAsync(long id)
        {
            var query = "DELETE FROM core.CompanyM WHERE id = @Id";
            var parameters = new[] { new SqlParameter("@Id", id) };
            _sqlHelper.ExecuteNonQuery(query, parameters);
            await Task.CompletedTask;
        }
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            var query = "SELECT * FROM core.userM";
            var dataTable = _sqlHelper.ExecuteQuery(query);
            var users = new List<User>();

            foreach (DataRow row in dataTable.Rows)
            {
                users.Add(MapDataRowToUser(row));
            }

            return await Task.FromResult(users);
        }
        public async Task<User> GetByIdAsync(long id)
        {
            var query = "SELECT * FROM core.userM WHERE id = @Id";
            var parameters = new[] { new SqlParameter("@Id", id) };
            var dataTable = _sqlHelper.ExecuteQuery(query, parameters);

            if (dataTable.Rows.Count == 0) return null;

            return await Task.FromResult(dataTable.Rows[0].ToObjectFromDR<User>());
        }
        public async Task<User> UpdateAsync(User user)
        {
            DataTable data = new DataTable();
            var query = @"Update core.userM set 
                              userId = @userId
                            , userName = @userName
                            , userPassword = @userPassword
                            , panNo = @panNo
                            , adharCardNo = @adharCardNo
                            , phoneNo = @phoneNo
                            , address = @address
                            , stateId = @stateId
                            , nationId = @nationId
                            , isActive = @isActive
                            WHERE id = @id;
                        select * from core.UserM where id=@id";

            var parameters = GetSqlParameters(user);
            data = await _sqlHelper.ExecuteQueryAsync(query, parameters);
            if (data.Rows.Count == 0) return null;
            user = data.Rows[0].ToObjectFromDR<User>();
            return user;
        }
        public async Task<User> GetByLoginAsync(string loginValue, string loginType)
        {
            string query;
            SqlParameter[] parameters;

            if (loginType.Equals("Email", StringComparison.OrdinalIgnoreCase))
            {
                query = "SELECT * FROM core.userM WHERE Email = @LoginValue";
                parameters = new[] { new SqlParameter("@LoginValue", loginValue) };
            }
            else if (loginType.Equals("Phone", StringComparison.OrdinalIgnoreCase))
            {
                query = "SELECT * FROM core.userM WHERE phoneNo = @LoginValue";
                parameters = new[] { new SqlParameter("@LoginValue", loginValue) };
            }
            else
            {
                return null;
            }

            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            if (dataTable.Rows.Count == 0) return null;

            return dataTable.Rows[0].ToObjectFromDR<User>();
        }




        private static User MapDataRowToUser(DataRow row)
        {
            return new User
            {
                id = Convert.ToInt64(row["id"]),
                userId = Convert.ToString(row["userId"]),
                userName = Convert.ToString(row["userName"]),
                userPassword = row["userPassword"].ToString(),
                panNo = row["panNo"].ToString(),
                adharCardNo = row["adharCardNo"].ToString(),
                phoneNo = row["phoneNo"].ToString(),
                address = row["address"].ToString(),
                stateId = row["stateId"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["stateId"]),
                nationId = row["nationId"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["nationId"]),
                isActive = Convert.ToBoolean(row["isActive"])
            };
        }


        private static SqlParameter[] GetSqlParameters(User User)
        {
            return new[]
            {
                new SqlParameter("@id",User.id),
                new SqlParameter("@userId",User.userId),
                new SqlParameter("@userName",User.userName),
                new SqlParameter("@userPassword",User.userPassword ?? (object)DBNull.Value),
                new SqlParameter("@Email",User.email ?? (object)DBNull.Value),
                new SqlParameter("@panNo",User.panNo ?? (object)DBNull.Value),
                new SqlParameter("@adharCardNo",User.adharCardNo ?? (object)DBNull.Value),
                new SqlParameter("@phoneNo",User.phoneNo ?? (object)DBNull.Value),
                new SqlParameter("@address",User.address ?? (object)DBNull.Value),
                new SqlParameter("@stateId",User.stateId ?? 0),
                new SqlParameter("@nationId",User.nationId ?? 0),
                new SqlParameter("@isActive",User.isActive)
                };
        }
    }
}
