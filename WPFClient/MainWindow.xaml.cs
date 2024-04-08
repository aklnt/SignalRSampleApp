using System.Text;
using System.Windows;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks.Dataflow;

namespace WPFClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        HubConnection connection;
        public MainWindow()
        {
            InitializeComponent();
            btnSendMsg.IsEnabled= false;

            connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7030/chathub")
                .WithAutomaticReconnect()
                .Build();

            connection.Reconnecting += (sender) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var newMessage = "Attempting to reconnect...";
                    lstMsgs.Items.Add(newMessage);
                });

                return Task.CompletedTask;
            };

            connection.Reconnected += (sender) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var newMessage = "Reconnected to the server";
                    lstMsgs.Items.Clear();
                    lstMsgs.Items.Add(newMessage);
                });

                return Task.CompletedTask;
            };

            connection.Closed += (sender) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var newMessage = "Connection Closed";
                    lstMsgs.Items.Add(newMessage);
                    btnOpenConnection.IsEnabled = true;
                    btnSendMsg.IsEnabled = false;
                });

                return Task.CompletedTask;
            };
        }

        private async void btnOpenConnection_Click(object sender, RoutedEventArgs e)
        {
            connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var newMessage = $"{user} : {message}";
                    lstMsgs.Items.Add(newMessage);
                });
            });

            try
            {
                await connection.StartAsync();
                lstMsgs.Items.Add("Connection Started");
                btnOpenConnection.IsEnabled = false;
                btnSendMsg.IsEnabled = true;
            }
            catch (Exception ex)
            {
                   lstMsgs.Items.Add($"{ex.Message}");
            }
        }

        private async void btnSendMsg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await connection.InvokeAsync("SendMessage",
                    "WPF Client",txtMsgInput.Text);
            }
            catch (Exception ex)
            {
                lstMsgs.Items.Add($"{ex.Message}");
            }
        }
    }
}