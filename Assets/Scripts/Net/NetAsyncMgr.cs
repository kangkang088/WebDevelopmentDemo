using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using GamePlayer;
using GameSystem;
using UnityEngine;

public class NetAsyncMgr : MonoBehaviour
{
    private static NetAsyncMgr instance;

    public static NetAsyncMgr Instance => instance;

    //�ͷ������������ӵ� Socket
    private Socket socket;

    //������Ϣ�õ� ��������
    private byte[] cacheBytes = new byte[1024 * 1024];
    private int cacheNum = 0;

    private Queue<BaseHandler> receiveQueue = new Queue<BaseHandler>();
    //��Ϣ�ض������ڿ��ٻ�ȡ��Ϣ�������Ϣ���������
    private MsgPool msgPool = new MsgPool();

    //����������Ϣ�ļ��ʱ��
    private int SEND_HEART_MSG_TIME = 2;
    private HeartMsg hearMsg = new HeartMsg();

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        //���������Ƴ�
        DontDestroyOnLoad(this.gameObject);
        //�ͻ���ѭ����ʱ������˷���������Ϣ
        InvokeRepeating("SendHeartMsg", 0, SEND_HEART_MSG_TIME);
    }

    private void SendHeartMsg()
    {
        if (socket != null && socket.Connected)
            Send(hearMsg);
    }

    // Update is called once per frame
    void Update()
    {
        if (receiveQueue.Count > 0)
        {
            // BaseMsg baseMsg = receiveQueue.Dequeue();
            // switch (baseMsg)
            // {
            //     case PlayerMsg msg:
            //         print(msg.playerID);
            //         // print(msg.playerData.name);
            //         // print(msg.playerData.lev);
            //         // print(msg.playerData.atk);
            //         break;
            // }
            receiveQueue.Dequeue().MsgHandler();

        }
    }

    //���ӷ������Ĵ���
    public void Connect(string ip, int port)
    {
        if (socket != null && socket.Connected)
            return;

        IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        SocketAsyncEventArgs args = new SocketAsyncEventArgs();
        args.RemoteEndPoint = ipPoint;
        args.Completed += (socket, args) =>
        {
            if (args.SocketError == SocketError.Success)
            {
                print("���ӳɹ�");
                //����Ϣ
                SocketAsyncEventArgs receiveArgs = new SocketAsyncEventArgs();
                receiveArgs.SetBuffer(cacheBytes, 0, cacheBytes.Length);
                receiveArgs.Completed += ReceiveCallBack;
                this.socket.ReceiveAsync(receiveArgs);
            }
            else
            {
                print("����ʧ��" + args.SocketError);
            }
        };
        socket.ConnectAsync(args);
    }

    //����Ϣ��ɵĻص�����
    private void ReceiveCallBack(object obj, SocketAsyncEventArgs args)
    {
        if (args.SocketError == SocketError.Success)
        {
            HandleReceiveMsg(args.BytesTransferred);
            //����ȥ����Ϣ
            args.SetBuffer(cacheNum, args.Buffer.Length - cacheNum);
            //�����첽����Ϣ
            if (this.socket != null && this.socket.Connected)
                socket.ReceiveAsync(args);
            else
                Close();
        }
        else
        {
            print("������Ϣ����" + args.SocketError);
            //�رտͻ�������
            Close();
        }
    }

    public void Close(bool isSelf = false)
    {
        if (socket != null)
        {
            QuitMsg msg = new QuitMsg();
            socket.Send(msg.Writing());
            socket.Shutdown(SocketShutdown.Both);
            socket.Disconnect(false);
            socket.Close();
            socket = null;
        }
        if (!isSelf)
        {

        }
    }

    public void SendTest(byte[] bytes)
    {
        SocketAsyncEventArgs args = new SocketAsyncEventArgs();
        args.SetBuffer(bytes, 0, bytes.Length);
        args.Completed += (socket, args) =>
        {
            if (args.SocketError != SocketError.Success)
            {
                print("������Ϣʧ��" + args.SocketError);
                Close();
            }

        };
        this.socket.SendAsync(args);
    }

    public void Send(BaseMsg msg)
    {
        if (this.socket != null && this.socket.Connected)
        {
            byte[] bytes = msg.Writing();
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.SetBuffer(bytes, 0, bytes.Length);
            args.Completed += (socket, args) =>
            {
                if (args.SocketError != SocketError.Success)
                {
                    print("������Ϣʧ��" + args.SocketError);
                    Close();
                }

            };
            this.socket.SendAsync(args);
        }
        else
        {
            Close();
        }
    }

    //����������Ϣ �ְ���������ķ���
    private void HandleReceiveMsg(int receiveNum)
    {
        int msgID = 0;
        int msgLength = 0;
        int nowIndex = 0;

        cacheNum += receiveNum;

        while (true)
        {
            //ÿ�ν���������Ϊ-1 �Ǳ�����һ�ν��������� Ӱ����һ�ε��ж�
            msgLength = -1;
            //��������һ����Ϣ
            if (cacheNum - nowIndex >= 8)
            {
                //����ID
                msgID = BitConverter.ToInt32(cacheBytes, nowIndex);
                nowIndex += 4;
                //��������
                msgLength = BitConverter.ToInt32(cacheBytes, nowIndex);
                nowIndex += 4;
            }

            if (cacheNum - nowIndex >= msgLength && msgLength != -1)
            {
                //������Ϣ��
                // BaseMsg baseMsg = null;
                // BaseHandler handler = null;
                // switch (msgID)
                // {
                //     case 1001:
                //         baseMsg = new PlayerMsg();
                //         baseMsg.Reading(cacheBytes, nowIndex);

                //         handler = new PlayerMsgHanlder();
                //         handler.message = baseMsg;
                //         break;
                // }
                // if (baseMsg != null)
                //     receiveQueue.Enqueue(handler);

                BaseMsg baseMsg = msgPool.GetMessage(msgID);
                if (baseMsg != null)
                {
                    baseMsg.Reading(cacheBytes, nowIndex);
                    BaseHandler baseHandler = msgPool.GetHandler(msgID);
                    baseHandler.message = baseMsg;
                    receiveQueue.Enqueue(baseHandler);
                }


                nowIndex += msgLength;
                if (nowIndex == cacheNum)
                {
                    cacheNum = 0;
                    break;
                }
            }
            else
            {
                if (msgLength != -1)
                    nowIndex -= 8;
                //���ǰ�ʣ��û�н������ֽ��������� �Ƶ�ǰ���� ���ڻ����´μ�������
                Array.Copy(cacheBytes, nowIndex, cacheBytes, 0, cacheNum - nowIndex);
                cacheNum = cacheNum - nowIndex;
                break;
            }
        }

    }

    private void OnDestroy()
    {
        Close(true);
    }
}