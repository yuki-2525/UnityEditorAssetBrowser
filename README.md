# Unity Editor Asset Browser

AvatarExplorerとKonoAssetのデータベースを統合して表示・管理するUnity Editor拡張です。

## 機能

- AvatarExplorerとKonoAssetのデータベースを統合表示
- アバター、アバター関連アイテム、ワールドの3つのタブで管理
- 検索機能によるアイテムのフィルタリング
- UnityPackageのインポート機能
- ページネーションによる大量のアイテムの効率的な表示

## インストール方法

### VPMを使用する場合

1. VRChat Creator Companionを開く
2. プロジェクトを選択
3. 「Packages」タブを開く
4. 「Add package from git URL」をクリック
5. 以下のURLを入力：
   ```
   https://github.com/yuki-2525/UnityEditorAssetBrowser.git?path=/Packages/com.yuki-2525.unityeditorassetbrowser
   ```

### 手動インストールの場合

1. このリポジトリをクローンまたはダウンロード
2. `Packages/com.yuki-2525.unityeditorassetbrowser`フォルダをプロジェクトの`Packages`フォルダにコピー

## 使用方法

1. Unity Editorで「Window > Unity Editor Asset Browser」を選択
2. AE Database PathとKA Database Pathを設定
3. 各タブでアイテムを閲覧・管理

## 依存関係

- VRChat SDK Base 3.x.x

## ライセンス

MIT License
