# Code-demo-Attack-Receive-System
Code Demo: Attack & Receive System (Combat Framework)
A combat Damage &amp; Buff System for Unity. Data-driven, with dynamic damage calculation, and an extensible buff pipeline



這是一個為 Unity 遊戲開發設計的核心戰鬥傷害系統範例。本系統採用「數據驅動」與「邏輯解耦」的設計模式，將攻擊行為（Attack）與傷害接收（Receive）徹底分離。

在本系統中，AttackData 扮演了傳遞資訊的「信使」角色。這種設計允許開發者：
1.輕鬆實作多段傷害：一個技能可以產生多個 AttackData 實例，分別設定不同的延遲與效果。
2.支援第三方修飾：在攻擊傳遞過程中，可以輕易加入「傷害倍率修正」或「陣營過濾」等外部要求，而不需要修改核心的 ReceiveAttackData 方法。


 核心架構特色
1. 解耦設計 (Decoupled Design)
AttackData: 將攻擊發起時的原始數據（來源、陣營、基礎傷害、附加 Buff）封裝為獨立對象。

DamageData: 將經過防禦計算後的最終結果封裝，便於後續視覺處理（如傷害數字跳字、受擊特效）。

這種架構允許攻擊者無需知道目標的具體屬性，完全由攻擊接收者(即此腳本)根據自身狀態決定最終傷害。

2. 多維度傷害計算公式
系統支援多種傷害類型的邏輯判斷：

True Damage: 無視防禦的真實傷害。

AD (物理傷害)：包含「未破防」與「碾壓」機制。

自定義公式：當防禦接近傷害時，採用雙曲線平滑曲線計算。

AP (魔法傷害)：基於抗性（RES）的乘法百分比減傷邏輯。

Heal (治療)：統一整合在傷害管道中，以負值處理，提高系統一致性。

3. Buff管線 (Buff Pipeline)
AdditionBuffDic: 攻擊資料中內嵌 Dictionary，支援一次攻擊附帶多種Buff。

Event-Driven: 利用 Delegate 通知攻擊來源與受擊單位，便於實作吸血、受擊反傷等連鎖反應。


 技術細節
語言: C#

環境: Unity 6

核心技術:

封裝與多型應用

事件驅動 (Delegate)

數據驅動架構 (Data-Driven Architecture)
