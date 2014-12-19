AssetBundleBuilder
==================
AssetBundleBuilderはUnity3DのAssetBundleを作成して、そのバージョンとCRCを管理するためのライブラリです。

## Unityのバージョン

Unity 5 beta 14

## ビルド方法

- Unityエディタのメニューから「Build」→「Export AssetBundle」を実行する
- [BatchBuild/BuildAssetBundle.sh](https://github.com/kiyoaki/AssetBundleBuilder/blob/master/BatchBuild/BuildAssetBundle.sh)を実行する

ExportフォルダにビルドされたAssetBundleが配置され、そのビルドで追加または変更されたAssetBundleはUpdatedフォルダにコピーされます。

## 設定の確認と変更

- Unityエディタのメニューから「Edit」→「Show AssetBundle Build Settings」を実行する

UnityエディタのInspectorで値を変更すると設定が反映されます。
