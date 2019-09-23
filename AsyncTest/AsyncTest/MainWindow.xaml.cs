using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using Microsoft.Win32;
using System.Windows.Threading;
using AsyncTest.ProgressStatus;

namespace AsyncTest
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public readonly string _regKey = @"Software\DaichiHasegawaHack\Win32AsyncTest\URI";

        public MainWindow()
        {
            InitializeComponent();
            var regKey = Registry.CurrentUser.OpenSubKey(_regKey,false);
            if (regKey == null)
            {
                return;
            }
            uriTextBox.Text = (string)regKey.GetValue("uri");
        }

        public void AddText()
        {

        }

        private async void UrtButton_Click(object sender, RoutedEventArgs e)
        {
            var uri = uriTextBox.Text;
            RegistryKey regkey = Registry.CurrentUser.CreateSubKey(_regKey);
            regkey.SetValue("uri",uri);

            try
            {
                var prog = new ProgStatus();
                prog.Show();
                prog.progressbar.IsIndeterminate = true;
                var result = await Task.Run(()=> GetData(uri));
                prog.Close();
                GenerateTextBox(result);
            }
            catch
            {
                Debug.WriteLine("jsonファイルの取得に失敗しました。");
            }
        }

        private async void GenerateTextBox(IList<User> users)
        {
            var marginTop = 0.0d;
            await Task.Run(()=> {
                foreach (var user in users)
                {
                    Dispatcher.Invoke(DispatcherPriority.Background,
                        new Action(() => {
                            var t = new TextBox();
                            t.Margin = new Thickness(t.Margin.Left, marginTop, t.Margin.Right, t.Margin.Bottom);
                            t.VerticalAlignment = VerticalAlignment.Top;
                            t.Height = 20;
                            t.Text = user.Name + "," + user.Age.ToString();
                            marginTop += 30d;
                            scrollGrid.Children.Add(t);
                        }));
                }
            });
        }

        private IList<User> GetData(string uri)
        {
            using (StreamReader sr = new StreamReader(uri))
            {
                var users = JsonConvert.DeserializeObject<IList<User>>(sr.ReadToEnd());
                return users;
            }
        }
    }
}
