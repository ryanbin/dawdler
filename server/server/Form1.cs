using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace server
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        const int MOUSEEVENTF_MOVE = 0x0001;   //   移动鼠标 
        const int MOUSEEVENTF_LEFTDOWN = 0x0002; //模拟鼠标左键按下 
        const int MOUSEEVENTF_LEFTUP = 0x0004; //模拟鼠标左键抬起 
        const int MOUSEEVENTF_RIGHTDOWN = 0x0008;// 模拟鼠标右键按下 
        const int MOUSEEVENTF_RIGHTUP = 0x0010; //模拟鼠标右键抬起 
        const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;// 模拟鼠标中键按下 
        const int MOUSEEVENTF_MIDDLEUP = 0x0040;// 模拟鼠标中键抬起 
        const int MOUSEEVENTF_ABSOLUTE = 0x8000; //标示是否采用绝对坐标 
        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern int mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        private Socket serverSocket;
        private Thread client_Thread;
        private Thread server_Thread;
        private Socket client_Socket;
        private void btn_start_Click(object sender, EventArgs e)
        {
            btn_start.Enabled = false;
            server_Thread = new Thread(new ThreadStart(ServerStart));
            server_Thread.IsBackground = true;
            server_Thread.Start();
        }
        private void btn_stop_Click(object sender, EventArgs e)
        {
            client_Socket.Close();
            client_Thread.Abort();
            serverSocket.Close();
            server_Thread.Abort();
            btn_start.Enabled = true;

        }


        private void ServerStart()
        {
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("192.168.1.8"), 5656);
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(ipep);
            serverSocket.Listen(10);
            while (true)
            {
                try
                {
                    client_Socket = serverSocket.Accept();
                    client_Thread = new Thread(new ThreadStart(ReceiveAndroidData));
                    client_Thread.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("start error: " + ex.Message);
                    break;
                }
            }
        }
        public static int bytesToInt(byte[] bytes)
        {

            int addr = bytes[0] & 0xFF;

            addr |= ((bytes[1] << 8) & 0xFF00);

            addr |= ((bytes[2] << 16) & 0xFF0000);

            addr |= (int)((bytes[3] << 24) & 0xFF000000);

            return addr;

        }

        private void ReceiveAndroidData()
        {
            bool keepalive = true;
            Socket socketclient = client_Socket;
            Byte[] buffer = new Byte[1024];

            //根据收听到的客户端套接字向客户端发送信息
            IPEndPoint clientep = (IPEndPoint)socketclient.RemoteEndPoint;

            string str = "connect server----- ";
            byte[] data = new byte[1024];
            byte[] tmp_data = new byte[1024];
            data = Encoding.ASCII.GetBytes(str);
            socketclient.Send(data, data.Length, SocketFlags.None);
            int cd_长度 = 0;
            int py_偏移 = 0;
            int is_sy = 0;
            List<string> ls = new List<string>();
            while (keepalive)
            {
                //在套接字上接收客户端发送的信息
                int buffer_lenght = 0;
                try
                {
                    buffer_lenght = socketclient.Available;
                    if (is_sy > 0 && buffer_lenght>0)
                    {
                        tmp_data.CopyTo(buffer, 0);
                    }
                    socketclient.Receive(buffer, is_sy, buffer_lenght, SocketFlags.None);
                    if (is_sy > 0 && buffer_lenght > 0)
                    {
                        buffer_lenght += is_sy;
                    }
                    if (buffer_lenght == 0)
                        continue;
                }
                catch (Exception ex)
                {
                   // MessageBox.Show("receive error:" + ex.Message);
                    socketclient.Close();
                    return;
                }
                //clientep = (IPEndPoint)socketclient.RemoteEndPoint;

                byte[] cd = new byte[4];

                Buffer.BlockCopy(buffer, 0, cd, 0, 4);//复制包头
                py_偏移 = 4;
                cd_长度 = BitConverter.ToInt32(cd, 0);
                int ii = 0;
                is_sy = 0;
                if (buffer_lenght - 4 > cd_长度)//还存在另外一个包
                {
                    Buffer.BlockCopy(buffer, py_偏移, tmp_data, 0, cd_长度);//复制包体
                    ls.Add(System.Text.Encoding.ASCII.GetString(tmp_data).Substring(0, cd_长度));
                    py_偏移 += cd_长度;
                    do
                    {
                        if (buffer_lenght == py_偏移)
                        {
                            break;//没有了
                        }
                        Buffer.BlockCopy(buffer, py_偏移, cd, 0, 4);//复制包头
                        py_偏移 += 4;
                        cd_长度 = Convert.ToInt32(BitConverter.ToUInt32(cd, 0));
                        if (buffer_lenght - py_偏移 - cd_长度 < 0)  //有下一个包的长度,但是包体不够大
                        {
                            Buffer.BlockCopy(buffer, py_偏移-4, tmp_data, 0, buffer_lenght - py_偏移 + 4);//复制包体
                            is_sy = buffer_lenght - py_偏移+4;
                            break;
                        }
                        Buffer.BlockCopy(buffer, py_偏移, tmp_data, 0, cd_长度);//复制包体
                        ls.Add(System.Text.Encoding.ASCII.GetString(tmp_data).Substring(0, cd_长度));
                        py_偏移 +=  cd_长度;
                        if (buffer_lenght - py_偏移 < 4 && buffer_lenght - py_偏移>0) //包头长度小于4
                        {
                            Buffer.BlockCopy(buffer, py_偏移 , tmp_data, 0, buffer_lenght - py_偏移 );//复制包体
                            is_sy = buffer_lenght - py_偏移;
                            break;
                        }
                    } while (true);
                }
                else if (buffer_lenght - 4 == cd_长度)//刚刚好一个包
                {
                    Buffer.BlockCopy(buffer, py_偏移, tmp_data, 0, cd_长度);//复制包体
                    ls.Add(System.Text.Encoding.ASCII.GetString(tmp_data).Substring(0, cd_长度));
                }
                else if (buffer_lenght - 4 < cd_长度)//不足一个包
                {
                    Buffer.BlockCopy(buffer, 0, tmp_data, 0, buffer_lenght);//复制包体
                    is_sy = buffer_lenght;
                }

                for (int i = 0; i < ls.Count; i++)
                {

                    string[] s = ls[i].Split(',');
                    if (s.Length == 2)
                    {
                        mouse_event(MOUSEEVENTF_MOVE, Convert.ToInt32(float.Parse(s[0])), Convert.ToInt32(float.Parse(s[1])), 0, 0);
                    }
                
                }
                ls.Clear();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            int s = 100;
            byte[] shi = System.BitConverter.GetBytes(s);
            int sh = System.BitConverter.ToInt32(shi, 0); 
        }
    }
}
