using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace UploadUserContrals
{
    public class ControlAttachProperty
    {
        #region FocusBorderBrush 焦点边框色，输入控件

        public static readonly DependencyProperty FocusBorderBrushProperty = DependencyProperty.RegisterAttached(
            "FocusBorderBrush", typeof(Brush), typeof(ControlAttachProperty), new FrameworkPropertyMetadata(null));
        public static void SetFocusBorderBrush(DependencyObject element, Brush value)
        {
            element.SetValue(FocusBorderBrushProperty, value);
        }
        public static Brush GetFocusBorderBrush(DependencyObject element)
        {
            return (Brush)element.GetValue(FocusBorderBrushProperty);
        }

        #endregion

        #region MouseOverBorderBrush 鼠标进入边框色，输入控件

        public static readonly DependencyProperty MouseOverBorderBrushProperty =
            DependencyProperty.RegisterAttached("MouseOverBorderBrush", typeof(Brush), typeof(ControlAttachProperty),
                new FrameworkPropertyMetadata(Brushes.Transparent,
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        /// Sets the brush used to draw the mouse over brush.
        /// </summary>
        public static void SetMouseOverBorderBrush(DependencyObject obj, Brush value)
        {
            obj.SetValue(MouseOverBorderBrushProperty, value);
        }

        /// <summary>
        /// Gets the brush used to draw the mouse over brush.
        /// </summary>
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        [AttachedPropertyBrowsableForType(typeof(CheckBox))]
        [AttachedPropertyBrowsableForType(typeof(RadioButton))]
        [AttachedPropertyBrowsableForType(typeof(DatePicker))]
        [AttachedPropertyBrowsableForType(typeof(ComboBox))]
        [AttachedPropertyBrowsableForType(typeof(RichTextBox))]
        public static Brush GetMouseOverBorderBrush(DependencyObject obj)
        {
            return (Brush)obj.GetValue(MouseOverBorderBrushProperty);
        }

        #endregion

        #region AttachContentProperty 附加组件模板
        /// <summary>
        /// 附加组件模板
        /// </summary>
        public static readonly DependencyProperty AttachContentProperty = DependencyProperty.RegisterAttached(
            "AttachContent", typeof(ControlTemplate), typeof(ControlAttachProperty), new FrameworkPropertyMetadata(null));

        public static ControlTemplate GetAttachContent(DependencyObject d)
        {
            return (ControlTemplate)d.GetValue(AttachContentProperty);
        }

        public static void SetAttachContent(DependencyObject obj, ControlTemplate value)
        {
            obj.SetValue(AttachContentProperty, value);
        }
        #endregion

        #region WatermarkProperty 水印
        /// <summary>
        /// 水印
        /// </summary>
        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.RegisterAttached(
            "Watermark", typeof(string), typeof(ControlAttachProperty), new FrameworkPropertyMetadata(""));

        public static string GetWatermark(DependencyObject d)
        {
            return (string)d.GetValue(WatermarkProperty);
        }

        public static void SetWatermark(DependencyObject obj, string value)
        {
            obj.SetValue(WatermarkProperty, value);
        }
        #endregion

        #region CornerRadiusProperty Border圆角
        /// <summary>
        /// Border圆角
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.RegisterAttached(
            "CornerRadius", typeof(CornerRadius), typeof(ControlAttachProperty), new FrameworkPropertyMetadata(null));

        public static CornerRadius GetCornerRadius(DependencyObject d)
        {
            return (CornerRadius)d.GetValue(CornerRadiusProperty);
        }

        public static void SetCornerRadius(DependencyObject obj, CornerRadius value)
        {
            obj.SetValue(CornerRadiusProperty, value);
        }
        #endregion

        #region LabelProperty TextBox的头部Label
        /// <summary>
        /// TextBox的头部Label
        /// </summary>
        public static readonly DependencyProperty LabelProperty = DependencyProperty.RegisterAttached(
            "Label", typeof(string), typeof(ControlAttachProperty), new FrameworkPropertyMetadata(null));

        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static string GetLabel(DependencyObject d)
        {
            return (string)d.GetValue(LabelProperty);
        }

        public static void SetLabel(DependencyObject obj, string value)
        {
            obj.SetValue(LabelProperty, value);
        }
        #endregion

        #region LabelTemplateProperty TextBox的头部Label模板
        /// <summary>
        /// TextBox的头部Label模板
        /// </summary>
        public static readonly DependencyProperty LabelTemplateProperty = DependencyProperty.RegisterAttached(
            "LabelTemplate", typeof(ControlTemplate), typeof(ControlAttachProperty), new FrameworkPropertyMetadata(null));

        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static ControlTemplate GetLabelTemplate(DependencyObject d)
        {
            return (ControlTemplate)d.GetValue(LabelTemplateProperty);
        }

        public static void SetLabelTemplate(DependencyObject obj, ControlTemplate value)
        {
            obj.SetValue(LabelTemplateProperty, value);
        }
        #endregion

        #region IsClearTextButtonBehaviorEnabledProperty 清除输入框Text值按钮行为开关（设为ture时才会绑定事件）
        /// <summary>
        /// 清除输入框Text值按钮行为开关
        /// </summary>
        public static readonly DependencyProperty IsClearTextButtonBehaviorEnabledProperty = DependencyProperty.RegisterAttached("IsClearTextButtonBehaviorEnabled"
            , typeof(bool), typeof(ControlAttachProperty), new FrameworkPropertyMetadata(false, IsClearTextButtonBehaviorEnabledChanged));

        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static bool GetIsClearTextButtonBehaviorEnabled(DependencyObject d)
        {
            return (bool)d.GetValue(IsClearTextButtonBehaviorEnabledProperty);
        }

        public static void SetIsClearTextButtonBehaviorEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsClearTextButtonBehaviorEnabledProperty, value);
        }

        /// <summary>
        /// 绑定清除Text操作的按钮事件
        /// </summary>
        private static void IsClearTextButtonBehaviorEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = d as ButtonEx;
            if (e.OldValue != e.NewValue && button != null)
            {
                button.CommandBindings.Add(ClearTextCommandBinding);
            }
        }

        #endregion

        #region ClearTextCommand 清除输入框Text事件命令

        /// <summary>
        /// 清除输入框Text事件命令，需要使用IsClearTextButtonBehaviorEnabledChanged绑定命令
        /// </summary>
        public static RoutedUICommand ClearTextCommand { get; private set; }

        /// <summary>
        /// ClearTextCommand绑定事件
        /// </summary>
        private static readonly CommandBinding ClearTextCommandBinding;

        /// <summary>
        /// 清除输入框文本值
        /// </summary>
        private static void ClearButtonClick(object sender, ExecutedRoutedEventArgs e)
        {
            var tbox = e.Parameter as FrameworkElement;
            if (tbox == null) return;
            if (tbox is TextBox)
            {
                ((TextBox)tbox).Clear();
            }
            if (tbox is PasswordBox)
            {
                ((PasswordBox)tbox).Clear();
            }
            if (tbox is ComboBox)
            {
                var cb = tbox as ComboBox;
                cb.SelectedItem = null;
                cb.Text = string.Empty;
            }
            if (tbox is DatePicker)
            {
                var dp = tbox as DatePicker;
                dp.SelectedDate = null;
                dp.Text = string.Empty;
            }
            tbox.Focus();
        }

        #endregion
        #region IsOpenFolderButtonBehaviorEnabledProperty 选择文件夹命令行为开关
        /// <summary>
        /// 选择文件夹命令行为开关
        /// </summary>
        public static readonly DependencyProperty IsOpenFolderButtonBehaviorEnabledProperty = DependencyProperty.RegisterAttached("IsOpenFolderButtonBehaviorEnabled"
            , typeof(bool), typeof(ControlAttachProperty), new FrameworkPropertyMetadata(false, IsOpenFolderButtonBehaviorEnabledChanged));

        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static bool GetIsOpenFolderButtonBehaviorEnabled(DependencyObject d)
        {
            return (bool)d.GetValue(IsOpenFolderButtonBehaviorEnabledProperty);
        }

        public static void SetIsOpenFolderButtonBehaviorEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsOpenFolderButtonBehaviorEnabledProperty, value);
        }

        private static void IsOpenFolderButtonBehaviorEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = d as ButtonEx;
            if (e.OldValue != e.NewValue && button != null)
            {
                button.CommandBindings.Add(OpenFolderCommandBinding);
            }
        }

        #endregion
        #region OpenFolderCommand 选择文件夹命令

        /// <summary>
        /// 选择文件夹命令
        /// </summary>
        public static RoutedUICommand OpenFolderCommand { get; private set; }

        /// <summary>
        /// OpenFolderCommand绑定事件
        /// </summary>
        private static readonly CommandBinding OpenFolderCommandBinding;

        /// <summary>
        /// 执行OpenFolderCommand
        /// </summary>
        private static void OpenFolderButtonClick(object sender, ExecutedRoutedEventArgs e)
        {
            var tbox = e.Parameter as FrameworkElement;
            var txt = tbox as TextBox;
            if (txt == null) return;

            System.Windows.Forms.FolderBrowserDialog fd = new System.Windows.Forms.FolderBrowserDialog();
            fd.Description = "请选择文件路径";
            fd.SelectedPath = txt.Text.Trim();
            if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txt.Text = fd.SelectedPath;
            }
            tbox.Focus();
        }

        #endregion
        /// <summary>
        /// 静态构造函数
        /// </summary>
        static ControlAttachProperty()
        {
            //ClearTextCommand
            ClearTextCommand = new RoutedUICommand();
            ClearTextCommandBinding = new CommandBinding(ClearTextCommand);
            ClearTextCommandBinding.Executed += ClearButtonClick;
            //OpenFolderCommand
            OpenFolderCommand = new RoutedUICommand();
            OpenFolderCommandBinding = new CommandBinding(OpenFolderCommand);
            OpenFolderCommandBinding.Executed += OpenFolderButtonClick;
        }
    }
}
