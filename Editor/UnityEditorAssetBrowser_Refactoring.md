# UnityEditorAssetBrowser リファクタリング計画

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
│   │   └── UnityEditorAssetBrowserWindow.cs ❌
│   ├── ViewModels/
│   │   ├── AssetBrowserViewModel.cs ⏳
│   │   ├── SearchViewModel.cs ✅
│   │   ├── SearchCriteriaManager.cs ✅
│   │   └── PaginationViewModel.cs ✅
│   ├── Views/
│   │   ├── AssetItemView.cs ❌
│   │   ├── SearchView.cs ❌
│   │   ├── PaginationView.cs ❌
│   │   └── MainView.cs ❌
│   ├── Models/
│   │   ├── AssetItem.cs ✅
│   │   ├── SearchCriteria.cs ✅
│   │   ├── PaginationInfo.cs ✅
│   │   ├── AvatarExplorerModels.cs ✅
│   │   └── KonoAssetModels.cs ✅
│   ├── Services/
│   │   ├── DatabaseService.cs ✅
│   │   ├── ImageService.cs ✅
│   │   ├── UnityPackageService.cs ✅
│   │   └── ItemSearchService.cs ✅
│   └── Helpers/
│       ├── AEDatabaseHelper.cs ✅
│       ├── KADatabaseHelper.cs ✅
│       ├── JsonSettings.cs ✅
│       └── CustomDateTimeConverter.cs ✅
```

## 3. クラス設計

### 3.1 Windows
- **UnityEditorAssetBrowserWindow** ❌
  - メインウィンドウの初期化とイベント処理
  - 各コンポーネントの統合
  - シーン変更イベントの処理

### 3.2 ViewModels
- **AssetBrowserViewModel** ⏳
  - メインのビジネスロジック
  - データの管理と操作
  - 検索とフィルタリング
  - ページネーション
  - 実装済み機能:
    - GetCurrentTabItemCount ✅
    - GetTotalPages ✅
    - GetFilteredAvatars ✅
    - GetFilteredItems ✅
    - GetFilteredWorldObjects ✅
    - SortItems ✅
    - GetCurrentTabItems ✅
    - SetSortMethod ✅
    - RefreshImageCache ✅
  - 未実装機能:
    - その他のビジネスロジック ❌

- **SearchViewModel** ✅
  - 検索条件の管理
  - 検索ロジック
  - フィルタリング
  - タブごとの検索条件の保存と復元
  - 実装済み機能:
    - IsItemMatchSearch ✅
    - SetCurrentTab ✅
    - SearchCriteria管理 ✅
    - タブごとの検索条件の保存と復元 ✅
    - 検索条件のクリア ✅

- **SearchCriteriaManager** ✅
  - タブごとの検索条件の管理
  - 検索条件の保存と読み込み
  - 実装済み機能:
    - SetCurrentTab ✅
    - SaveCurrentTabCriteria ✅
    - LoadTabCriteria ✅

- **PaginationViewModel** ✅
  - ページネーション情報の管理
  - ページ切り替えロジック
  - タブ関連のアイテム取得ロジック

### 3.3 Views
- **MainView** ❌
  - メインウィンドウのUI
  - タブバーの表示
  - コンテンツエリアの表示
  - スクロールビューの管理

- **AssetItemView** ❌
  - アセットアイテムの表示
  - 画像表示
  - UnityPackage表示

- **SearchView** ❌
  - 検索UI
  - フィルターUI
  - 検索結果件数の表示

- **PaginationView** ❌
  - ページネーションUI
  - ページ切り替えボタン

### 3.4 Models
- **AssetItem** ✅
  - アセット情報のデータモデル
  - AEとKAのデータを統合

- **SearchCriteria** ✅
  - 検索条件のデータモデル
  - キーワード、フィルター、ソート条件

- **PaginationInfo** ✅
  - ページネーション情報のデータモデル
  - 現在のページ、総ページ数、アイテム数
  - タブ選択状態の管理

- **AvatarExplorerModels** ✅
  - AEデータベースモデル ✅
  - AEアイテムモデル ✅
  - カテゴリー名の取得 ✅

- **KonoAssetModels** ✅
  - KAデータベースモデル ✅
  - KAアイテムモデル（アバター、ウェアラブル、ワールドオブジェクト） ✅
  - KAアイテムの詳細情報モデル ✅

### 3.5 Services
- **DatabaseService** ✅
  - データベース操作の統合 ✅
  - AEとKAのデータベース管理 ✅
  - 設定の保存と読み込み ✅

- **ImageService** ✅
  - 画像の読み込みと管理 ✅
  - キャッシュ管理 ✅
  - 画像パスの取得 ✅

- **UnityPackageService** ✅
  - UnityPackageの操作 ✅
  - インポート処理 ✅
  - パッケージ情報の取得 ✅
  - パッケージファイルの検索と解析 ✅

- **ItemSearchService** ✅
  - アイテム検索の実装 ✅
  - 基本検索と詳細検索 ✅
  - 検索条件に基づくフィルタリング ✅

### 3.6 Helpers
- **AEDatabaseHelper** ✅
  - AEデータベースの読み込みと保存 ✅
  - ファイル操作の抽象化 ✅

- **KADatabaseHelper** ✅
  - KAデータベースの読み込みと保存 ✅
  - ファイル操作の抽象化 ✅

- **JsonSettings** ✅
  - JSONシリアライズ設定の提供 ✅
  - 日付フォーマットの設定 ✅

- **CustomDateTimeConverter** ✅
  - DateTime型のJSON変換 ✅
  - 日付フォーマットの処理 ✅

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

1. 新しいディレクトリ構造の作成 ✅
2. 基本クラスの実装 ✅
3. 既存コードの段階的な移行 ⏳
   - PaginationViewModelの実装と統合 ✅
   - SearchViewModelの実装と統合 ✅
   - AssetBrowserViewModelの実装 ⏳
   - Viewの実装 ❌
   - Windowの実装 ❌
4. テストの実装 ❌
5. パフォーマンス最適化 ❌

## 6. 注意点

- 既存機能の動作を維持
- 段階的な移行によるリスク管理
- パフォーマンスへの影響を考慮
- ユーザビリティの維持

## 7. 関数の割り当て

### 7.1 Windows/UnityEditorAssetBrowserWindow.cs ❌
- **ShowWindow** - メニューからウィンドウを表示する
- **OnEnable** - ウィンドウが有効になった時の処理
- **OnDisable** - ウィンドウが無効になった時の処理
- **OnHierarchyChanged** - シーン階層が変更された時の処理
- **OnGUI** - GUIの描画処理（MainViewに委譲）

### 7.2 ViewModels/AssetBrowserViewModel.cs ❌
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

### 7.3 ViewModels/SearchViewModel.cs ✅
- **IsItemMatchSearch** - アイテムが検索条件に一致するかチェック
- **SetCurrentTab** - 現在のタブを設定し、検索条件を保存/復元
- **ClearSearchCriteria** - 現在のタブの検索条件をクリア
- **SearchCriteria** - 現在のタブの検索条件を管理
- **_tabSearchCriteria** - タブごとの検索条件を保存

### 7.4 ViewModels/SearchCriteriaManager.cs ✅
- **SetCurrentTab** - 現在のタブを設定
- **SaveCurrentTabCriteria** - 現在のタブの検索条件を保存
- **LoadTabCriteria** - タブの検索条件を読み込み

### 7.5 ViewModels/PaginationViewModel.cs ✅
- **GetTotalPages** - 総ページ数を取得
- **GetCurrentPageItems** - 現在のページのアイテムを取得
- **ResetPage** - ページをリセット
- **MoveToNextPage** - 次のページに移動
- **MoveToPreviousPage** - 前のページに移動
- **MoveToPage** - 指定したページに移動
- **GetCurrentTabItemCount** - 現在のタブのアイテム数を取得
- **GetCurrentTabItems** - 現在のタブのアイテムを取得
- **CurrentPage** - 現在のページ番号
- **SelectedTab** - 選択中のタブ
- **ItemsPerPage** - 1ページあたりのアイテム数

### 7.6 Views/MainView.cs ❌
- **DrawMainWindow** - メインウィンドウの描画
- **DrawTabBar** - タブバーの描画
- **DrawContentArea** - コンテンツエリアの描画
- **DrawScrollView** - スクロールビューの描画
- **DrawCurrentTabContent** - 現在のタブのコンテンツを描画
- **InitializeStyles** - GUIスタイルの初期化
- **ShowAvatarsContent** - アバターコンテンツの表示
- **ShowItemsContent** - アバター関連アイテムコンテンツの表示
- **ShowWorldObjectsContent** - ワールドオブジェクトコンテンツの表示

### 7.7 Views/AssetItemView.cs ❌
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

### 7.8 Views/SearchView.cs ❌
- **DrawSearchField** - 検索フィールドの描画
  - 基本検索フィールド
  - クリアボタン
  - 詳細検索トグル
  - 詳細検索フィールド
- **DrawSearchResultCount** - 検索結果件数の描画
- **DrawDatabasePathFields** - データベースパス入力フィールドの描画
- **DrawDatabasePathField** - 個別のデータベースパス入力フィールドの描画

### 7.9 Views/PaginationView.cs ❌
- **DrawPaginationButtons** - ページネーションボタンの描画

### 7.10 Services/DatabaseService.cs ✅
- **LoadAndUpdateAEDatabase** ✅ - AEデータベースの読み込みと更新
- **LoadAndUpdateKADatabase** ✅ - KAデータベースの読み込みと更新
- **GetAEDatabase** ✅ - AEデータベースの取得
- **GetKAAvatarsDatabase** ✅ - KAアバターデータベースの取得
- **GetKAWearablesDatabase** ✅ - KAウェアラブルデータベースの取得
- **GetKAWorldObjectsDatabase** ✅ - KAワールドオブジェクトデータベースの取得
- **SetAEDatabasePath** ✅ - AEデータベースパスの設定
- **SetKADatabasePath** ✅ - KAデータベースパスの設定
- **LoadSettings** ✅ - 設定の読み込み
- **SaveSettings** ✅ - 設定の保存

### 7.11 Services/ImageService.cs ✅
- **LoadTexture** ✅ - テクスチャを読み込む
- **GetItemImagePath** ✅ - アイテムの画像パスを取得
- **ClearCache** ✅ - キャッシュをクリアする

### 7.12 Services/UnityPackageService.cs ✅
- **FindUnityPackages** ✅ - 指定されたディレクトリ内のUnityPackageファイルを検索する
- **ImportUnityPackage** ✅ - UnityPackageをインポートする

### 7.13 Services/ItemSearchService.cs ✅
- **IsItemMatchSearch** ✅ - アイテムが検索条件に一致するかチェック
- **IsBasicSearchMatch** ✅ - 基本検索に一致するかチェック
- **IsAdvancedSearchMatch** ✅ - 詳細検索に一致するかチェック

### 7.14 Helpers/AEDatabaseHelper.cs ✅
- **LoadAEDatabaseFile** ✅ - AEデータベースファイルを読み込む
- **SaveAEDatabase** ✅ - AEデータベースを保存する

### 7.15 Helpers/KADatabaseHelper.cs ✅
- **LoadKADatabaseFiles** ✅ - KAデータベースファイルを読み込む
- **SaveKADatabase** ✅ - KAデータベースを保存する

### 7.16 Helpers/JsonSettings.cs ✅
- **Settings** ✅ - JSONシリアライズ設定を提供する

### 7.17 Helpers/CustomDateTimeConverter.cs ✅
- **ReadJson** ✅ - JSONからDateTimeを読み込む
- **WriteJson** ✅ - DateTimeをJSONに書き込む

### 7.18 Models/AssetItem.cs ✅
- **GetCategoryName** ✅ - カテゴリー名を取得する
- **GetTitle** ✅ - タイトルを取得する
- **GetAuthor** ✅ - 作者名を取得する
- **GetCreatedDate** ✅ - 作成日を取得する
- **GetMemo** ✅ - メモを取得する

### 7.19 Models/SearchCriteria.cs ✅
- **IsMatch** ✅ - 検索条件に一致するかチェックする
- **GetKeywords** ✅ - キーワードを取得する
- **GetFilters** ✅ - フィルターを取得する
- **GetSortMethod** ✅ - ソート方法を取得する

### 7.20 Models/PaginationInfo.cs ✅
- **GetCurrentPage** ✅ - 現在のページを取得する
- **GetTotalPages** ✅ - 総ページ数を取得する
- **GetItemsPerPage** ✅ - 1ページあたりのアイテム数を取得する
- **GetPageItems** ✅ - 現在のページのアイテムを取得する
- **ResetPage** ✅ - ページをリセットする
- **MoveToNextPage** ✅ - 次のページに移動する
- **MoveToPreviousPage** ✅ - 前のページに移動する
- **MoveToPage** ✅ - 指定したページに移動する
- **SelectedTab** ✅ - 選択中のタブを管理する

### 7.21 Models/AvatarExplorerModels.cs ✅
- **AvatarExplorerDatabase** ✅ - AEデータベースモデル
- **AvatarExplorerItem** ✅ - AEアイテムモデル
- **GetCategoryName** ✅ - カテゴリー名を取得する

### 7.22 Models/KonoAssetModels.cs ✅
- **KonoAssetDatabase** ✅ - KA基本データベースモデル
- **KonoAssetAvatarsDatabase** ✅ - KAアバターデータベースモデル
- **KonoAssetWearablesDatabase** ✅ - KAウェアラブルデータベースモデル
- **KonoAssetWorldObjectsDatabase** ✅ - KAワールドオブジェクトデータベースモデル
- **KonoAssetAvatarItem** ✅ - KAアバターアイテムモデル
- **KonoAssetWearableItem** ✅ - KAウェアラブルアイテムモデル
- **KonoAssetWorldObjectItem** ✅ - KAワールドオブジェクトアイテムモデル
- **KonoAssetDescription** ✅ - KAアイテムの詳細情報モデル 

## 8. 次のステップ

1. **ViewModelsの実装**:
   - `AssetBrowserViewModel.cs`の実装 ❌
   - `SearchViewModel.cs`の機能拡張（タブごとの検索条件の永続化）❌

2. **Viewsの実装**:
   - `MainView.cs`の実装 ❌
   - `AssetItemView.cs`の実装 ❌
   - `SearchView.cs`の実装（クリアボタンの追加）❌
   - `PaginationView.cs`の実装 ❌

3. **Windowsの実装**:
   - `UnityEditorAssetBrowserWindow.cs`の実装 ❌

4. **テストの実装**:
   - 各コンポーネントのユニットテスト ❌
   - 統合テスト ❌
   - タブごとの検索条件の保存/復元テスト ❌

5. **パフォーマンス最適化**:
   - 画像キャッシュの効率化 ❌
   - データベースアクセスの最適化 ❌
   - UI更新の効率化 ❌
   - 検索条件の保存/復元の効率化 ❌ 