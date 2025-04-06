# UnityEditorAssetBrowser 関数一覧

## 1. Unity Editor Window Methods

### ShowWindow
- **説明**: メニューからウィンドウを表示する
- **機能**: `Window/Unity Editor Asset Browser`メニューからウィンドウを開く

### OnEnable
- **説明**: ウィンドウが有効になった時の処理
- **機能**: 設定の読み込みとシーン変更時の自動更新イベントの登録

### OnDisable
- **説明**: ウィンドウが無効になった時の処理
- **機能**: イベントの解除

### OnHierarchyChanged
- **説明**: シーン階層が変更された時の処理
- **機能**: 画像キャッシュの更新

### OnGUI
- **説明**: GUIの描画処理
- **機能**: スタイルの初期化とメインウィンドウの描画

## 2. UI Drawing Methods

### DrawMainWindow
- **説明**: メインウィンドウの描画
- **機能**: データベースパス、タブ、検索、コンテンツエリアの描画

### DrawDatabasePathFields
- **説明**: データベースパス入力フィールドの描画
- **機能**: AEとKAのデータベースパス設定と更新ボタンの表示

### DrawDatabasePathField
- **説明**: 個別のデータベースパス入力フィールドの描画
- **機能**: パスの表示、削除、参照ボタンの提供

### DrawTabBar
- **説明**: タブバーの描画
- **機能**: アバター、アバター関連、ワールドのタブ切り替え

### DrawSearchField
- **説明**: 検索フィールドの描画
- **機能**: 基本検索と詳細検索のUI表示

### DrawSearchResultCount
- **説明**: 検索結果件数の描画
- **機能**: 現在のタブの検索結果数を表示

### DrawContentArea
- **説明**: コンテンツエリアの描画
- **機能**: スクロールビューとページネーションボタンの表示

### DrawScrollView
- **説明**: スクロールビューの描画
- **機能**: 現在のタブのコンテンツをスクロール可能に表示

### DrawCurrentTabContent
- **説明**: 現在のタブのコンテンツを描画
- **機能**: 選択されたタブに応じたコンテンツの表示

### DrawPaginationButtons
- **説明**: ページネーションボタンの描画
- **機能**: 前へ、次へボタンとページ情報の表示

## 3. Helper Methods

### GetCurrentTabItemCount
- **説明**: 現在のタブのアイテム数を取得
- **機能**: 選択されたタブのフィルターされたアイテム数を返す

### IsWorldCategory
- **説明**: カテゴリーがワールド関連かどうかを判定
- **機能**: カテゴリー名に「ワールド」または「world」が含まれているかチェック

### InitializeStyles
- **説明**: GUIスタイルの初期化
- **機能**: タイトルとボックスのスタイル設定

### ShowAvatarsContent
- **説明**: アバターコンテンツの表示
- **機能**: フィルターされたアバターアイテムの表示

### ShowItemsContent
- **説明**: アバター関連アイテムコンテンツの表示
- **機能**: フィルターされたアバター関連アイテムの表示

### ShowWorldObjectsContent
- **説明**: ワールドオブジェクトコンテンツの表示
- **機能**: フィルターされたワールドオブジェクトの表示

### GetTotalPages
- **説明**: 総ページ数を取得
- **機能**: 現在のタブのアイテム数から総ページ数を計算

### GetFilteredAvatars
- **説明**: フィルターされたアバターリストを取得
- **機能**: AEとKAのアバターを統合して検索条件でフィルタリング

### GetFilteredItems
- **説明**: フィルターされたアイテムリストを取得
- **機能**: AEとKAのアバター関連アイテムを統合して検索条件でフィルタリング

### GetFilteredWorldObjects
- **説明**: フィルターされたワールドオブジェクトリストを取得
- **機能**: AEとKAのワールドオブジェクトを統合して検索条件でフィルタリング

### SortItems
- **説明**: アイテムをソートする
- **機能**: 選択されたソート方法に基づいてアイテムを並び替え

### GetDate
- **説明**: 日付文字列をDateTimeに変換
- **機能**: 様々な形式の日付文字列をDateTime型に変換

### GetCreatedDate
- **説明**: アイテムの作成日を取得
- **機能**: アイテムの種類に応じて作成日を取得しUnixTimeMillisecondsに変換

### GetTitle
- **説明**: アイテムのタイトルを取得
- **機能**: アイテムの種類に応じてタイトルを取得

### GetAuthor
- **説明**: アイテムの作者名を取得
- **機能**: アイテムの種類に応じて作者名を取得

### IsItemMatchSearch
- **説明**: アイテムが検索条件に一致するかチェック
- **機能**: 基本検索と詳細検索の条件に基づいてアイテムをフィルタリング

## 4. Item Display Methods

### ShowAvatarItem
- **説明**: AEアバターアイテムの表示
- **機能**: アバターアイテムの詳細情報を表示

### ShowKonoAssetItem
- **説明**: KAアバターアイテムの表示
- **機能**: KonoAssetアバターアイテムの詳細情報を表示

### ShowKonoAssetWearableItem
- **説明**: KAウェアラブルアイテムの表示
- **機能**: KonoAssetウェアラブルアイテムの詳細情報を表示

### ShowKonoAssetWorldObjectItem
- **説明**: KAワールドオブジェクトアイテムの表示
- **機能**: KonoAssetワールドオブジェクトアイテムの詳細情報を表示

### DrawItemHeader
- **説明**: アイテムヘッダーの描画
- **機能**: アイテムの基本情報（タイトル、作者、画像など）を表示

### DrawItemImage
- **説明**: アイテム画像の描画
- **機能**: アイテムの画像を読み込んで表示

### GetFullImagePath
- **説明**: 完全な画像パスを取得
- **機能**: 相対パスを絶対パスに変換

### DrawOpenButton
- **説明**: 開くボタンの描画
- **機能**: アイテムのディレクトリを開くボタンを表示

### DrawUnityPackageSection
- **説明**: UnityPackageセクションの描画
- **機能**: UnityPackageの一覧とインポートボタンを表示

### DrawUnityPackageItem
- **説明**: UnityPackageアイテムの描画
- **機能**: 個別のUnityPackageとインポートボタンを表示

## 5. Database Loading Methods

### LoadAEDatabase
- **説明**: AEデータベースの読み込み
- **機能**: AvatarExplorerのデータベースを読み込む

### LoadKADatabase
- **説明**: KAデータベースの読み込み
- **機能**: KonoAssetのデータベースを読み込む

### LoadKADatabaseFile
- **説明**: KAデータベースファイルの読み込み
- **機能**: 指定されたKonoAssetデータベースファイルを読み込む

### GetItemCount
- **説明**: データベースのアイテム数を取得
- **機能**: データベースの種類に応じてアイテム数を返す

## 6. Utility Methods

### LoadTexture
- **説明**: テクスチャを読み込む
- **機能**: 画像ファイルをテクスチャとして読み込み、キャッシュを管理

### LoadSettings
- **説明**: 設定の読み込み
- **機能**: EditorPrefsからデータベースパスを読み込む

### SaveSettings
- **説明**: 設定の保存
- **機能**: データベースパスをEditorPrefsに保存

### RefreshDatabases
- **説明**: データベースを再読み込みする
- **機能**: 画像キャッシュのクリアとデータベースの再読み込み

### RefreshImageCache
- **説明**: 画像キャッシュを再取得する
- **機能**: 現在表示中のアイテムの画像を再読み込み

### GetCurrentTabItems
- **説明**: 現在のタブのアイテムを取得
- **機能**: 選択されたタブに応じたアイテムリストを返す

### GetItemImagePath
- **説明**: アイテムの画像パスを取得
- **機能**: アイテムの種類に応じて画像パスを取得

### SetSortMethod
- **説明**: ソート方法を設定
- **機能**: ソート方法を変更し、ページをリセット

## 7. Helper クラスの関数

### AEDatabaseHelper

#### LoadAEDatabase
- **説明**: AEデータベースを読み込む
- **機能**: AvatarExplorerのデータベースをJSONから読み込み、AvatarExplorerDatabaseオブジェクトを返す

#### SaveAEDatabase
- **説明**: AEデータベースを保存する
- **機能**: AvatarExplorerItemの配列をJSONとして保存する

### KADatabaseHelper

#### LoadKADatabase
- **説明**: KAデータベースを読み込む
- **機能**: KonoAssetのデータベースをJSONから読み込み、KonoAssetDatabaseオブジェクトを返す

#### SaveKADatabase
- **説明**: KAデータベースを保存する
- **機能**: KonoAssetDatabaseオブジェクトをJSONとして保存する

### JsonSettings

#### Settings
- **説明**: JSONシリアライズ設定を提供する
- **機能**: インデント付きフォーマット、null値の無視、循環参照の無視、日付変換の設定を行う

### CustomDateTimeConverter

#### ReadJson
- **説明**: JSONからDateTimeを読み込む
- **機能**: 文字列形式の日付をDateTime型に変換する

#### WriteJson
- **説明**: DateTimeをJSONに書き込む
- **機能**: DateTime型を「yyyy-MM-dd HH:mm:ss」形式の文字列に変換する

### UnityPackageHelper

#### FindUnityPackages
- **説明**: 指定されたディレクトリ内のUnityPackageファイルを検索する
- **機能**: 指定されたディレクトリとそのサブディレクトリ内の「.unitypackage」拡張子のファイルを検索する

## 8. Models クラスの関数

### AvatarExplorerItem

#### GetCategoryName
- **説明**: カテゴリー名を取得する
- **機能**: Typeの値に基づいてカテゴリー名を返す。Type=9の場合はCustomCategoryを返す

### KonoAssetDatabase

#### コンストラクタ
- **説明**: 基本データベースモデルのコンストラクタ
- **機能**: バージョンとデータ配列を初期化する

### KonoAssetAvatarsDatabase

#### コンストラクタ
- **説明**: アバター用データベースのコンストラクタ
- **機能**: 親クラスのコンストラクタを呼び出し、dataプロパティをKonoAssetAvatarItem[]型に初期化する

### KonoAssetWearablesDatabase

#### コンストラクタ
- **説明**: ウェアラブル用データベースのコンストラクタ
- **機能**: 親クラスのコンストラクタを呼び出し、dataプロパティをKonoAssetWearableItem[]型に初期化する

### KonoAssetWorldObjectsDatabase

#### コンストラクタ
- **説明**: ワールドオブジェクト用データベースのコンストラクタ
- **機能**: 親クラスのコンストラクタを呼び出し、dataプロパティをKonoAssetWorldObjectItem[]型に初期化する 