#!/bin/sh

UNITY_APP_PATH="/Applications/Unity/Unity.app/Contents/MacOS/Unity"
TARGET_PROJECT_PATH="/XXX/jenkins/jobs/AssetBundle/workspace"
BUILD_METHOD="AssetBundleBuilder.ExportAssetBundle"

UPLOAD_TARGET_DIR="$TARGET_PROJECT_PATH/Updated"
UPLOAD_TARGET_FILE_EXTENSION=".unity3d"
ASSET_BUNDLE_VERSION_FILE_EXTENSION=".txt"

#Azure Storage Sample
#AZURE_STORAGE_BLOB_ACCOUNT_NAME="XXXX"
#AZURE_STORAGE_BLOB_ACCOUNT_KEY="XXXX"
#AZURE_STORAGE_BLOB_CONTAINER="XXXX"
#AZURE_STORAGE_BLOB_PROPERTIES="cacheControl=max-age=86400"

BuildAssetBundle() {
    $UNITY_APP_PATH -batchmode -quit -projectPath $TARGET_PROJECT_PATH -executeMethod $BUILD_METHOD -logFile ./build.log
    local readonly exitCode=$?  
    cat ./build.log
    if [ $exitCode -ne 0 ]; then
        exit $exitCode
    fi
}

#UploadAssetBundle() {
#    for f in $(find $UPLOAD_TARGET_DIR -type f -name *$UPLOAD_TARGET_FILE_EXTENSION -or -name *$ASSET_BUNDLE_VERSION_FILE_EXTENSION) ; do
#        azure storage blob upload -f $f -b ${f#$UPLOAD_TARGET_DIR/} -p $AZURE_STORAGE_BLOB_PROPERTIES --container $AZURE_STORAGE_BLOB_CONTAINER -a $AZURE_STORAGE_BLOB_ACCOUNT_NAME -k $AZURE_STORAGE_BLOB_ACCOUNT_KEY >./upload.log
#    done
#    
#    local readonly exitCode=$?  
#    cat ./upload.log
#    if [ $exitCode -ne 0 ]; then
#        exit $exitCode
#    fi
#}

BuildAssetBundle
#UploadAssetBundle
