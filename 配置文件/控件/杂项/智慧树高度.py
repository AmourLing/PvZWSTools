#智慧树高度
#为什么会有这个文件？

from Lawn import *
from Sexy import *

TREE_HEIGHT = {TREEHEIGHT}

app = GlobalStaticVars.gLawnApp
board=app.mBoard

AppVersionNumber = app.AppVersionNumber
if "PGvZ" in AppVersionNumber:
    app.mPlayerInfo.mChallengeRecords[48]=TREE_HEIGHT-1
    board.mChallenge.TreeOfWisdomGrow()
