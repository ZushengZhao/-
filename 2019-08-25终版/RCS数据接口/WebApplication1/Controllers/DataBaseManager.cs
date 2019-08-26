    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Data.SqlClient;
using System.Collections;
using System.Data;

namespace Command
{
    public class DataBaseManager
    {
        private string _Connection = "";
        private bool _Connected = false;
        private bool _Ready = false;

        public string Connection { get { return _Connection; } }
        public bool Connected { get { return _Connected; } }
        public bool Ready { get { return _Ready; } }

        private EventArgs e = new EventArgs();
        public event EventHandler ConnectedChanged;
        public event EventHandler ReadyChanged;

        Thread TestConnectionThread;
        bool TestConnectionBool = false;

        public DataBaseManager() 
        {
            
        }

        public bool SetConnection(string ConnectionString)
        {
            if (!string.IsNullOrEmpty(ConnectionString) && Connection != ConnectionString)
            {
                _Connection = ConnectionString;
                TestConnection();
                return true;
            }
            return false;
        }
        /// <summary>
        /// 开启新线程，在线程中调用()函数，对数据库进行连接测试，并不断刷新mConnected值
        /// </summary>
        private void TestConnection()
        {
            TestConnectionThread = new Thread(TestConnectionHandler);
            TestConnectionBool = true;
            TestConnectionThread.IsBackground = true;
            TestConnectionThread.Start();
        }
        /// <summary>
        /// 创建在线程中使用的函数
        /// 通过SqlConnection.open检查数据库连接是否正常，open()打开正常，mConnected=true，同时检查数据库是否存在command表，若不存在，根据CreateCommandTable()函数创建command表
        /// </summary>
        private void TestConnectionHandler()
        {
            CreateDataBase("Plugins");
            using (SqlConnection objConnection = new SqlConnection(Connection))
            {
                mConnected = false;
                try
                {
                    objConnection.Open();
                    mConnected = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + " @" + this.GetType().FullName + "-ConnectionTestTask");
                }
            }
            if (Connected)
            {
                CreateCommandTable();
            }
            mReady = true;
        }
        /// <summary>
        /// 与TestConnectionHandler基本相同，通过SqlConnection.open检查数据库连接是否正常，open()打开正常，mConnected=true
        /// </summary>
        public void TestConnected()
        {
            try
            {
                using (SqlConnection objConnection = new SqlConnection(Connection))
                {
                    mConnected = false;
                    try
                    {
                        objConnection.Open();
                        mConnected = true;
                    }
                    catch (Exception ex)
                    {
                        mConnected = false;
                        Console.WriteLine(ex.Message + " @" + this.GetType().FullName + "-ConnectionTestTask");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " @" + this.GetType().FullName + "-ConnectionTestTask");
            }
        }
        /// <summary>
        /// 检查数据库是否存在command表，若不存在，按照要求新建command表
        /// </summary>
        private void CreateCommandTable()
        {
            if (!ExistTable("Command"))
            {
                CreateTable("Command", @"create table Command(id int primary key identity(1,1),
                                                 station1 int null,station2 int null,station3 int null,station4 int null,station5 int null,
                                                 status int default(0),AGVID int default(0),DB_Call varchar(1000) null,IssuedTime datetime null,
                                                 FinishTime datetime2(7) null,remark varchar(50) null,keyid varchar(50) null,TasksManagerID int null,
                                                 CurrentTarget int null,Mode int null,LandMark1 int null,LandMark2 int null,LandMark3 int null,
                                                 LandMark4 int null,LandMark5 int null,PRI int null,Goods varchar(50) null)");
            }
        }

        private bool mConnected
        {
            get
            {
                return _Connected;
            }
            set
            {
                if (_Connected != value)
                {
                    _Connected = value;

                    if (ConnectedChanged != null)
                    {
                        ConnectedChanged(this, e);
                    }

                    if (!Connected)
                    {
                        mReady = false;
                    }
                }
            }
        }

        private bool mReady
        {
            get
            {
                return _Ready;
            }
            set
            {
                if (_Ready != value)
                {
                    _Ready = value;

                    if (ReadyChanged != null)
                    {
                        ReadyChanged(this, e);
                    }
                }
            }
        }

        #region SQL常用方法
        /// <summary>
        /// 清空Table
        /// </summary>
        /// <param name="tableName"></param>
        public void TableClear(string tableName)
        {
            if (!Connected) return;

            string sql = string.Format(@"delete [{0}]", tableName);
            SqlServerCommand(sql);
        }

        /// <summary>
        /// 清空Table
        /// </summary>
        /// <param name="tableName"></param>
        public void TableTruncate(string tableName)
        {
            if (!Connected) return;

            string sql = string.Format(@"truncate table [{0}]", tableName);
            SqlServerCommand(sql);
        }

        /// <summary>
        /// 判断表是否存在
        /// </summary>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public bool TableExist(string TableName)
        {
            bool output = false;

            if (!Connected) return false;

            string sql = string.Format(@"if not exists (select * from sys.objects 
                                        where object_id = OBJECT_ID(N'[dbo].[{0}]') 
                                        and type in (N'U')) 
                                        select 0
                                        else
                                        select 1
                                        go", TableName);

            object o = SqlServerScalar(sql);

            int nn;
            if (o != null && int.TryParse(o.ToString(), out nn))
            {
                if (nn == 1)
                    output = true;
                else
                    output = false;
            }

            return output;
        }
        #endregion

        #region SQL底层语句
        /// <summary>
        /// 创建数据库
        /// </summary>
        /// <param name="databasename">数据库名称</param>
        public void CreateDataBase(string databasename)
        {
            if (!ExistDataBase(databasename))
            {
                string creadatabasestring = string.Format(@"CREATE DATABASE {0} ON PRIMARY"
                       + @"(name=test_data, filename = 'C:\Program Files (x86)\Microsoft SQL Server\MSSQL10_50.MSSQLSERVER\MSSQL\DATA\{1}_data.mdf', size=3,"
                       + "maxsize=5, filegrowth=10%) log on"
                       + @"(name=mydbb_log, filename='C:\Program Files (x86)\Microsoft SQL Server\MSSQL10_50.MSSQLSERVER\MSSQL\DATA\{2}_log.ldf',size=3,"
                       + "maxsize=20,filegrowth=1)", databasename, databasename, databasename);
                //SqlServerCommand(createtablestring);
                 using (SqlConnection sqlconn = new SqlConnection("Data Source=.;Integrated Security=True"))
                 {
                     using (SqlCommand sqlcomm = new SqlCommand(creadatabasestring, sqlconn))
                     {
                         try
                         {
                             sqlconn.Open();
                             sqlcomm.ExecuteNonQuery();
                         }
                         catch
                         {
                         }
                     }
                 }
            }
        }
        /// <summary>
        /// 判断数据库是否存在
        /// </summary>
        /// <param name="databasename">数据库名称</param>
        /// <returns></returns>
        public bool ExistDataBase(string databasename)
        {
            string connstring = "Data Source=.;Initial Catalog = master;Integrated Security=True";
            string tablestring = string.Format("select * from sys.databases where name='{0}'", databasename);
            using (SqlConnection sqlconn = new SqlConnection(connstring))
            {
                using (SqlCommand sqlcomm = new SqlCommand(tablestring, sqlconn))
                {
                    try
                    {
                        sqlconn.Open();
                        if (sqlcomm.ExecuteReader().HasRows)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
        }
        /// <summary>
        /// 创建数据表
        /// </summary>
        /// <param name="tablename">数据表名称</param>
        public bool CreateTable(string tablename,string sqlstring)
        {
            if (!ExistTable(tablename))
            {
                SqlServerCommand(sqlstring);
            }
            return ExistTable(tablename);
        }
        /// <summary>
        /// 判断数据表是否存在
        /// </summary>
        /// <param name="tablename">数据表名称</param>
        /// <returns></returns>
        public bool ExistTable(string tablename)
        {
            string sqlstring = string.Format("select * from sys.tables where name='{0}'", tablename);
            DataTable Table = DataTableReader(sqlstring);
            if (Table.Rows.Count>0)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 删除表格
        /// </summary>
        /// <param name="tablename">表名</param>
        /// <returns></returns>
        public bool DropTable(string tablename)
        {
            string sql = string.Format("drop table {0}", tablename);
            SqlServerCommand(sql);
            if (ExistTable(tablename))
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 执行语句，并返回受影响的行数。
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public int SqlServerCommand(string sql)
        {
            int n = 0;
            if (Connected)
            {
                using (SqlConnection objConnection = new SqlConnection(Connection))
                {
                    using (SqlCommand sqlcmd = new SqlCommand(sql, objConnection))
                    {
                        try
                        {
                            objConnection.Open();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message + " @" + this.GetType().FullName + "-SqlServerCommand");
                        }

                        try
                        {
                            n = sqlcmd.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            
                            Console.WriteLine(ex.Message + " @" + this.GetType().FullName + "-SqlServerCommand");
                        }
                    }
                }
            }
            return n;
        }

        /// <summary>
        /// 执行语句，并返回结果的第一个值。
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public object SqlServerScalar(string sql)
        {
            object obj = null;
            if (Connected)
            {
                using (SqlConnection objConnection = new SqlConnection(Connection))
                {
                    using (SqlCommand sqlcmd = new SqlCommand(sql, objConnection))
                    {
                        try
                        {
                            objConnection.Open();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message + " @" + this.GetType().FullName + "-SqlServerScalar");
                        }

                        try
                        {
                            obj = sqlcmd.ExecuteScalar();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message + " @" + this.GetType().FullName + "-SqlServerScalar");
                        }
                    }
                }
            }
            return obj;
        }


        /// <summary>
        /// 执行语句，并返回全部结果
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public DataTable DataTableReader(string sql)
        {
            SqlDataReader Reader;

           DataTable list = new DataTable();
            //if (Connected)
            {
                using (SqlConnection objConnection = new SqlConnection(Connection))
                {
                    using (SqlCommand sqlcmd = new SqlCommand(sql, objConnection))
                    {
                        try
                        {
                            objConnection.Open();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message + " @" + this.GetType().FullName + "- SqlServerReader");
                        }

                        try
                        {
                            Reader = sqlcmd.ExecuteReader();
                      //      Reader.Read();

                           list.Load(Reader);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message + " @" + this.GetType().FullName + "- SqlServerReader");
                        }
                    }
                }
            }
            return list;
        }
        #endregion

        /// <summary>
        /// 执行语句，并返回全部结果
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public List<ArrayList> SqlServerReader(string sql)
        {
            SqlDataReader Reader;

            List<ArrayList> list = new List<ArrayList>();
            if (Connected)
            {
                using (SqlConnection objConnection = new SqlConnection(Connection))
                {
                    using (SqlCommand sqlcmd = new SqlCommand(sql, objConnection))
                    {
                        try
                        {
                            objConnection.Open();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message + " @" + this.GetType().FullName + "- SqlServerReader");
                        }

                        try
                        {
                            Reader = sqlcmd.ExecuteReader();

                            while (Reader.Read())
                            {
                                ArrayList sublist = new ArrayList();
                                int n = Reader.FieldCount;

                                for (int i = 0; i < n; i++)
                                {
                                    sublist.Add(Reader[i]);
                                }

                                list.Add(sublist);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message + " @" + this.GetType().FullName + "- SqlServerReader");
                        }
                    }
                }
            }
            return list;
        }
      

        #region CallMater数据库
        //表名
        private string TableJobs = "Jobs";
        private string TableTasks = "Tasks";
        private string TableSteps = "Steps";
        private string TablePoints = "Points";

        #region 创建新表
        /// <summary>
        /// 创建所有表
        /// </summary>
        private void CreateTables()
        {
            if (!Connected) return;

            //CreateTableJobs();
            //CreateTableTasks();      
            //CreateTableSteps();
            //CreateTablePoints();
        }
        /// <summary>
        /// 创建呼叫点表
        /// </summary>
        private void CreateTablePoints()
        {
            if (!TableExist(TablePoints))
            {
                string sql = "";
                sql = @"CREATE TABLE [dbo].[Points]
                        (
                            [PointID] [int] NOT NULL,
                            [Name] [nvarchar](max) NOT NULL,
                            [LandMarks] [nvarchar](max) NOT NULL,
                            [CallType] [int] NOT NULL,
                            CONSTRAINT [PK_Points] PRIMARY KEY CLUSTERED 
                            (
                                [PointID] ASC
                            )
                            WITH 
                            (
                                PAD_INDEX  = OFF, 
                                STATISTICS_NORECOMPUTE  = OFF, 
                                IGNORE_DUP_KEY = OFF, 
                                ALLOW_ROW_LOCKS  = ON, 
                                ALLOW_PAGE_LOCKS  = ON
                            ) 
                            ON [PRIMARY]
                        ) 
                        ON [PRIMARY]";
                SqlServerCommand(sql);
            }
        }
        /// <summary>
        /// 创建步骤表
        /// </summary>
        private void CreateTableSteps()
        {
            if (!TableExist(TableSteps))
            {
                string sql = "";
                sql = @"CREATE TABLE [dbo].[Steps](
                        [StepID] [int] IDENTITY(1,1) NOT NULL,
                        [TaskID] [int] NOT NULL,
                        [StepType] [int] NOT NULL,
                        [StepParameter] [int] NOT NULL,
                        [Name] [nvarchar](max) NULL,
                        [StepOrder] [int] NOT NULL,
                        [StepStartTime] [datetime] NULL,
                        [StepEndTime] [datetime] NULL,
                        [StepAGV] [int] NULL,
                        [StepStatus] [int] NOT NULL,
                        CONSTRAINT [PK_Steps] PRIMARY KEY CLUSTERED 
                        (
                        [StepID] ASC
                        )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
                        ) ON [PRIMARY]";
                SqlServerCommand(sql);

                sql = @"EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'None（0），Run（1），Stop（2），Target（3），Velocity（4），WaitRun（11），WaitStop（12），WaitTarget（13），WaitVelocity（14），Close（-1）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Steps', @level2type=N'COLUMN',@level2name=N'StepType'";
                SqlServerCommand(sql);

                sql = @"ALTER TABLE [dbo].[Steps]  WITH CHECK ADD  CONSTRAINT [FK_Steps_Tasks] FOREIGN KEY([TaskID]) REFERENCES [dbo].[Tasks] ([TaskID]) ON UPDATE CASCADE";
                SqlServerCommand(sql);

                sql = @"ALTER TABLE [dbo].[Steps] CHECK CONSTRAINT [FK_Steps_Tasks]";
                SqlServerCommand(sql);

                sql = @"ALTER TABLE [dbo].[Steps] ADD  CONSTRAINT [DF_Steps_StepType]  DEFAULT ((0)) FOR [StepType]";
                SqlServerCommand(sql);

                sql = @"ALTER TABLE [dbo].[Steps] ADD  CONSTRAINT [DF_Steps_StepParameter]  DEFAULT ((0)) FOR [StepParameter]";
                SqlServerCommand(sql);

                sql = @"ALTER TABLE [dbo].[Steps] ADD  CONSTRAINT [DF_Steps_StepOrder]  DEFAULT ((0)) FOR [StepOrder]";
                SqlServerCommand(sql);

                sql = @"ALTER TABLE [dbo].[Steps] ADD  CONSTRAINT [DF_Steps_StepStatus]  DEFAULT ((0)) FOR [StepStatus]";
                SqlServerCommand(sql);
            }
        }
        /// <summary>
        /// 创建任务表
        /// </summary>
        private void CreateTableTasks()
        {
            if (!TableExist(TableTasks))
            {
                string sql = "";
                sql = @"CREATE TABLE [dbo].[Tasks](
                        [TaskID] [int] IDENTITY(1,1) NOT NULL,
                        [JobID] [int] NOT NULL,
                        [Name] [nvarchar](max) NULL,
                        [PointID] [int] NOT NULL,
                        [TaskOrder] [int] NOT NULL,
                        [TaskExecute] [int] NOT NULL,
                        [TaskStartTime] [datetime] NULL,
                        [TaskEndTime] [datetime] NULL,
                        [TaskAGV] [int] NULL,
                        [TaskResult] [int] NOT NULL,
                        [TaskStatus] [int] NOT NULL,
                        CONSTRAINT [PK_Tasks] PRIMARY KEY CLUSTERED 
                        (
                        [TaskID] ASC
                        )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
                        ) ON [PRIMARY]";
                SqlServerCommand(sql);

                sql = @"ALTER TABLE [dbo].[Tasks]  WITH CHECK ADD  CONSTRAINT [FK_Tasks_AGVStatus] FOREIGN KEY([TaskAGV]) REFERENCES [dbo].[AGVStatus] ([ID]) ON UPDATE CASCADE";
                SqlServerCommand(sql);

                sql = @"ALTER TABLE [dbo].[Tasks] CHECK CONSTRAINT [FK_Tasks_AGVStatus]";
                SqlServerCommand(sql);

                sql = @"ALTER TABLE [dbo].[Tasks]  WITH CHECK ADD  CONSTRAINT [FK_Tasks_Jobs] FOREIGN KEY([JobID]) REFERENCES [dbo].[Jobs] ([JobID]) ON UPDATE CASCADE";
                SqlServerCommand(sql);

                sql = @"ALTER TABLE [dbo].[Tasks] CHECK CONSTRAINT [FK_Tasks_Jobs]";
                SqlServerCommand(sql);

                sql = @"ALTER TABLE [dbo].[Tasks] ADD  CONSTRAINT [DF_Tasks_CallPointID]  DEFAULT ((1)) FOR [PointID]";
                SqlServerCommand(sql);

                sql = @"ALTER TABLE [dbo].[Tasks] ADD  CONSTRAINT [DF_Tasks_TaskOrder]  DEFAULT ((0)) FOR [TaskOrder]";
                SqlServerCommand(sql);

                sql = @"ALTER TABLE [dbo].[Tasks] ADD  CONSTRAINT [DF_Tasks_TaskExecute]  DEFAULT ((0)) FOR [TaskExecute]";
                SqlServerCommand(sql);

                sql = @"ALTER TABLE [dbo].[Tasks] ADD  CONSTRAINT [DF_Tasks_TaskResult]  DEFAULT ((0)) FOR [TaskResult]";
                SqlServerCommand(sql);

                sql = @"ALTER TABLE [dbo].[Tasks] ADD  CONSTRAINT [DF_Tasks_TaskStatus]  DEFAULT ((0)) FOR [TaskStatus]";
                SqlServerCommand(sql);
            }
        }
        /// <summary>
        /// 创建叫料做业表
        /// </summary>
        private void CreateTableJobs()
        {
            if (!TableExist(TableJobs))
            {
                string sql = "";
                sql = @"CREATE TABLE [dbo].[Jobs](
                        [JobID] [int] IDENTITY(1,1) NOT NULL,
                        [Name] [nvarchar](max) NULL,
                        [JobType] [int] NOT NULL,
                        [CallTime] [datetime] NOT NULL,
                        [JobExecute] [int] NOT NULL,
                        [JobStartTime] [datetime] NULL,
                        [JobEndTime] [datetime] NULL,
                        [JobStatus] [int] NOT NULL,
                        CONSTRAINT [PK_Jobs] PRIMARY KEY CLUSTERED 
                        (
                        [JobID] ASC
                        )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
                        ) ON [PRIMARY]";
                SqlServerCommand(sql);

                sql = @"ALTER TABLE [dbo].[Jobs] ADD  CONSTRAINT [DF_Jobs_JobType]  DEFAULT ((1)) FOR [JobType]";
                SqlServerCommand(sql);

                sql = @"ALTER TABLE [dbo].[Jobs] ADD  CONSTRAINT [DF_Jobs_CallTime]  DEFAULT (getdate()) FOR [CallTime]";
                SqlServerCommand(sql);

                sql = @"ALTER TABLE [dbo].[Jobs] ADD  CONSTRAINT [DF_Jobs_JobExecute]  DEFAULT ((-1)) FOR [JobExecute]";
                SqlServerCommand(sql);

                sql = @"ALTER TABLE [dbo].[Jobs] ADD  CONSTRAINT [DF_Jobs_JobStatus]  DEFAULT ((0)) FOR [JobStatus]";
                SqlServerCommand(sql);
            }
        }
        #endregion

//        #region Jobs
//        public List<CallJobData> ReadJobs()
//        {
//            List<ArrayList> list;

//            string sql;
//            sql = string.Format(@"SELECT [JobID], [JobType], [JobExecute], [CallTime], [JobStatus] FROM [{0}]
//                                WHERE [CallTime] < '{1}' and [JobStatus] < 4", TableJobs, DateTime.Now);
//            list = SqlServerReader(sql);

//            List<CallJobData> datas = new List<CallJobData>();
//            if (list != null && list.Count > 0)
//            {
//                int row = list.Count;
//                int col = list[0].Count;

//                if (col != 5) return datas;

//                for (int i = 0; i < row; i++)
//                {
//                    int id, ctype, etype, status;
//                    DateTime ctime;

//                    if (!int.TryParse(list[i][0].ToString(), out id)) continue;
//                    if (!int.TryParse(list[i][1].ToString(), out ctype)) continue;
//                    if (!int.TryParse(list[i][2].ToString(), out etype)) continue;
//                    if (!DateTime.TryParse(list[i][3].ToString(), out ctime)) continue;
//                    if (!int.TryParse(list[i][4].ToString(), out status)) continue;

//                    datas.Add(new CallJobData(id, (JobType)ctype, etype, ctime, (JobStatusType)status));
//                }
//            }

//            return datas;
//        }

        //public void WriteJobsStart(int id)
        //{
        //    string sql;
        //    sql = string.Format(@"update [{0}] set [JobStartTime] = getdate() where [JobID] = {1}", TableJobs, id);
        //    SqlServerCommand(sql);
        //}

        //public void WriteJobsEnd(int id)
        //{
        //    string sql;
        //    sql = string.Format(@"update [{0}] set [JobEndTime] = getdate() where [JobID] = {1}", TableJobs, id);
        //    SqlServerCommand(sql);
        //}

        //public void WriteJobsStatus(int id, JobStatusType status)
        //{
        //    string sql;
        //    sql = string.Format(@"update [{0}] set [JobStatus] = {2} where [JobID] = {1}", TableJobs, id, (int)status);
        //    SqlServerCommand(sql);
        //}

        //public void WriteJobsFinished(int id)
        //{
        //    string sql;
        //    sql = string.Format(@"exec JobFinished {0}", id);
        //    SqlServerCommand(sql);
        //}
        #endregion

        #region Tasks
//        public List<CallTaskData> ReadTasks(int job)
//        {
//            List<ArrayList> list;

//            string sql;
//            sql = string.Format(@"SELECT [TaskID], [PointID], [TaskOrder], [TaskExecute], [TaskStatus] FROM [{0}]
//                                WHERE [JobID] = {1} ORDER BY [TaskOrder] ", TableTasks, job);
//            list = SqlServerReader(sql);

//            List<CallTaskData> datas = new List<CallTaskData>();
//            if (list != null && list.Count > 0)
//            {
//                int row = list.Count;
//                int col = list[0].Count;

//                if (col != 5) return datas;

//                for (int i = 0; i < row; i++)
//                {
//                    int id, point, order, execute, status;

//                    if (!int.TryParse(list[i][0].ToString(), out id)) continue;
//                    if (!int.TryParse(list[i][1].ToString(), out point)) continue;
//                    if (!int.TryParse(list[i][2].ToString(), out order)) continue;
//                    if (!int.TryParse(list[i][3].ToString(), out execute)) continue;
//                    if (!int.TryParse(list[i][4].ToString(), out status)) continue;

//                    datas.Add(new CallTaskData(id, job, point, order, execute, (TaskStatusType)status));
//                }
//            }
//            return datas;
//        }

        public void WriteTasksStart(int id)
        {
            string sql;
            sql = string.Format(@"update [{0}] set [TaskStartTime] = getdate() where [TaskID] = {1}", TableTasks, id);
            SqlServerCommand(sql);
        }

        public void WriteTasksEnd(int id)
        {
            string sql;
            sql = string.Format(@"update [{0}] set [TaskEndTime] = getdate() where [TaskID] = {1}", TableTasks, id);
            SqlServerCommand(sql);
        }

        //public void WriteTasksStatus(int id, TaskStatusType status)
        //{
        //    string sql;
        //    sql = string.Format(@"update [{0}] set [TaskStatus] = {2} where [TaskID] = {1}", TableTasks, id, (int)status);
        //    SqlServerCommand(sql);
        //}

        //public void WriteTasksResult(int id, TaskResultType result)
        //{
        //    string sql;
        //    sql = string.Format(@"update [{0}] set [TaskResult] = {2} where [TaskID] = {1}", TableTasks, id, (int)result);
        //    SqlServerCommand(sql);
        //}

        public void WriteTasksAGV(int id, int agv)
        {
            string sql;
            sql = string.Format(@"update [{0}] set [TaskAGV] = {2} where [TaskID] = {1}", TableTasks, id, agv);
            SqlServerCommand(sql);
        }
        #endregion

//        #region Steps
//        public List<CallStepData> ReadSteps(int task)
//        {
//            List<ArrayList> list;

//            string sql;
//            sql = string.Format(@"SELECT [StepID], [StepType], [StepParameter], [StepOrder], [StepStatus] FROM [{0}]
//                                WHERE [TaskID] = {1} ORDER BY [StepOrder] ", TableSteps, task);
//            list = SqlServerReader(sql);

//            List<CallStepData> datas = new List<CallStepData>();
//            if (list != null && list.Count > 0)
//            {
//                int row = list.Count;
//                int col = list[0].Count;

//                if (col != 5) return datas;

//                for (int i = 0; i < row; i++)
//                {
//                    int id, type, param, order, status;

//                    if (!int.TryParse(list[i][0].ToString(), out id)) continue;
//                    if (!int.TryParse(list[i][1].ToString(), out type)) continue;
//                    if (!int.TryParse(list[i][2].ToString(), out param)) continue;
//                    if (!int.TryParse(list[i][3].ToString(), out order)) continue;
//                    if (!int.TryParse(list[i][4].ToString(), out status)) continue;

//                    datas.Add(new CallStepData(id, task, (StepType)type, param, order, (StepStatusType)status));
//                }
//            }
//            return datas;
//        }

        public void WriteStepsStart(int id)
        {
            string sql;
            sql = string.Format(@"update [{0}] set [StepStartTime] = getdate() where [StepID] = {1}", TableSteps, id);
            SqlServerCommand(sql);
        }

        public void WriteStepsEnd(int id)
        {
            string sql;
            sql = string.Format(@"update [{0}] set [StepEndTime] = getdate() where [StepID] = {1}", TableSteps, id);
            SqlServerCommand(sql);
        }

        //public void WriteStepsStatus(int id, StepStatusType status)
        //{
        //    string sql;
        //    sql = string.Format(@"update [{0}] set [StepStatus] = {2} where [StepID] = {1}", TableSteps, id, (int)status);
        //    SqlServerCommand(sql);
        //}

        //public void WriteStepsAGV(int id, int agv)
        //{
        //    string sql;
        //    sql = string.Format(@"update [{0}] set [StepAGV] = {2} where [StepID] = {1}", TableSteps, id, agv);
        //    SqlServerCommand(sql);
        //}
        //#endregion

        //#region CallPoints
        //public void WritePoints(List<CallPoint> points)
        //{
        //    TableTruncate(TablePoints);
        //    foreach (CallPoint point in points)
        //    {
        //        WritePointAdd(point);
        //    }
        //}

        //public void WritePointEdit(CallPoint point)
        //{
        //    WriteCallPointRemove(point.ID);
        //    WritePointAdd(point);
        //}

        //public void WritePointAdd(CallPoint point)
        //{
        //    string sql;

        //    string marks = "";
        //    foreach (int lm in point.LandMarks)
        //    {
        //        marks += string.Format("{0},", lm);
        //    }

        //    sql = string.Format(@"insert into [{0}]([PointID], [Name], [LandMarks], [CallType]) values ({1}, '{2}', '{3}', {4})", TablePoints, point.ID, point.Name, marks, (int)point.Type);
        //    SqlServerCommand(sql);
        //}

        //public void WriteCallPointRemove(int id)
        //{
        //    string sql;

        //    sql = string.Format(@"delete [{0}] where [PointID] = {1}", TablePoints, id);
        //    SqlServerCommand(sql);
        //}
        //#endregion
      

        #region 数据转换
        private string IntListToString(List<int> list)
        {
            string str = "";
            foreach (int i in list)
            {
                str += string.Format(", {0}", i);
            }
            if (!string.IsNullOrEmpty(str))
            {
                str = str.Substring(2);
            }
            return str;
        }

        private List<int> StringToIntList(string str)
        {
            List<int> output = new List<int>();

            string[] strs = str.Split(',', '，');
            foreach (string substr in strs)
            {
                string[] substrs = substr.Split('-', '_');
                if (substrs.Length == 1)
                {
                    int num;
                    if (int.TryParse(substrs[0], out num) && num > 0)
                    {
                        if (!output.Contains(num))
                        {
                            output.Add(num);
                        }
                    }
                }
                else if (substrs.Length == 2)
                {
                    int num1, num2;
                    if (int.TryParse(substrs[0], out num1) && int.TryParse(substrs[1], out num2) && num1 > 0 && num2 >= num1 && num2 < 65536)
                    {
                        for (int i = num1; i <= num2; i++)
                        {
                            if (!output.Contains(i))
                            {
                                output.Add(i);
                            }
                        }
                    }
                }
            }

            return output;
        }
        #endregion
    }
}
