using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using System.Globalization;
using System.Linq;
using Microsoft.Data.SqlClient;
using Microsoft.OpenApi.Models;
using System.Runtime.InteropServices;
using RestSharp;
using backend.Models;

public static class SelectExtention
{
    public static IEnumerable<T> Select<T>(this IDataReader reader,
                                      Func<IDataReader, T> projection)
    {
        while (reader.Read())
        {
            yield return projection(reader);
        }
    }
}


public class Connection
{
    private string m_connectionString;

    private static Connection m_instant;

    public static Connection GetSingleton()
    {
        if (m_instant == null) m_instant = new Connection();
        return m_instant;
    }


    public static Connection GetSingletonServer()
    {

        if (m_instant == null) m_instant = new Connection();

        //m_instant.m_connectionString = "Data Source=192.168.11.1\\MSSQLSERVER,1319;Initial Catalog=AUPOS;persist security info=True;User id=super_au; Password=vkag9viNp^$1319^&";
        return m_instant;
    }
    private Connection()
    {
        EnvironmentConfiguration.GetSingleton().AttachNotifiToMe(LoadConnectionString);
        LoadConnectionString(EnvironmentConfiguration.GetSingleton().GetConfig());
    }

    private void LoadConnectionString(JObject configTemp)
    {
        try
        {
            JObject config;

            //switch (EnvironmentConfiguration.GetSingleton().m_mode)
            //{
            //    case API_MODE.ADMIN:
            //        config = JObject.FromObject(configTemp["PRODUCTION"]); break;
            //    case API_MODE.BACKOFFICE:
            //        config = JObject.FromObject(configTemp["BACKOFFICE"]); break;
            //    case API_MODE.DOCKER:
            //        config = JObject.FromObject(configTemp["DOCKER"]); break;
            //    case API_MODE.LOCAL:
            //        config = JObject.FromObject(configTemp["LOCAL"]); break;
            //    default:
            //        config = JObject.FromObject(configTemp["PRODUCTION"]); break;
            //}

            //if (config["PASS"].ToString() == "")
            //{
            //    config["PASS"] = "au.2020";
            //}

            m_connectionString = "Server=192.168.11.3\\MSSQLSERVER,1590;Database=AUPOS_UAT;User Id=super_dev_test;Password=vkag9viNp^mflv[7890@@1;TrustServerCertificate=True;";

            //m_connectionString = $@"Data Source={config["TARGET_DB"]};Initial Catalog={config["CATALOG"]};User ID={config["USER"]};Password={config["PASS"]};Connection Timeout=15";
        }
        catch (Exception e)
        {
            Logger.GetSingleton().LogEvent("ErrInit.txt", e.Message);
        }

    }
    public Task<JObject> ExcuteQueryReturnSelectStatementJson(string commandString)
    {
        SqlConnection conn = new SqlConnection();
        SqlCommand cmd = new SqlCommand();
        SqlDataAdapter objAdapter = new SqlDataAdapter();
        JObject returnValues = new JObject();
        returnValues.Add("Data", null);
        returnValues.Add("Status", null);
        returnValues.Add("Message", null);
        try
        {
            conn.ConnectionString = m_connectionString;
            conn.Open();
            objAdapter.SelectCommand = cmd;
            objAdapter.SelectCommand.Connection = conn;
            objAdapter.SelectCommand.CommandType = CommandType.Text;
            objAdapter.SelectCommand.CommandText = commandString.Trim() + " FOR JSON AUTO";

            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Read();
                returnValues["Data"] = JToken.Parse(reader.GetString(0));
                returnValues["Status"] = 1;
                returnValues["Message"] = "";
            }
            else
            {
                returnValues["Data"] = null;
                returnValues["Status"] = 0;
                returnValues["Message"] = "";
            }


        }
        catch (Exception e)
        {
            returnValues["Status"] = 0;
            returnValues["Message"] = e.Message.Trim();
        }
        finally
        {
            if (conn != null)
            {
                conn.Close();
                conn.Dispose();
            }
        }
        return Task.FromResult(returnValues);
    }

    /// <summary>
    /// This method will always Excute by ExecuteReader
    /// it's mean commandString for this must be select statement
    /// </summary>
    /// <param name="commandString"></param>
    /// <returns></returns>
    public Task<JObject> ExcuteQueryReturnSelectStatement(string commandString, string prefix = null)
    {
        if (prefix != null)
        {
            string fname = prefix + "_" + DateTime.Now.ToString("dd_MM_yyyy_hh_mm_ss") + ".txt";
            Logger.GetSingleton().LogEvent(fname, "Query: " + commandString);
        }

        SqlConnection conn = new SqlConnection();
        SqlCommand cmd = new SqlCommand();
        SqlDataAdapter objAdapter = new SqlDataAdapter();
        JObject returnValues = new JObject();
        returnValues.Add("Data", null);
        returnValues.Add("Status", null);
        returnValues.Add("Message", null);
        try
        {
            conn.ConnectionString = m_connectionString;
            conn.Open();
            objAdapter.SelectCommand = cmd;
            objAdapter.SelectCommand.Connection = conn;
            objAdapter.SelectCommand.CommandType = CommandType.Text;
            objAdapter.SelectCommand.CommandText = commandString.Trim();

            SqlDataReader reader = cmd.ExecuteReader();
            returnValues["Data"] = GetDataFromReaderIntoJArray(reader);
            returnValues["Status"] = 1;
            returnValues["Message"] = "";
        }
        catch (Exception e)
        {
            returnValues["Status"] = 0;
            returnValues["Message"] = e.Message.Trim();
        }
        finally
        {
            if (conn != null)
            {
                conn.Close();
                conn.Dispose();
            }
        }
        return Task.FromResult(returnValues);
    }

    /// <summary>
    /// Mostly used this method are insert/update statement
    /// Because they need just affected row
    /// </summary>
    /// <param name="commandString"></param>
    /// <returns></returns>
    public Task<JObject> ExcuteNonQueryReturnOnlyAffectedRow(string commandString, string prefix = null)
    {
        if (prefix != null)
        {
            string fname = prefix + "_SQL_" + DateTime.Now.ToString("dd_MM_yyyy_hh_mm_ss") + ".txt";
            Logger.GetSingleton().LogEvent(fname, "Query: " + commandString);
        }

        SqlConnection conn = new SqlConnection();
        SqlCommand cmd = new SqlCommand();
        SqlDataAdapter objAdapter = new SqlDataAdapter();
        JObject returnValues = new JObject();
        returnValues.Add("Status", null);
        returnValues.Add("Message", null);
        try
        {
            conn.ConnectionString = m_connectionString;
            conn.Open();
            objAdapter.SelectCommand = cmd;
            objAdapter.SelectCommand.Connection = conn;
            objAdapter.SelectCommand.CommandType = CommandType.Text;
            objAdapter.SelectCommand.CommandText = commandString.Trim();

            var affectedRow = cmd.ExecuteNonQuery();
            returnValues["Status"] = 1;
            returnValues["Message"] = $"Affected {affectedRow} row(s) ";
        }
        catch (Exception e)
        {
            returnValues["Status"] = 0;
            returnValues["Message"] = e.Message.Trim();
        }
        finally
        {
            if (conn != null)
            {
                conn.Close();
                conn.Dispose();
            }
        }
        return Task.FromResult(returnValues);
    }


    public Task<DataTable> ExcuteNonQueryReturnDataTable(string commandString)
    {

        DataTable dt = new DataTable();
        SqlConnection conn = new SqlConnection();
        SqlCommand cmd = new SqlCommand();
        SqlDataAdapter objAdapter = new SqlDataAdapter();
        JObject returnValues = new JObject();
        returnValues.Add("Status", null);
        returnValues.Add("Message", null);
        try
        {
            conn.ConnectionString = m_connectionString;
            conn.Open();
            objAdapter.SelectCommand = cmd;
            objAdapter.SelectCommand.Connection = conn;
            objAdapter.SelectCommand.CommandType = CommandType.Text;
            objAdapter.SelectCommand.CommandText = commandString.Trim();
            objAdapter.Fill(dt);

            //var affectedRow = cmd.ExecuteNonQuery();
            //returnValues["Status"] = 1;
            //returnValues["Message"] = $"Affected {affectedRow} row(s) ";
        }
        catch (Exception e)
        {
            dt = null;
            //returnValues["Status"] = 0;
            //returnValues["Message"] = e.Message.Trim();
        }
        finally
        {
            if (conn != null)
            {
                conn.Close();
                conn.Dispose();
            }
        }
        return Task.FromResult(dt);
    }
    private JObject GetDataFromReaderIntoJObject(SqlDataReader reader)
    {
        JObject jDataObject = new JObject();
        while (reader.Read())
        {
            for (int j = 0; j < reader.FieldCount; j++)
            {
                string fieldName = reader.GetName(j);
                object fieldData = reader[fieldName];

                if (!jDataObject.ContainsKey(fieldName))
                {
                    jDataObject.Add(fieldName, new JArray());
                }

                ((JArray)jDataObject[fieldName]).Add(fieldData.ToString());
            }
        }
        return jDataObject;
    }

    private JArray GetDataFromReaderIntoJArray(SqlDataReader reader)
    {
        JArray jDataArray = new JArray();
        while (reader.Read())
        {
            JObject objectInArray = new JObject();

            for (int j = 0; j < reader.FieldCount; j++)
            {
                string fieldName = reader.GetName(j);
                var fieldData = reader[fieldName];

                objectInArray.Add(fieldName, JToken.FromObject(fieldData));
            }
            jDataArray.Add(objectInArray);
        }
        return jDataArray;
    }


    public Task<JObject> ExcuteStoredProcedureThatRespondSelectIntoJsonObject_Server(string commandString, List<string> paramList, CommandType ctype = CommandType.StoredProcedure, string defaultMessage = "Success")
    {
        // Function เอาไว้เทส 

        SqlConnection conn = new SqlConnection();
        SqlCommand cmd = new SqlCommand() { CommandTimeout = 900 };
        SqlDataAdapter objAdapter = new SqlDataAdapter();
        JObject returnValues = new JObject();
        returnValues.Add("Status", 0);
        returnValues.Add("Message", defaultMessage);
        returnValues.Add("Data", "");
        try
        {
            conn.ConnectionString = "Data Source=192.168.11.3\\MSSQLSERVER,1590;Initial Catalog=AUPOS;persist security info=True;User id=super_au; Password=vkag9viNp^$1319^&";
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            objAdapter.SelectCommand = cmd;
            objAdapter.SelectCommand.Connection = conn;
            objAdapter.SelectCommand.CommandType = ctype;
            objAdapter.SelectCommand.CommandText = commandString.Trim();
            SqlCommandBuilder.DeriveParameters(objAdapter.SelectCommand);
            for (int i = 0; i < objAdapter.SelectCommand.Parameters.Count; i++)
            {
                if (objAdapter.SelectCommand.Parameters[i].ParameterName.ToUpper() == ("@RETURN_VALUE"))
                {
                    objAdapter.SelectCommand.Parameters.RemoveAt(i);
                    break;
                }
            }

            int usedParamIndex = 0;
            for (int i = 0; i < objAdapter.SelectCommand.Parameters.Count; i++)
            {
                if (objAdapter.SelectCommand.Parameters[i].Direction == ParameterDirection.Input)
                {
                    cmd.Parameters[i].Value = paramList[usedParamIndex];
                    usedParamIndex++;
                }
                else
                {
                    cmd.Parameters[i].Direction = ParameterDirection.Output;
                }
            }

            DataTable dtable = new DataTable();
            objAdapter.Fill(dtable);
            if (dtable.Rows.Count > 0)
            {
                foreach (var item in dtable.Rows)
                {
                    JObject data = JObject.FromObject(item);
                    JToken jd = data["Table"];
                    if (jd[0]["ErrorNumber"] != null)
                    {
                        returnValues["Status"] = 0;
                        returnValues["Message"] = $"{jd[0]["ErrorMessage"]} on {jd[0]["ErrorProcedure"]}";
                    }
                    else
                    {
                        returnValues["Status"] = 1;
                        returnValues["Data"] = jd;
                    }

                    break;
                }
            }
            else
            {
                returnValues["Data"] = null;
                returnValues["Message"] = $"No data return from {commandString}";
            }
        }
        catch (Exception e)
        {
            returnValues["Status"] = 0;
            returnValues["Message"] = e.Message;
        }
        finally
        {
            if (conn != null)
            {
                conn.Close();
                conn.Dispose();
            }
        }
        return Task.FromResult(returnValues);
    }


    public Task<JObject> ExcuteStoredProcedureThatRespondSelectIntoJsonObject(string commandString, List<string> paramList, CommandType ctype = CommandType.StoredProcedure, string defaultMessage = "Success")
    {
        SqlConnection conn = new SqlConnection();
        SqlCommand cmd = new SqlCommand() { CommandTimeout = 900 };
        SqlDataAdapter objAdapter = new SqlDataAdapter();
        JObject returnValues = new JObject();
        returnValues.Add("Status", 0);
        returnValues.Add("Message", defaultMessage);
        returnValues.Add("Data", "");
        try
        {

            //conn.ConnectionString = $@"Data Source=192.168.11.1\\MSSQLSERVER,1319;Initial Catalog=AUPOS;User ID=super_au;Password=vkag9viNp^$1319^&;Connection Timeout=15";
            conn.ConnectionString = m_connectionString;

            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            objAdapter.SelectCommand = cmd;
            objAdapter.SelectCommand.Connection = conn;
            objAdapter.SelectCommand.CommandType = ctype;
            objAdapter.SelectCommand.CommandText = commandString.Trim();
            SqlCommandBuilder.DeriveParameters(objAdapter.SelectCommand);
            for (int i = 0; i < objAdapter.SelectCommand.Parameters.Count; i++)
            {
                if (objAdapter.SelectCommand.Parameters[i].ParameterName.ToUpper() == ("@RETURN_VALUE"))
                {
                    objAdapter.SelectCommand.Parameters.RemoveAt(i);
                    break;
                }
            }

            int usedParamIndex = 0;
            for (int i = 0; i < objAdapter.SelectCommand.Parameters.Count; i++)
            {
                if (objAdapter.SelectCommand.Parameters[i].Direction == ParameterDirection.Input)
                {
                    cmd.Parameters[i].Value = paramList[usedParamIndex];
                    usedParamIndex++;
                }
                else
                {
                    cmd.Parameters[i].Direction = ParameterDirection.Output;
                }
            }

            DataTable dtable = new DataTable();
            objAdapter.Fill(dtable);
            if (dtable.Rows.Count > 0)
            {
                foreach (var item in dtable.Rows)
                {
                    JObject data = JObject.FromObject(item);
                    JToken jd = data["Table"];
                    if (jd[0]["ErrorNumber"] != null)
                    {
                        returnValues["Status"] = 0;
                        //returnValues["isSuccess"] = "false";
                        returnValues["Message"] = $"{jd[0]["ErrorMessage"]} on {jd[0]["ErrorProcedure"]}";
                    }
                    else
                    {
                        returnValues["Status"] = 1;
                        //returnValues["isSuccess"] = "true";
                        returnValues["Data"] = jd;
                    }

                    break;
                }
            }
            else
            {
                returnValues["Data"] = null;
                returnValues["Message"] = $"No data return from {commandString}";
            }
        }
        catch (Exception e)
        {
            returnValues["Status"] = 0;
            returnValues["Message"] = e.Message;
        }
        finally
        {
            if (conn != null)
            {
                conn.Close();
                conn.Dispose();
            }
        }
        return Task.FromResult(returnValues);
    }
    public Task<JObject> ExcuteStoredProcedureThatRespondSelectIntoJsonObject(string commandString, List<object> paramList, CommandType ctype, string defaultMessage = "Success")
    {
        SqlConnection conn = new SqlConnection();
        SqlCommand cmd = new SqlCommand();
        SqlDataAdapter objAdapter = new SqlDataAdapter();
        JObject returnValues = new JObject();
        returnValues.Add("Status", 0);
        returnValues.Add("Message", defaultMessage);
        returnValues.Add("Data", "");
        try
        {
            conn.ConnectionString = m_connectionString;
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            objAdapter.SelectCommand = cmd;
            objAdapter.SelectCommand.Connection = conn;
            objAdapter.SelectCommand.CommandType = ctype;
            objAdapter.SelectCommand.CommandText = commandString.Trim();
            SqlCommandBuilder.DeriveParameters(objAdapter.SelectCommand);
            for (int i = 0; i < objAdapter.SelectCommand.Parameters.Count; i++)
            {
                if (objAdapter.SelectCommand.Parameters[i].ParameterName.ToUpper() == ("@RETURN_VALUE"))
                {
                    objAdapter.SelectCommand.Parameters.RemoveAt(i);
                    break;
                }
            }

            int usedParamIndex = 0;
            for (int i = 0; i < objAdapter.SelectCommand.Parameters.Count; i++)
            {
                if (objAdapter.SelectCommand.Parameters[i].Direction == ParameterDirection.Input)
                {
                    cmd.Parameters[i].Value = paramList[usedParamIndex];
                    usedParamIndex++;
                }
                else
                {
                    cmd.Parameters[i].Direction = ParameterDirection.Output;
                }
            }

            DataTable dtable = new DataTable();
            objAdapter.Fill(dtable);
            if (dtable.Rows.Count > 0)
            {
                foreach (var item in dtable.Rows)
                {
                    JObject data = JObject.FromObject(item);
                    JToken jd = data["Table"];
                    if (jd[0]["ErrorNumber"] != null)
                    {
                        returnValues["Status"] = 0;
                        returnValues["Message"] = $"{jd[0]["ErrorMessage"]} on {jd[0]["ErrorProcedure"]}";
                    }
                    else
                    {
                        returnValues["Status"] = 1;
                        returnValues["Data"] = jd;
                    }

                    break;
                }
            }
            else
            {
                returnValues["Data"] = null;
                returnValues["Message"] = $"No data return from {commandString}";
            }
        }
        catch (Exception e)
        {
            returnValues["Status"] = 0;
            returnValues["Message"] = e.Message;
        }
        finally
        {
            if (conn != null)
            {
                conn.Close();
                conn.Dispose();
            }
        }
        return Task.FromResult(returnValues);
    }
    public async Task<JObject> GetData(string store, string mode, string wherecurse, string KeyEncrypt = "")
    {

        List<string> list = new List<string>();

        if (KeyEncrypt == "")
        {
            list = new List<string>
            {
                mode,
                wherecurse
            };
        }
        else
        {

            list = new List<string>
            {
                mode,
                wherecurse,
                KeyEncrypt
            };
        }

        return await ExcuteStoredProcedureThatRespondSelectIntoJsonObject(store, list, CommandType.StoredProcedure);
    }

    public async Task<JObject> GetData2(string store, string mode, string wherecurse, string wherecurse2, string KeyEncrypt = "")
    {

        List<string> list = new List<string>();

        list = new List<string>
        {
            mode,
            wherecurse,
            wherecurse2,
            KeyEncrypt
        };


        return await ExcuteStoredProcedureThatRespondSelectIntoJsonObject(store, list, CommandType.StoredProcedure);
    }

    public async Task<JObject> SetStatAndStateCode(string tableName, string columnName, object autoID, object updateby, string stat, string DeletionStateCode)
    {
        JObject result = new JObject();
        List<string> list = new List<string>();
        try
        {
            list.Add(autoID.ToString());
            list.Add(tableName);
            list.Add(columnName);
            list.Add(stat);
            list.Add(DeletionStateCode);
            list.Add(updateby.ToString());

        }
        catch (Exception e)
        {
            JObject exceptionResult = new JObject();
            exceptionResult.Add("Status", "0");
            exceptionResult.Add("Message", $"Please input all field : {e.Message}");
            exceptionResult.Add("Path", $"--> : {e.StackTrace}");
            exceptionResult.Add("Data", null);
            return result;
        }

        result = await ExcuteStoredProcedureThatRespondSelectIntoJsonObject("UpdateDeletionAndStat",
            list,
            CommandType.StoredProcedure);
        return result;
    }
    public async Task<JObject> FunctionSetStatAndStateCode(string tableName, string columnName, object autoID, object updateby, string stat, string DeletionStateCode)
    {
        JObject result = await SetStatAndStateCode(tableName, columnName, autoID, updateby, stat, DeletionStateCode);
        try
        {
            //force to single data object
            result["Data"] = result["Data"][0];
        }
        catch
        {
            result["Data"] = null;
        }
        return result;
    }
    public async Task<JObject> FunctionSetStatAndStateCodeList(string tableName, string columnName, JObject bodyObject, string stat, string DeletionStateCode)
    {
        JObject result = new JObject();
        result.Add("Data", null);
        result.Add("Message", "Ok");
        result.Add("Status", "1");
        try
        {
            JArray results = new JArray();
            foreach (var item in bodyObject["AutoIdList"])
            {
                JObject DbResult = await SetStatAndStateCode(tableName, columnName, item, bodyObject["MaintainedBy"], stat, DeletionStateCode);
                try { results.Add(DbResult["Data"][0]); }
                catch { results.Add($"AutoID: {item.ToString()} {DbResult["Message"]}"); }
                result["Data"] = results;
            }
        }
        catch (Exception e)
        {
            JObject exceptionResult = new JObject();
            exceptionResult.Add("Status", "0");
            exceptionResult.Add("Message", $"Please input all field : {e.Message}");
            exceptionResult.Add("Path", $"--> : {e.StackTrace}");
            exceptionResult.Add("Data", null);
            return exceptionResult;
        }
        return result;
    }

    public async Task<JObject> CheckShift(JObject bodyObject = null)
    {
        List<string> list = new List<string>();
        if (bodyObject != null)
        {
            if (bodyObject.ContainsKey("User_Start_Id"))
            {
                list.Add(bodyObject["User_Start_Id"].ToString());
            }
            else if (bodyObject.ContainsKey("User_End_Id"))
            {
                list.Add(bodyObject["User_End_Id"].ToString());
            }
            else if (bodyObject.ContainsKey("A_User_Id"))
            {
                list.Add(bodyObject["A_User_Id"].ToString());
            }
            list.Add(bodyObject["M_Cashier_Id"].ToString());
        }
        else
        {
            list.Add("any");
            list.Add("any");
        }

        JObject result = await ExcuteStoredProcedureThatRespondSelectIntoJsonObject("CheckMyShift",
            list,
            CommandType.StoredProcedure);
        return result;
    }
    public async Task<JObject> GetMemberBalanchPoint(string C_MB_Id)
    {
        JObject result = new JObject();
        try
        {

            bool NeedProduction = false;
            string path = "member/getonlymember";
            JObject param = new JObject();
            param.Add("AutoID", C_MB_Id);
            var requestResult = await RequestToServer(EnvironmentConfiguration.GetSingleton().GetServerSyncURL(NeedProduction),
                     path, param, EnvironmentConfiguration.GetSingleton().WaitingTime() * 1000);
            JObject dataFromContent = JObject.Parse(requestResult.Content);

            result = JObject.FromObject(dataFromContent["Data"]);// JObject.FromObject(dataFromContent["Data"][0]);

        }
        catch (Exception e)
        {
            JObject cannotConnectToServer = new JObject();
            result.Add("MB_Name", "");
            result.Add("MB_Lastname", "");
            result.Add("MB_Birthday", "");
            result.Add("MB_IdCard", "");
            result.Add("MB_Tel", "");
            result.Add("MB_Email", "");
            result.Add("MB_Address", "");
            result.Add("BalancePoint", 0);
        }
        return result;
    }
    public async Task<JObject> GetOrderDetatail(JObject bodyObject)
    {
        JObject orderH = new JObject();
        JObject FinalResult = new JObject();
        JObject result = new JObject();
        JObject OrderMember = new JObject();
        try
        {
            List<string> list = new List<string>();
            list.Add(bodyObject["S_Branch_Id"].ToString());
            list.Add(bodyObject["Ord_No"].ToString());
            list.Add(bodyObject["Ord_RD"].ToString());
            list.Add(bodyObject["Ord_Cashier"].ToString());


            orderH = await ExcuteStoredProcedureThatRespondSelectIntoJsonObject("GetOrderData",
                        list,
                        CommandType.StoredProcedure);
            if (orderH["Status"].ToString() == "0")
            {
                return orderH;
            }
            FinalResult = orderH;
            result = JObject.FromObject(orderH["Data"][0]);
            try
            {
                result.Add("IsFirstVoid", bodyObject["IsFirstVoid"]);
            }
            catch (Exception)
            {
                result.Add("IsFirstVoid", 0);
            }

            string S_Ord_H_Id = result["S_Ord_H_Id"].ToString();
            list = new List<string>();
            list.Add(S_Ord_H_Id);
            JObject pay = await ExcuteStoredProcedureThatRespondSelectIntoJsonObject("GetOrderPayData",
                        list,
                        CommandType.StoredProcedure);
            result.Add("OrderPays", pay["Data"]);

            JObject me = await ExcuteStoredProcedureThatRespondSelectIntoJsonObject("GetOrderMemberData",
                       list,
                       CommandType.StoredProcedure);
            try
            {
                OrderMember = JObject.FromObject(me["Data"][0]);
                JObject memberPointData = await GetMemberBalanchPoint(OrderMember["C_MB_Id"].ToString());
                memberPointData.Add("PreviosPoint", OrderMember["PreviosPoint"]);
                memberPointData.Add("Ord_Point", OrderMember["Ord_Point"]);
                memberPointData["BalancePoint"] = OrderMember["BalancePoint"];
                memberPointData.Add("PickupBranch", OrderMember["PickupBranch"]);
                result.Add("OrderMember", memberPointData);
            }
            catch (Exception)
            {
                result.Add("OrderMember", null);
            }


            JObject de = await ExcuteStoredProcedureThatRespondSelectIntoJsonObject("GetOrderDetailData",
                        list,
                        CommandType.StoredProcedure);

            JArray details = new JArray();
            foreach (var OderDetail in de["Data"])
            {
                JObject oDetail = JObject.FromObject(OderDetail);

                string S_Ord_D_Id = oDetail["S_Ord_D_Id"].ToString();

                list = new List<string>();
                list.Add(S_Ord_D_Id);
                JObject add = await ExcuteStoredProcedureThatRespondSelectIntoJsonObject("GetOrderAddData", list,
                     CommandType.StoredProcedure);
                oDetail.Add("OrderAdds", add["Data"]);

                try
                {
                    list = new List<string>();
                    list.Add(S_Ord_H_Id);
                    list.Add(S_Ord_D_Id);
                    list.Add(bodyObject["S_Branch_Id"].ToString());

                    JObject printers = await ExcuteStoredProcedureThatRespondSelectIntoJsonObject("GetOrderDetailPrinter", list,
                      CommandType.StoredProcedure);
                    //oDetail.Add("OrderPrinter", printers["Data"]);
                    JArray orderPrinters = new JArray();
                    foreach (var item in printers["Data"])
                    {
                        try { orderPrinters.Add(item["Printer_Name"].ToObject<string>()); }
                        catch (Exception) { }
                    }
                    oDetail.Add("OrderPrinter", orderPrinters);
                }
                catch (Exception)
                {
                    oDetail.Add("OrderPrinter", null);
                }


                list = new List<string>();
                list.Add(S_Ord_D_Id);
                list.Add(S_Ord_H_Id);
                JObject promo = await ExcuteStoredProcedureThatRespondSelectIntoJsonObject("GetOrderPromotionData",
                    list,
                    CommandType.StoredProcedure);

                try
                {
                    oDetail.Add("OrderPromotion", promo["Data"][0]);
                }
                catch (Exception)
                {
                    oDetail.Add("OrderPromotion", null);
                }

                details.Add(oDetail);
            }
            result.Add("OrderDetails", details);



            list = new List<string>();
            list.Add(S_Ord_H_Id);
            JObject promo_group = await ExcuteStoredProcedureThatRespondSelectIntoJsonObject("GetOrderPromotionDataGroup", list, CommandType.StoredProcedure);

            try
            {

                if (promo_group["Data"].HasValues)
                {
                    result.Add("OrderPromotions", promo_group["Data"]);
                    result.Add("OrderPromotion", promo_group["Data"][0]);
                }
                else
                {
                    result.Add("OrderPromotions", null);
                    result.Add("OrderPromotion", null);

                }
            }
            catch (Exception)
            {
                result.Add("OrderPromotions", null);
                result.Add("OrderPromotion", null);
            }



            FinalResult["Data"] = result;

        }
        catch (Exception e)
        {
            FinalResult.Add("Exception_m", e.Message);
            FinalResult.Add("Exception_t", e.StackTrace);
            FinalResult.Add("orderH", orderH);
        }
        return FinalResult;
    }


    public async Task<JObject> GetOrderByOrderNo(JObject bodyObject)
    {
        JObject orderH = new JObject();
        JObject FinalResult = new JObject();
        JObject result = new JObject();
        JObject OrderMember = new JObject();
        try
        {
            List<string> list = new List<string>();
            list.Add(bodyObject["S_Branch_Id"].ToString());
            list.Add(bodyObject["Ord_No"].ToString());
            list.Add(bodyObject["Ord_RD"].ToString());
            list.Add(bodyObject["Ord_Cashier"].ToString());


            orderH = await ExcuteStoredProcedureThatRespondSelectIntoJsonObject("GetOrderDataByOrderNo",
                        list,
                        CommandType.StoredProcedure);
            if (orderH["Status"].ToString() == "0")
            {
                return orderH;
            }
            FinalResult = orderH;
            result = JObject.FromObject(orderH["Data"][0]);
            try
            {
                result.Add("IsFirstVoid", bodyObject["IsFirstVoid"]);
            }
            catch (Exception)
            {
                result.Add("IsFirstVoid", 0);
            }

            string S_Ord_H_Id = result["S_Ord_H_Id"].ToString();
            list = new List<string>();
            list.Add(S_Ord_H_Id);
            JObject pay = await ExcuteStoredProcedureThatRespondSelectIntoJsonObject("GetOrderPayData",
                        list,
                        CommandType.StoredProcedure);
            result.Add("OrderPays", pay["Data"]);


            JObject promotiongroup = await ExcuteStoredProcedureThatRespondSelectIntoJsonObject("GetOrderPromotionDataGroup",
                        list,
                        CommandType.StoredProcedure);


            JArray pros = new JArray();
            if (promotiongroup["Data"].HasValues)
            {

                //var ipro = 0;
                //foreach (var OderPro in promotiongroup["Data"])
                //{
                //    JObject oPro = JObject.FromObject(OderPro);


                //    string PromotionType_Code = oPro["PromotionType_Code"].ToString();


                //    if (PromotionType_Code == "PT01")
                //    {

                //        result.Add("OrderPromotion", promotiongroup["Data"][0]);
                //    }
                //    else
                //    {

                //        result.Add("OrderPromotions", promotiongroup["Data"][ipro]);
                //    }
                //    ipro++;

                //    //result.Add("OrderPromotions", promotiongroup["Data"]);
                //}

                string PromotionType_Code = promotiongroup["Data"][0]["PromotionType_Code"].ToString();
                if (PromotionType_Code == "PT01")
                {

                    result.Add("OrderPromotion", promotiongroup["Data"][0]);
                }
                else
                {

                    result.Add("OrderPromotions", promotiongroup["Data"]);
                }
            }
            else
            {
                result.Add("OrderPromotion", null);
                result.Add("OrderPromotions", null);
            }

            //    JObject OrderPro = new JObject();
            //OrderPro = JObject.FromObject(promotiongroup["Data"][0]);
            //string proid = OrderPro["proid"];
            //if (proid.StartsWith("RD"))
            //{
            //    promotiongroup.Add("Order_Disc_Total", OrderPro["Ord_Disc"]);
            //}
            //else
            //{
            //    promotiongroup.Add("Order_Disc_Total", OrderPro["Ord_Disc"] * OrderPro["Ord_Disc"]);
            //}

            //result.Add("OrderPromotions", promotiongroup["Data"]);


            JObject me = await ExcuteStoredProcedureThatRespondSelectIntoJsonObject("GetOrderMemberData",
                       list,
                       CommandType.StoredProcedure);
            try
            {
                OrderMember = JObject.FromObject(me["Data"][0]);
                JObject memberPointData = await GetMemberBalanchPoint(OrderMember["C_MB_Id"].ToString());
                memberPointData.Add("PreviosPoint", OrderMember["PreviosPoint"]);
                memberPointData.Add("Ord_Point", OrderMember["Ord_Point"]);
                memberPointData["BalancePoint"] = OrderMember["BalancePoint"];
                memberPointData.Add("PickupBranch", OrderMember["PickupBranch"]);
                result.Add("OrderMember", memberPointData);
            }
            catch (Exception)
            {
                result.Add("OrderMember", null);
            }


            JObject de = await ExcuteStoredProcedureThatRespondSelectIntoJsonObject("GetOrderDetailData",
                        list,
                        CommandType.StoredProcedure);

            JArray details = new JArray();
            foreach (var OderDetail in de["Data"])
            {
                JObject oDetail = JObject.FromObject(OderDetail);

                string S_Ord_D_Id = oDetail["S_Ord_D_Id"].ToString();

                list = new List<string>();
                list.Add(S_Ord_D_Id);
                JObject add = await ExcuteStoredProcedureThatRespondSelectIntoJsonObject("GetOrderAddData", list,
                     CommandType.StoredProcedure);
                oDetail.Add("OrderAdds", add["Data"]);

                try
                {
                    list = new List<string>();
                    list.Add(S_Ord_H_Id);
                    list.Add(S_Ord_D_Id);
                    list.Add(bodyObject["S_Branch_Id"].ToString());

                    JObject printers = await ExcuteStoredProcedureThatRespondSelectIntoJsonObject("GetOrderDetailPrinter", list,
                      CommandType.StoredProcedure);
                    //oDetail.Add("OrderPrinter", printers["Data"]);
                    JArray orderPrinters = new JArray();
                    foreach (var item in printers["Data"])
                    {
                        try { orderPrinters.Add(item["Printer_Name"].ToObject<string>()); }
                        catch (Exception) { }
                    }
                    oDetail.Add("OrderPrinter", orderPrinters);
                }
                catch (Exception)
                {
                    oDetail.Add("OrderPrinter", null);
                }


                list = new List<string>();
                list.Add(S_Ord_D_Id);
                list.Add(S_Ord_H_Id);
                JObject promo = await ExcuteStoredProcedureThatRespondSelectIntoJsonObject("GetOrderPromotionData",
                    list,
                    CommandType.StoredProcedure);

                if (promo["Data"].HasValues == false)
                {
                    oDetail.Add("OrderPromotions", null);
                    oDetail.Add("OrderPromotion", null);
                }
                else
                {
                    try
                    {

                        JArray orderPromotions = new JArray();


                        foreach (var item in promo["Data"])
                        {
                            try
                            {
                                if (item["PromotionType_Code"].ToString() == "PT01" || item["S_Promotion_Id"].ToString() == "72A0ECF4-6411-4AFB-87FB-E9544EF63667".ToLower())
                                {
                                    oDetail.Add("OrderPromotion", item);
                                    //oDetail.Add("OrderPromotions", null);
                                }
                                else
                                {
                                    orderPromotions.Add(item);
                                }
                            }
                            catch (Exception) { }
                        }

                        if (orderPromotions.Count > 0)
                        {
                            oDetail.Add("OrderPromotions", orderPromotions);
                            oDetail.Add("OrderPromotion", null);
                        }

                    }
                    catch (Exception)
                    {
                        oDetail.Add("OrderPromotions", null);
                        oDetail.Add("OrderPromotion", null);
                    }
                }


                details.Add(oDetail);
            }

            result.Add("OrderDetails", details);

            FinalResult["Data"] = result;

        }
        catch (Exception e)
        {
            FinalResult.Add("Exception_m", e.Message);
            FinalResult.Add("Exception_t", e.StackTrace);
            FinalResult.Add("orderH", orderH);
        }
        return FinalResult;
    }


    public async Task<JObject> GetHoldOrderDetatailWeb()
    {
        JObject orderH = new JObject();
        JObject FinalResult = new JObject();
        //JObject result = new JObject();
        JArray resultArray = new JArray();
        try
        {
            List<string> list = new List<string>();


            orderH = await ExcuteStoredProcedureThatRespondSelectIntoJsonObject("GetHoldOrderData",
                        list,
                        CommandType.StoredProcedure);
            if (orderH["Status"].ToString() == "0")
            {
                return orderH;
            }
            FinalResult = orderH;
            JArray orderHArr = JArray.FromObject(orderH["Data"]);
            foreach (var orh in orderHArr)
            {
                JObject tempObject = new JObject();
                tempObject = JObject.FromObject(orh);
                tempObject.Add("IsFirstVoid", 0);
                string hold_S_Ord_H_Id = tempObject["S_Ord_H_Id"].ToString();
                list = new List<string>();
                list.Add(hold_S_Ord_H_Id);

                JObject GetOrderMemberData = await ExcuteStoredProcedureThatRespondSelectIntoJsonObject("GetHoldOrderMemberData",
                     list,
                     CommandType.StoredProcedure);
                try
                {
                    tempObject.Add("OrderMember", GetOrderMemberData["Data"][0]);
                }
                catch (Exception)
                {
                    tempObject.Add("OrderMember", null);
                }

                JObject GetOrderDetailData = await ExcuteStoredProcedureThatRespondSelectIntoJsonObject("GetHoldOrderDetailDataWithPrinter",
                      list,
                      CommandType.StoredProcedure);

                //JObject orddetail = new JObject();
                JArray jdetails = new JArray();
                JArray jadds = new JArray();
                JArray jpro = new JArray();
                JArray jprinters = new JArray();


                JObject orddetail = new JObject();
                try
                {

                    foreach (var OderDetail in GetOrderDetailData["Data"])
                    {
                        orddetail = JObject.FromObject(OderDetail);

                        JObject oDetailPrinter = JObject.FromObject(OderDetail);

                        //details = JObject.FromObject(OderDetail);


                        #region OrderAdds
                        jadds = new JArray();
                        list = new List<string>();
                        list.Add(OderDetail["S_Ord_D_Id"].ToString());
                        JObject add = await ExcuteStoredProcedureThatRespondSelectIntoJsonObject("GetHoldOrderAddData",
                             list,
                             CommandType.StoredProcedure);
                        //oDetail.Add("OrderAdds", add["Data"]);                    
                        try
                        {
                            //oDetail.Add("OrderPromotion", promo["Data"][0]); 
                            if (add["Data"].HasValues)
                            {
                                jadds.Add(add["Data"]);
                            }
                            else
                            {
                                jadds = null;
                            }

                        }
                        catch (Exception)
                        {
                            //oDetail.Add("OrderPromotion", null);
                            jadds = null;
                        }
                        orddetail.Add("OrderAdds", jadds);
                        #endregion

                        #region OrderPromotionPart
                        jpro = new JArray();
                        list = new List<string>();
                        list.Add(OderDetail["S_Ord_D_Id"].ToString());
                        list.Add(hold_S_Ord_H_Id);
                        JObject promo = await ExcuteStoredProcedureThatRespondSelectIntoJsonObject("GetHoldOrderPromotionData",
                            list,
                            CommandType.StoredProcedure);

                        try
                        {
                            if (promo["Data"].HasValues)
                            {
                                jpro.Add(promo["Data"][0]);
                            }
                            else
                            {
                                jpro = null;
                            }
                        }
                        catch (Exception)
                        {
                            //oDetail.Add("OrderPromotion", null);
                            jpro = null;
                        }
                        orddetail.Add("OrderPromotions", jpro);
                        //details.Add(oDetail);
                        #endregion

                        #region OrderPrinter Part
                        try
                        {
                            if (oDetailPrinter.TryGetValue("Printer_Name", out JToken value) && value.Type != JTokenType.Null)
                            {
                                string printerName = value.ToString();
                                jprinters.Add(printerName);
                            }
                            else
                            {
                                jprinters = null;
                            }


                            //foreach (KeyValuePair<string, JToken?> kvp in oDetailPrinter)
                            //{

                            //    if (kvp.Key == "Printer_Name")
                            //    {
                            //        if (oDetailPrinter.TryGetValue("Printer_Name", out JToken value) && value.Type != JTokenType.Null)
                            //        {
                            //            string printerName = value.ToString();
                            //            //Console.WriteLine(printerName); // Output: CSR
                            //            jprinters.Add(printerName);
                            //        }
                            //    }
                            //}
                        }
                        catch (Exception)
                        {
                            //jprinters.Add(null);
                            jprinters = null;
                        }
                        orddetail.Add("OrderPrinter", jprinters);
                        #endregion

                        jdetails.Add(orddetail);
                    }
                }
                catch (Exception e)
                {
                    orddetail = null;
                    jdetails.Add(orddetail);
                }

                tempObject.Add("OrderDetails", jdetails);




                resultArray.Add(tempObject);

            }
            FinalResult["Data"] = resultArray;


        }
        catch (Exception e)
        {
            FinalResult.Add("Exception_m", e.Message);
            FinalResult.Add("Exception_t", e.StackTrace);
            FinalResult.Add("orderH", orderH);
        }
        return FinalResult;
    }

    public async Task<JObject> GetHoldOrderDetatail()
    {
        JObject orderH = new JObject();
        JObject FinalResult = new JObject();
        //JObject result = new JObject();
        JArray resultArray = new JArray();
        try
        {
            List<string> list = new List<string>();


            orderH = await ExcuteStoredProcedureThatRespondSelectIntoJsonObject("GetHoldOrderData",
                        list,
                        CommandType.StoredProcedure);
            if (orderH["Status"].ToString() == "0")
            {
                return orderH;
            }
            FinalResult = orderH;
            JArray orderHArr = JArray.FromObject(orderH["Data"]);
            foreach (var orh in orderHArr)
            {
                JObject tempObject = new JObject();
                tempObject = JObject.FromObject(orh);
                tempObject.Add("IsFirstVoid", 0);
                string hold_S_Ord_H_Id = tempObject["S_Ord_H_Id"].ToString();
                list = new List<string>();
                list.Add(hold_S_Ord_H_Id);

                JObject GetOrderMemberData = await ExcuteStoredProcedureThatRespondSelectIntoJsonObject("GetHoldOrderMemberData",
                     list,
                     CommandType.StoredProcedure);
                try
                {
                    tempObject.Add("OrderMember", GetOrderMemberData["Data"][0]);
                }
                catch (Exception)
                {
                    tempObject.Add("OrderMember", null);
                }

                JObject GetOrderDetailData = await ExcuteStoredProcedureThatRespondSelectIntoJsonObject("GetHoldOrderDetailData",
                      list,
                      CommandType.StoredProcedure);

                JArray details = new JArray();
                foreach (var OderDetail in GetOrderDetailData["Data"])
                {
                    JObject oDetail = JObject.FromObject(OderDetail);
                    list = new List<string>();
                    list.Add(oDetail["S_Ord_D_Id"].ToString());
                    JObject add = await ExcuteStoredProcedureThatRespondSelectIntoJsonObject("GetHoldOrderAddData",
                         list,
                         CommandType.StoredProcedure);
                    oDetail.Add("OrderAdds", add["Data"]);

                    list = new List<string>();
                    list.Add(oDetail["S_Ord_D_Id"].ToString());
                    list.Add(hold_S_Ord_H_Id);
                    JObject promo = await ExcuteStoredProcedureThatRespondSelectIntoJsonObject("GetHoldOrderPromotionData",
                        list,
                        CommandType.StoredProcedure);

                    try
                    {
                        oDetail.Add("OrderPromotion", promo["Data"][0]);
                    }
                    catch (Exception)
                    {
                        oDetail.Add("OrderPromotion", null);
                    }


                    details.Add(oDetail);
                }

                tempObject.Add("OrderDetails", details);
                resultArray.Add(tempObject);

            }
            FinalResult["Data"] = resultArray;


            //result = JObject.FromObject(orderH["Data"][0]);
            //result.Add("IsFirstVoid", 0);

            //string S_Ord_H_Id = result["S_Ord_H_Id"].ToString();
            //list = new List<string>();
            //list.Add(S_Ord_H_Id);


            //JObject me = await ExcuteStoredProcedureThatRespondSelectIntoJsonObject("GetOrderMemberData",
            //           list,
            //           CommandType.StoredProcedure);
            //try
            //{
            //    result.Add("OrderMember", me["Data"][0]);
            //}
            //catch (Exception)
            //{
            //    result.Add("OrderMember", null);
            //}


            //JObject de = await ExcuteStoredProcedureThatRespondSelectIntoJsonObject("GetOrderDetailData",
            //            list,
            //            CommandType.StoredProcedure);

            //JArray details = new JArray();
            //foreach (var OderDetail in de["Data"])
            //{
            //    JObject oDetail = JObject.FromObject(OderDetail);
            //    list = new List<string>();
            //    list.Add(oDetail["S_Ord_D_Id"].ToString());
            //    JObject add = await ExcuteStoredProcedureThatRespondSelectIntoJsonObject("GetOrderAddData",
            //         list,
            //         CommandType.StoredProcedure);
            //    oDetail.Add("OrderAdds", add["Data"]);

            //    list = new List<string>();
            //    list.Add(oDetail["S_Ord_D_Id"].ToString());
            //    list.Add(S_Ord_H_Id);
            //    JObject promo = await ExcuteStoredProcedureThatRespondSelectIntoJsonObject("GetOrderPromotionData",
            //        list,
            //        CommandType.StoredProcedure);

            //    try
            //    {
            //        oDetail.Add("OrderPromotion", promo["Data"][0]);
            //    }
            //    catch (Exception)
            //    {
            //        oDetail.Add("OrderPromotion", null);
            //    }


            //    details.Add(oDetail);
            //}

            //result.Add("OrderDetails", details);

            //FinalResult["Data"] = result;

        }
        catch (Exception e)
        {
            FinalResult.Add("Exception_m", e.Message);
            FinalResult.Add("Exception_t", e.StackTrace);
            FinalResult.Add("orderH", orderH);
        }
        return FinalResult;
    }
    public async Task<RestResponse> RequestToServer(string url, string target, JObject body = null, int timeout = 9999999)
    {
        //url = "http://mikkaapigetdata.cafemikka.com/";
        //url = "https://apiposbranchau.mmm2007.net/";
        // url = "https://localhost:44393/";


        RestClient client = new RestClient(url);
        //client.ClearHandlers();
        client.AddDefaultHeader("Content-Type", "application/json");
        client.AddDefaultHeader("Accept", "application/json");
        string bcode = EnvironmentConfiguration.GetSingleton().GetBranchCode();

        client.AddDefaultHeader("User-Agent", bcode != "Empty" ? bcode : "NoSetBranch");

        var request = new RestRequest(target, Method.Post);

        request.RequestFormat = DataFormat.Json;
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Accept", "application/json");
        if (timeout != 9999999)
        {
            request.Timeout = TimeSpan.FromMilliseconds(timeout); // ✅ if timeout is in ms
        }
        if (body != null)
        {
            request.AddParameter("application/json", body.ToString(), ParameterType.RequestBody);
        }

        return await client.ExecuteAsync(request);
    }
    public async Task<ResultAPI> RequestToServer(string action, object obj)
    {
        try
        {
            var url = EnvironmentConfiguration.GetSingleton().GetServerSyncURL(true);
            //var url = "";
            //url = "https://apiposbranchau.mmm2007.net/";
            RestClient client = new RestClient(url);
            //client.ClearHandlers();
            client.AddDefaultHeader("Content-Type", "application/json");
            client.AddDefaultHeader("Accept", "application/json");

            RestRequest request = new RestRequest(action, Method.Post);
            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(obj);

            var response = await client.ExecuteAsync(request);
            var result = JsonConvert.DeserializeObject<ResultAPI>(response.Content);
            return result;
        }
        catch (Exception)
        {
            throw;
        }
    }
}

