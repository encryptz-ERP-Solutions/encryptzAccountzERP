﻿using System;
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
    public class UserRepository:IUserRepository
    {
        private readonly CoreSQLDbHelper _sqlHelper;

        public UserRepository(CoreSQLDbHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }
        public async Task<User> AddAsync(User user)
        {
            DataTable data = new DataTable();
            var query = @"Insert Into core.userM( userId, userName, userPassword, panNo, adharCardNo, phoneNo, address, stateId, nationId, isActive )
                            Values(  @userId, @userName, @userPassword, @panNo, @adharCardNo, @phoneNo, @address, @stateId, @nationId, @isActive );
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

            //foreach (DataRow row in dataTable.Rows)
            //{
            //    users.Add(MapDataRowToUser(row));
            //}

            users = dataTable.ToList<User>();

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
             data= await _sqlHelper.ExecuteQueryAsync(query, parameters);
            if (data.Rows.Count == 0) return null;
            user = data.Rows[0].ToObjectFromDR<User>();
            return user;
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
                stateId = row["stateId"] as int?,
                nationId = row["nationId"] as int?,
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
                new SqlParameter("@panNo",User.panNo ?? (object)DBNull.Value),
                new SqlParameter("@adharCardNo",User.adharCardNo ?? (object)DBNull.Value),
                new SqlParameter("@phoneNo",User.phoneNo ?? (object)DBNull.Value),
                new SqlParameter("@address",User.address ?? (object)DBNull.Value),
                new SqlParameter("@stateId",User.stateId ?? (object)DBNull.Value),
                new SqlParameter("@nationId",User.nationId ?? (object)DBNull.Value),
                new SqlParameter("@isActive",User.isActive)
                };
        }
    }
}
