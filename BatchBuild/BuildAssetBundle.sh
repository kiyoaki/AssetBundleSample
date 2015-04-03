#!/bin/sh

UNITY_APP_PATH="/Applications/Unity/Unity.app/Contents/MacOS/Unity"
TARGET_PROJECT_PATH="/XXX/jenkins/jobs/AssetBundle/workspace"
BUILD_METHOD="BuildScript.BuildAssetBundles"

UPLOAD_TARGET_DIR="$TARGET_PROJECT_PATH/AssetBundles"
AZURE_STORAGE_BLOB_ACCOUNT_NAME="XXXXX"
AZURE_STORAGE_BLOB_ACCOUNT_KEY="XXXXX"
AZURE_STORAGE_BLOB_CONTAINER="XXXXX"
AZURE_STORAGE_BLOB_PROPERTIES="cacheControl=max-age=86400"

BuildAssetBundle() {
    $UNITY_APP_PATH -batchmode -quit -projectPath $TARGET_PROJECT_PATH -executeMethod $BUILD_METHOD -logFile ./build.log
    local readonly exitCode=$?  
    cat ./build.log
    if [ $exitCode -ne 0 ]; then
        exit $exitCode
    fi
}

UploadAssetBundle() {
    for f in $(find $UPLOAD_TARGET_DIR -type f) ; do
        azure storage blob upload -q -f $f -b ${f#$UPLOAD_TARGET_DIR/} -p $AZURE_STORAGE_BLOB_PROPERTIES --container $AZURE_STORAGE_BLOB_CONTAINER -a $AZURE_STORAGE_BLOB_ACCOUNT_NAME -k $AZURE_STORAGE_BLOB_ACCOUNT_KEY >./upload.log
    done
    
    local readonly exitCode=$?  
    cat ./upload.log
    if [ $exitCode -ne 0 ]; then
        exit $exitCode
    fi
}

BuildAssetBundle
UploadAssetBundle

