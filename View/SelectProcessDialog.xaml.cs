using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DataSynchronizor.View
{
    public partial class SelectProcessDialog : Window
    {
        public bool IsOk = false;
        public string ProcessName = string.Empty;
        private static readonly Process[] Processes = Process.GetProcesses();

        public SelectProcessDialog(Window app)
        {
            Owner = app;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            InitializeComponent();

            // 列出进程
            ListProcess();
        }

        private void BtnOk_OnClick(object sender, RoutedEventArgs e)
        {
            IsOk = true;
            Close();
        }

        private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
        {
            IsOk = false;
            Close();
        }

        private void ListProcess()
        {
            var lbItems = LbProcessSelect.Items;
            lbItems.Clear();
            foreach (var process in Processes)
            {
                var processName = process.ProcessName;
                // 去重
                if (lbItems.Contains(processName))
                {
                    continue;
                }

                // 筛选
                var filterText = TbProcessNameFilter.Text;
                if (filterText != string.Empty && !processName.ToUpper().Contains(filterText.Trim().ToUpper()))
                {
                    continue;
                }

                lbItems.Add(processName);
            }
        }

        private void TbProcessNameFilter_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ListProcess();
        }

        private void LbProcessSelect_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = LbProcessSelect.SelectedItem;
            if (selected == null)
            {
                BtnOk.IsEnabled = false;
                return;
            }

            BtnOk.IsEnabled = true;
            ProcessName = LbProcessSelect.SelectedItem.ToString();
        }

        private void SelectProcessDialog_OnKeyDown(object sender, KeyEventArgs e)
        {
            // Esc 取消
            if (e.Key == Key.Escape)
            {
                BtnCancel_OnClick(sender, e);
            }

            // 不是回车 不响应
            if (e.Key != Key.Enter)
            {
                return;
            }

            // 如果没有选中 不响应
            var selected = LbProcessSelect.SelectedItem;
            if (selected == null)
            {
                return;
            }

            BtnOk_OnClick(sender, e);
        }
    }
}