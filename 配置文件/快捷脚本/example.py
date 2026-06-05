from Lawn import *
from Sexy import *

app = GlobalStaticVars.gLawnApp

#获取输入的参数
para0,para1,para2,para3,para4={0},{1},{2},{3},{4}
para5={5}
para6={6}
para7={7}

outputHeadstr = "这个例子貌似很成功"
outputBodystr = f"输入的参数分别是:0->{para0},1->{para1}"
print(f"{outputHeadstr}\n{outputBodystr}")

print(para0,para1)
app.DoDialog(16,True,outputHeadstr,outputBodystr,"OK",3)
