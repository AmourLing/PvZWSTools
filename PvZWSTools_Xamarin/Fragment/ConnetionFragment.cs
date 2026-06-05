using System;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;

namespace PvZWSTools_Xamarin
{
    public class ConnectionFragment:Fragment
    {
        private EditText editTextAddress;
        private Button buttonConnect;
        private CheckBox checkBoxAutoConnect;
        private MainActivity mainActivity;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            mainActivity = Activity as MainActivity;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.connection_fragment, container, false);

            editTextAddress = view.FindViewById<EditText>(Resource.Id.editText);
            buttonConnect = view.FindViewById<Button>(Resource.Id.button1);

            // 如果有保存的地址，自动填充
            var lastAddress = mainActivity?.GetLastWebSocketAddress();
            if(!string.IsNullOrEmpty(lastAddress))
            {
                editTextAddress.Text = lastAddress;
            }

            buttonConnect.Click += OnConnectButtonClick;

            return view;
        }

        private void OnConnectButtonClick(object sender, EventArgs e)
        {
            string address = editTextAddress.Text?.Trim();

            if(string.IsNullOrEmpty(address))
            {
                Toast.MakeText(Activity, "请输入WebSocket地址", ToastLength.Short).Show();
                return;
            }

            // 保存地址到设置
            mainActivity?.SaveWebSocketAddress(address);

            // 连接WebSocket
            if(MainActivity.ws != null)
            {
                if(MainActivity.ws.IsConnected)
                {
                    MainActivity.ws.Disconnect();
                    buttonConnect.Text = "连接";
                }
                else
                {
                    MainActivity.ws.Connect(address);
                    buttonConnect.Text = "断开连接";

                    // 如果启用了连接提醒，显示通知
                    if(mainActivity?.ShouldShowConnectionNotification() == true)
                    {
                        Toast.MakeText(Activity, "正在连接...", ToastLength.Short).Show();
                    }
                }
            }
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();
            buttonConnect.Click -= OnConnectButtonClick;
        }
    }
}
