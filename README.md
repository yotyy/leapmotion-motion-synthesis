# leapmotion-motion-synthesis

## 🎮 初回クローン & セットアップ手順

> **対象読者**  
> このリポジトリをはじめてクローンし、ローカルで Unity プロジェクトを開く人

---

### 0. 前提

| ツール | バージョン例 | 備考 |
|--------|--------------|------|
| **Unity Hub** | 3.x 系 | プロジェクト管理に必須 |
| **Unity エディタ** | `2023.3.4f1` | `ProjectSettings/ProjectVersion.txt` で確認 |
| **Git** | 2.40 以上 | `git --version` |
| **Git LFS** | 3.x | 大容量アセット用（※インストール必須） |

### 1. リポジトリをクローン

```bash
git clone https://github.com/<org-or-user>/<repo>.git
cd <repo>
# LFS がまだなら
git lfs install
git lfs pull            # 大容量ファイルを取得
