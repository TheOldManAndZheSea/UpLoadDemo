using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UpLoadDemo.XmlModel
{
    public static class StaticModel
    {
        /// <summary>
        /// 我的配置文件
        /// </summary>
        public static UpLoadOption MyUpLoadModel { get; set; }
        /// <summary>
        /// 服务器配置文件
        /// </summary>
        public static UpLoadOption ServerUpLoadModel { get; set; }
        /// <summary>
        /// 需要更新的内容
        /// </summary>
        public static List<UpLoad> UpLoads { get; set; }
        /// <summary>
        /// 更新错误的内容
        /// </summary>
        public static List<UpLoad> ErroUpLoads { get; set; }
        public static string[] MainArgs { get; set; }
    }
}
