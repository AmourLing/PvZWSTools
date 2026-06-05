#查看草坪
#2025.07.05

from Lawn import *
from Sexy import *
app=GlobalStaticVars.gLawnApp
try:
    app.mSeedChooserScreen.mChooseState = SeedChooserState.ViewLawn
    app.mSeedChooserScreen.mMenuButton.mDisabled = True
    app.mSeedChooserScreen.mViewLawnTime = 0
except Exception as e:
    app.DoDialog(16,True,"ERROR!",repr(e),"OK",3)