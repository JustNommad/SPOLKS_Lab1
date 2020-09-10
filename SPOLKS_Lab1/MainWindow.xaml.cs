using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace SPOLKS_Lab1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ClientClass client = new ClientClass();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void nickname_Click(object sender, RoutedEventArgs e)
        {
            var compName = computerName.Text;
            var result = client.ConnectToServer(compName);
            MessageBox.Show(result);
            AddUserList();
            Thread serverListenerThread = new Thread(new ThreadStart(client.Process));
            serverListenerThread.IsBackground = true;
            serverListenerThread.Start();
        }

        private void speedUpload_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void send_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var message = client.SendFile(protocolName.Text, onlineList.SelectedItem.ToString());
                if (message != null)
                    MessageBox.Show(message);
            }catch(Exception ex)
            {
                MessageBox.Show($"{ex.Message}");
            }
        }

        private void folder_Click(object sender, RoutedEventArgs e)
        {
            if (client.OpenDialog())
                filePath.Text = client.FilePath;
        }

        private void protocolName_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        public void AddUserList()
        {
            onlineList.ItemsSource = client.clientList;
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            AddUserList();
        }
    }
}
