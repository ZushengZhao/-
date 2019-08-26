using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Windows.Forms;
using System.Security.Cryptography;
using Command;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace WebApplication1.Controllers
{
    public class agvController : ApiController
    {
        [DllImport("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize")]
        public static extern int SetProcessWorkingSetSize(IntPtr process, int minSize, int maxSize);
        private object obj = new object();
        private static DataBaseManager DataBase;
        public  DataBaseManager sql = new DataBaseManager();
        //数据库地址和账户
        //static string account = File.ReadAllText(@"account.txt");
        //static string account = "Data Source =192.168.0.177;Initial Catalog = Plugins; User=sa; Password=zu19960818";
        static StreamReader ac = new StreamReader("account.txt");
        static string account = ac.ReadLine();
        public static string strName { get; set; }
        string taskCode; //任务编号
        string reqTime;//任务时间
        string method;//每一步任务状态
        string robotCode;//AGVID

        private static void InitialMember()
        {
            DataBase = new DataBaseManager();
            DataBase.SetConnection(account);
        }
       
        // GET: api/agv
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/agv/5
        [HttpGet]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/agv
       
        public object agvCallbackService(dynamic value)
        {
            ClearMemory();
            InitialMember();
            strName = Convert.ToString(value);

           // MessageBox.Show("测试"+value);//测试接收到的信息
         
            var jObject = JObject.Parse(strName);
            taskCode = jObject["taskCode"].ToString();//获取reqCode 
            reqTime = jObject["reqTime"].ToString();
            method = jObject["method"].ToString();
            robotCode = jObject["robotCode"].ToString();

             //MessageBox.Show("测试" + "   taskCode:"+taskCode+ "   reqTime:" + reqTime+ "   method:" + method+ "   robotCode：" + robotCode );//测试从接收到的信息截取我们想要的信息
            string logstring = "测试" + "   taskID:" + taskCode + "   reqTime:" + reqTime + "   UnitStatus:" + method + "   AGVID：" + robotCode;

            lock (obj)
            {
                AddLgoToTXT(logstring);
            }

            int methodstatus = Convert.ToInt32(method);
            DateTime time = DateTime.Now;  
            string time_ = Convert.ToString(time);

            switch (methodstatus)
            {              
                case 2:
                    {
                        for (int i = 0; i < 10; i++)
                        {
                           lock (obj)
                            { 
                                int n = DataBase.SqlServerCommand(string.Format(" UPDATE [Plugins].[dbo].[Command]  SET Time2= '{0}',UnitStatus='{1}',AGVID='{2}',Readsta=0 WHERE taskid='{3}'", reqTime, methodstatus, robotCode, taskCode));

                                if (n == 1)
                                {
                                    break;//数据库更新成功
                                 }
                           
                            if (i == 9)
                              {
                                MessageBox.Show("数据库写入失败，请联系系统工程师，数据库写入行数n: "+n);
                                lock (obj)
                                {
                                    string e = "数据库写入失败 " + methodstatus + "   taskID:" + taskCode + "   RCS请求时间:" + reqTime + "   系统中断时间:" + time_ + "   AGVID：" + robotCode;
                                    AddLgoToTXT_error(e);
                                }
                                //数据库更新失败
                              }
                            }
                            Thread.Sleep(1000);
                        }
                        break;
                    }
                case 3:
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            //n = DataBase.SqlServerCommand(string.Format("update Command set UnitStatus = '{0}'", taskstatus));
                            lock (obj)
                            {
                                int n = DataBase.SqlServerCommand(string.Format(" UPDATE [Plugins].[dbo].[Command]  SET Time3= '{0}',UnitStatus='{1}',AGVID='{2}',Readsta=0 WHERE taskid='{3}'", reqTime, methodstatus, robotCode, taskCode));

                                if (n == 1)
                                {
                                    break;//数据库更新成功
                                }
                            
                            if (i == 9)
                            {
                                    MessageBox.Show("数据库写入失败，请联系系统工程师，数据库写入行数n: " + n);
                                    lock (obj)
                                {
                                    string e = "数据库写入失败 " + methodstatus + "   taskID:" + taskCode + "   RCS请求时间:" + reqTime + "   系统中断时间:" + time_ + "   AGVID：" + robotCode ;
                                    AddLgoToTXT_error(e);
                                }
                                //数据库更新失败
                            }
                            }
                            Thread.Sleep(1000);
                        }
                        break;
                    }
                case 4:
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            lock (obj)
                            {
                                int n = DataBase.SqlServerCommand(string.Format(" UPDATE [Plugins].[dbo].[Command]  SET Time4= '{0}',UnitStatus='{1}',AGVID='{2}',Readsta=0 WHERE taskid='{3}'", reqTime, methodstatus, robotCode, taskCode));

                                if (n == 1)
                                {
                                    break;//数据库更新成功
                                }
                            
                            if (i == 9)
                            {
                                    MessageBox.Show("数据库写入失败，请联系系统工程师，数据库写入行数n: " + n);
                                    lock (obj)
                                {
                                    string e = "数据库写入失败 " + methodstatus + "   taskID:" + taskCode + "   RCS请求时间:" + reqTime + "   系统中断时间:" + time_ + "   AGVID：" + robotCode;
                                    AddLgoToTXT_error(e);
                                }
                                //数据库更新失败
                            }
                            Thread.Sleep(1000);
                        }
                        }
                        break;
                    }
                case -1:
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            lock (obj)
                            {
                                int n = DataBase.SqlServerCommand(string.Format(" UPDATE [Plugins].[dbo].[Command]  SET Finishtime= '{0}',UnitStatus='{1}',AGVID='{2}',Readsta=0 WHERE taskid='{3}'", reqTime, methodstatus, robotCode, taskCode));

                                if (n == 1)
                                {
                                    break;//数据库更新成功
                                }                         
                            if (i == 9)
                            {
                                    MessageBox.Show("数据库写入失败，请联系系统工程师，数据库写入行数n: " + n);
                                    lock (obj)
                                {
                                    string e = "数据库写入失败 " + methodstatus + "   taskID:" + taskCode + "   RCS请求时间:" + reqTime + "   系统中断时间:" + time_ + "   AGVID：" + robotCode;
                                    AddLgoToTXT_error(e);
                                }
                                //数据库更新失败
                            }
                            Thread.Sleep(1000);
                        }
                        }
                        break;
                    }
                default:
                    DataBase.SqlServerCommand(string.Format(" UPDATE [Plugins].[dbo].[unit]     SET unitstatus=-1  WHERE UnitName='{0}'", taskCode));
                    DataBase.SqlServerCommand(string.Format(" UPDATE [Plugins].[dbo].[Command]  SET unitstatus=-1  WHERE UnitName='{0}'", taskCode));
                    MessageBox.Show("RCS故障 , 请检查agv状态码报文： " + methodstatus);
                    lock(obj)
                    {
                    string error = "!!!error!!!  RCS故障 , 请检查agv状态码报文:  " + methodstatus + "   taskID:" + taskCode + "   RCS请求时间:" + reqTime + "   系统中断时间:" + time_ + "   AGVID：" + robotCode ;                   
                    AddLgoToTXT_error(error);
                    }
                    break;
            }

            string s = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            byte[] randomBytes = new byte[4];
            RNGCryptoServiceProvider rngServiceProvider = new RNGCryptoServiceProvider();
            rngServiceProvider.GetBytes(randomBytes);
            Int32 result = BitConverter.ToInt32(randomBytes, 0);
            string r = Convert.ToString(result);
            string reqCode = s + r;//生成基于时间的ms真随机数
            string returnedValue = "{\"code\":\"0\",\"message\":\"成功\",\"reqCode\":\""+ reqCode + "\"}";//reqCode改成我们的流水号
            dynamic domDetailObj = Newtonsoft.Json.JsonConvert.DeserializeObject(returnedValue);//字符串转json
            return domDetailObj;
     
        }
        /// <summary>
        /// 系统日志写入函数，日志名operalog.txt
        /// </summary>
        /// <param name="logstring"></param>
        public static void AddLgoToTXT(string logstring)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "operalog.txt";
            if (!System.IO.File.Exists(path))
            {
                FileStream stream = System.IO.File.Create(path);
                stream.Close();
                stream.Dispose();
            }
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(logstring);
            }
        }
        /// <summary>
        /// 系统日志写入函数，日志名errorlog.txt
        /// </summary>
        /// <param name="logstring_e"></param>
        public static void AddLgoToTXT_error(string logstring_e)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "errorlog.txt";
            if (!System.IO.File.Exists(path))
            {
                FileStream stream_e = System.IO.File.Create(path);
                stream_e.Close();
                stream_e.Dispose();
            }
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(logstring_e);
            }
        }

        // PUT: api/agv/5
        [HttpPut]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/agv/5
        [HttpDelete]
        public void Delete(int id)
        {
        }
        /// <summary>
        /// 释放内存
        /// </summary>
        public static void ClearMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
            }
        }

    }
}
