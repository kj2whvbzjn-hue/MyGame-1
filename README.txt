削除Gate差分修正
- Studio正式仕様 DELETE_MANIFEST.txt を維持
- 既存MyGame差分ZIP互換として DELETE_FILES.txt も削除定義として受理
- どちらもGitHubへ配置しない
- 1行1件の完全一致パスのみ削除候補化
- 重複パスは1件へ正規化
- unsafe pathはローカルGateでSTOP
- 削除は明示チェックON時のみ候補化
