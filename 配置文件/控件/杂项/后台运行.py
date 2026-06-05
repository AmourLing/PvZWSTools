import clr
clr.AddReference("Lawn")
from Sexy import *
from LawnMod import MonoModUtils as M
import System

RunWhileLockedCheck = {CHECK}

#确保游戏在窗口非激活状态也能进行更新
@M.HookTo(Main.Update)
def Main_Update(orig,self,gameTime):
    if not self.IsActive and not RunWhileLockedCheck:
        return   
    if GlobalStaticVars.gSexyAppBase.WantsToExit:
        self.Exit()   
    self.HandleInput(gameTime)
    GlobalStaticVars.gSexyAppBase.UpdateApp()        
    try:
        game_type = System.Object.GetType().Assembly.GetType("Microsoft.Xna.Framework.Game")
        if game_type:
            update_method = game_type.GetMethod("Update", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            if update_method:
                update_method.Invoke(self, [gameTime])
    except:
        pass       

#确保游戏在窗口非激活状态时不会弹出游戏暂停
@M.HookTo(Main.OnDeactivated)
def Main_OnDeactivated(orig,self,sender,args):
    if RunWhileLockedCheck:
        app = GlobalStaticVars.gSexyAppBase
        if app and not app.mMusicInterface.isStopped:
            app.mMusicInterface.ResumeMusic()      
        return    
    orig(self, sender, args)

#确保游戏在窗口非激活状态时不会有输入（鼠标点击等）
@M.HookTo(Main.HandleInput)
def Main_HandleInput(orig,self,gameTime):
    if not self.IsActive:
        return
    orig(self,gameTime)