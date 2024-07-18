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
