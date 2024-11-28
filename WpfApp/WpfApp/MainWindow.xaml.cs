using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using Microsoft.Win32.SafeHandles;

namespace DriverCommunicator
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string message = InputTextBox.Text;
            if (string.IsNullOrEmpty(message))
            {
                MessageBox.Show("Please enter a message to send to the driver.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                string response = CommunicateWithDriver(message);
                ResponseTextBox.Text = response;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to communicate with the driver.\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string CommunicateWithDriver(string message)
        {
            const string devicePath = @"\\.\MyDevice";
            const uint SIOCTL_TYPE = 40000;
            const uint METHOD_BUFFERED = 0;
            const uint FILE_READ_DATA = 0x0001;
            const uint FILE_WRITE_DATA = 0x0002;

            uint IOCTL_HELLO = (SIOCTL_TYPE << 16) | (FILE_READ_DATA | FILE_WRITE_DATA) << 14 | 0x800 << 2 | METHOD_BUFFERED;

            byte[] inputBuffer = Encoding.ASCII.GetBytes(message);
            byte[] outputBuffer = new byte[256];

            using (SafeFileHandle handle = CreateFile(
                devicePath,
                FILE_READ_DATA | FILE_WRITE_DATA,
                0,
                IntPtr.Zero,
                3, // OPEN_EXISTING
                0,
                IntPtr.Zero))
            {
                if (handle.IsInvalid)
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());

                uint bytesReturned;
                if (DeviceIoControl(
                    handle,
                    IOCTL_HELLO,
                    inputBuffer,
                    (uint)inputBuffer.Length,
                    outputBuffer,
                    (uint)outputBuffer.Length,
                    out bytesReturned,
                    IntPtr.Zero))
                {
                    return Encoding.ASCII.GetString(outputBuffer, 0, (int)bytesReturned);
                }
                else
                {
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                }
            }
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern SafeFileHandle CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool DeviceIoControl(
            SafeFileHandle hDevice,
            uint dwIoControlCode,
            byte[] lpInBuffer,
            uint nInBufferSize,
            byte[] lpOutBuffer,
            uint nOutBufferSize,
            out uint lpBytesReturned,
            IntPtr lpOverlapped);
    }
}