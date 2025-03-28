# Unity Editor Asset Browser

Unity Editor用のアセットデータベースビューワーです。Avatar ExplorerとKonoAssetのデータベースを統合して表示・管理できます。

## 機能

- Avatar ExplorerとKonoAssetのデータベースを統合表示
- アバター、アバター関連アイテム、ワールドオブジェクトの3つのタブで表示
- 検索機能によるアイテムのフィルタリング
- ページネーションによる大量のアイテムの表示
- UnityPackageのインポート機能
- 画像のプレビュー表示

## 使い方

1. Unity Editorのメニューから「Window > Unity Editor Asset Browser」を選択してウィンドウを開きます。

2. データベースパスの設定
   - AE Database Path: Avatar Explorerのデータベースフォルダを選択
   - KA Database Path: KonoAssetのデータベースフォルダを選択

3. タブの切り替え
   - アバター: Avatar ExplorerのアバターとKonoAssetのアバターを表示
   - アバター関連: Avatar ExplorerのアイテムとKonoAssetのウェアラブルを表示
   - ワールド: KonoAssetのワールドオブジェクトを表示

4. アイテムの表示
   - 各アイテムは画像、タイトル、作者情報を表示
   - UnityPackageがある場合は「UnityPackage」セクションでインポート可能

5. 検索機能
   - 検索フィールドにキーワードを入力すると、タイトルでフィルタリングされます

6. ページネーション
   - 「前へ」「次へ」ボタンでページを切り替え
   - 現在のページ番号と総ページ数を表示

## 注意事項

- Avatar Explorerの画像パスは以下のように処理されます：
  - "Datas"で始まるパス: `{aeDatabasePath}\\{パス}`として処理
  - それ以外のパス: そのままのパスとして使用

- データベースパスは自動的に保存され、次回起動時に復元されます
