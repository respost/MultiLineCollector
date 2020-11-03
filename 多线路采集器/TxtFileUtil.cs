using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace 多线路采集器
{
    class TxtFileUtil
    {
        #region 读txt文件

        public static string ReadTxtData(string txtFilePath)
        {

            //1.创建文件流
            FileStream fs = new FileStream(txtFilePath, FileMode.OpenOrCreate);
            //2.创建读取器
            StreamReader sr = new StreamReader(fs, Encoding.UTF8);
            //3.读取内容

            string content = "";
            try
            {
                content = sr.ReadToEnd();
            }
            catch (Exception)
            {
            }
            finally
            {
                //4.关闭读取器
                if (sr != null)
                {
                    sr.Close();
                }
                //5.关闭文件流
                if (fs != null)
                {
                    fs.Close();
                }
            }

            return content;
        }

        #endregion

        #region 写txt文件

        public static bool WriteTxtData(string content, string txtFilePath)
        {
            //1.创建文件流
            FileStream fs = new FileStream(txtFilePath, FileMode.Append);

            //2.创建写入器
            StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
            //3.开始写入

            bool result = false;//标识是否写入成功
            try
            {
                sw.WriteLine(content);

                result = true;
            }
            catch (Exception)
            {
                result = false;
            }
            finally
            {
                //4.关闭写入器
                if (sw != null)
                {
                    sw.Close();
                }
                //5.关闭文件流
                if (fs != null)
                {
                    fs.Close();
                }
            }
            return result;
        }

        #endregion
    }
}
