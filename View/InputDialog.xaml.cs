using System.Windows;
using System.Windows.Input;

namespace DataSynchronizor.View
{

    /// <summary>
    /// InputDialog.xaml 的交互逻辑
    /// </summary>
    public partial class InputDialog : Window
    {
        public bool IsOk = false;
        public string Text = "";

        public InputDialog(Window app, string title)
        {
            InitializeComponent();
            Owner = app;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            FocusManager.SetFocusedElement(this, TbInput);
            Title = title;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            Text = TbInput.Text;
            IsOk = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Text = TbInput.Text;
            IsOk = false;
            Close();
        }

        private void TbInput_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            BtnOk.IsEnabled = TbInput.Text != string.Empty;
        }

        private void TbInput_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnOk_Click(sender, e);
            }
            else if (e.Key == Key.Escape)
            {
                BtnCancel_Click(sender, e);
            }
        }
    }
}
