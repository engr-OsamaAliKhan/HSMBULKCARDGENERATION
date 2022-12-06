using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LumenWorks.Framework.IO.Csv;
using NLog;

namespace GenerationUtility
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private TcpClient client;
        private NetworkStream stream;
        public string txtIP;
        public string txtPort;
        DBExecuter execute = new DBExecuter();
        
        static void Main(string[] args)
        {
            Program pr = new Program();
            pr.Execute();
       

        }
        public void Execute() {
            string dirpath = string.Empty;
            string excelFileName = string.Empty;
            string excelpath = string.Empty;
            string command = string.Empty;
            string execPinoffset = ConfigurationManager.AppSettings["Pinoffset"]; ;
            string execCVV = ConfigurationManager.AppSettings["CVV"]; ;
            string execCVV2 = ConfigurationManager.AppSettings["CVV2"]; ;
            string execICVV = ConfigurationManager.AppSettings["ICVV"]; ;
            string result = string.Empty;
            int i = 0;
            string cvv = string.Empty;
            string cvv2 = string.Empty;
            string icvv = string.Empty;
            string pan = string.Empty;
            string commandoutput = string.Empty;
            
            execute.GenerateConData();
            execute.OpenConnection();
            
            GenrateConData();
            Connect(txtIP,txtPort);
            dirpath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            excelFileName = ConfigurationManager.AppSettings["ExcelFileName"];
            excelpath = dirpath + "\\" + excelFileName;
            var csvTable = new DataTable();
            using (var csvReader = new CsvReader(new StreamReader(System.IO.File.OpenRead(excelpath)), true))
            {
                csvTable.Load(csvReader);
            }
            int rows = csvTable.Rows.Count;
            for (i = 0; i < rows; i++)
            {
                logger.Info("Executing Record No :" + (i + 1));
                if (execPinoffset.Equals("Y"))
                {
                    logger.Info("Generating Pinoffset");
                    command = csvTable.Rows[i][0].ToString();
                    pan = csvTable.Rows[i][2].ToString();
                    logger.Info("Command : " + command);
                    string pinresp = SendRequest(command);
                    logger.Info("Resp : " + pinresp);

                    Thread.Sleep(500);
                    GetResponse(pinresp, pan);



                }

                if (execCVV.Equals("Y"))
                {
                    logger.Info("Generating CVV");
                    command = csvTable.Rows[i][10].ToString();
                    pan = csvTable.Rows[i][2].ToString();

                    logger.Info("Command : " + command);
                    commandoutput = SendRequest(command);
                    Thread.Sleep(500);

                    cvv = commandoutput.Substring(8);
                    logger.Info("Command Output : " + commandoutput);
                    logger.Info("CVV : " + cvv);
                    Thread.Sleep(100);

                }

                if (execCVV2.Equals("Y"))
                {
                    logger.Info("Generating CVV2");
                    command = csvTable.Rows[i][11].ToString();
                    pan = csvTable.Rows[i][2].ToString();


                    logger.Info("Command : " + command);

                    Thread.Sleep(500);

                    commandoutput =SendRequest(command);
                    Thread.Sleep(500);
                    cvv2 = commandoutput.Substring(8);
                    logger.Info("Command Output : " + commandoutput);
                    logger.Info("CVV2 : " + cvv2);
                    Thread.Sleep(100);

                }

                if (execICVV.Equals("Y"))
                {
                    logger.Info("Generating ICVV");
                    command = csvTable.Rows[i][12].ToString();
                    pan = csvTable.Rows[i][2].ToString();

                    logger.Info("Command : " + command);


                    commandoutput = SendRequest(command);
                    Thread.Sleep(500);
                    icvv = commandoutput.Substring(8);
                    logger.Info("Command Output : " + commandoutput);
                    logger.Info("ICVV : " + icvv);
                    Thread.Sleep(100);


                }


                GetResponse1(pan, cvv, cvv2, icvv);
                Thread.Sleep(500);

            }

            logger.Info("Activity Finished");
            execute.CloseConnection();
            Disconnect();
        }

       private void GenrateConData()
        {
            txtIP = ConfigurationManager.AppSettings["UtilIP"];
            logger.Info("Getting IP "+txtIP);
            Console.WriteLine("Getting IP " + txtIP);
            txtPort = ConfigurationManager.AppSettings["UtilPort"];
            logger.Info("Getting Port " + txtPort);
            Console.WriteLine("Getting Port " + txtPort);
        }

        private void Connect(string ip,string port)
        {
            try
            {
                this.client = new TcpClient(ip, int.Parse(port));
                this.stream = this.client.GetStream();
            }
            catch (ArgumentNullException ex)
            {
                logger.Info(ex);
                Console.WriteLine(ex);
            }
            catch (SocketException ex)
            {
                logger.Info(ex);
                Console.WriteLine(ex);
            }
        }

        private void Disconnect()
        {
            try
            {
                this.stream.Close();
                this.client.Close();
            }
            catch (ArgumentNullException ex)
            {
                logger.Info(ex);
                Console.WriteLine(ex);
            }
            catch (SocketException ex)
            {
                logger.Info(ex);
                Console.WriteLine(ex);
            }
        }

        private string SendRequest(string request)
        {
            string txtResponse=string.Empty;
            try
            {
                byte[] bytes = Encoding.ASCII.GetBytes("  " + request);
                bytes[0] = (byte)0;
                bytes[1] = (byte)request.Length;
                this.stream.Write(bytes, 0, bytes.Length);
                byte[] numArray = new byte[256];
                string empty = string.Empty;
                int count = this.stream.Read(numArray, 0, numArray.Length);
                txtResponse = Encoding.ASCII.GetString(numArray, 0, count).Substring(2);
                
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine(ex);
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex);
            }
            return txtResponse;
        }


        public void GetResponse(string offset, string cardno)
        {

            string pinoffset = offset.Substring(8);
            logger.Info("Command Output : " + offset);
            logger.Info("Pin Offset : " + pinoffset);
            //excel.Range  row1 = sheet1.Rows.Cells[row, 4];
            //row1.Value = pinoffset;
            execute.UPDATEOFFSET(cardno, pinoffset);

        }

        public void GetResponse1(string cardno, string scvv, string scvv2, string sicvv)
        {
            execute.InsertCardData(cardno, scvv, scvv2, sicvv);

        }


    }
}
