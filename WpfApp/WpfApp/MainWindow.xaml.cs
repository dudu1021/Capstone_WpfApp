using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace AgentApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            // Open file dialog to select a file
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Text Files (*.txt)|*.txt|PNG Images (*.png)|*.png|JPEG Images (*.jpeg)|*.jpeg";

            if (dialog.ShowDialog() == true)
            {
                FilePathTextBox.Text = dialog.FileName;
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            // Retrieve selected file extension and path
            string selectedExtension = (FileExtensionComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            string filePath = FilePathTextBox.Text;

            // Validate inputs
            if (string.IsNullOrEmpty(selectedExtension) || string.IsNullOrEmpty(filePath))
            {
                StatusTextBlock.Text = "Please select a file extension and path.";
                return;
            }

            if (!filePath.EndsWith(selectedExtension, StringComparison.OrdinalIgnoreCase))
            {
                StatusTextBlock.Text = "File extension and path do not match.";
                return;
            }

            try
            {
                // Send data to server
                SendDataToServer($"GET_FILES|EXT={selectedExtension}|PATH={filePath}");
                StatusTextBlock.Text = "Data sent successfully.";
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Error: {ex.Message}";
            }
        }

        private void SendDataToServer(string data)
        {
            // Define server connection details
            const string serverAddress = "127.0.0.1";
            const int serverPort = 8080;

            // Connect to the server and send data
            using (TcpClient client = new TcpClient(serverAddress, serverPort))
            using (NetworkStream stream = client.GetStream())
            {
                byte[] message = Encoding.UTF8.GetBytes(data);
                stream.Write(message, 0, message.Length);

                // Receive server response
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                MessageBox.Show($"Server Response: {response}", "Response");
            }
        }
    }
}