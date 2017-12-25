using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.Data.SqlClient;


namespace Website_TS
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    

    public class Service3
    {
        private SqlConnection myConnection;
        // To use HTTP GET, add [WebGet] attribute. (Default ResponseFormat is WebMessageFormat.Json)
        // To create an operation that returns XML,
        //     add [WebGet(ResponseFormat=WebMessageFormat.Xml)],
        //     and include the following line in the operation body:
        //         WebOperationContext.Current.OutgoingResponse.ContentType = "text/xml";


        [OperationContract]
        [WebGet(UriTemplate = "/{server}/datalogvalues/{name}/{interval}/{number}", ResponseFormat = WebMessageFormat.Json)]
        public string DataLogValues(string server, string name, string interval, string number)
        {
            myConnection = new SqlConnection("server=" + server + "\\SQLEXPRESS;Trusted_Connection = no;user id=LocalProxy;password=LocalProxy;"); //LAPTOP
            //myConnection = new SqlConnection("server=WENDY-PC\\SQLEXPRESS;Trusted_Connection = yes;"); //SERVER
            myConnection.Open();
            /*     SqlCommand myCommand = new SqlCommand("SELECT TOP ("+ number + ") * FROM[CVLogger].[dbo].["+name+"]"+
                     "WHERE(([ID] - (SELECT MAX(ID) FROM[CVLogger].[dbo].[" + name + "])) % "+ interval + ") = 0" +
                     "ORDER BY Id DESC"
                     , myConnection);*/

            SqlCommand myCommand = new SqlCommand(
                "SELECT * FROM (" +
                "SELECT TOP(" + number + ") * FROM[CVLogger].[dbo].[" + name + "] WHERE(([ID] - (SELECT MAX(ID) FROM[CVLogger].[dbo].[" + name + "])) % " + interval + ") = 0" +
                "ORDER BY Id DESC" +
                ")SQ ORDER BY Id ASC"
               , myConnection);

            SqlDataReader myReader = myCommand.ExecuteReader();
            string valueString = "0";
            string timeString = "0";
            List<OutputTypeValue> output = new List<OutputTypeValue>();
            List<int> output2 = new List<int>();
            while (myReader.Read())
            {
                valueString = (myReader["Value"].ToString());
                timeString = (myReader["DateTime"].ToString());
                output.Add(new OutputTypeValue(valueString, timeString));
                output2.Add(int.Parse(valueString));
            }
            myReader.Close();
            myConnection.Close();

            string result = JsonConvert.SerializeObject(output2);

            return result;
        }



        [OperationContract]
        [WebGet(UriTemplate = "/{server}/datalogcounters/{name}/{interval}/{number}", ResponseFormat = WebMessageFormat.Json)]
        public string DataLogCounters(string server, string name, string interval, string number)
        {
            myConnection = new SqlConnection("server=" + server + "\\SQLEXPRESS;Trusted_Connection = no;user id=LocalProxy;password=LocalProxy;"); //LAPTOP
            //myConnection = new SqlConnection("server=WENDY-PC\\SQLEXPRESS;Trusted_Connection = yes;"); //SERVER
            myConnection.Open();


            SqlCommand myCommand = new SqlCommand(
                "SELECT *, SQ1.Counter - SQ2.PreviousCounter As Difference FROM" +
                "(SELECT TOP(" + number + ") * FROM[CVLogger].[dbo]." + name + " WHERE(([ID] - (SELECT MAX(ID) FROM[CVLogger].[dbo].[" + name + "])) % " + interval + ") = 0" +
                "ORDER BY Id DESC)SQ1 " +
                "INNER JOIN(" +
                "SELECT TOP(" + number + ") ID, Counter As PreviousCounter FROM[CVLogger].[dbo]." + name + " WHERE(([ID] - (SELECT MAX(ID) FROM[CVLogger].[dbo].[" + name + "])) % " + interval + ") = 0" +
                "ORDER BY Id DESC)SQ2 " +
                "ON SQ1.ID = SQ2.ID + " + interval + "" +
                "ORDER BY SQ1.Id ASC"
               , myConnection);

            SqlDataReader myReader = myCommand.ExecuteReader();
            string valueString = "0";
            string timeString = "0";
            List<OutputTypeValue> output = new List<OutputTypeValue>();
            List<int> output2 = new List<int>();
            while (myReader.Read())
            {
                valueString = (myReader["Difference"].ToString());
                timeString = (myReader["DateTime"].ToString());
                output.Add(new OutputTypeValue(valueString, timeString));
                output2.Add(int.Parse(valueString));
            }
            myReader.Close();
            myConnection.Close();

            string result = JsonConvert.SerializeObject(output2);

            return result;
        }

        


        [OperationContract]
        [WebGet(UriTemplate = "/GetService/{serviceString}", ResponseFormat = WebMessageFormat.Json)]
        public int GetService(string serviceString)
        {
            int result = int.Parse(getFromWebService(serviceString.Replace("_", "/").Replace("*","?")));
            return result;
        }


        [OperationContract]
        [WebGet(UriTemplate = "/test/NHCSystemInfo", ResponseFormat = WebMessageFormat.Json)]
        public string NHCSystemInfoTest()
        {
            return "{\"cmd\":\"systeminfo\",\"data\":{\"swversion\":\"1.7.0.30715\",\"api\":\"1.19\",\"time\":\"20170217091038\",\"language\":\"NL\",\"currency\":\"EUR\",\"units\":0,\"DST\":0,\"TZ\":3600,\"lastenergyerase\":\"20000101000001\",\"lastconfig\":\"20160327121701\"}}";
        }


        [OperationContract]
        [WebGet(UriTemplate = "/NHCSystemInfo", ResponseFormat = WebMessageFormat.Json)]
        public string NHCSystemInfo()
        {
            TcpClient clientSocket = new TcpClient();
            string IP = "192.168.0.171";
            int port = 8000;
            clientSocket.Connect(IP, port);

            NetworkStream serverStream = clientSocket.GetStream();
            NHCCommand startCommand = new NHCCommand("startevents");
            string response1 = startCommand.execute(serverStream, clientSocket);
            NHCCommand getActionsCommand = new NHCCommand("systeminfo");
            string response2 = getActionsCommand.execute(serverStream, clientSocket);



            return response2;
        }


        [OperationContract]
        [WebGet(UriTemplate = "/NHCActions", ResponseFormat = WebMessageFormat.Json)]
        public string NHCActions()
        {
            TcpClient clientSocket = new TcpClient();
            string IP = "192.168.0.171";
            int port = 8000;
            clientSocket.Connect(IP, port);
          
            NetworkStream serverStream = clientSocket.GetStream();
            NHCCommand startCommand = new NHCCommand("startevents");
            string response1 = startCommand.execute(serverStream, clientSocket);
            NHCCommand getActionsCommand = new NHCCommand("listactions");
            string response2 = getActionsCommand.execute(serverStream, clientSocket);



            return response2;
            
            
          
        }

        [OperationContract]
        [WebGet(UriTemplate = "/test/NHCActions", ResponseFormat = WebMessageFormat.Json)]
        public string NHCActionsTest()
        {
            return "{\"cmd\":\"listactions\",\"data\":[ {\"id\":5,\"name\":\"Verlichting hal\",\"type\":1,\"location\":3,\"value1\":100},{\"id\":6,\"name\":\"Buitenverlichting West\",\"type\":1,\"location\":3,\"value1\":0},{\"id\":7,\"name\":\"1 - Badkamer\",\"type\":1,\"location\":1,\"value1\":0},{\"id\":8,\"name\":\"1 - Badkamer Leds\",\"type\":1,\"location\":1,\"value1\":0},{\"id\":11,\"name\":\"1 - Hall\",\"type\":1,\"location\":1,\"value1\":0},{\"id\":12,\"name\":\"0 - Keuken\",\"type\":1,\"location\":1,\"value1\":0},{\"id\":19,\"name\":\"0 - Eetplaats - dimmer\",\"type\":2,\"location\":1,\"value1\":0},{\"id\":23,\"name\":\"0 - Keuken Leds\",\"type\":1,\"location\":1,\"value1\":0},{\"id\":24,\"name\":\"0 - WC\",\"type\":1,\"location\":1,\"value1\":0},{\"id\":25,\"name\":\"2 - Hall\",\"type\":1,\"location\":1,\"value1\":0},{\"id\":26,\"name\":\"2 - Badkamer\",\"type\":1,\"location\":1,\"value1\":0},{\"id\":27,\"name\":\"2 - Kinderkamer\",\"type\":1,\"location\":1,\"value1\":0},{\"id\":28,\"name\":\"0 - Salon Dimmer\",\"type\":2,\"location\":1,\"value1\":60},{\"id\":33,\"name\":\"0 - sfeer 1 leefruimte\",\"type\":1,\"location\":1,\"value1\":0},{\"id\":36,\"name\":\"1 - Slaapkamer\",\"type\":1,\"location\":1,\"value1\":0},{\"id\":35,\"name\":\"1 - Slaapkamer - dimmer\",\"type\":2,\"location\":1,\"value1\":0},{\"id\":41,\"name\":\"0 - _ Alles Uit\",\"type\":1,\"location\":1,\"value1\":100},{\"id\":42,\"name\":\"Poort\",\"type\":1,\"location\":3,\"value1\":0},{\"id\":48,\"name\":\"Poort open & sluiten\",\"type\":1,\"location\":3,\"value1\":0},{\"id\":55,\"name\":\"Verlichting houtkot\",\"type\":1,\"location\":3,\"value1\":0}]}";
        }

        [OperationContract]
        [WebGet(UriTemplate = "/NHCSet/{id}/{value}", ResponseFormat = WebMessageFormat.Json)]
        public string NHCSet(string id, string value)
        {


                      TcpClient clientSocket = new TcpClient();
                      string IP = "192.168.0.171";
                      int port = 8000;
                      clientSocket.Connect(IP, port);

                      NetworkStream serverStream = clientSocket.GetStream();
                      NHCCommand startCommand = new NHCCommand("startevents");
                      string response1 = startCommand.execute(serverStream, clientSocket);
                      NHCCommandAdvanced setActionsCommand = new NHCCommandAdvanced("executeactions",id,value);
                      string response2 = setActionsCommand.execute(serverStream, clientSocket);

                      serverStream.Close();
                      clientSocket.Close();

                      return response2;
                      
                     

        }

        [OperationContract]
        [WebGet(UriTemplate = "test/NHCSet/{id}/{value}", ResponseFormat = WebMessageFormat.Json)]
        public string NHCSetTest(string id, string value)
        {
            return "{\"cmd\":\"listactions\",\"data\":[ {\"id\":5,\"name\":\"Verlichting hal\",\"type\":1,\"location\":3,\"value1\":100},{\"id\":6,\"name\":\"Buitenverlichting West\",\"type\":1,\"location\":3,\"value1\":0},{\"id\":7,\"name\":\"1 - Badkamer\",\"type\":1,\"location\":1,\"value1\":0},{\"id\":8,\"name\":\"1 - Badkamer Leds\",\"type\":1,\"location\":1,\"value1\":0},{\"id\":11,\"name\":\"1 - Hall\",\"type\":1,\"location\":1,\"value1\":0},{\"id\":12,\"name\":\"0 - Keuken\",\"type\":1,\"location\":1,\"value1\":0},{\"id\":19,\"name\":\"0 - Eetplaats - dimmer\",\"type\":2,\"location\":1,\"value1\":0},{\"id\":23,\"name\":\"0 - Keuken Leds\",\"type\":1,\"location\":1,\"value1\":0},{\"id\":24,\"name\":\"0 - WC\",\"type\":1,\"location\":1,\"value1\":0},{\"id\":25,\"name\":\"2 - Hall\",\"type\":1,\"location\":1,\"value1\":0},{\"id\":26,\"name\":\"2 - Badkamer\",\"type\":1,\"location\":1,\"value1\":0},{\"id\":27,\"name\":\"2 - Kinderkamer\",\"type\":1,\"location\":1,\"value1\":0},{\"id\":28,\"name\":\"0 - Salon Dimmer\",\"type\":2,\"location\":1,\"value1\":0},{\"id\":33,\"name\":\"0 - sfeer 1 leefruimte\",\"type\":1,\"location\":1,\"value1\":0},{\"id\":36,\"name\":\"1 - Slaapkamer\",\"type\":1,\"location\":1,\"value1\":0},{\"id\":35,\"name\":\"1 - Slaapkamer - dimmer\",\"type\":2,\"location\":1,\"value1\":0},{\"id\":41,\"name\":\"0 - _ Alles Uit\",\"type\":1,\"location\":1,\"value1\":100},{\"id\":42,\"name\":\"Poort\",\"type\":1,\"location\":3,\"value1\":0},{\"id\":48,\"name\":\"Poort open & sluiten\",\"type\":1,\"location\":3,\"value1\":0},{\"id\":55,\"name\":\"Verlichting houtkot\",\"type\":1,\"location\":3,\"value1\":0}]}";

        }





        public string getFromWebService(string url)
        {
            string result = string.Empty;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                result = reader.ReadToEnd();
            }
             return result;
        }




    }

    public class NHCCommand
    {
        public string cmd;


        public NHCCommand(string _command)
        {
            cmd = _command;

        }

        public string execute(NetworkStream serverStream, TcpClient clientSocket)
        {
          
            string sendString = JsonConvert.SerializeObject(this);
           
            byte[] outStream = Encoding.ASCII.GetBytes(sendString /*+ "$"*/);
            serverStream.Write(outStream, 0, outStream.Length);

            serverStream.Flush();
            byte[] inStream = new byte[100250];
            serverStream.Read(inStream, 0, (int)clientSocket.ReceiveBufferSize);

            int i = inStream.Length - 1;
    while (inStream[i] == 0)
        --i;

    byte[] result = new byte[i + 1];
    Array.Copy(inStream, result, i + 1);




            string resultString = Encoding.ASCII.GetString(result);
            return resultString;
        }
    }

    public class NHCCommandAdvanced
    {
        public string cmd;
        public int id;
        public int value1;

        public NHCCommandAdvanced(string _command, string _id, string _value)
        {
            cmd = _command;
            id = int.Parse(_id);
            value1 = int.Parse(_value);
        }

        public string execute(NetworkStream serverStream, TcpClient clientSocket)
        {

            string sendString = JsonConvert.SerializeObject(this);

            byte[] outStream = Encoding.ASCII.GetBytes(sendString /*+ "$"*/);
            serverStream.Write(outStream, 0, outStream.Length);

            serverStream.Flush();
            byte[] inStream = new byte[100250];
            serverStream.Read(inStream, 0, (int)clientSocket.ReceiveBufferSize);

         /*   int i = inStream.Length - 1;
            while (inStream[i] == 0)
                --i;

            byte[] result = new byte[i + 1];
            Array.Copy(inStream, result, i + 1);
            */


            string resultString = Encoding.ASCII.GetString(inStream);
            return resultString;
        }




    }

    public class OutputTypeValue
    {
        public int value { get; set; }
        public string time { get; set; }
        public OutputTypeValue(string v, string t)
        {
            value = int.Parse(v);
            time = t;
        }
    }



}
