# VRChatExpressionParametersOptimizer
使われていないExpressionParameterのパラメーターを削除するNDMFプラグイン

## なぜ生まれたのか？
Modular Avatarの[Extract Menu](https://modular-avatar.nadena.dev/ja/docs/tutorials/menu) を再帰的に適用したり使わないアニメーションを削除したりした結果、使われないパラメーターが生まれることがあります。
特に、髪の長さや角度はメニューから小数精度で変えたいことがほとんどなく、たいてい消されます。

しかし、昨今はギミックの高度化によって消費される同期パラメーターの割合が増えており、結果としてVRChatにおいて[256ビットの壁](https://creators.vrchat.com/avatars/animator-parameters/#parameter-types) を軽く超えることがあります。
当プログラムはアバターを再帰的に走査し、使われていないパラメーターを削除することによってこの問題に対処しようとするプログラムです。

## 対応環境
|プラットフォーム|Unity|対応状況|
|:------------|:----|:-----|
|VRChat|2018.x|unsupported|
|VRChat|2019.x|unsupported|
|VRChat|2022.x|supported|
|*|*|unsuppoered|

## インストール
VPMには対応していません。

1. エディタのツールバーから\[Window > Package Manager\] を開きます
2. 画面上部の\[+▼\] を押します
3. `https://github.com/KisaragiEffective/VRChatExpressionParametersOptimizer.git` と入力します

## Getting started
アバターのルート (`VRC Avatar Descriptor` コンポーネントがついているゲームオブジェクトと同じゲームオブジェクト) に `Expression Parameter Optimizer Setting` を `Add Component` してください。

EPOSは[AAO Trace and Optimize](https://vpm.anatawa12.com/avatar-optimizer/ja/docs/reference/trace-and-optimize/)のように、すべてのAnimatorControllerを自動的に走査し、使われていないパラメーターを削除します。

何らかの理由で特定のパラメーターをEPOSから除外したいときは、新規に作成したゲームオブジェクトに `Marker Exclusion` を `Add Component` し、そのゲームオブジェクトを `Expression Parameter Optimizer Setting` の `Exclusions` に代入します。

`Marker Exclusion` は有効化状態を制御する `Applies` 、パラメーター名に対してマッチさせる正規表現 `Exclusion Name Pattern`、処理内容に影響を及ぼさない自由記述かつエンドユーザーがメモとして使用することができる `Comment` からなります。

`Exclusion Name Pattern` に指定する正規表現は　[C# の正規表現](https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-language-quick-reference) として有効な構文にしてください。無効な構文を与えた場合、ツールはエラーを報告しアップロードが阻止されます。

## トラブルシューティング
### パラメーター超過時の対応
コントロールパネルがExpression Parameterについて同期パラメーターのビット数の制限の超過を報告する場合、以下の手順でビット数の評価を実際のビルド時まで遅延させることで回避することができます。

1. Modular Avatarをインストールする
2. [Extract Menu](https://modular-avatar.nadena.dev/ja/docs/tutorials/menu) で再帰的にメニューを展開する
3. 呼び出さないメニューを消す
4. ビルドし直す

もしどうしてもModular Avatarを使うことが難しい場合、さやまめさんが作成した[Upload-without-preCheck](https://github.com/Sayamame-beans/Upload-without-preCheck) を使用してください。

## 実装の詳細
このプラグインはNDMFの[`BuildPhase.Optimizing`](https://ndmf.nadena.dev/api/nadena.dev.ndmf.BuildPhase.html) フェーズで動作します。他のプラグインとの前後関係は不定です。
もし他の最適化プラグインとの食い合わせが悪い場合、issueを立ててください。

このプラグインは、同期する・しない、Expression Parameterに含まれている・いないに関わらず、以下の条件を満たしたパラメーターをすべて削除します。

1. VRC Avatar Descriptorから参照できる
    * VRC Avatar Descriptorに設定されている各[レイヤー](https://creators.vrchat.com/avatars/playable-layers/) を起点として、そのレイヤー内部に含まれているAnimator Controllerとしてのパラメーターの遷移で使われている場合
3. 
