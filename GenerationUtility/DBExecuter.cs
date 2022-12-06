using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using Oracle.ManagedDataAccess.Client;

namespace GenerationUtility
{
    class DBExecuter
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        string columnval = string.Empty;
        string columnname = string.Empty;
        string dbcon = string.Empty;

        OracleConnection conn;
        string dbip = string.Empty;
        string dBPort = string.Empty;
        string dataSource = string.Empty;
        string dbUserId = string.Empty;
        string dbPassword = string.Empty;

        public void GenerateConData()
        {

            dbip = ConfigurationManager.AppSettings["DBIP"];
            logger.Info(dbip);
            dBPort = ConfigurationManager.AppSettings["DBPort"];
            logger.Info(dBPort);
            dataSource = ConfigurationManager.AppSettings["DBSid"];
            logger.Info(dataSource);
            dbUserId = ConfigurationManager.AppSettings["DBUserName"];
            logger.Info(dbUserId);
            dbPassword = ConfigurationManager.AppSettings["DBPassword"];
            logger.Info(dbPassword);

        }


        public void GenerateConnectionString()
        {
            try
            {
                logger.Info("DB credentials : " + dbip + " " + dBPort + " " + dataSource + " " + dbUserId + " " + dbPassword);
                dbcon = "Data Source = (DESCRIPTION = " + "(ADDRESS = (PROTOCOL = TCP)(HOST = " + dbip + ")(PORT = " + dBPort + "))" +
                                     "(CONNECT_DATA = " +
                                     "(SERVER = DEDICATED)" +
                                     "(SERVICE_NAME = " + dataSource + " )" +
                                     ")" +
                                     ");User Id = " + dbUserId + "; Password = " + dbPassword + ";";
                logger.Info("Connection String : " + dbcon);

                conn = new OracleConnection(dbcon);
            }catch(Exception e)
            {
                logger.Info(e);
            }


        }


        // OracleConnection conn = new OracleConnection(ConfigurationManager.ConnectionStrings["con"].ConnectionString);
        public void OpenConnection()
        {
            GenerateConnectionString();

            try
            {
                conn.Open();
            }
            catch (Exception e)
            {
                logger.Info("Issue while Connecting Database : " + e);
            }

        }

        public void CloseConnection()
        {
            conn.Close();
        }


        public void UPDATEOFFSET(string cardno, string pin)
        {
            try
            {
                OracleCommand cmd1 = conn.CreateCommand();
                cmd1.CommandText = "PKGPINOFFSET.spUpdatePinOffset";
                cmd1.CommandType = System.Data.CommandType.StoredProcedure;
                cmd1.Parameters.Add("inCardno", OracleDbType.Varchar2).Value = cardno;
                cmd1.Parameters.Add("inPIN", OracleDbType.Varchar2).Value = pin;
                //hp = $"update TBLOTP set OTP='FB13D6CC7ED7B16E77C4B0F42256D284' where RELATIONSHIP_ID like'{cardno}%'";
                //OracleCommand cmd1 = new OracleCommand(hp, conn);
                OracleDataReader rd1 = cmd1.ExecuteReader();
                logger.Info("Offset Updated : ");
                //hp = $"commit";
                //OracleCommand cmd2 = new OracleCommand(hp, conn);
                //OracleDataReader rd2 = cmd1.ExecuteReader();

            }
            catch (Exception ex)
            {
                logger.Info("No Data Found Against Requested Card "+ex);
            }
        }

        public void InsertCardData(string cardno, string cvv, string cvv2, string icvv)
        {
            try
            {

                OracleCommand cmd1 = conn.CreateCommand();
                cmd1.CommandText = "PKGPINOFFSET.spUpdate_CVV_ICVV_CVV2";
                cmd1.CommandType = System.Data.CommandType.StoredProcedure;
                cmd1.Parameters.Add("inCardno", OracleDbType.Varchar2).Value = cardno;
                cmd1.Parameters.Add("inCVV", OracleDbType.Varchar2).Value = cvv;
                cmd1.Parameters.Add("inICVV", OracleDbType.Varchar2).Value = icvv;
                cmd1.Parameters.Add("inCVV2", OracleDbType.Varchar2).Value = cvv2;
                //hp = $"update TBLOTP set OTP='FB13D6CC7ED7B16E77C4B0F42256D284' where RELATIONSHIP_ID like'{cardno}%'";
                //OracleCommand cmd1 = new OracleCommand(hp, conn);
                OracleDataReader rd1 = cmd1.ExecuteReader();
                logger.Info("Row Inserted Sucessfully : ");
                //hp = $"commit";
                //OracleCommand cmd2 = new OracleCommand(hp, conn);
                //OracleDataReader rd2 = cmd1.ExecuteReader();

            }
            catch(Exception ex)
            {
                logger.Info("No Data Inserted Against Requested Card " +ex);
            }
        }


    }
}
