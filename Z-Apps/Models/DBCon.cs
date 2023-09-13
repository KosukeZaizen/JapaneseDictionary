using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Z_Apps.Util;
using System.Data;

namespace Z_Apps.Models
{
    public class DBCon
    {
        private string connectionString;
        public enum DBType
        {
            z_apps,
            wiki_db
        }
        public DBCon(DBType type = DBType.z_apps)
        {
            if (type == DBType.wiki_db)
            {
                connectionString = PrivateConsts.CONNECTION_STRING;
            }
            else
            {
                connectionString = PrivateConsts.CONNECTION_STRING;
            }
        }
        public List<Dictionary<string, Object>> ExecuteSelect(
            string sql,
            Dictionary<string, object[]> dicParams = null,
            int timeoutSecond = 0
        )
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                if (timeoutSecond != 0)
                {
                    //参考：https://netsystem.jpn.org/t_nary/vb-net/sql-server-%E3%82%BF%E3%82%A4%E3%83%A0%E3%82%A2%E3%82%A6%E3%83%88%E9%96%A2%E9%80%A3%E3%81%AE%E8%A8%AD%E5%AE%9A/
                    command.CommandTimeout = timeoutSecond; //コマンド実行タイムアウト
                }

                try
                {
                    // パラーメータの置換
                    if (dicParams != null)
                    {
                        foreach (KeyValuePair<string, object[]> kvp in dicParams)
                        {
                            var param = command.CreateParameter();
                            param.ParameterName = kvp.Key;
                            param.SqlDbType = (SqlDbType)kvp.Value[0];
                            param.Direction = ParameterDirection.Input;
                            param.Value = kvp.Value[1];

                            command.Parameters.Add(param);
                        }
                    }

                    // データベースの接続開始
                    connection.Open();

                    // SQLの実行
                    SqlDataReader sdr = command.ExecuteReader();

                    var records = new List<Dictionary<string, Object>>();

                    while (sdr.Read() == true)
                    {
                        var record = new Dictionary<string, Object>();
                        for (int i = 0; i < sdr.FieldCount; i++)
                        {
                            var value = sdr.GetValue(i);
                            record.Add(sdr.GetName(i), DBNull.Value.Equals(value) ? null : value);
                        }
                        records.Add(record);
                    }
                    return records;
                }
                catch (Exception exception)
                {
                    ErrorLog.InsertErrorLog("SQL: " + sql + "  Exception.Message: " + exception.Message);
                    throw;
                }
                finally
                {
                    // データベースの接続終了
                    connection.Close();
                }
            }
        }

        public bool ExecuteUpdate(
            string sql,
            Dictionary<string, object[]> dicParams = null,
            int timeoutSecond = 0
        )
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand("SET ANSI_WARNINGS OFF; " + sql, connection))
            {
                if (timeoutSecond != 0)
                {
                    //参考：https://netsystem.jpn.org/t_nary/vb-net/sql-server-%E3%82%BF%E3%82%A4%E3%83%A0%E3%82%A2%E3%82%A6%E3%83%88%E9%96%A2%E9%80%A3%E3%81%AE%E8%A8%AD%E5%AE%9A/
                    command.CommandTimeout = timeoutSecond; //コマンド実行タイムアウト
                }

                try
                {
                    // パラーメータの置換
                    if (dicParams != null)
                    {
                        foreach (KeyValuePair<string, object[]> kvp in dicParams)
                        {
                            var param = command.CreateParameter();
                            param.ParameterName = kvp.Key;
                            param.SqlDbType = (SqlDbType)kvp.Value[0];
                            param.Direction = ParameterDirection.Input;
                            param.Value = kvp.Value[1];

                            command.Parameters.Add(param);
                        }
                    }

                    // データベースの接続開始
                    connection.Open();

                    // SQLの実行
                    int result = command.ExecuteNonQuery();
                    return result >= 0;
                }
                catch (Exception exception)
                {
                    ErrorLog.InsertErrorLog("SQL: " + sql + "  Exception.Message: " + exception.Message);
                    throw;
                }
                finally
                {
                    // データベースの接続終了
                    connection.Close();
                }
            }
        }

        private Func<string, Dictionary<string, object[]>, int> GetUpdateFunc(
            SqlCommand command)
        {
            return (string sql, Dictionary<string, object[]> dicParams) =>
            {
                command.CommandText = "SET ANSI_WARNINGS OFF; " + sql;
                command.Parameters.Clear();

                // パラーメータの置換
                if (dicParams != null)
                {
                    foreach (KeyValuePair<string, object[]> kvp in dicParams)
                    {
                        var param = command.CreateParameter();
                        param.ParameterName = kvp.Key;
                        param.SqlDbType = (SqlDbType)kvp.Value[0];
                        param.Direction = ParameterDirection.Input;
                        param.Value = kvp.Value[1];

                        command.Parameters.Add(param);
                    }
                }

                // SQLの実行
                int result = command.ExecuteNonQuery();
                return result;
            };
        }

        // funcを引数として受け取り、そのfunc内の処理の前後に
        // TransactionのBeginやCommitを行う
        public bool UseTransaction(
            Func<Func<string, Dictionary<string, object[]>, int>, bool> func,
            int timeoutSecond = 0
        )
        {
            using (var connection = new SqlConnection(connectionString))
            {
                using (var command = connection.CreateCommand())
                {
                    if (timeoutSecond != 0)
                    {
                        //参考：https://netsystem.jpn.org/t_nary/vb-net/sql-server-%E3%82%BF%E3%82%A4%E3%83%A0%E3%82%A2%E3%82%A6%E3%83%88%E9%96%A2%E9%80%A3%E3%81%AE%E8%A8%AD%E5%AE%9A/
                        command.CommandTimeout = timeoutSecond; //コマンド実行タイムアウト
                    }

                    // データベースの接続開始
                    connection.Open();
                    SqlTransaction transaction = connection.BeginTransaction();
                    command.Transaction = transaction;
                    try
                    {
                        var execUpdate = GetUpdateFunc(command);
                        // 引数で受け取った関数に、update実行用の関数を渡す
                        bool result = func(execUpdate);
                        if (result)
                        {
                            transaction.Commit();
                            return true;
                        }
                        transaction.Rollback();
                        return false;
                    }
                    catch (Exception ex)
                    {
                        ErrorLog.InsertErrorLog(ex.Message);
                        transaction.Rollback();
                        return false;
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }
    }
}