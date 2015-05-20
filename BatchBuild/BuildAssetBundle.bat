chcp 65001
set UNITY_APP_PATH="C:\Program Files\Unity\Editor\Unity.exe"
set BUILD_METHOD="BuildScript.BuildAssetBundles"
%UNITY_APP_PATH% -batchmode -quit -projectPath "%WORKSPACE%" -executeMethod %BUILD_METHOD%
