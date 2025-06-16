# Unity Editor Asset Browser

[Avatar Explorer](https://booth.pm/ja/items/6372968)と[KonoAsset](https://silolab.booth.pm/items/6641548)によって保存されているアイテムを表示し、簡単にインポートすることが出来るエディタ拡張です。

## 機能

- [Avatar Explorer](https://booth.pm/ja/items/6372968)と[KonoAsset](https://silolab.booth.pm/items/6641548)のアイテムを統合表示
- アバター、アバター関連アイテム、ワールドオブジェクトの3つのタブで表示
- 検索機能によるアイテムのフィルタリング
- UnityPackageのインポート機能

## 使い方

1. Unity Editorのメニューから「Window > Unity Editor Asset Browser」を選択してウィンドウを開きます。

2. データベースパスの設定
   - AE Database Path: Avatar Explorerのデータフォルダを選択
     - 例：`C:\VRC-Avatar-Explorer\Datas`
   - KA Database Path: KonoAssetのデータフォルダを選択
     - 例：`C:\KonoAssetData`

3. タブの切り替え
   - アバター: Avatar Explorerの「アバター」とKonoAssetの「アバター素体」を表示
   - アバター関連: Avatar ExplorerのほとんどのアイテムとKonoAssetの「アバター関連アセット」を表示
   - ワールド: Avatar Explorerのカスタムカテゴリ名に「ワールド」「world」が含まれているアイテムとKonoAssetの「ワールドアセット」を表示

4. アイテムの表示
   - 各アイテムは画像、タイトル、作者情報を表示
   - UnityPackageがある場合は「UnityPackage」セクションでインポート可能

5. 検索機能
   - 検索フィールドにキーワードを入力すると、タイトル・作者名でフィルタリングされます

## 注意事項

- データベースパスは自動的に保存され、次回起動時に復元されます

## ライセンス

このプロジェクトは[MITライセンス](LICENSE)の下で公開されています。

## 謝辞

素晴らしいアセット管理ツールを開発していただいた、以下の方々に感謝申し上げます：

- Avatar Explorer開発者のぷこるふさん
- silonecoさんをはじめとするKonoAsset開発チームのみなさん

また、以下のプロジェクトのコードを一部借用しています：

- [AE-Tools](https://github.com/puk06/AE-Tools) - MIT Licenseに基づいて使用
- [AssetLibraryManager](https://github.com/MAIOTAchannel/AssetLibraryManager) - MAIOTAchannel様の許可を得て使用
