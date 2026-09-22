@echo off
chcp 65001 >nul
echo ============================================
echo   正在发布 PvZWSTools Android APK
echo ============================================

REM ---- 检查密钥文件是否存在 ----
if not exist "PvZWSTools.keystore" (
    echo [错误] 未找到密钥文件 PvZWSTools.keystore！
    echo 请将密钥文件放在当前目录：%cd%
    pause
    exit /b 1
)

REM ---- 检查环境变量是否已设置 ----
if "%ANDROID_STORE_PASS%"=="" (
    echo [错误] 环境变量 ANDROID_STORE_PASS 未设置！
    echo 请打开命令提示符，执行：
    echo   setx ANDROID_STORE_PASS "你的密码"
    echo 然后重新打开此窗口。
    pause
    exit /b 1
)
if "%ANDROID_KEY_PASS%"=="" (
    echo [错误] 环境变量 ANDROID_KEY_PASS 未设置！
    echo 请打开命令提示符，执行：
    echo   setx ANDROID_KEY_PASS "你的密码"
    echo 然后重新打开此窗口。
    pause
    exit /b 1
)

echo 正在编译并签名 APK，请稍候...
echo.

REM ---- 执行发布（密码从环境变量读取） ----
dotnet publish -c Release ^
    -p:AndroidKeyStore=true ^
    -p:AndroidSigningKeyStore="PvZWSTools.keystore" ^
    -p:AndroidSigningKeyAlias="PvZWSTools" ^
    -p:AndroidSigningStorePass="env:ANDROID_STORE_PASS" ^
    -p:AndroidSigningKeyPass="env:ANDROID_KEY_PASS" ^
    -p:CopyApkToPublishDirectory=true

if %errorlevel%==0 (
    echo.
    echo ============================================
    echo   ✅ 发布成功！
    echo   APK 位置：bin\Release\net10.0-android\publish\
    echo ============================================
) else (
    echo.
    echo ============================================
    echo   ❌ 发布失败，请检查上方错误信息。
    echo ============================================
)
pause