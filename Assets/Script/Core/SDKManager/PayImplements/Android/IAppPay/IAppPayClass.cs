﻿using FrameWork.SDKManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class IAppPayClass : PayInterface
{
    public string appid;
    public string mchID;
    public string appSecret;
    string goodsID;
    string mch_orderID;
    GameObject androidListener;
    public override List<RuntimePlatform> GetPlatform()
    {
        return new List<RuntimePlatform>() { RuntimePlatform.Android, RuntimePlatform.WindowsEditor};
    }
    public override StoreName GetStoreName()
    {
        return StoreName.IAppPay;
    }
    public override void Init()
    {
        m_SDKName = StoreName.IAppPay.ToString();

        Debug.LogWarning("=========IAppPayClass Init===========");
        //SDKManagerNew.OnPayCallBack += SetPayResult;
        GlobalEvent.AddTypeEvent<PrePay2Client>(OnPrePay);

        appid = SDKManager.GetProperties(StoreName.IAppPay.ToString(), "AppID", appid);
        mchID = SDKManager.GetProperties(StoreName.IAppPay.ToString(), "MchID", mchID);
        appSecret = SDKManager.GetProperties(StoreName.IAppPay.ToString(), "AppSecret", appSecret);
    }

    /// <summary>
    /// 获得预支付订单
    /// </summary>
    /// <param name="e"></param>
    /// <param name="args"></param>
    private void OnPrePay(PrePay2Client e, object[] args)
    {
        if (e.storeName != StoreName.IAppPay)
        {
            return;
        }
        Debug.LogWarning("OnPrePay=========：" + e.prepay_id + "=prepay_id==");
        //DateTime dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        string nonceStr = "";
        string timeStamp = "";

        string sign = "";

        OnPayInfo onPayInfo = new OnPayInfo();
        onPayInfo.isSuccess = true;
        onPayInfo.goodsId = e.goodsID;
        onPayInfo.storeName = StoreName.IAppPay;
        onPayInfo.receipt = e.mch_orderID;
        PayReSend.Instance.AddPrePayID(onPayInfo);
        IndentListener(e.mch_orderID, e.prepay_id, nonceStr, timeStamp, sign);
    }

    /// <summary>
    /// 统一支付入口
    /// </summary>
    /// <param name="goodsID"></param>
    /// <param name="tag"></param>
    /// <param name="goodsType"></param>
    /// <param name="orderID"></param>
    public override void Pay(string goodsID, string tag, FrameWork.SDKManager.GoodsType goodsType = FrameWork.SDKManager.GoodsType.NORMAL, string orderID = null)
    {
        this.goodsID = goodsID;
        
        Debug.LogWarning("send IAppPay----message-----" + goodsID);
        //给服务器发消息1
        PrePay2Service.SendPrePayMsg(StoreName.IAppPay, goodsID);
    }


    /// <summary>
    /// 消息1 的监听， 获得订单信息，然后调支付sdk
    /// </summary>
    private void IndentListener(string mch_orderID,string prepay_id, string nonceStr ,string timeStamp , string sign)
    {
        this.mch_orderID = mch_orderID;

        string tag = prepay_id + "|" + nonceStr + "|" + timeStamp + "|" + sign;
        PayInfo payInfo = new PayInfo(prepay_id, "", tag, FrameWork.SDKManager.GoodsType.NORMAL, "", 0, GetGoodsInfo(goodsID).isoCurrencyCode);

        SDKManagerNew.Pay(StoreName.IAppPay.ToString(), payInfo);
    }

    /// <summary>
    /// 从sdk返回支付结果
    /// </summary>
    /// <param name="result"></param>
    /// <param name="goodID"></param>
    /// <param name="Mch_orderID"></param>
    public void SetPayResult(OnPayInfo info)
    {
        Debug.LogWarning("从sdk返回支付结果.storeName======" + info.storeName);
        if (info.storeName != StoreName.IAppPay)
        {
            return;
        }
        Debug.LogWarning("IAppPay Result======" + info.isSuccess);

        var goodsInfo =  GetGoodsInfo(goodsID);

        OnPayInfo payInfo = new OnPayInfo();
        payInfo.isSuccess = info.isSuccess;
        payInfo.goodsId = goodsID;
        payInfo.orderID = mch_orderID;
        payInfo.goodsType = GetGoodType(goodsID);
        payInfo.storeName = StoreName.IAppPay;

        payInfo.currency = goodsInfo.isoCurrencyCode;
        payInfo.price = (float)goodsInfo.localizedPrice;

        Debug.Log("SetPayResult " + payInfo.price + " goodsID " + goodsID);

        payInfo.receipt = mch_orderID;
        
        PayCallBack(payInfo);
    }

    public override void ConfirmPay(string goodsID, string mch_orderID)
    {
        PayReSend.Instance.ClearPrePayID(mch_orderID);
    }

}
