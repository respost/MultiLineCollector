using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;

namespace 多线路采集器
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        //软件标题
        private string softTitle = "";
        //代理
        private delegate void SetPos(int ipos, string vinfo, TextBox textbox);
        //网络线程1
        private Thread thread1;
        //网络线程2
        private Thread thread2;
        //网络线程3
        private Thread thread3;
        //网络线程4
        private Thread thread4;
        //网络线程5
        private Thread thread5;
        //网络线程6
        private Thread thread6;
        //网络线程7
        private Thread thread7;
        //网络线程8
        private Thread thread8;
        //线程延迟时间
        private int speed = 1000;
        //存放任务集
        private Dictionary<int, List<string>> taskMap = new Dictionary<int, List<string>>();

        #region 内存回收
        [DllImport("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize")]
        public static extern int SetProcessWorkingSetSize(IntPtr process, int minSize, int maxSize);
        /// <summary>
        /// 释放内存
        /// </summary>
        public static void ClearMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                //Form1为我窗体的类名
                Form1.SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
            }
        }
        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {          
            //读取软件标题
            softTitle = this.Text;
            //滑杆设置
            this.trackBar1.Value = speed / 100;
            showTrackBar();
            //1.获取当前文件目录
            string path = System.Environment.CurrentDirectory;
            //读取当前文件目录中的txt文件
            for (int i = 1; i <= 8; i++)
            {
                //2.配置文件路径
                string configFilePath = "" + path + "\\"+i+".txt";
                string content = TxtFileUtil.ReadTxtData(configFilePath);
                //3.以空格拆分字符串
                string[] temps = content.Trim().Split('\n');
                //4.把urlArrys数组放入list中
                List<string> urlList = new List<string>();

                string tabTitle= "";
                for (int j = 0; j < temps.Length; j++)
                {
                    //如果不是以#开头，就添加到List中
                    if (!temps[j].StartsWith("#"))
                    {
                        urlList.Add(temps[j]);
                    }
                    else
                    {
                        tabTitle = temps[j].Replace("#","").Trim();
                    }
                }
                taskMap.Add(i, urlList);
                //显示任务数量到tab上
                TabPage tabpage = this.tabPage1;
                switch (i)
                {
                    case 1:
                        tabpage = this.tabPage1;
                        break;
                    case 2:
                        tabpage = this.tabPage2;
                        break;
                    case 3:
                        tabpage = this.tabPage3;
                        break;
                    case 4:
                        tabpage = this.tabPage4;
                        break;
                    case 5:
                        tabpage = this.tabPage5;
                        break;
                    case 6:
                        tabpage = this.tabPage6;
                        break;
                    case 7:
                        tabpage = this.tabPage7;
                        break;
                    case 8:
                        tabpage = this.tabPage8;
                        break;
                }
                if (tabTitle != "")
                {
                    tabpage.Text = tabTitle + "(" + urlList.Count + ")";
                }                           
            }
            //自动点击开始按钮
            startApp();
            //开机启动
            //Boot.SetMeStart(this.cbBootUp.Checked);
            string cacheDir = this.txtCacheDir.Text.Trim();
            if (this.cbBootUp.Checked && cacheDir != string.Empty)
            {
                //清空缓存文件
                ClearDirectory(cacheDir);         
            }
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            speed = this.trackBar1.Value * 100;
            showTrackBar();

        }
        private void showTrackBar()
        {
            float f = speed * 1.0f / 1000;
            this.lbTigs.Text = "速率" + f + "秒";
        }

        int time = 1;
        private void timer1_Tick(object sender, EventArgs e)
        {
            //释放内存
            ClearMemory();
            this.Text = softTitle + "  当前已运行：" + parseTimeSeconds(time, 0);
            time++;
        }
        /// <summary>
        /// 输出日志信息
        /// </summary>
        /// <param name="ipos"></param>
        /// <param name="vinfo"></param>
        private void printLog(int ipos, string vinfo, TextBox textbox)
        {
            if (this.InvokeRequired)
            {
                SetPos setpos = new SetPos(printLog);
                this.Invoke(setpos, new object[] { ipos, vinfo, textbox });
            }
            else
            {
                textbox.AppendText(vinfo);
            }
        }
        ///<summary>
        ///由秒数得到日期几天几小时。。。
        ///</summary
        ///<param name="t">秒数</param>
        ///<param name="type">0：转换后带秒，1:转换后不带秒</param>
        ///<returns>几天几小时几分几秒</returns>
        public static string parseTimeSeconds(int t, int type)
        {
            string r = "";
            int day, hour, minute, second;
            if (t >= 86400) //天,
            {
                day = Convert.ToInt16(t / 86400);
                hour = Convert.ToInt16((t % 86400) / 3600);
                minute = Convert.ToInt16((t % 86400 % 3600) / 60);
                second = Convert.ToInt16(t % 86400 % 3600 % 60);
                if (type == 0)
                    r = day + ("天") + hour + ("时") + minute + ("分") + second + ("秒");
                else
                    r = day + ("天") + hour + ("时") + minute + ("分");

            }
            else if (t >= 3600)//时,
            {
                hour = Convert.ToInt16(t / 3600);
                minute = Convert.ToInt16((t % 3600) / 60);
                second = Convert.ToInt16(t % 3600 % 60);
                if (type == 0)
                    r = hour + ("时") + minute + ("分") + second + ("秒");
                else
                    r = hour + ("时") + minute + ("分");
            }
            else if (t >= 60)//分
            {
                minute = Convert.ToInt16(t / 60);
                second = Convert.ToInt16(t % 60);
                r = minute + ("分") + second + ("秒");
            }
            else
            {
                second = Convert.ToInt16(t);
                r = second + ("秒");
            }
            return r;
        }
        /// <summary>
        /// 线程执行逻辑
        /// </summary>
        private void HttpThread(object obj)
        {          
           int num = (int)obj;
           if (taskMap.ContainsKey(num))
           {
               ThreadExecute(taskMap[num], num);
           }
        }
        /// <summary>
        /// 线程执行任务的通用方法，这里因为2个线程执行逻辑相同，就写到一起了。用一个参数加以区分
        /// </summary>
        /// <param name="ThreadNum"></param>
        private void ThreadExecute(List<string> urlList,int num)
        {
            do
            {
                if (urlList != null && urlList.Count > 0)
                {
                    for (int i = 0; i < urlList.Count; i++)
                    {
                        try
                        {
                            //如果是以#开头，就不读取
                            //if (noUrlList[i].StartsWith("#"))
                            //{
                            //    break;
                            //}
                            TextBox textbox = this.txtLog1;
                            switch (num)
                            {
                                case 1:
                                    textbox = this.txtLog1;
                                    break;
                                case 2:
                                    textbox = this.txtLog2;
                                    break;
                                case 3:
                                    textbox = this.txtLog3;
                                    break;
                                case 4:
                                    textbox = this.txtLog4;
                                    break;
                                case 5:
                                    textbox = this.txtLog5;
                                    break;
                                case 6:
                                    textbox = this.txtLog6;
                                    break;
                                case 7:
                                    textbox = this.txtLog7;
                                    break;
                                case 8:
                                    textbox = this.txtLog8;
                                    break;
                            }
                            string result = HttpUtil.Send(urlList[i],"GET");
                            string msg = string.Format("{0}\r\n",result);
                            printLog(i, msg, textbox);
                            //线程延迟
                            System.Threading.Thread.Sleep(speed);
                        }
                        catch (Exception)
                        {
                            break;
                        }
                    }
                }

            } while (true);

        }
        private void btnSwitch_Click(object sender, EventArgs e)
        {
            startApp();
        }

        private void startApp()
        {
            if (this.btnSwitch.Text == "开 始")
            {
                this.btnSwitch.Text = "停 止";
                this.btnSwitch.BackColor = Color.Firebrick;
                //启动定时器
                this.timer1.Start();
                //启动线程1
                if (this.cbLine1.Checked)
                {
                    if (thread1 == null)
                    {
                        thread1 = new Thread(new ParameterizedThreadStart(HttpThread));
                    }
                    thread1.Start(1);
                }
                //启动线程2
                if (this.cbLine2.Checked)
                {
                    if (thread2 == null)
                    {
                        thread2 = new Thread(new ParameterizedThreadStart(HttpThread));
                    }
                    thread2.Start(2);
                }
                //启动线程3
                if (this.cbLine3.Checked)
                {
                    if (thread3 == null)
                    {
                        thread3 = new Thread(new ParameterizedThreadStart(HttpThread));
                    }
                    thread3.Start(3);
                }
                //启动线程4
                if (this.cbLine4.Checked)
                {
                    if (thread4 == null)
                    {
                        thread4 = new Thread(new ParameterizedThreadStart(HttpThread));
                    }
                    thread4.Start(4);
                }
                //启动线程5
                if (this.cbLine5.Checked)
                {
                    if (thread5 == null)
                    {
                        thread5 = new Thread(new ParameterizedThreadStart(HttpThread));
                    }
                    thread5.Start(5);
                }
                //启动线程6
                if (this.cbLine6.Checked)
                {
                    if (thread6 == null)
                    {
                        thread6 = new Thread(new ParameterizedThreadStart(HttpThread));
                    }
                    thread6.Start(6);
                }
                //启动线程7
                if (this.cbLine7.Checked)
                {
                    if (thread7 == null)
                    {
                        thread7 = new Thread(new ParameterizedThreadStart(HttpThread));
                    }
                    thread7.Start(7);
                }
                //启动线程8
                if (this.cbLine8.Checked)
                {
                    if (thread8 == null)
                    {
                        thread8 = new Thread(new ParameterizedThreadStart(HttpThread));
                    }
                    thread8.Start(8);
                }
            }
            else
            {
                //销毁进程1              
                if (thread1 != null)
                {
                    thread1.Abort();
                    thread1 = null;
                }
                //销毁进程2              
                if (thread2 != null)
                {
                    thread2.Abort();
                    thread2 = null;
                }
                //销毁进程3              
                if (thread3 != null)
                {
                    thread3.Abort();
                    thread3 = null;
                }
                //销毁进程4              
                if (thread4 != null)
                {
                    thread4.Abort();
                    thread4 = null;
                }
                //销毁进程5              
                if (thread5 != null)
                {
                    thread5.Abort();
                    thread5 = null;
                }
                //销毁进程6              
                if (thread6 != null)
                {
                    thread6.Abort();
                    thread6 = null;
                }
                //销毁进程7              
                if (thread7 != null)
                {
                    thread7.Abort();
                    thread7 = null;
                }
                //销毁进程8              
                if (thread8 != null)
                {
                    thread8.Abort();
                    thread8 = null;
                }
                this.btnSwitch.Text = "开 始";
                this.btnSwitch.BackColor = Color.Green;
                //停止定时器
                this.timer1.Stop();
            }
        }

        private void cbGpc_CheckedChanged(object sender, EventArgs e)
        {
            //启用进程1
            if (this.cbLine1.Checked)
            {
                if (thread1 == null)
                {
                    thread1 = new Thread(new ParameterizedThreadStart(HttpThread));
                }
                thread1.Start(1);
            }
            else
            {
                //销毁进程1              
                if (thread1 != null)
                {
                    thread1.Abort();
                    thread1 = null;
                }

            }
        }

        private void cbDpc_CheckedChanged(object sender, EventArgs e)
        {
            //启用进程2
            if (this.cbLine2.Checked)
            {
                if (thread2 == null)
                {
                    thread2 = new Thread(new ParameterizedThreadStart(HttpThread));
                }
                thread2.Start(2);
            }
            else
            {
                //销毁进程2              
                if (thread2 != null)
                {
                    thread2.Abort();
                    thread2 = null;
                }

            }
        }

        private void cbK3_CheckedChanged(object sender, EventArgs e)
        {
            //启用进程3
            if (this.cbLine3.Checked)
            {
                if (thread3 == null)
                {
                    thread3 = new Thread(new ParameterizedThreadStart(HttpThread));
                }
                thread3.Start(3);
            }
            else
            {
                //销毁进程3              
                if (thread3 != null)
                {
                    thread3.Abort();
                    thread3 = null;
                }

            }
        }

        private void cb11x5_CheckedChanged(object sender, EventArgs e)
        {
            //启用进程4
            if (this.cbLine4.Checked)
            {
                if (thread4 == null)
                {
                    thread4 = new Thread(new ParameterizedThreadStart(HttpThread));
                }
                thread4.Start(4);
            }
            else
            {
                //销毁进程4             
                if (thread4 != null)
                {
                    thread4.Abort();
                    thread4 = null;
                }

            }
        }

        private void cbKl8_CheckedChanged(object sender, EventArgs e)
        {
            //启用进程5
            if (this.cbLine5.Checked)
            {
                if (thread5 == null)
                {
                    thread5 = new Thread(new ParameterizedThreadStart(HttpThread));
                }
                thread5.Start(5);
            }
            else
            {
                //销毁进程5             
                if (thread5 != null)
                {
                    thread5.Abort();
                    thread5 = null;
                }

            }
        }

        private void cbJsc_CheckedChanged(object sender, EventArgs e)
        {
            //启用进程6
            if (this.cbLine6.Checked)
            {
                if (thread6 == null)
                {
                    thread6 = new Thread(new ParameterizedThreadStart(HttpThread));
                }
                thread6.Start(6);
            }
            else
            {
                //销毁进程6             
                if (thread6 != null)
                {
                    thread6.Abort();
                    thread6 = null;
                }

            }
        }

        private void cbJwc_CheckedChanged(object sender, EventArgs e)
        {
            //启用进程7
            if (this.cbLine7.Checked)
            {
                if (thread7 == null)
                {
                    thread7 = new Thread(new ParameterizedThreadStart(HttpThread));
                }
                thread7.Start(7);
            }
            else
            {
                //销毁进程7             
                if (thread7 != null)
                {
                    thread7.Abort();
                    thread7 = null;
                }

            }
        }

        private void cbFfc_CheckedChanged(object sender, EventArgs e)
        {
            //启用进程8
            if (this.cbLine8.Checked)
            {
                if (thread8 == null)
                {
                    thread8 = new Thread(new ParameterizedThreadStart(HttpThread));
                }
                thread8.Start(8);
            }
            else
            {
                //销毁进程8             
                if (thread8 != null)
                {
                    thread8.Abort();
                    thread8 = null;
                }

            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        private void cbBootUp_CheckedChanged(object sender, EventArgs e)
        {
            //开机启动
            Boot.SetMeStart(this.cbBootUp.Checked);
        }
        /// <summary>
        /// 清空文件夹
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns>是否成功</returns>
        /// <remarks>删除指定文件夹中所有文件</remarks>
        public static bool ClearDirectory(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path)
                    || !Directory.Exists(path))
                {
                    return true;  // 如果参数为空，则视为已成功清空
                }
                // 删除当前文件夹下所有文件
                foreach (string strFile in Directory.GetFiles(path))
                {
                    System.IO.File.Delete(strFile);
                }
                // 删除当前文件夹下所有子文件夹(递归)
                foreach (string strDir in Directory.GetDirectories(path))
                {
                    Directory.Delete(strDir, true);
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("清空 {0} 异常, 消息:{1}, 堆栈:{2}", path, ex.Message, ex.StackTrace));
                return false;
            }
        }

        private void picCache_Click(object sender, EventArgs e)
        {
            //获取文件和路径名 一起显示在 txtbox 控件里
            FolderBrowserDialog folder = new FolderBrowserDialog();
            if (folder.ShowDialog() == DialogResult.OK)
            {
                this.txtCacheDir.Text = folder.SelectedPath;
            }
        }

        private void tsslUrl_Click(object sender, EventArgs e)
        {
            Process.Start("http://www.zy13.net");
        }
    }
}
