# 发酵罐仿真系统

<a name="Secxd"></a>

## ⚡ 预排工期：7 天\*8 小时

<a name="F0ibZ"></a>

## 一.开发注意事项

产品号：**583419**<br />设备号：**1059893029 **设备名称：**mqtt-can1**

- **API 访问 token**
  - res: products/{**产品号**}
  - 用于签名的 Key 为 **access_key**
- **MQTT 访问 token**
  - res：product/{**产品号}**/devices/{**设备名称}**
  - 用于签名的 Key 为 **device_key**

<a name="rZ23g"></a>

## 二.发酵罐传感器开发(基于 C#)

- 1.✅ 根据范围随机产生传感器的属性值
- 产生数据
- 数据转 JSON
- 调用 MQTT publish

- 2.✅ 从**ONENET**中订阅**状态信息**

  - C#设备订阅相关 Topic 选用何种 Topic？

    - 在平台设置设备镜像
    - 使用镜像的各 topic 同步状态信息
    - ⭕ 初始化时如何设计状态的同步信息

      - 和传感器数据一起执行？

        3.**本地/远程控制模式**设计<br />注：发送的 DELTA 信息为**D**esired 的值<br />![控制逻辑v1.1.png](https://cdn.nlark.com/yuque/0/2023/png/29532050/1679475750956-b5c9249c-96b9-47e0-a592-563dca5d9457.png#averageHue=%23f8f6f5&clientId=u12828290-50d0-4&from=paste&height=674&id=ud6062166&name=%E6%8E%A7%E5%88%B6%E9%80%BB%E8%BE%91v1.1.png&originHeight=842&originWidth=762&originalType=binary&ratio=1.25&rotation=0&showTitle=false&size=65281&status=done&style=none&taskId=u64e3d911-1867-4cab-bf94-75f6329485f&title=&width=609.6)

![image.png](https://cdn.nlark.com/yuque/0/2023/png/29532050/1679451140323-85144c98-8afb-4a93-8854-a2e7b757ec7d.png#averageHue=%23e8e7e5&clientId=u12828290-50d0-4&from=paste&height=304&id=u8f1014d8&name=image.png&originHeight=380&originWidth=906&originalType=binary&ratio=1.25&rotation=0&showTitle=false&size=45558&status=done&style=none&taskId=u71fc5965-ec94-49f8-a6e3-e3e21b0f5c4&title=&width=724.8)

- 4.✅ 模糊集合库的使用

  ⭐ 定义整个系统类<br />每个**FussySystem**可以有不同的**INPUT/OUTPUT**，每个**INPUT/OUTPUT**定义为一个**FuzzyVariable**<br />每个**FuzzyVariable**有不同的**描述词，**每个描述定义为一个**FuzzyTerm**<br />每个**FuzzyTerm**有如下参数

  - **TermName** 描述名词
  - **mf 隶属度**
    - 隶属度通常用**MembershipFunction**计算得出

  每个**FussySystem**对应多个**FuzzyRule**<br />每个**FuzzyRule**需要使用**ParseRule**进行转换<br /> ⭐ 使用类实例进行控制<br />![image.png](https://cdn.nlark.com/yuque/0/2023/png/29532050/1679553352140-bee79972-f35c-48d5-a5a4-b6fbbbf20b74.png#averageHue=%23fef8f8&clientId=u063ed6c6-2bdd-4&from=paste&height=474&id=u942667ae&name=image.png&originHeight=593&originWidth=765&originalType=binary&ratio=1.25&rotation=0&showTitle=false&size=62370&status=done&style=none&taskId=u82703058-1b98-488c-8dac-abfe7fd11f4&title=&width=612)<br />![image.png](https://cdn.nlark.com/yuque/0/2023/png/29532050/1679551279731-83e1a558-a7f7-4ae0-9de1-5dfd1e064996.png#averageHue=%23b3b2b1&clientId=u063ed6c6-2bdd-4&from=paste&height=184&id=ueb679e03&name=image.png&originHeight=230&originWidth=787&originalType=binary&ratio=1.25&rotation=0&showTitle=false&size=35173&status=done&style=none&taskId=u8115e731-cae3-4ece-929d-90ff5e357b4&title=&width=629.6)

- 模糊规则的定义 - 隶属度的定义：描述某个确定量隶属于某个模糊语言的程度

- 5.✅ 远程控制接口开发
  - 接收远程状态变化 **DELTA**
  - **DELTA** 转发至 **Remote_handleDelta**
  - **Remote_handleDelta** 进行各 state 同步
    - 首先判断控制模式【controlState 是否改变】
      - 若改变则 **EmitControlChangeEvent** 上报，切换控制模式
    - 若未改变，将每一个存在 DELTA 的字段使用 **EmitStateChangeEvent** 上报
    - 最后将同步后的本地字段使用 **EmitDeltaStateChangeEvent** 上报
    - **EmitDeltaStateChangeEvent** 委托 **State_Publish** 将数据同步到 **_ONENET_** 镜像

<a name="hSWnb"></a>

## 三.远端控制器开发(基于 Vue2 暂定)
