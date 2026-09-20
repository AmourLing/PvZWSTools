@echo off
setlocal
cd /d "%~dp0"
title PvZWSTools 使用手册生成

if not exist "手册内容.md" (
    echo [错误] 未找到 手册内容.md,请把本脚本放在 文档 目录下运行。
    echo.
    pause
    exit /b 1
)

set "PY=C:\Users\AmourLing\miniconda3\python.exe"
if not exist "%PY%" (
    for /f "delims=" %%p in ('where python 2^>nul') do set "PY=%%p"
)
if not exist "%PY%" (
    echo [错误] 未找到可用的 Python,请先安装 Python 或修改本脚本中的 PY 路径。
    echo.
    pause
    exit /b 1
)

echo [1/2] 解释器: %PY%
echo [2/2] 正在生成 使用手册.pdf ...
echo.
"%PY%" "_build\gen_manual.py"
if errorlevel 1 (
    echo.
    echo [失败] 生成出错,请把上方报错信息发给开发者,或检查 手册内容.md 是否改坏了格式。
) else (
    echo.
    echo [完成] 已生成: %~dp0使用手册.pdf
    if exist "%~dp0使用手册_新.pdf" (
        echo [提示] 原 PDF 正被占用,新版保存在 使用手册_新.pdf,关闭阅读器后改名即可。
    )
)
echo.
pause
