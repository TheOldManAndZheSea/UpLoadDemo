using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using UploadCommon.Unitity;
using UploadCommon.XmlModel;

namespace UpLoadSocketServer
{
    public class AddExeDetailVM : ViewModelBase
    {
        public AddExeDetailVM(ExeEntityModel exeEntity)
        {
            Title = "添加";
            if (exeEntity != null)
            {
                Title = "修改";
                EntityModel = exeEntity;
            }
            else
            {
                EntityModel = new ExeEntityModel { Index= DataList==null?1:DataList.Count+1 };
            }
            DefineCommand=new RelayCommand(new
                 Action<object>(DefineExaute));
            CloseWinCommand = new RelayCommand(new
                 Action<object>(CloseExaute));
        }

        

        #region 字段
        public List<ExeEntityModel> DataList { get; set; }
        private ExeEntityModel _entityModel;
        /// <summary>
        /// 当前需要新增或者修改的model
        /// </summary>
        public ExeEntityModel EntityModel
        {
            get
            {
                if (_entityModel == null) _entityModel = new ExeEntityModel();
                return _entityModel;
            }
            set { _entityModel = value; this.NotifyPropertyChanged("EntityModel"); }
        }

        private string _title;
        /// <summary>
        /// 标题
        /// </summary>
        public string Title
        {
            get { return _title; }
            set { _title = value; this.NotifyPropertyChanged("Title"); }
        }
        private string _ShowMsg;
        /// <summary>
        /// 消息
        /// </summary>
        public string ShowMsg
        {
            get { return _ShowMsg; }
            set { _ShowMsg = value; this.NotifyPropertyChanged("ShowMsg"); }
        }

        #endregion

        #region 命令
        /// <summary>
        /// 关闭窗口的命令
        /// </summary>
        public ICommand CloseWinCommand { get; set; }
        /// <summary>
        /// 确定命令
        /// </summary>
        public ICommand DefineCommand { get; set; }
        #endregion

        #region 方法
        /// <summary>
        /// 添加或者修改
        /// </summary>
        /// <param name="obj"></param>
        private void DefineExaute(object obj)
        {
            if (string.IsNullOrEmpty(EntityModel.ExeName))
            {
                ShowMsg = "更新程序名称不能为空！";
                return;
            }
            else if (!Regex.Match(EntityModel.ExeName, @"^[A-Za-z]+$").Success)
            {
                ShowMsg = "更新程序名称必须是英文字母！";
                return;
            }
            if (string.IsNullOrEmpty(EntityModel.ExePath))
            {
                ShowMsg = "更新路径不能为空！";
                return;
            }
            else if (!System.IO.Directory.Exists(EntityModel.ExePath))
            {
                ShowMsg = "更新路径不存在，请重新选择！";
                return;
            }
            if (DataList.Where(s=>s.Index==EntityModel.Index).Count()>0)
            {
                DataList.Where(s => s.Index == EntityModel.Index).ToList()[0] = EntityModel;
            }
            else
            {
                DataList.Add(EntityModel);
            }
            if (obj is Window)
            {
                (obj as Window).Close();
            }
        }
        /// <summary>
        /// 关闭窗体
        /// </summary>
        /// <param name="obj"></param>
        private void CloseExaute(object obj)
        {
            if (obj is Window)
            {
                (obj as Window).Close();
            }
        }
        #endregion
    }
}
