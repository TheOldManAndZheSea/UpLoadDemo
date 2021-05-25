# UpLoadDemo
可以通过xml文件进行更新和打开程序的一个更新程序。

## UpLoadVersion.xml配置更新文件说明
UpLoadVersion.xml需要放置在程序安装目录和服务器更新目录里，是重要的更新与对比的文件！

```
<UpLoadContent>更新内容</UpLoadContent>
  <UpLoadFileUrl>更新的地址（带输出目录的网站地址）</UpLoadFileUrl>
  <ProgrmStartupDir>更新完成后需要运行的程序</ProgrmStartupDir>
  <UpLoadFiles>
    <UpLoad>
      <Version>1.0.0.3 更新版本号必须是四位</Version>
      <FileName>xxx.exe更新文件名，本地没有更新后会自动添加，支持几乎所有的文件类型</FileName>
    </UpLoad>
  </UpLoadFiles>
  ```
  
## IIS发布目录
   * 一、添加网站
 > 物理路径选择你的最新程序更新目录
 > ![image](https://user-images.githubusercontent.com/30279211/119330347-b153de00-bcb8-11eb-9866-ca13bfb6dbd1.png)
   * 二、如果浏览网站报403

 > ![image](https://user-images.githubusercontent.com/30279211/119330943-6090b500-bcb9-11eb-8636-34490ef1c977.png)
 > 在网站的**功能目录**中点击**目录浏览**，然后点击目录浏览右侧的**启用**
 > ![image](https://user-images.githubusercontent.com/30279211/119331287-c67d3c80-bcb9-11eb-8ab6-6ceadb659f0e.png)

## 配置服务器的UpLoadVersion.xml文件
>如图，可以添加多个需要下载的文件，新文件只需添加一组UpLoad即可，旧文件需要更新，则在Version上相应的加一个版本即可。
>![image](https://user-images.githubusercontent.com/30279211/119331713-41deee00-bcba-11eb-9056-0a71a41438fd.png)

## 测试更新
> 如图，本地现在是没有文件和信息的
> 
> ![image](https://user-images.githubusercontent.com/30279211/119332267-eeb96b00-bcba-11eb-9c99-888c82c231f6.png)

>运行程序，会提示你更新
>
>![image](https://user-images.githubusercontent.com/30279211/119332326-01cc3b00-bcbb-11eb-862a-545b30575313.png)

>点击更新，开始下载，下载完成后会自动关闭更新程序，运行你**ProgrmStartupDir**里配置的程序
>
>![image](https://user-images.githubusercontent.com/30279211/119332369-101a5700-bcbb-11eb-8560-7a46a917103e.png)

> 让我们看一看配置文件里有啥更改，跟服务器里的配置文件变得一模一样了
> 
> ![image](https://user-images.githubusercontent.com/30279211/119332586-5ff91e00-bcbb-11eb-95a0-32d033760346.png)




