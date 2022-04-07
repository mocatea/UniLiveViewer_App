# Uni Live Viewer
<img src="https://user-images.githubusercontent.com/86557651/161434522-03bb8a2f-899e-42a2-82fb-b2172381a42e.png" >  

VRライブビューアアプリのリポジトリです。Webサイトは[こちら](https://mocatea.github.io/UniLiveViewer/)になります。  

## 公開の理由
アプリのユーザーさんに向けて、**私が認知してるバグなどをお知らせ**することを主な目的として、  
~~私が楽をしたいので~~Issuesという形で公開することにしました。  

ほとんどOSSのお陰なので、私の大したことないソースはついで程度に。  
（何も考えていなかったとは言えUniなんて大層な名前つけてしまった手前、謎の義務感も多少？）  

## Cloneしようとしている方へ注意
使用している**Unityアセット、OSS、画像や音声リソース**などは含まれていません。  
※一番気になるであろう某ダンスフォーマット変換もOSSなので無いです。  

開発当初はUniversal RP(~~何か知らんけどかっこいい~~)で!Oculus Questで！  
Candy Rock Starだけ見られれば良いや程度に作り始めたので、  
正直ここまで機能を追加する予定はありませんでした！汚いのは無計画だからです(言い訳)  

このプロジェクトは現在も継ぎ足し/変更/試行中です（スパゲティ...密結合etc...)  
なのでライブラリ等の詳細は列挙しません、それでも良ければご自由に。  
（ちゃんとしたチーム開発経験ないので察してください、慌ててGit勉強中...）  

<details>  
<summary>見どころ？2つの意味で</summary>  
  
 ・触れるバネボタン、なんちゃって掴めるスライダー（眺めてる時間が長いので許されてる感の時代錯誤UI、趣味）  
 ・VRMの揺れモノに触れられるの含めて色々カスタマイズしてるとこ  
 ・URP対応のShader    
 ・AndroidManifest（Quest直下に専用フォルダ作りたいなら...~~せっかくのモバイルなのにPC使わせる人って~~）  
 ・Timelineのランタイムバインドとか(動いてるけど使い方は自信ない)  
 ・単一責任の原則できてない肥大化してしまったクラス達  
 ・OVRGrabberを半端に生かしてるが、作りが雑でバグのある掴み関係(0から作り直したい)   
 
 （でもモデルやステージの軽量化の方が専門外で苦労したのでそっちのがアピールポイントです...~~誰かBlenderやって~~）
</details>  

## Issuesの確認方法
<img src="https://user-images.githubusercontent.com/86557651/161434333-8069687a-3b76-4b2b-a16b-8c8d756b572e.jpg" width="800"> 

①上のタブからIssuesページに飛びます  
②Openが対応中/未着手、Closedが修正/実装済み・・・のタブになります  
③bugは優先度の高いものから対応しています、enhancementは新機能とかです  
④Label▼から条件を絞り込むこともできます  

気になるトピックがあれば開いて詳細を確認してみてください。  
個人的なメモで技術的な内容を含むので、「あぁ何かしら苦戦してるのね」みたいな感じで進捗伝われば幸いです。  
(あるか分からないけどサプライズ的な新要素は載せないつもり)

### 連絡先
何かあればDM公開しているのでどうぞ[@VRmoca](https://twitter.com/VRmoca)  
ここに無いバグとか報告してくれると喜びます...多分  

v1.00までは作り続けるつもり、ネタが尽きるか興味が逸れなければ...
