using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UpLoadDemo.XmlModel
{
    [Serializable]
    public class UpLoadOption
    {
        /// <summary>
        /// 更新内容
        /// </summary>
        public string UpLoadContent { get; set; }
        /// <summary>
        /// 更新地址
        /// </summary>
        public string UpLoadFileUrl { get; set; }
        /// <summary>
        /// 程序启动目录
        /// </summary>
        public string ProgrmStartupDir { get; set; }
        /// <summary>
        /// 更新文件的集合
        /// </summary>
        public List<UpLoad> UpLoadFiles { get; set; }
    }
}
