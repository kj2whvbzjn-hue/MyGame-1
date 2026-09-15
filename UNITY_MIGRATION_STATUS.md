# Unityゲーム本体移植 状態

## 今回追加
- GS-02 能力値と成長の純粋C#ドメイン
- 用途別に注入可能な乱数入口 `IRandomSource`
- 固定乱数列によるEditMode試験
- 期待能力値計算
- Level上限処理
- 7能力値それぞれの成長結果・乱数・前後値の記録

## まだ接続していない
- GS-03 ジョブデータ
- GS-04 EXP/SPからの複数Level上昇
- SaveTransaction
- Export JSON Loader
- 実ゲームUI

GS-02単体を先行実装した段階であり、ゲーム全体完成を意味しない。

## v2追加
- GS-03 ジョブカタログ検証・転職成立条件・履歴proposal
- GS-04 生存者へのEXP均等配分・端数破棄
- GS-04 複数Level上昇・現ジョブGS-02成長接続・1Level=SP1
- 最大Level余剰EXP破棄
- 必要EXP表は仕様未確定のため外部注入。推測式は実装していない
- Studio Export/master/jobs.json を Assets/GameData/master/jobs.json に配置

## 未完了
- jobs.json のRuntime Loader（次段階）
- 転職変更＋履歴のSaveTransaction原子保存
- 冒険帰還確定とのEXP永続化接続
- GS-04スキル習得条件はGS-34と合わせて後続実装

## v3追加
- Studio `jobs.json` を直接解釈する Unity `JsonUtility` Loader
- GS-05 雇用ドメイン：所属上限、種別/Job解放、正式Job存在、新規個体ID、Level1、7能力値初期化
- GS-05 解雇ドメイン：パーティ所属拒否、装備返却容量の事前検証、装備個体ID維持、所有/slot解除、個体削除proposal
- GS-05失敗時は入力状態を書き換えない純粋proposal方式

## 次段階
- SaveTransactionでGS-03転職・GS-04帰還EXP・GS-05解雇を原子的に永続化
- GS-06 装備スロット/装備条件
- GS-07〜09 装備生成・MOD/レアリティ

## v4追加
- 共通 SaveTransaction：clone → mutate → validate → write の順でのみcommit
- validation失敗/書込失敗時に元Saveを変更しない境界を追加
- GS-06装備基盤：slot/category適合、Job条件、同一個体の重複装備防止、対象slot置換
- 両手武器のmain固定とsub解除をproposalとして実装
- EditMode試験を追加

## v4で意図的に未確定
- GS-06の正式slot名称・武器種別・Job別装備可能表は、現行仕様/Exportデータとの詳細照合前なので固定しない
- GS-07〜09の生成式・レアリティ・MOD抽選は未実装
- SaveTransactionは共通原子境界。実ファイルSaveStoreへの接続は後続

## v5追加
- GS-07 武器生成：iLv 1〜11、13武器種のSTR/DEX/INT要求係数、Attack/Accuracy/Magic Accuracy/Magic Weapon Bonus/Critical
- 盾のBlock Rate / Block Damage Cutを現行Active Balance Configから実装
- GS-08 防具生成基盤：重装/軽装/ローブ、鎧/頭/手/足、VIT/MND/AGI要求、HP/MP/Evasion
- Studio Export/equipment/equipment.json と生成規則/Active Balance ConfigをUnity GameDataへ同梱
- GS-09契約の一部：6 Rarity、3 MOD slot kind、Normal routeでUNIQUE/LEGENDARY/MYTHIC禁止、tag互換判定
- GS-07のStudio Export traceに対するEditMode試験を追加

## v5で未確定・未実装
- GS-08 magic_resistance の数値式：Generation Rulesはfieldを要求するが、今回確認したActive Balance Config本文には式がないため0を仮のゲーム値として採用せず、接続前に要確定。現コードの0は未接続placeholder。
- GS-09 mods.json は現在 data=[]。MOD抽選・tier/category weight・mod count・quality→rarityは正式Config照合後に実装。
- Studio UI/生成画面/CPFは移植していない。

## v6追加
- GS-10 Current combat capability契約：現行Studio実装に合わせ DUAL_WIELD のみを正式化
- GS-11 Critical Base Rate、物理/魔法Hit Rate、Critical先行判定、Critical時Guaranteed Hit、RNG消費順をC#化
- GS-12 Resistance 0〜75 clamp、Critical倍率、属性share、Formation/Random multiplier、最終floorをC#化
- GS-12 Block、Barrier FIFO、HP commit基盤を追加
- Studio JSの境界値を固定するEditMode試験を追加

## v6で未接続
- GS-10のFormal Passiveからcombat capabilityを収集するcompiler/runtime接続
- GS-06正式10スロット/weapon styleの完全置換（v4の簡易loadoutは後でGS-10と統合）
- GS-12 fatal interrupt / HP・MP吸収 / reflection の完全C#化
- GS-13 Formation/Target resolver

## v7追加
- GS-06正式Loadout基盤：10スロットへ拡張し、weapon style / DUAL_WIELD条件をGS-10と接続
- GS-13 Formation/Target Resolver基盤：前衛優先単体敵、単体味方、全体、自己、最低HP率味方
- GS-14修正版の重要ルール「戦闘開始時に一度だけランダム化し、同一戦闘中は固定」をC#化
- 死亡/離脱Actorは固定順を再抽選せずfilterする
- EditMode境界試験を追加

## v7で未完了
- GS-13の全selector/範囲形状/skill target schemaとの完全接続
- GS-14 Tick/行動ゲージ本体、詠唱、行動可能判定
- GS-12 Fatal Interrupt / Drain / Reflection
- v4 EquipmentLoadout.cs は互換確認用旧基盤。FormalEquipmentLoadout.cs を正式移行先とする。

## v8追加
- GS-14 Action Gauge基盤：speed×tickScaleで加算、100でReady、行動後は100だけ消費してoverflow維持
- 死亡/詠唱中ActorはGauge進行停止
- GS-15 Skill使用条件基盤：enabled/dead/silence/resource cost/MP/HP検証
- HPコストで使用者自身が0以下になる使用を拒否
- GS-15 Cast Runtime：castTicks、進行、完了、cancel
- GS-17 Unity側Skill runtime contractを追加
- EditMode試験を追加

## v8で未確定・未接続
- GS-14の正式Tick単位・speed→gauge換算係数は現行仕様との追加照合前なので、tickScale注入式の基盤としている
- GS-15の正式な詠唱中断条件、Cooldown、対象再検証、発動時点のcost timing
- Export/skill/skills.json の完全Loader
- GS-16 skill compiler/generator
- GS-18 status/lifecycle

## v9追加
- Export/skill/skills.json を Unity GameData に同梱
- GS-17 Skill Export Loader：schemaVersion=1、target、MP cost、castTime、cooldownをRuntimeへ変換
- 現Exportの10 Skill（SKL-0001〜0010）を入力境界として保持
- GS-18 Apply Lifecycle基盤：STATUS/DOT/BUFF/DEBUFF/SHIELD、apply/expire/consume/effective
- GS-18 Condition Engine現行契約 TARGET_POISONED をC#化
- proposal方式を維持し、入力collectionを直接変更しない
- EditMode試験追加

## v9で未完了
- GS-16 Formal Skill Compiler全体（DAMAGE/HEAL/APPLY/REMOVE/TARGET_CONTROL/REVIVEのcompile validation）
- GS-18 effect別の正式stack/refresh/overwrite policy
- Status master (`Export/master/statuses.json`) Loader
- Cooldown実行状態とGS-15発動フローへの統合
- Skill ExportのCORPSE targetは現SkillTargetKindに未追加のためRuntime変換未完

## v10追加
- GS-16 Formal Skill Compiler基盤：DAMAGE/HEAL/APPLY/REMOVE/TARGET_CONTROL/REVIVE effectを明示的にcompile
- 未登録effectを推測せず拒否し、APPLY/REMOVEのstatusId必須境界を追加
- GS-18 Status Master Loader基盤を追加
- GS-19 Passive Runtime基盤：同一passive series重複拒否、property contribution、combat capability収集
- GS-19からDUAL_WIELD capabilityをGS-10へ渡せる契約を追加
- GS-20 Trigger Runtime基盤：event/priority/GS-14固定battle order/idで決定的順序化、once trigger消費
- EditMode試験追加

## v10で未完了
- Skill Compilerの各effect payload完全validation
- Status master実データのstack/refresh/overwrite policy完全接続
- Passive Export Loader / formal-passive-compiler.js の全contribution型
- Trigger Engineの全trigger種別・再入防止・chain制御
- GS-21 AI Runtime接続

## v11追加
- GS-21 Formal AI Runtime基盤：ai_program_runtime.json Loader、entry instruction、ACTION evaluator、target selector
- 現行AIP-0001 / action.attack / ATS-0001をUnity GameDataへ接続
- GS-22 Monster Master Loader：active/enabled、Enemy Budget Cost、Spawn Weight/Tags、Drop Table、Formal AI binding
- GS-23 Quest LoaderとEncounter基盤：Quest Context、Box/Event Zone、required_monsters override、Budget編成
- Studioの現行AI/Monster/Quest ExportをUnity GameDataへ同梱
- Budget編成はspawn tag・cost・weight・maxUnitsを尊重
- EditMode試験追加

## v11で未完了
- GS-21 CONDITION/CALL/branch等の全AI instructionとtarget selector全種
- GS-22 monster-generation-domainの仮想iLv/装備性能による完全生成
- monster_mods.json は現行data=[]のためMOD生成未接続
- GS-23 random_event filterからEvent Masterを選ぶ処理、failure_policy、QuestRun Save統合
- GS-24 Story System

## v12追加
- GS-23 Event Master基盤とweighted outcome選択
- GS-24用Scenario Export（chapters/scenes/sections）をUnity GameDataへ収録
- GS-25 Stone Master基盤とstones/stone_mods Exportを収録
- GS-26 Drop Table Loader / weighted drop / amount rangeをRNG注入式で追加
- GS-27 AdventureRunSnapshot：ACTIVE→SUCCESS_PENDING_RETURN→RETURNED、FAILED/ABANDONED
- GS-04との重要接続境界として、冒険EXPはtemporaryのまま保持し、成功帰還確認時だけ取り出す
- Quest失敗/放棄時はtemporary EXPを破棄
- adventure_settings / exploration outcomesをGameDataへ収録
- EditMode試験追加

## v12で未完了
- Event Masterの実際のfilter/flag/condition全契約
- GS-24 Scenarioのscene/section実行器
- GS-25 difficulty補正とStone生成/装着の完全式
- GS-26 quest reward + monster drop + inventory capacity + SaveTransactionの統合
- GS-27 Encounter/Battle/Eventを時系列で回すAdventure Orchestrator

## v13追加
- GS-27 Adventure Orchestrator基盤：EVENT/ENCOUNTER/BATTLE/REWARD/RETURNを固定timelineで順次進行
- GS-28 Guild Progression基盤：rank up条件、Guild Point消費、member/warehouse capacity更新
- GS-29 Inventory/Warehouse基盤：instance ID一意性、capacity、add/remove proposal
- GS-30 GameSaveState：schemaVersion、Guild、Inventory、active QuestRunを共通Save DTOへ統合
- GS-30 Save whole-validation：domain欠落、capacity超過、inventory instance ID重複をcommit前に拒否
- 既存SaveTransactionのDeepClone境界へGameSaveStateを接続可能化
- EditMode試験追加

## v13で未完了
- Adventure OrchestratorからGS-21 AI/GS-22 Monster/GS-23 Event/GS-26 Rewardを実際にdispatchする統合runner
- GS-28正式Guild Rank Master/必要ポイント表とのデータ接続
- GS-29 stack可能item/warehouse移動/equipment inventoryとの型統合
- GS-30実ファイルSaveStore、backup/previous-good-save、migration
- Character/Equipment/Skill loadoutをGameSaveStateへ完全統合

## v14重要修正
- GS-06 FormalEquipmentLoadoutをStudio `equipment-loadout-domain.js` の実契約へ置換
- 正式slot IDs: weapon1/weapon2/head/armor/gloves/feet/amulet/ring1/ring2/belt
- 正式weapon styles: single/two_hand/dual_wield/weapon_shield/bow_quiver
- two_handは同一instanceをweapon1/weapon2両方へ配置
- dual_wieldはDUAL_WIELD capability必須、shield/quiver禁止
- weapon_shield / bow_quiver、bow_action_ready、MAIN/OFF strikeを実装
- STR/VIT/AGI/DEX/INT/MND/LUK全要求値とtwo-hand STR reliefを実装
- owner/instance/definition/slot/duplicate検証を追加
- 旧v7 FormalEquipment API依存テストを正式契約テストへ置換

## v14 Save強化
- JsonFileGameSaveStoreを追加
- tempへserialize→再読込validation→既存saveを.bakへ退避→commit
- primary破損時はprevious-good backupからLoad可能
- 既存SaveTransactionのclone→mutate→whole validation→write境界と接続可能

## v14未完了
- persisted equipment ref state互換変換
- Character/Skill/AI状態のGameSaveState完全統合
- Save schema migration/version upgrade
- Unity上でEditMode testsは未実行

## v15追加
- GameSaveStateへCharacter / Equipment instance / Equipment loadout / Skill IDs / Passive IDs / AI Program IDを統合
- RuntimeDomain whole-validation：Character ID重複、Equipment instance ID重複、未知ownerを拒否
- DeepCloneでCharacterのSkill/Passive/Equipment状態も分離
- AdventureRuntimeRunnerを追加しEVENT/ENCOUNTER/BATTLE/REWARD/RETURNをdispatch
- Reward commit失敗時はtimeline cursorを進めない
- RETURNはSUCCESS_PENDING_RETURN必須
- GS-04 temporary EXP確定境界をRETURN dispatchへ接続
- 統合EditMode試験追加

## v15未完了
- Characterの7能力値/growth/historyをSave DTOへ完全収録
- Skill cooldown/cast/status/AI execution stateの戦闘中snapshot保存
- Reward commitをInventory + SaveTransactionへ直接接続するproduction adapter
- AdventureRunSnapshotとtimelineのSave統合
- Unity上でEditMode testsは未実行

## v16追加
- Character SaveへSTR/VIT/AGI/DEX/INT/MND/LUKとgrowth history/RNG rollsを追加
- AdventureTimelineをGameSaveStateへ統合し、AdventureResumeAdapterで中断→ロード→再開可能なDTO変換を追加
- RewardInventoryProposalを追加し、RewardをInventory proposalへ変換
- RewardSaveTransactionで既存SaveTransactionのclone→mutate→whole-validation→writeへReward commitを接続
- capacity途中失敗はdraft内だけに留まり、production transactionでは元Saveを変更しない設計
- DeepCloneをstats/growth history/timelineまで拡張
- EditMode試験追加

## v16未完了
- BattleSnapshot（cast/cooldown/status/AI/RNG state）のSave統合
- Save schema migration/version upgrade
- Rewardのstackable item正式契約
- Unity上でEditMode testsは未実行

## v17追加
- F02/C01 BattleSnapshot Save DTOを追加：battleId/schemaVersion/tick/fixedActorOrder
- ActorごとにHP/MP、ActionGauge/Speed、Cast、Cooldown、Applied Effects、AI Programを保存
- RNGをpurpose別streamとしてcursor + recorded rollsで保存
- fixed battle orderをSaveし、再開時にGS-14の順序を再抽選しない境界を追加
- BattleResumeAdapterでGauge/Cast/Status/Cooldown/AIをRuntimeへ復元
- ReplayRandomSourceで保存cursor位置から決定的にRNG再開
- GameSaveState activeBattleへ統合、whole-validation/DeepClone対象化
- EditMode試験追加

## v17未完了
- BattleSnapshotから完全なBattle engine instanceを組み立てるproduction factory
- RNGの新規draw生成器とrecord/replay統合
- ActionReservation / ResolvedHit Save DTO
- Save schema migration/version upgrade
- Unity上でEditMode testsは未実行

## v18追加
- F02/C02 ActionReservation Save DTOをBattleSnapshotへ統合
- F02/C03 ResolvedHit Save DTOをBattleSnapshotへ統合
- reservation/hit ID一意性、actor参照、tick、terminal conflict、damage/RNGをwhole-validation
- RecordingRandomSourceを追加し、通常実行時のRNG drawをpurpose streamへ記録
- ReplayRandomAdapterを追加し、保存cursorからIRandomSourceとして再生
- BattleRngStreamsで用途別RNG streamを明示管理
- BattleRuntimeFactoryを追加しSnapshotから固定順・Actor runtime・Reservation・ResolvedHitを復元
- GameSaveState DeepCloneへReservation/ResolvedHitを追加
- EditMode試験追加

## v18未完了
- Battle engineのstep executorそのものへのActionReservation生成/ResolvedHit記録フック
- purpose別RNGのproduction seed生成と新規draw/replay切替
- QuestRunSnapshotのRNG stream統合
- Save schema migration/version upgrade
- Unity上でEditMode testsは未実行

## v19追加
- BattleStepExecutorを追加し、ActionReservation → Hit/Critical → Damage → Block → HP commit → ResolvedHit記録を一本化
- GS-10 HitCriticalのcritical-first契約をそのまま利用（critical時はhit RNGを消費しない）
- GS-11 DamageDefenseのresistance/critical/element/formation/random/floor順序を利用
- Block RNGを専用sourceとして分離
- missでもReservation/ResolvedHitをterminal recordとしてSnapshotへ保存
- HP mutationとfatalフラグをBattleSnapshotへ即時反映
- EditMode試験追加

## v19未完了
- Barrier FIFO / fatal resolver / absorb / reflectionをBattleStepExecutorへ統合
- Skill cost/cooldown/cast/action gauge consumeとのActionReservation統合
- RecordingRandomSourceをproduction BattleStepへ注入するcomposition root
- Unity上でEditMode testsは未実行

## v20追加
- BattleStepExecutorへBarrier FIFOを統合し、Block後DamageをBarrier→HPの順で処理
- fatalResolverをCommitHp境界へ接続
- HP absorb / MP absorbをactual HP loss基準・ceilで統合
- reflectionをactual HP loss基準・floorで統合
- reflectionは耐性再適用なし・再帰なしの参照effectとして直接HP commit
- BattleStepResultへBarrier残量/吸収量/反射量を記録
- EditMode試験追加

## v20未完了
- Barrier状態そのものをAppliedEffect/Status Masterと同期
- fatal resolverを正式Trigger/Lifecycleへ接続
- Skill cost/cooldown/cast/action gauge consumeとReservationのatomic統合
- Unity上でEditMode testsは未実行

## v21追加
- SkillActionTransactionを追加しSkill use condition / Resource Cost / Action Gauge / Cast / Cooldown / BattleStepを一本化
- Gauge>=100、Cooldown非active、Cast非active、SkillUseCondition成功をmutation前に検証
- 即時SkillはCost→Gauge consume→BattleStep→Cooldownを一行動として処理
- Cast SkillはCost/Gaugeを確定してCastSaveRecordを開始し、攻撃解決を保留
- AdvanceCastAndCooldownsでCast/Cooldown tick更新
- BattleStepがterminal commit前に失敗した場合はResource/Gaugeをrollback
- EditMode試験追加

## v21未完了
- Cast完了時に保存済みActionReservation/Targetを使ってBattleStepを発火するCastCompletion executor
- target revalidation / interruption / cost timingの正式仕様確定
- ActionGauge tickScaleの正式係数
- Unity上でEditMode testsは未実行

## v22重要修正 — Studio参考元でC01/C02/C03を正式化
参照: guild-adventure-studio-sub(24).zip / assets/shared/js/runtime-boundary-contracts.js

- v17〜v21で暫定実装していたC01/C02/C03 DTOをStudioの正式contract schema_version=1へ修正
- C01 BattleSnapshot: battle_id / settings_version / seed / actors / formation / fixed_order
- C02 ActionReservation: reservation_id / actor_id / skill_id / start_tick / complete_tick / fixed_target_ids / usage_conditions
- C03 ResolvedHit: action_id / hit_index / source_id / target_id / judgement / per_hit_damage / block / barrier / committed_hp / actual_hp_loss / trigger_context
- Unityのtick/RNG streams/reservation history/hit historyはC01本体とは区別したcontinuation extensionとして保持
- usage_conditions / block / barrier / trigger_contextは意味を推測せずopaque JSONとして保持
- C01はsettingsVersionとseed必須、fixed orderはactor全員を一度ずつ含むことを検証
- 暫定contract前提だったv17〜v21のEditMode testsを削除し、正式contract testsへ置換

Studio側で確認できたSkill実行契約:
- precheck時にtargetIds/targetStates/costs/conditionResultをexecutionSnapshotへ固定
- 実行時にcostを再確認して消費
- cooldownは実行成立時に開始
- effect開始時、precheck時にaliveだったtargetが死亡/退出/untargetableならeffectをskip
- cooldown duration = ceil(base * (1 - COOLDOWN_REDUCTION))
- cast duration = ceil(base * (1 - CAST_TIME_REDUCTION))

## v22未完了
- 上記executionSnapshotをUnity CastSaveRecord/ActionReservationへ正式移植
- cooldown/cast reduction modifierのUnity統合
- C03 opaque block/barrier/trigger_contextの正式型は追加仕様確認後
- Unity上でEditMode testsは未実行
