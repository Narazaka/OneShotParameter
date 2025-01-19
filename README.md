# One Shot Parameter

トグルメニュー等を一定時間後自動的に元に戻す設定をするツール。

単発実行するパーティクルや音声等に使えます。

## Install

### VCC用インストーラーunitypackageによる方法（おすすめ）

https://github.com/Narazaka/OneShotParameter/releases/latest から `net.narazaka.vrchat.one-shot-parameter-installer.zip` をダウンロードして解凍し、対象のプロジェクトにインポートする。

### VCCによる方法

1. https://vpm.narazaka.net/ から「Add to VCC」ボタンを押してリポジトリをVCCにインストールします。
2. VCCでSettings→Packages→Installed Repositoriesの一覧中で「Narazaka VPM Listing」にチェックが付いていることを確認します。
3. アバタープロジェクトの「Manage Project」から「One Shot Parameter」をインストールします。

## Usage

アバター内のどこかにインスペクターの「Add Component」ボタンから「OneShotParameter」を付けて、制御したいメニューのパラメーター等を設定します。

### 例: 音声の再生

1. [Avatar Menu Creator for MA](https://avatar-menu-creator-for-ma.vrchat.narazaka.net) で音などのON/OFFメニューを作る
2. そのメニューオブジェクト等に「Add Component」ボタンから「OneShotParameter」を付けて、「パラメーター名」にパラメーター名を指定します。
  - Avatar Menu Creatorではデフォルトでオブジェクト名がパラメーター名です。
3. 音の継続秒数を確認し、「リセット時間(秒)」に指定します。

## License

[Zlib License](LICENSE.txt)
