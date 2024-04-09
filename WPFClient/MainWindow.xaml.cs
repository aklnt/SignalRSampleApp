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
        HubConnection counterConnection;
        public MainWindow()
        {
            InitializeComponent();
            btnSendMsg.IsEnabled= false;
            btnIncCounter.IsEnabled= false;

            connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7030/chathub")
                .WithAutomaticReconnect()
                .Build();

            counterConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7030/counterhub")
                .WithAutomaticReconnect()
                .Build();

            #region connection_Events
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
            #endregion
        }

        #region chat_btns...
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
                    "WPF Client", txtMsgInput.Text);
            }
            catch (Exception ex)
            {
                lstMsgs.Items.Add($"{ex.Message}");
            }
        }
        #endregion

        #region counter_btns...
        private async void btnOpenCounter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await counterConnection.StartAsync();
                btnOpenCounter.IsEnabled = false;
                btnIncCounter.IsEnabled = true;
            }
            catch (Exception ex)
            {
                lstMsgs.Items.Add($"{ex.Message}");
            }
        }

        private async void btnIncCounter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await counterConnection.InvokeAsync("AddToTotal",
                    "WPF Client", 1);
            }
            catch (Exception ex)
            {

                lstMsgs.Items.Add($"{ex.Message}");
            }
        } 
        #endregion
    }
}