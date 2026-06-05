#红眼与飞贼的特殊处理
#2025.07.05

BUNGEE_FLAG_CHECK = "{BUNGEE_CHECK}"
REDEYE_FLAG_CHECK = "{REDEYE_CHECK}"

from Lawn import *
from Sexy import *
from Sexy.TodLib import *
from LawnMod import MonoModUtils as M


@M.HookTo(Board.PickZombieType)
def Board_PickZombieType(orig,self,theZombiePoints,theWaveIndex,theZombiePicker):
    num = 0
    for i in range(len(Board.aZombieWeightArray)):
        Board.aZombieWeightArray[i].Reset()
    
    for zombie_type in range(int(ZombieType.ZombieTypesCount)):
        zombie_def = Zombie.GetZombieDefinition(ZombieType(zombie_type))      
        if not self.mZombieAllowed[zombie_type]:
            continue

        if zombie_type == int(ZombieType.Bungee) and self.mApp.IsSurvivalEndless(self.mApp.mGameMode):
            if not (BUNGEE_FLAG_CHECK=="1"):
                if not self.IsFlagWave(theWaveIndex):
                    continue
        elif self.mApp.mGameMode not in [GameMode.ChallengePogoParty,GameMode.ChallengeBobsledBonanza,GameMode.ChallengeAirRaid]:
            first_wave = zombie_def.mFirstAllowedWave
            if self.mApp.IsSurvivalEndless(self.mApp.mGameMode):
                flags_completed = self.GetSurvivalFlagsCompleted()
                wave_adjust = TodCommon.TodAnimateCurve(18, 50, flags_completed, 0, 15, TodCurves.Linear)
                first_wave = max(first_wave - wave_adjust, 1)           
            if theWaveIndex + 1 < first_wave or theZombiePoints < zombie_def.mZombieValue:
                continue
        
        pick_weight = zombie_def.mPickWeight
        
        if self.mApp.IsSurvivalMode():
            flags_completed = self.GetSurvivalFlagsCompleted()
            
            if zombie_type in [int(ZombieType.Gargantuar), int(ZombieType.Zamboni)]:
                max_count = TodCommon.TodAnimateCurve(10, 50, flags_completed, 2, 50, TodCurves.Linear)
                if theZombiePicker.mZombieTypeCount[zombie_type] >= max_count:
                    continue
            
            if zombie_type == int(ZombieType.RedeyeGargantuar):
                if self.IsFlagWave(theWaveIndex):
                    max_count = TodCommon.TodAnimateCurve(14, 100, flags_completed, 1, 50, TodCurves.Linear)
                    if theZombiePicker.mZombieTypeCount[zombie_type] >= max_count:
                        continue
                else:
                    max_count = TodCommon.TodAnimateCurve(10, 110, flags_completed, 1, 50, TodCurves.Linear)
                    if not (REDEYE_FLAG_CHECK=="1"):
                        if theZombiePicker.mAllWavesZombieTypeCount[zombie_type] >= max_count:
                            continue
                    pick_weight = 1000
            if zombie_type == int(ZombieType.Normal):
                pick_weight = TodCommon.TodAnimateCurve(10, 50, flags_completed, zombie_def.mPickWeight, zombie_def.mPickWeight // 10, TodCurves.Linear)
            
            if zombie_type == int(ZombieType.TrafficCone):
                pick_weight = TodCommon.TodAnimateCurve(10, 50, flags_completed, zombie_def.mPickWeight, zombie_def.mPickWeight // 4, TodCurves.Linear)
        
        Board.aZombieWeightArray[num].mItem = zombie_type
        Board.aZombieWeightArray[num].mWeight = pick_weight
        num += 1

    picked_index = int(TodCommon.TodPickFromWeightedArray(Board.aZombieWeightArray, num))
    return ZombieType(picked_index)