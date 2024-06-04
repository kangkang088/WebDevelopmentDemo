using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 消息处理器基类
/// </summary>
public abstract class BaseHandler
{
    //处理者将要处理的对象
    public BaseMsg message;
    //处理消息的方法
    public abstract void MsgHandler();
}
