# Финальная проверка практической работы 9–10 / Лаба 5

Проект: `/Users/ruslanvakhrusev/Documents/GitHub/ShablonC-`

Дата проверки: 27.04.2026

## 1. Краткий вердикт

**ГОТОВО К СДАЧЕ ПОСЛЕ РУЧНЫХ НАСТРОЕК В UNITY**

Основные требования практической работы по архитектуре выполнены: обычные мобы и босс используют явные State Machine, состояния вынесены в отдельные классы, контексты передают состояниям зависимости, а MonoBehaviour-контроллеры в основном работают как Unity-адаптеры. У обычных мобов есть минимум четыре состояния: `Idle`, `Aggression`, `Attack`, `Flee`. У босса есть больше семи state-классов, включая обязательные `Idle`, `Aggro`, `Attack`, `HeavyAttack`, а также `Chase`, `Enrage`, `Heal`, `Death`.

Мирный режим реализован централизованно через `GameModeService`, который создаётся в `ProjectEntryPoint`, выбирается из главного меню и передаётся в игровую сцену через `GameSceneEntryPoint`. UI главного меню не управляет врагами напрямую: он только выставляет режим и загружает игровую сцену. Урон идёт через общую систему `DamageInfo` / `IDamageable` / `EnemyHealth` / `PlayerHealth`.

Критических compile errors не найдено: Unity batchmode успешно загрузил проект и выполнил script compilation, `git diff --check` чистый. При этом есть несколько важных рисков перед защитой: у boss prefab-источника слой всё ещё `Default`, хотя в `GameScene` объект `Boss 1` переопределён на layer `Enemy`; у `Boss 1` включён `Animator.applyRootMotion`, что может конфликтовать с `NavMeshAgent`; в `BossAnimationController` не найден параметр `Heal`; `PlayerCombatLogic` не предотвращает повторный урон одной цели, если в радиус атаки попало несколько collider’ов одного врага. Эти пункты нужно либо проверить вручную в Unity, либо исправить точечно перед финальной демонстрацией.

## 2. Таблица соответствия требованиям

| Требование | Статус | Подтверждение в коде | Проблема/риск | Что сделать |
|---|---|---|---|---|
| Обычные мобы через State Machine | OK | `Assets/Scripts/Gameplay/Enemies/States/EnemyStateMachine.cs`, `EnemyStatefulAIBase.cs` | Нет | Не требуется |
| 4 состояния обычных мобов | OK | `EnemyIdleState.cs`, `EnemyAggressionState.cs`, `EnemyMeleeAttackState.cs` / `EnemyRangedAttackState.cs`, `EnemyFleeState.cs` | Нет | Не требуется |
| Бегство обычных мобов | OK | `EnemyFleeState.FixedTick()` двигает от игрока через `GetDirectionAwayFromPlayer()` | Нет | Проверить в Play Mode |
| Босс добавлен | OK | `Assets/Scripts/Gameplay/Boss/BossController.cs`, prefab `Assets/OurPrefabs/Boss 1.prefab`, instance в `GameScene.unity` | В `GameSceneEntryPoint` имя поиска `_bossObjectName: Boss`, а объект называется `Boss 1`; есть fallback через `FindFirstObjectByType` | Желательно переименовать объект в `Boss` или поставить `_bossObjectName = Boss 1` |
| 4 состояния босса | OK | `BossIdleState`, `BossAggroState`, `BossAttackState`, `BossHeavyAttackState` | Нет | Не требуется |
| 7+ состояний босса | OK | Дополнительно есть `BossChaseState`, `BossEnrageState`, `BossHealState`, `BossDeathState`, alias-состояния | Нет | Не требуется |
| Ускорение атак босса при HP <= 50% | OK | `BossContext.ShouldEnterEnrage`, `BossContext.EnterEnrage`, `TriggerAttackCooldown()`, `TriggerHeavyAttackCooldown()` | В edge-case “потерял цель и ушёл в Heal” Enrage может быть отложен/сброшен лечением | Проверить сценарий в Play Mode |
| Мирный режим для обычных мобов | OK | `EnemyContext.IsPeacefulMode`, `EnemyIdleState`, `EnemyAggressionState`, `EnemyAttackStateBase`, `EnemyHealth.TakeDamage` | Нет | Проверить в Play Mode |
| Мирный режим для босса | OK | `BossIdleState`, `BossAggroState`, `BossChaseState`, `BossController.NotifyHitByPlayer()` | Зависит от корректной доставки melee-урона игрока до `EnemyHealth` босса | Проверить layer/collider и удар в Play Mode |
| Паттерн State | OK | `IEnemyState`, `IBossState`, отдельные классы состояний | Нет | Не требуется |
| State Machine как отдельная подсистема | OK | `EnemyStateMachine`, `BossStateMachine` хранят `CurrentState`, вызывают `Enter/Exit/Tick/FixedTick` | Нет | Не требуется |
| Контекст для состояний | OK | `EnemyContext`, `BossContext` | Нет | Не требуется |
| UI не управляет врагами напрямую | OK | `MainMenuView` -> `MainMenuController` -> `GameModeService` -> `GameSceneEntryPoint` | Нет | Не требуется |
| GameModeService или аналог | OK | `GameMode.cs`, `IGameModeService.cs`, `GameModeService.cs`, `AppServices.cs` | Нет | Не требуется |
| DamageInfo/IDamageable | OK | `Assets/Scripts/Gameplay/Common/DamageInfo.cs`, `IDamageable.cs` | Нет | Не требуется |
| BossDamageHitbox | OK | `BossDamageHitbox.SetActive`, `TryDamage`, `HashSet<IDamageable>` | `_targetLayers = Everything` в prefab/scene; может задеть не только игрока, если рядом есть другие `IDamageable` | Желательно ограничить mask на Player, если будет player layer |
| Animator setup | NEEDS_MANUAL_UNITY_SETUP | `BossController` ожидает `IsMoving`, `Attack`, `HeavyAttack`, `Enrage`, `IsEnraged`, `AttackSpeedMultiplier`, `Heal` | В `BossAnimationController.controller` найдено всё кроме `Heal`; `Apply Root Motion` у boss Animator включён | Добавить `Heal`; выключить Apply Root Motion |
| NavMesh setup | NEEDS_MANUAL_UNITY_SETUP | `BossController.MoveToTarget()` использует `NavMeshAgent.SetDestination` | Наличие запечённого NavMesh кодом не гарантируется | Проверить NavMesh и позицию босса на NavMesh |
| layer Enemy для босса | PARTIAL | В `GameScene.unity` instance `Boss 1` переопределён на layer 6 `Enemy`; `PlayerCombat.enemyLayers` = 64 | Prefab `Assets/OurPrefabs/Boss 1.prefab` остаётся на layer 0 `Default` | Для безопасности поставить layer `Enemy` и на prefab-источник |
| Melee-атака игрока | PARTIAL | `PlayerCombatLogic` использует `Physics.OverlapSphere(..., _enemyLayers)` и ищет `IDamageable` на parent | Нет защиты от повторного урона одной цели за swing при нескольких collider’ах | Минимально добавить `HashSet<IDamageable>` внутри `PerformAttack` |

## 3. Проверка обычных мобов

### Найденные классы обычных мобов

- `Assets/Scripts/Gameplay/Enemies/EnemyAI.cs` — ближний враг.
- `Assets/Scripts/Gameplay/Enemies/EnemyRangedAI.cs` — дальний враг.
- `Assets/Scripts/Gameplay/Enemies/EnemyStatefulAIBase.cs` — базовый MonoBehaviour-адаптер для state machine.
- `Assets/Scripts/Gameplay/Enemies/EnemyHealth.cs` — HP, `IDamageable`, события урона/смерти.
- `Assets/Scripts/Gameplay/Enemies/EnemyCombat.cs` — ближняя атака моба.
- `Assets/Scripts/Gameplay/Enemies/EnemyRangedCombat.cs` — дальняя атака моба.

### State Machine обычных мобов

- State Machine: `Assets/Scripts/Gameplay/Enemies/States/EnemyStateMachine.cs`
- Интерфейс состояния: `Assets/Scripts/Gameplay/Enemies/States/IEnemyState.cs`
- Базовый класс состояния: `Assets/Scripts/Gameplay/Enemies/States/EnemyStateBase.cs`
- Контекст: `Assets/Scripts/Gameplay/Enemies/States/EnemyContext.cs`

`EnemyStateMachine` хранит `CurrentState`, выполняет `ChangeState(...)`, вызывает `Exit()` у старого состояния и `Enter()` у нового, а также прокидывает `Tick()` и `FixedTick()`.

### Найденные состояния

- `EnemyIdleState` — `Assets/Scripts/Gameplay/Enemies/States/EnemyIdleState.cs`
- `EnemyAggressionState` — `Assets/Scripts/Gameplay/Enemies/States/EnemyAggressionState.cs`
- `EnemyMeleeAttackState` — `Assets/Scripts/Gameplay/Enemies/States/EnemyMeleeAttackState.cs`
- `EnemyRangedAttackState` — `Assets/Scripts/Gameplay/Enemies/States/EnemyRangedAttackState.cs`
- `EnemyFleeState` — `Assets/Scripts/Gameplay/Enemies/States/EnemyFleeState.cs`

### Как работает Idle -> Aggro

В `EnemyIdleState.Tick()` сначала проверяется возможность бегства, затем мирный режим, затем дистанция атаки и радиус обнаружения. В обычном режиме при `Context.HasDetectedPlayer` вызывается:

```csharp
StateMachine.ChangeState(Context.AggressionState, "Player detected from idle");
```

В мирном режиме переход блокируется:

```csharp
if (Context.IsPeacefulMode)
    return;
```

### Как работает Attack

Общая логика атаки находится в `EnemyAttackStateBase`. Состояние останавливает движение, смотрит на игрока, проверяет cooldown через `CanAttack()` и запускает `Animator` trigger `Attack`. Конкретная реализация отличается для ближнего и дальнего врага:

- `EnemyMeleeAttackState` использует `EnemyCombat`;
- `EnemyRangedAttackState` использует `EnemyRangedCombat`.

В peaceful mode атака подавляется: состояние возвращает врага в `Idle` или `Flee`.

### Как работает Flee

`EnemyFleeState.FixedTick()` реально двигает моба в противоположную сторону от игрока:

```csharp
Vector3 direction = Context.GetDirectionAwayFromPlayer();
Context.Move(direction, Time.fixedDeltaTime);
Context.FaceDirection(direction, Time.fixedDeltaTime);
```

Порог для peaceful flee зафиксирован в `EnemyContext`:

```csharp
private const float PeacefulFleeHealthThreshold = 0.5f;
```

### Как low HP переводит моба в Flee

`EnemyHealth.TakeDamage()` после применения урона проверяет `EnemyContext`. В peaceful mode моб не агрится, а только запрашивает бегство, если HP <= 50%:

```csharp
if (_fleeContext.IsPeacefulMode)
{
    if (_fleeContext.ShouldFleeInPeacefulMode)
        _fleeContext.RequestFlee(EnemyFleeReason.LowHealth);
}
```

## 4. Проверка босса

### Основные файлы

- `Assets/Scripts/Gameplay/Boss/BossController.cs`
- `Assets/Scripts/Gameplay/Boss/BossContext.cs`
- `Assets/Scripts/Gameplay/Boss/BossStateMachine.cs`
- `Assets/Scripts/Gameplay/Boss/IBossState.cs`
- `Assets/Scripts/Gameplay/Boss/AbstractBossState.cs`
- `Assets/Scripts/Gameplay/Boss/BossDamageHitbox.cs`
- `Assets/Scripts/Gameplay/Boss/States/*.cs`

### Обязательные 4 состояния босса

- `BossIdleState` — покой.
- `BossAggroState` — агрессия и выбор следующего действия.
- `BossAttackState` — обычная атака.
- `BossHeavyAttackState` / `BossStrongAttackState` — сильная атака.

### Состояния, засчитываемые в 7+

- `BossIdleState`
- `BossAggroState`
- `BossChaseState`
- `BossAttackState`
- `BossHeavyAttackState`
- `BossEnrageState`
- `BossHealState`
- `BossDeathState`

Дополнительно есть alias/legacy-классы:

- `BossAggressionState : BossAggroState`
- `BossRageState : BossEnrageState`
- `BossStrongAttackState : BossHeavyAttackState`
- `BossPatrolState`

### BossStateMachine

Файл: `Assets/Scripts/Gameplay/Boss/BossStateMachine.cs`

Проверено:

- есть `CurrentState`;
- есть `ChangeState(IBossState nextState, string reason)`;
- вызываются `Exit()` и `Enter()`;
- есть `Tick()` и `FixedTick()`;
- Animator не заменяет игровую State Machine.

### BossController

Файл: `Assets/Scripts/Gameplay/Boss/BossController.cs`

`BossController` выполняет роль Unity-адаптера:

- кэширует `NavMeshAgent`, `Animator`, `EnemyHealth`;
- создаёт `BossContext`;
- создаёт state-классы;
- вызывает `Context.Tick`, `StateMachine.Tick`, `StateMachine.FixedTick`;
- содержит методы для Animation Events: `EnableDamageHitboxes`, `DisableDamageHitboxes`, `OnAttackAnimationFinished`;
- подписывается на события HP.

Логика переходов находится в state-классах, а не в одном большом `Update()`.

### BossContext

Файл: `Assets/Scripts/Gameplay/Boss/BossContext.cs`

Контекст содержит:

- `Transform` босса;
- `Transform` цели;
- `NavMeshAgent`;
- `Animator`;
- `EnemyHealth`;
- `PlayerHealth`;
- флаги `IsPeacefulMode`, `WasProvokedByPlayer`, `IsEnraged`, `HasEnteredEnrage`, `HasHealed`;
- cooldown-таймеры;
- `AttackSpeedMultiplier`;
- проверки радиусов через свойства `HasDetectedTarget`, `HasLostTarget`, `IsTargetInAttackRange`.

### Idle

`BossIdleState` останавливает движение и выключает hitbox’ы. В peaceful mode не агрится по радиусу, пока `WasProvokedByPlayer == false`.

### Aggro

`BossAggroState`:

- подавляет агрессию в peaceful mode, если босс не был спровоцирован;
- при потере цели уходит в `Idle` или `Heal`;
- при HP <= 50% запрашивает `Enrage`;
- если игрок в радиусе атаки, выбирает `HeavyAttack` или `Attack`;
- если игрок далеко, переводит босса в `Chase`.

### Chase

`BossChaseState` реально отвечает за преследование:

```csharp
float speed = Context.IsTargetHealthLow ? Boss.FinisherChaseSpeed : Boss.ChaseSpeed;
Boss.MoveToTarget(speed);
```

Если HP игрока ниже порога, скорость преследования повышается.

### Attack и HeavyAttack

`BossAttackState` и `BossHeavyAttackState`:

- останавливают движение;
- запускают соответствующий Animator trigger;
- включают окно урона через hitbox’ы;
- имеют fallback по таймеру, если Animation Events не подключены;
- после завершения возвращают босса в `Aggro`, `Chase` или `Idle` через `SelectMovementOrIdleState()`.

### Enrage

`BossEnrageState` является отдельным состоянием. Вход в фазу выполняет:

- `Boss.EnterPhaseTwo()`;
- `Boss.BeginEnrageAnimation()`;
- установку `IsEnraged`;
- установку `AttackSpeedMultiplier`;
- запуск trigger `Enrage`.

`BossContext.HasEnteredEnrage` защищает от повторного входа каждый кадр.

Ускорение атак реализовано через деление cooldown на multiplier:

```csharp
AttackCooldownRemaining = Controller.AttackCooldown / AttackSpeedMultiplier;
HeavyAttackCooldownRemaining = Controller.HeavyAttackCooldown / AttackSpeedMultiplier;
```

### Heal

`BossHealState` — отдельное состояние. После завершения вызывает `Context.MarkHealed()`, поэтому лечение не бесконечное. После лечения босс возвращается в `Idle`.

### Мирный режим босса

Босс не агрится по радиусу в `BossIdleState`, если включён peaceful mode и нет `WasProvokedByPlayer`.

Переход в бой после удара реализован через:

- `EnemyHealth.OnDamaged`;
- `BossController.HandleDamaged(DamageInfo damage)`;
- `BossController.IsDamageFromPlayer(...)`;
- `BossController.NotifyHitByPlayer()`.

`NotifyHitByPlayer()` выставляет `WasProvokedByPlayer` и переводит босса в `AggroState`.

## 5. Проверка мирного режима

### Где хранится GameMode

- `Assets/Scripts/App/Services/GameMode.cs`
- `Assets/Scripts/App/Services/IGameModeService.cs`
- `Assets/Scripts/App/Services/GameModeService.cs`

`GameModeService` хранит `CurrentMode` и свойство `IsPeacefulMode`.

### Где кнопка Play включает Normal

Файл: `Assets/Scripts/UI/MainMenu/MainMenuController.cs`

Метод:

```csharp
private void OnPlayClicked()
{
    _gameModeService.SetMode(GameMode.Normal);
    _sceneLoader.Load(_gameSceneName);
}
```

### Где кнопка Play Peace Mode включает Peaceful

Файл: `Assets/Scripts/UI/MainMenu/MainMenuController.cs`

Метод:

```csharp
private void OnPlayPeaceModeClicked()
{
    _gameModeService.SetMode(GameMode.Peaceful);
    _sceneLoader.Load(_gameSceneName);
}
```

### Как режим попадает в игровую сцену

`ProjectEntryPoint` создаёт `AppServices`, включая `GameModeService`, и передаёт их в `SceneEntryPointBase.Initialize(...)`. В игровой сцене `GameSceneEntryPoint` получает `appServices.GameModeService`.

### Как режим получают обычные мобы

В `GameSceneEntryPoint.InjectPlayerIntoEnemies()` вызывается:

```csharp
enemyAI.SetPeacefulMode(_gameModeService.IsPeacefulMode);
enemyAI.Construct(_playerController.transform, roomReference.RoomBounds);
```

Для ranged-мобов логика аналогична.

### Как режим получает босс

В `GameSceneEntryPoint.InjectBoss()` вызывается:

```csharp
bossController.SetPeacefulMode(_gameModeService.IsPeacefulMode);
bossController.Construct(_playerController.transform, roomReference != null ? roomReference.RoomBounds : null);
```

### Сценарии Play Mode

Нужно проверить два запуска:

- обычный `Play`: мобы и босс агрятся по радиусу;
- `Play peace mode`: мобы не агрятся, босс агрится только после удара.

## 6. Проверка архитектуры по лекциям

| Лекция | Что требуется по смыслу лекции | Как реализовано | Статус |
|---|---|---|---|
| Лекция 1 | Модульность, читаемость, разделение зон ответственности | Враги, босс, сервисы, entry points и UI разнесены по папкам; логика состояний вынесена в отдельные классы | OK |
| Лекция 2 | SOLID: SRP, OCP, DIP, ISP, отсутствие огромного switch | State-классы отвечают за отдельные состояния; новые состояния добавляются новыми классами; `IGameModeService` отделяет высокоуровневую логику от реализации | OK |
| Лекция 3 | Bootstrapper/EntryPoint/DI/Service Locator; MonoBehaviour как Unity-адаптер | `ProjectEntryPoint` создаёт `AppServices`; `MainMenuSceneEntryPoint` и `GameSceneEntryPoint` собирают сцену; контроллеры вызывают StateMachine и кэшируют Unity-компоненты | OK |
| Лекция 4 | UI/MV-X; UI не управляет игровой логикой напрямую | `MainMenuView` даёт события, `MainMenuController` выставляет режим и грузит сцену, враги получают режим позже через `GameSceneEntryPoint` | OK |
| Лекция 5 | State Pattern, State Machine, контекст, Enter/Exit/Tick, Animator как интеграция | Есть `IEnemyState`, `IBossState`, `EnemyStateMachine`, `BossStateMachine`, `EnemyContext`, `BossContext`; Animator используется только через параметры | OK |

Примечание: текст PDF не был извлечён shell-инструментами (`pdftotext`/Python PDF-библиотеки отсутствуют), поэтому сверка выполнена по архитектурным пунктам лекций, указанным в задании.

## 7. Проверка системы урона

### IDamageable

Файл: `Assets/Scripts/Gameplay/Common/IDamageable.cs`

```csharp
public interface IDamageable
{
    void TakeDamage(DamageInfo damage);
}
```

### DamageInfo

Файл: `Assets/Scripts/Gameplay/Common/DamageInfo.cs`

`DamageInfo` содержит:

- `Amount`;
- `DamageType`;
- `Source`.

Поле `Source` позволяет понять, кто нанёс урон.

### Как игрок наносит урон мобам

Файлы:

- `Assets/Scripts/Gameplay/Player/Components/PlayerCombat.cs`
- `Assets/Scripts/Gameplay/Player/Core/PlayerCombatLogic.cs`

`PlayerCombatLogic.PerformAttack(...)` вызывает:

```csharp
Physics.OverlapSphere(attackPoint.position, _attackRange, _enemyLayers)
```

Затем ищет `IDamageable` на collider или parent.

### Как игрок наносит урон боссу

Так как босс использует `EnemyHealth : IDamageable`, игрок наносит урон боссу тем же способом, что и обычным мобам. Важно, чтобы damageable collider босса был на layer, входящем в `PlayerCombat.enemyLayers`.

В сцене `GameScene.unity` instance `Boss 1` имеет override `m_Layer = 6`, где layer 6 — `Enemy`. Это хорошо для текущей сцены. Но prefab `Assets/OurPrefabs/Boss 1.prefab` сам по себе остаётся на `Default`, поэтому при использовании prefab в другой сцене melee-урон может не работать.

### Нет ли костылей обхода enemyLayers

В текущем `PlayerCombatLogic` нет специального обхода вида “если найден `BossController`, ударить вне mask”. Это соответствует требованию. Однако есть другой риск: нет `HashSet<IDamageable>` для предотвращения повторного урона одной цели за swing.

### Как босс понимает, что его ударил игрок

`BossController` подписывается на `EnemyHealth.OnDamaged`. Затем `IsDamageFromPlayer(...)` проверяет `damage.Source` по тегу `player` / `Player` или наличию `PlayerController` в parent.

### Как hitbox’ы босса наносят урон игроку

Файл: `Assets/Scripts/Gameplay/Boss/BossDamageHitbox.cs`

`BossDamageHitbox`:

- активируется только через `SetActive(true)`;
- очищает `_damagedTargets` при каждом включении;
- проверяет layer mask;
- игнорирует собственный root;
- ищет `IDamageable`;
- наносит `DamageInfo` с source = root босса.

## 8. Ручные настройки Unity перед сдачей

- [ ] Босс в сцене на layer `Enemy`.
- [ ] Prefab-источник `Assets/OurPrefabs/Boss 1.prefab` тоже желательно перевести на layer `Enemy`.
- [ ] Damageable collider’ы босса на layer `Enemy`.
- [ ] `enemyLayers` у игрока включает `Enemy`.
- [ ] На боссе есть `BossController`.
- [ ] На боссе есть `EnemyHealth` / `Health`.
- [ ] На боссе есть `NavMeshAgent`.
- [ ] На боссе есть `Animator`.
- [ ] В `BossController` назначены `damageHitboxes`.
- [ ] На hitbox’ах `Collider Is Trigger = true`.
- [ ] В Animator созданы параметры `IsMoving`, `Attack`, `HeavyAttack`, `Enrage`, `IsEnraged`, `AttackSpeedMultiplier`, `Heal`.
- [ ] В `BossAnimationController` добавить параметр `Heal`, потому что сейчас он не найден.
- [ ] Animation Events добавлены на клипы атак:
  - `EnableDamageHitboxes`;
  - `DisableDamageHitboxes`;
  - `OnAttackAnimationFinished`.
- [ ] NavMesh запечён.
- [ ] `Apply Root Motion` выключен у Animator босса.
- [ ] `attackRange > stoppingDistance`; сейчас у boss instance `attackRange = 2.02`, `stoppingDistance = 0`, запас есть.
- [ ] Кнопка `Play Peace Mode` назначена в `MainMenuView`.
- [ ] Игрок имеет корректный tag/layer/`PlayerController`.
- [ ] В `GameSceneEntryPoint` желательно выставить `_bossObjectName = Boss 1` или переименовать объект босса в `Boss`.

## 9. Play Mode тесты перед отправкой

### Normal mode

1. Запустить через `Play`.
2. Подойти к обычному мобу.
3. Проверить, что моб агрится по радиусу.
4. Проверить, что моб преследует игрока.
5. Проверить, что моб атакует.
6. Уменьшить HP моба и проверить переход в `Flee`, если low HP включён настройками.
7. Подойти к боссу.
8. Проверить, что босс агрится по радиусу.
9. Проверить переход в `BossChaseState`.
10. Проверить обычную атаку.
11. Проверить сильную атаку.
12. Снизить HP босса ниже 50%.
13. Проверить вход в `BossEnrageState`.
14. Проверить, что после Enrage атаки происходят быстрее.
15. Проверить `BossHealState`, если босс потерял цель/оказался в Idle при HP ниже порога и ещё не лечился.

### Peaceful mode

1. Запустить через `Play Peace Mode`.
2. Подойти к обычным мобам.
3. Проверить, что они не агрятся по радиусу.
4. Ударить обычного моба.
5. Проверить, что он не атакует в ответ.
6. Снизить HP моба ниже 50%.
7. Проверить, что моб убегает.
8. Подойти к боссу.
9. Проверить, что босс не агрится по радиусу.
10. Ударить босса.
11. Проверить, что босс вызывает `NotifyHitByPlayer()` и начинает бой.
12. Снизить HP босса ниже 50%.
13. Проверить Enrage и ускорение атак.
14. Проверить отсутствие `NullReferenceException` в Console.

## 10. Найденные проблемы

### Критические

Критических архитектурных или compile-проблем не найдено. Unity batchmode завершился успешно, script compilation прошёл.

### Некритические

1. **`PlayerCombatLogic` не предотвращает повторный урон одной цели за swing.**
   - Файл: `Assets/Scripts/Gameplay/Player/Core/PlayerCombatLogic.cs`
   - Риск: если в радиус melee-атаки попадёт несколько collider’ов одного босса/врага, `TakeDamage` может быть вызван несколько раз.
   - Минимальный fix: внутри `PerformAttack` завести `HashSet<IDamageable>` и перед `TakeDamage` проверять, не была ли цель уже поражена в этом swing.

2. **`GameSceneEntryPoint._bossObjectName = Boss`, но объект в сцене называется `Boss 1`.**
   - Файл сцены: `Assets/Scenes/SceneRuslan/GameScene.unity`
   - Риск: сейчас спасает fallback `FindFirstObjectByType<BossController>()`; при нескольких боссах или других объектах с `BossController` может выбрать не тот объект.
   - Минимальный fix: в Inspector поставить `_bossObjectName = Boss 1` или переименовать объект в `Boss`.

3. **Edge-case Enrage/Heal.**
   - Файлы: `BossAggroState.cs`, `BossChaseState.cs`, `BossIdleState.cs`, `BossHealState.cs`
   - Риск: если босс при HP <= 50% потерял цель или оказался в Idle, он может сначала войти в `HealState`, а `HealState` очищает enrage request. В активном бою Enrage работает корректно, но сценарий “сначала потерял цель, потом heal” требует Play Mode проверки.

### Требующие ручной настройки Unity

1. **`Apply Root Motion` у Animator босса включён.**
   - Файл: `Assets/OurPrefabs/Boss 1.prefab`
   - Строка YAML: `m_ApplyRootMotion: 1`
   - Риск: root motion может конфликтовать с `NavMeshAgent.SetDestination`.
   - Минимальный fix: выключить `Apply Root Motion` на Animator босса.

2. **Параметр Animator `Heal` не найден в `BossAnimationController.controller`.**
   - Файл: `Assets/OurPrefabs/BossAnimationController.controller`
   - Найдены параметры `IsMoving`, `Attack`, `HeavyAttack`, `Enrage`, `IsEnraged`, `AttackSpeedMultiplier`, но не найден `Heal`.
   - Минимальный fix: добавить Trigger `Heal`.

3. **Prefab-источник `Boss 1` на layer `Default`.**
   - Файл: `Assets/OurPrefabs/Boss 1.prefab`
   - В сцене `GameScene` layer переопределён на `Enemy`, но сам prefab остаётся на `Default`.
   - Минимальный fix: поставить prefab root и damageable collider’ы на layer `Enemy`.

4. **`BossDamageHitbox._targetLayers = Everything`.**
   - Файл: `Assets/OurPrefabs/Boss 1.prefab`
   - Риск: hitbox может пытаться нанести урон любому `IDamageable`, если такой объект окажется рядом.
   - Минимальный fix: при наличии отдельного Player layer ограничить target mask на игрока.

### Рекомендации

- Добавить `HashSet<IDamageable>` в `PlayerCombatLogic`.
- Ограничить `BossDamageHitbox._targetLayers`.
- Сохранить scene/prefab после ручных Inspector-настроек.
- Проверить `Play peace mode` именно из главного меню, а не прямым запуском `GameScene`, чтобы `GameModeService` успел выставить режим.

## 11. Итоговый вывод

Проект можно отправлять преподавателю после ручной проверки Unity-настроек и желательно одной точечной правки `PlayerCombatLogic` против повторного урона по одной цели за swing.

Минимальные действия перед сдачей:

1. В Unity проверить, что `Boss 1` и его damageable collider’ы в сцене стоят на layer `Enemy`.
2. Для безопасности перевести prefab `Assets/OurPrefabs/Boss 1.prefab` на layer `Enemy`.
3. Выключить `Apply Root Motion` на Animator босса.
4. Добавить Animator trigger `Heal` в `BossAnimationController`.
5. Проверить `Play` и `Play Peace Mode` по чек-листу.
6. Если преподаватель будет проверять мульти-collider melee, добавить дедупликацию `IDamageable` в `PlayerCombatLogic`.

## 12. Техническая проверка

| Проверка | Результат |
|---|---|
| `git diff --check` | OK, ошибок whitespace нет |
| TODO/FIXME/HACK в проверяемых gameplay/app/ui скриптах | Не найдено |
| Большой `switch` / `enum state` в логике врагов/босса | Не найдено |
| Boss-specific обход `enemyLayers` в `PlayerCombatLogic` | Не найден |
| Unity batchmode | OK, проект загрузился, script compilation прошёл, exit code 0 |
| `dotnet --info` | Недоступен: `zsh: command not found: dotnet` |
| PDF text extraction | Текст лекций не извлечён shell-инструментами; аудит сверен по архитектурным критериям лекций, перечисленным в задании |

