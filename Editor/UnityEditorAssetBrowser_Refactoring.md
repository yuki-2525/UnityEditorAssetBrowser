# UnityEditorAssetBrowser リファクタリング提案

## 1. 現状の課題

### 1.1 コードの複雑性
- 単一のクラスに多くの責務が集中している
- メソッドが長く、可読性が低下している
- UIの描画ロジックとビジネスロジックが混在している

### 1.2 保守性の低下
- 機能追加時に既存コードへの影響が大きい
- テストが困難な構造
- 依存関係が複雑で把握しづらい

## 2. リファクタリング方針

### 2.1 アーキテクチャパターン
- MVVMパターンを採用
- 責務の明確な分離
- 依存性注入の活用

### 2.2 ディレクトリ構造
```
UnityEditorAssetBrowser/
├── Editor/
│   ├── Windows/
│   │   └── UnityEditorAssetBrowserWindow.cs
│   ├── ViewModels/
│   │   ├── AssetBrowserViewModel.cs
│   │   ├── SearchViewModel.cs
│   │   └── PaginationViewModel.cs
│   ├── Views/
│   │   ├── AssetItemView.cs
│   │   ├── SearchView.cs
│   │   └── PaginationView.cs
│   ├── Models/
│   │   ├── AssetItem.cs
│   │   ├── SearchCriteria.cs
│   │   └── PaginationInfo.cs
│   ├── Services/
│   │   ├── DatabaseService.cs
│   │   ├── ImageService.cs
│   │   └── UnityPackageService.cs
│   └── Helpers/
│       ├── AEDatabaseHelper.cs
│       ├── KADatabaseHelper.cs
│       └── UnityPackageHelper.cs
```

## 3. クラス設計

### 3.1 ViewModels
- **AssetBrowserViewModel**
  - メインのビジネスロジック
  - データの管理と操作
  - 検索とフィルタリング
  - ページネーション

- **SearchViewModel**
  - 検索条件の管理
  - 検索ロジック
  - フィルタリング

- **PaginationViewModel**
  - ページネーション情報の管理
  - ページ切り替えロジック

### 3.2 Views
- **AssetItemView**
  - アセットアイテムの表示
  - 画像表示
  - UnityPackage表示

- **SearchView**
  - 検索UI
  - フィルターUI

- **PaginationView**
  - ページネーションUI

### 3.3 Models
- **AssetItem**
  - アセット情報のデータモデル
  - AEとKAのデータを統合

- **SearchCriteria**
  - 検索条件のデータモデル

- **PaginationInfo**
  - ページネーション情報のデータモデル

### 3.4 Services
- **DatabaseService**
  - データベース操作
  - AEとKAのデータ統合

- **ImageService**
  - 画像の読み込みと管理
  - キャッシュ管理

- **UnityPackageService**
  - UnityPackageの操作
  - インポート処理

## 4. 改善効果

### 4.1 保守性の向上
- 責務の明確な分離
- テスト容易性の向上
- コードの再利用性向上

### 4.2 拡張性の向上
- 新機能追加が容易
- 既存機能の修正が局所化
- 依存関係の明確化

### 4.3 パフォーマンスの改善
- 画像キャッシュの効率化
- データベースアクセスの最適化
- UI更新の効率化

## 5. 実装手順

1. 新しいディレクトリ構造の作成
2. 基本クラスの実装
3. 既存コードの段階的な移行
4. テストの実装
5. パフォーマンス最適化

## 6. 注意点

- 既存機能の動作を維持
- 段階的な移行によるリスク管理
- パフォーマンスへの影響を考慮
- ユーザビリティの維持

## 7. 関数の割り当て

### 7.1 Windows/UnityEditorAssetBrowserWindow.cs
- **ShowWindow** - メニューからウィンドウを表示する
- **OnEnable** - ウィンドウが有効になった時の処理
- **OnDisable** - ウィンドウが無効になった時の処理
- **OnHierarchyChanged** - シーン階層が変更された時の処理
- **OnGUI** - GUIの描画処理

### 7.2 ViewModels/AssetBrowserViewModel.cs
- **GetCurrentTabItemCount** - 現在のタブのアイテム数を取得
- **GetTotalPages** - 総ページ数を取得
- **GetFilteredAvatars** - フィルターされたアバターリストを取得
- **GetFilteredItems** - フィルターされたアイテムリストを取得
- **GetFilteredWorldObjects** - フィルターされたワールドオブジェクトリストを取得
- **SortItems** - アイテムをソートする
- **GetCurrentTabItems** - 現在のタブのアイテムを取得
- **SetSortMethod** - ソート方法を設定
- **RefreshDatabases** - データベースを再読み込みする
- **RefreshImageCache** - 画像キャッシュを再取得する

### 7.3 ViewModels/SearchViewModel.cs
- **IsItemMatchSearch** - アイテムが検索条件に一致するかチェック
- **GetTitle** - アイテムのタイトルを取得
- **GetAuthor** - アイテムの作者名を取得
- **GetCreatedDate** - アイテムの作成日を取得
- **GetDate** - 日付文字列をDateTimeに変換
- **IsWorldCategory** - カテゴリーがワールド関連かどうかを判定

### 7.4 ViewModels/PaginationViewModel.cs
- **GetTotalPages** - 総ページ数を取得
- **GetCurrentTabItemCount** - 現在のタブのアイテム数を取得

### 7.5 Views/AssetItemView.cs
- **ShowAvatarItem** - AEアバターアイテムの表示
- **ShowKonoAssetItem** - KAアバターアイテムの表示
- **ShowKonoAssetWearableItem** - KAウェアラブルアイテムの表示
- **ShowKonoAssetWorldObjectItem** - KAワールドオブジェクトアイテムの表示
- **DrawItemHeader** - アイテムヘッダーの描画
- **DrawItemImage** - アイテム画像の描画
- **GetFullImagePath** - 完全な画像パスを取得
- **DrawOpenButton** - 開くボタンの描画
- **DrawUnityPackageSection** - UnityPackageセクションの描画
- **DrawUnityPackageItem** - UnityPackageアイテムの描画

### 7.6 Views/SearchView.cs
- **DrawSearchField** - 検索フィールドの描画
- **DrawSearchResultCount** - 検索結果件数の描画
- **DrawDatabasePathFields** - データベースパス入力フィールドの描画
- **DrawDatabasePathField** - 個別のデータベースパス入力フィールドの描画

### 7.7 Views/PaginationView.cs
- **DrawPaginationButtons** - ページネーションボタンの描画

### 7.8 Views/MainView.cs
- **DrawMainWindow** - メインウィンドウの描画
- **DrawTabBar** - タブバーの描画
- **DrawContentArea** - コンテンツエリアの描画
- **DrawScrollView** - スクロールビューの描画
- **DrawCurrentTabContent** - 現在のタブのコンテンツを描画
- **InitializeStyles** - GUIスタイルの初期化
- **ShowAvatarsContent** - アバターコンテンツの表示
- **ShowItemsContent** - アバター関連アイテムコンテンツの表示
- **ShowWorldObjectsContent** - ワールドオブジェクトコンテンツの表示

### 7.9 Services/DatabaseService.cs
- **LoadAEDatabase** - AEデータベースの読み込み
- **LoadKADatabase** - KAデータベースの読み込み
- **LoadKADatabaseFile** - KAデータベースファイルの読み込み
- **GetItemCount** - データベースのアイテム数を取得
- **LoadSettings** - 設定の読み込み
- **SaveSettings** - 設定の保存

### 7.10 Services/ImageService.cs ✅
- **LoadTexture** - テクスチャを読み込む
- **GetItemImagePath** - アイテムの画像パスを取得

### 7.11 Services/UnityPackageService.cs ✅
- **FindUnityPackages** - 指定されたディレクトリ内のUnityPackageファイルを検索する

### 7.12 Helpers/AEDatabaseHelper.cs
- **LoadAEDatabase** - AEデータベースを読み込む
- **SaveAEDatabase** - AEデータベースを保存する

### 7.13 Helpers/KADatabaseHelper.cs
- **LoadKADatabase** - KAデータベースを読み込む
- **SaveKADatabase** - KAデータベースを保存する

### 7.14 Helpers/JsonSettings.cs
- **Settings** - JSONシリアライズ設定を提供する

### 7.15 Helpers/CustomDateTimeConverter.cs
- **ReadJson** - JSONからDateTimeを読み込む
- **WriteJson** - DateTimeをJSONに書き込む

### 7.16 Models/AssetItem.cs
- **GetCategoryName** - カテゴリー名を取得する

### 7.17 Models/KonoAssetDatabase.cs
- **コンストラクタ** - 基本データベースモデルのコンストラクタ

### 7.18 Models/KonoAssetAvatarsDatabase.cs
- **コンストラクタ** - アバター用データベースのコンストラクタ

### 7.19 Models/KonoAssetWearablesDatabase.cs
- **コンストラクタ** - ウェアラブル用データベースのコンストラクタ

### 7.20 Models/KonoAssetWorldObjectsDatabase.cs
- **コンストラクタ** - ワールドオブジェクト用データベースのコンストラクタ 