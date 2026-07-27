#放置道具
#2025.07.05
#2026.07.27

from Lawn import *
from Sexy import *
from Sexy.TodLib import *

app=GlobalStaticVars.gLawnApp
board=app.mBoard
if board==None:
    app.DoDialog(16,True,"ERROR!","未找到board进程","OK",3)
else:
    col_in = {COL}
    row_in = {ROW}
    gameObjectdeltaX = {DELTA_MX}
    gameObjectdeltaY = {DELTA_MY}
    gridItemType = GridItemType.{ITEM}
    scaryPotSeedType = SeedType.{SCARYPOT_SEEDTYPE} if gridItemType == GridItemType.ScaryPot else None
    scaryPotZombieType = ZombieType.{SCARYPOT_ZOMBIETYPE} if gridItemType == GridItemType.ScaryPot else None
    scaryPotScaryPotType = ScaryPotType({SCARYPOT_SCARYPOTTYPE}) if gridItemType == GridItemType.ScaryPot else None
    scaryPotState = GridItemState({SCARYPOT_STATE}) if gridItemType == GridItemType.ScaryPot else None

    X_MAX = Constants.GRIDSIZEX
    Y_MAX = Constants.MAX_GRIDSIZEY
    if not board.StageHas6Rows():
        Y_MAX = 5

    allCol = (col_in == -666)
    allRow = (row_in == -666)

    def place_grid_item(item_x, item_y):
        newGridItem = GridItem.GetNewGridItem()
        newGridItem.mGridItemType = gridItemType
        if gridItemType == GridItemType.Gravestone:
            newGridItem.mGridItemCounter = 100
            newGridItem.mRenderOrder = Board.MakeRenderOrder(RenderLayer.GraveStone, item_y, 3)
        elif gridItemType == GridItemType.Crater:
            newGridItem.mRenderOrder = 200000 + 10000 * item_y + 1
            newGridItem.mGridItemCounter = GameConstants.CRATER_TIME
        elif gridItemType == GridItemType.Ladder:
            newGridItem.mRenderOrder = 302000 + 10000 * item_y + 800
        elif gridItemType == GridItemType.PortalCircle or gridItemType == GridItemType.PortalSquare:
            newGridItem.mGridX = item_x
            newGridItem.mGridY = item_y
            newGridItem.mRenderOrder = Board.MakeRenderOrder(RenderLayer.Particle, item_y, 0)
            newGridItem.OpenPortal()
        elif gridItemType == GridItemType.Brain:
            newGridItem.mRenderOrder = 400000
            newGridItem.mGridItemCounter = 0
            newGridItem.mPosX = board.GridToPixelX(item_x, item_y) - 15.0
            newGridItem.mPosY = board.GridToPixelY(item_x, item_y) - 15.0
        elif gridItemType == GridItemType.ScaryPot:
            newGridItem.mRenderOrder = Board.MakeRenderOrder(RenderLayer.Plant, item_y, 0)
            newGridItem.mSeedType = scaryPotSeedType
            newGridItem.mZombieType = scaryPotZombieType
            newGridItem.mScaryPotType = scaryPotScaryPotType
            newGridItem.mGridItemState = scaryPotState
            if newGridItem.mScaryPotType == ScaryPotType.Sun:
                newGridItem.mSunCount = TodCommon.RandRangeInt(1, 3)
        elif gridItemType == GridItemType.IzombieBrain:
            newGridItem.mRenderOrder = Board.MakeRenderOrder(RenderLayer.Plant, item_y, 0)
            newGridItem.mGridItemCounter = 70
            newGridItem.mPosX = board.GridToPixelX(item_x, item_y) - 40.0
            newGridItem.mPosY = board.GridToPixelY(item_x, item_y) + 40.0
        elif gridItemType == GridItemType.Rake:
            newGridItem.mPosX = board.GridToPixelX(item_x, item_y)
            newGridItem.mPosY = board.GridToPixelY(item_x, item_y)
            newGridItem.mRenderOrder = Board.MakeRenderOrder(RenderLayer.GraveStone, item_y, 9)
            theReanimation = board.CreateRakeReanim(newGridItem.mPosX, newGridItem.mPosY, 0)
            newGridItem.mGridItemReanimID = app.ReanimationGetID(theReanimation)
            newGridItem.mGridItemState = GridItemState.RakeAttracting
        elif gridItemType == GridItemType.Talisman:
            num = board.GridToPixelX(item_x, item_y)
            num2 = board.GridToPixelY(item_x, item_y) - 30
            reanimation = app.AddReanimation(num, num2, 0, ReanimationType.Talisman)
            reanimation.PlayReanim("anim_overdue_souls", ReanimLoopType.Loop, 10, 12.0)
            newGridItem.mGridItemReanimID = app.ReanimationGetID(reanimation)
            newGridItem.mGridItemCounter = 3000

        newGridItem.mGridX = item_x
        newGridItem.mGridY = item_y
        if not (gameObjectdeltaX == 0 and gameObjectdeltaY == 0):
            newGridItem.mX += gameObjectdeltaX
            newGridItem.mY += gameObjectdeltaY

        board.mGridItems.Add(newGridItem)

    if allCol and allRow:
        for row in range(Y_MAX):
            for col in range(X_MAX):
                place_grid_item(col, row)
    elif allRow:
        col = col_in - 1
        for row in range(Y_MAX):
            place_grid_item(col, row)
    elif allCol:
        row = row_in - 1
        for col in range(X_MAX):
            place_grid_item(col, row)
    else:
        col = col_in - 1
        row = row_in - 1
        place_grid_item(col, row)
