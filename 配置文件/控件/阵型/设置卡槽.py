#设置卡槽
#2025.07.05

from Lawn import *
from Sexy import *

app=GlobalStaticVars.gLawnApp
board=app.mBoard
if board==None:
    app.DoDialog(16,True,"ERROR!","未找到board进程","OK",3)
else:
    seedPacketNum = {SPNUM}
    seedType = SeedType.{ST}
    imitaterType = SeedType["None"]
    if {ITCHECK}:
        imitaterType = seedType
        seedType = SeedType.Imitater
    board.mSeedBank.mSeedPackets[seedPacketNum-1].SetPacketType(seedType,imitaterType)
