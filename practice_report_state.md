# Отчёт по практической работе: доработка логики врагов, мирный режим и паттерн State

## 1. Цель работы

Целью практической работы была доработка игровой логики врагов в Unity-проекте с использованием архитектурного подхода на основе паттерна **State** и отдельной подсистемы **State Machine**.

В рамках работы требовалось:

- доработать поведение обычных врагов через машину состояний;
- реализовать минимум 4 состояния для обычных мобов:
  - `Idle` / покой;
  - `Aggro` / агрессия;
  - `Attack` / атака;
  - `Flee` / бегство;
- добавить босса-гиганта;
- реализовать минимум 4 состояния для босса:
  - `Idle` / покой;
  - `Aggro` / агрессия;
  - `Attack` / атака;
  - `HeavyAttack` / сильная атака;
- расширить босса до 7+ состояний для дополнительного балла;
- реализовать вторую фазу босса: при HP ниже 50% атаки ускоряются;
- реализовать мирный режим:
  - обычные мобы не агрятся по радиусу и после удара игроком;
  - при низком HP обычные мобы убегают;
  - босс в мирном режиме не агрится по радиусу, но начинает бой после удара игроком;
- явно применить паттерн **State** и подсистему **State Machine**;
- сохранить разделение ответственности между UI, сервисами, MonoBehaviour-компонентами и игровой логикой.

Работа выполнена не через `enum + switch`, а через отдельные классы состояний, контексты и машины состояний.

---

## 2. Теоретическая основа

### 2.1. Паттерн State

Паттерн **State** позволяет объекту менять своё поведение в зависимости от текущего внутреннего состояния. При этом поведение каждого состояния выносится в отдельный класс.

Например, враг в состоянии `Idle` ожидает игрока, в состоянии `Aggro` преследует его, в состоянии `Attack` атакует, а в состоянии `Flee` убегает. Сам враг при этом не содержит один огромный метод `Update()` со всеми вариантами поведения.

### 2.2. State Machine

**State Machine** — это отдельная подсистема, которая хранит текущее состояние объекта и управляет переходами между состояниями.

В проекте машины состояний выполняют следующие задачи:

- хранят `CurrentState`;
- вызывают `Enter()` при входе в состояние;
- вызывают `Exit()` при выходе из состояния;
- вызывают `Tick()` каждый кадр;
- вызывают `FixedTick()` для физической логики;
- выполняют переход через `ChangeState(...)`.

### 2.3. Почему не enum + switch

Подход `enum + switch` подходит только для очень простой логики. Для врагов и босса он быстро превращается в большой класс, где смешиваются:

- движение;
- атаки;
- анимации;
- проверки радиусов;
- мирный режим;
- бегство;
- лечение;
- ярость босса;
- проверка HP;
- работа с NavMeshAgent.

Такой класс становится трудно поддерживать и расширять. Добавление нового состояния требует изменения большого `switch`, что нарушает принцип **Open/Closed Principle**.

В данной работе каждое состояние вынесено в отдельный класс. Это позволяет добавлять новые состояния, не переписывая уже существующие.

### 2.4. Связь с архитектурой из лекций

**Лекция 1: модульность и читаемость**

Логика разделена по модулям:

- обычные враги находятся в `Assets/Scripts/Gameplay/Enemies`;
- босс находится в `Assets/Scripts/Gameplay/Boss`;
- UI главного меню находится в `Assets/Scripts/UI`;
- сервисы приложения находятся в `Assets/Scripts/App/Services`;
- точки входа сцен находятся в `Assets/Scripts/App/EntryPoints`.

**Лекция 2: SOLID**

В проекте соблюдаются основные идеи SOLID:

- **SRP**: состояние отвечает только за поведение в рамках одного состояния;
- **OCP**: новое состояние добавляется новым классом;
- **DIP**: режим игры доступен через интерфейс `IGameModeService`, а не через хаотичные static-переменные.

**Лекция 3: Bootstrapper, сервисы, MonoBehaviour как адаптер**

`ProjectEntryPoint` создаёт сервисы приложения и передаёт их в entry point текущей сцены. MonoBehaviour-компоненты в основном кэшируют Unity-зависимости, вызывают `Tick()` у машин состояний и прокидывают контекст.

**Лекция 4: UI отделён от игровой логики**

Главное меню не меняет врагов напрямую. Оно только выбирает режим игры и загружает сцену. Режим затем передаётся врагам через `GameSceneEntryPoint`.

**Лекция 5: State и State Machine**

Поведение обычных врагов и босса реализовано через явные state-классы и отдельные машины состояний.

---

## 3. Общая архитектура решения

Общая архитектура строится вокруг трёх основных частей:

1. **Обычные враги**
   - `EnemyStateMachine`;
   - `EnemyContext`;
   - отдельные классы состояний.

2. **Босс**
   - `BossStateMachine`;
   - `BossContext`;
   - отдельные классы состояний босса.

3. **Режим игры**
   - `GameMode`;
   - `IGameModeService`;
   - `GameModeService`;
   - передача режима из главного меню в игровую сцену.

Схема передачи режима:

```text
MainMenuView
    ↓ событие кнопки
MainMenuController
    ↓ устанавливает Normal / Peaceful
GameModeService
    ↓ хранит режим между сценами
GameSceneEntryPoint
    ↓ прокидывает режим в игровые объекты
EnemyContext / BossContext
    ↓ используются состояниями
EnemyStateMachine / BossStateMachine
```

Урон реализован через общую систему:

```text
DamageInfo
    ↓
IDamageable
    ↓
EnemyHealth / PlayerHealth
```

Таким образом, для босса не создавалась отдельная несовместимая система HP или урона. Босс использует существующий `EnemyHealth`, который реализует `IDamageable`.

---

## 4. Реализация обычных врагов

### 4.1. Основные файлы

Обычные враги реализованы в следующих файлах:

- `Assets/Scripts/Gameplay/Enemies/EnemyStatefulAIBase.cs`;
- `Assets/Scripts/Gameplay/Enemies/EnemyAI.cs`;
- `Assets/Scripts/Gameplay/Enemies/EnemyRangedAI.cs`;
- `Assets/Scripts/Gameplay/Enemies/States/EnemyStateMachine.cs`;
- `Assets/Scripts/Gameplay/Enemies/States/EnemyContext.cs`;
- `Assets/Scripts/Gameplay/Enemies/States/IEnemyState.cs`;
- `Assets/Scripts/Gameplay/Enemies/States/EnemyIdleState.cs`;
- `Assets/Scripts/Gameplay/Enemies/States/EnemyAggressionState.cs`;
- `Assets/Scripts/Gameplay/Enemies/States/EnemyAttackStateBase.cs`;
- `Assets/Scripts/Gameplay/Enemies/States/EnemyMeleeAttackState.cs`;
- `Assets/Scripts/Gameplay/Enemies/States/EnemyRangedAttackState.cs`;
- `Assets/Scripts/Gameplay/Enemies/States/EnemyFleeState.cs`;
- `Assets/Scripts/Gameplay/Enemies/EnemyHealth.cs`.

`EnemyAI` используется для ближнего врага, а `EnemyRangedAI` — для дальнего врага. Оба класса наследуются от `EnemyStatefulAIBase`, где создаётся общая машина состояний.

### 4.2. Интерфейс состояния обычного врага

Путь: `Assets/Scripts/Gameplay/Enemies/States/IEnemyState.cs`

```csharp
public interface IEnemyState
{
    void Enter();
    void Exit();
    void Tick();
    void FixedTick();
}
```

Интерфейс задаёт единый контракт для всех состояний обычного моба. Благодаря этому `EnemyStateMachine` может работать с любым состоянием через абстракцию `IEnemyState`.

### 4.3. Машина состояний обычного врага

Путь: `Assets/Scripts/Gameplay/Enemies/States/EnemyStateMachine.cs`

```csharp
public sealed class EnemyStateMachine
{
    public EnemyContext Context { get; }
    public IEnemyState CurrentState { get; private set; }

    public void ChangeState(IEnemyState nextState, string reason = "No reason provided")
    {
        if (nextState == null || ReferenceEquals(CurrentState, nextState))
            return;

        CurrentState?.Exit();
        CurrentState = nextState;
        CurrentState.Enter();
    }

    public void Tick()
    {
        CurrentState?.Tick();
    }

    public void FixedTick()
    {
        CurrentState?.FixedTick();
    }
}
```

Машина состояний хранит текущее состояние и выполняет все переходы. За счёт этого переходы централизованы и не размазаны по MonoBehaviour.

### 4.4. Создание состояний врага

Путь: `Assets/Scripts/Gameplay/Enemies/EnemyStatefulAIBase.cs`

```csharp
StateMachine = new EnemyStateMachine(Context, gameObject.name);

Context.IdleState = new EnemyIdleState(Context, StateMachine);
Context.AggressionState = new EnemyAggressionState(Context, StateMachine);
Context.FleeState = new EnemyFleeState(Context, StateMachine);
Context.AttackState = CreateAttackState(Context, StateMachine);

StateMachine.ChangeState(Context.IdleState, "Initial state");
```

В этом фрагменте видно, что `EnemyStatefulAIBase` выступает как Unity-адаптер и composition root для конкретного врага: он создаёт контекст, машину состояний и отдельные state-классы.

### 4.5. Состояние покоя обычного врага

Путь: `Assets/Scripts/Gameplay/Enemies/States/EnemyIdleState.cs`

```csharp
public override void Tick()
{
    if (!Context.HasPlayer)
        return;

    if (Context.ShouldEnterFlee)
    {
        StateMachine.ChangeState(Context.FleeState, Context.GetFleeReasonLabel());
        return;
    }

    if (Context.IsPeacefulMode)
        return;

    if (Context.IsPlayerInAttackRange)
    {
        StateMachine.ChangeState(Context.AttackState, "Player entered attack range from idle");
        return;
    }

    if (Context.HasDetectedPlayer)
        StateMachine.ChangeState(Context.AggressionState, "Player detected from idle");
}
```

В этом состоянии видно главное отличие мирного режима: если включён `IsPeacefulMode`, враг не переходит в агрессию по радиусу. При этом проверка бегства остаётся активной.

### 4.6. Состояние агрессии

Путь: `Assets/Scripts/Gameplay/Enemies/States/EnemyAggressionState.cs`

```csharp
public override void Tick()
{
    if (!Context.HasPlayer)
    {
        StateMachine.ChangeState(Context.IdleState, "Lost player reference during aggression");
        return;
    }

    if (Context.IsPeacefulMode)
    {
        if (Context.ShouldEnterFlee)
        {
            StateMachine.ChangeState(Context.FleeState, Context.GetFleeReasonLabel());
            return;
        }

        StateMachine.ChangeState(Context.IdleState, "Peaceful mode suppresses aggression");
        return;
    }

    if (Context.IsPlayerInAttackRange)
        StateMachine.ChangeState(Context.AttackState, "Player reached attack range during aggression");
}
```

Если враг оказался в агрессии, но включён мирный режим, состояние переводит его обратно в `Idle` или в `Flee`, если HP низкое.

### 4.7. Состояние бегства

Путь: `Assets/Scripts/Gameplay/Enemies/States/EnemyFleeState.cs`

```csharp
public override void FixedTick()
{
    Vector3 direction = Context.GetDirectionAwayFromPlayer();
    Context.Move(direction, Time.fixedDeltaTime);
    Context.FaceDirection(direction, Time.fixedDeltaTime);
}

private IEnemyState GetNextStateAfterFlee()
{
    if (Context.IsPeacefulMode)
        return Context.IdleState;

    if (!Context.HasDetectedPlayer)
        return Context.IdleState;

    if (Context.IsPlayerInAttackRange)
        return Context.AttackState;

    return Context.AggressionState;
}
```

`FleeState` не просто ставит флаг, а реально двигает моба в сторону от игрока через `Context.GetDirectionAwayFromPlayer()`.

### 4.8. HP и реакция на получение урона

Путь: `Assets/Scripts/Gameplay/Enemies/EnemyHealth.cs`

```csharp
public void TakeDamage(DamageInfo damage)
{
    if (!_state.ApplyDamage(damage.Amount))
        return;

    OnDamaged?.Invoke(damage);

    if (_fleeContext != null)
    {
        if (_fleeContext.IsPeacefulMode)
        {
            if (_fleeContext.ShouldFleeInPeacefulMode)
                _fleeContext.RequestFlee(EnemyFleeReason.LowHealth);
        }
        else if (damage.Type == DamageType.Physical && _fleeContext.FleeOnMeleeHit)
        {
            _fleeContext.RequestFlee(EnemyFleeReason.MeleeHit, wasMeleeHit: true);
        }
    }
}
```

В мирном режиме обычный моб не переходит в агрессию после удара. Он только проверяет HP и при необходимости запрашивает бегство.

---

## 5. Реализация босса

### 5.1. Основные файлы босса

Логика босса находится в папке:

`Assets/Scripts/Gameplay/Boss`

Основные файлы:

- `BossController.cs`;
- `BossContext.cs`;
- `BossStateMachine.cs`;
- `IBossState.cs`;
- `AbstractBossState.cs`;
- `BossDamageHitbox.cs`.

Состояния находятся в папке:

`Assets/Scripts/Gameplay/Boss/States`

Финальный список состояний босса:

- `BossIdleState`;
- `BossAggroState`;
- `BossChaseState`;
- `BossAttackState`;
- `BossHeavyAttackState`;
- `BossEnrageState`;
- `BossHealState`;
- `BossDeathState`.

Также присутствуют дополнительные alias/legacy-классы:

- `BossRageState`;
- `BossStrongAttackState`;
- `BossAggressionState`;
- `BossPatrolState`.

Таким образом, у босса реализовано больше 7 state-классов.

### 5.2. Интерфейс состояния босса

Путь: `Assets/Scripts/Gameplay/Boss/IBossState.cs`

```csharp
public interface IBossState
{
    void Enter();
    void Exit();
    void Tick();
    void FixedTick();
}
```

Интерфейс аналогичен интерфейсу обычных врагов, но отделён в подсистему босса, так как логика босса сложнее.

### 5.3. Машина состояний босса

Путь: `Assets/Scripts/Gameplay/Boss/BossStateMachine.cs`

```csharp
public sealed class BossStateMachine
{
    public BossContext Context { get; }
    public IBossState CurrentState { get; private set; }

    public void ChangeState(IBossState nextState, string reason = "No reason provided")
    {
        if (nextState == null || ReferenceEquals(CurrentState, nextState))
            return;

        CurrentState?.Exit();
        CurrentState = nextState;
        CurrentState.Enter();
    }

    public void Tick()
    {
        CurrentState?.Tick();
    }
}
```

`BossStateMachine` хранит текущее состояние босса и вызывает методы жизненного цикла состояния.

### 5.4. BossController как Unity-адаптер

Путь: `Assets/Scripts/Gameplay/Boss/BossController.cs`

```csharp
private void Awake()
{
    CacheComponents();
    DisableLegacyEnemyLogic();

    Context = new BossContext(this, transform, _agent, _animator, _health);
    Context.SetPeacefulMode(_isPeacefulMode);
    StateMachine = new BossStateMachine(Context, gameObject.name);

    IdleState = new BossIdleState(Context, StateMachine);
    AggroState = new BossAggroState(Context, StateMachine);
    ChaseState = new BossChaseState(Context, StateMachine);
    AttackState = new BossAttackState(Context, StateMachine);
    HeavyAttackState = new BossHeavyAttackState(Context, StateMachine);
    EnrageState = new BossEnrageState(Context, StateMachine);
    HealState = new BossHealState(Context, StateMachine);
    DeathState = new BossDeathState(Context, StateMachine);

    SubscribeToHealth();
    DisableDamageHitboxes();
    StateMachine.ChangeState(IdleState, "Initial boss state");
}
```

`BossController` не реализует всё поведение босса в `Update()`. Он кэширует Unity-компоненты, создаёт контекст и состояния, подписывается на события HP и вызывает машину состояний.

Путь: `Assets/Scripts/Gameplay/Boss/BossController.cs`

```csharp
private void Update()
{
    Context.Tick(Time.deltaTime);
    StateMachine.Tick();
}

private void FixedUpdate()
{
    StateMachine.FixedTick();
}
```

Это соответствует требованию: MonoBehaviour используется как адаптер к Unity, а бизнес-логика вынесена в состояния.

### 5.5. BossContext

Путь: `Assets/Scripts/Gameplay/Boss/BossContext.cs`

```csharp
public sealed class BossContext
{
    public BossController Controller { get; }
    public Transform Transform { get; }
    public NavMeshAgent Agent { get; }
    public Animator Animator { get; }
    public EnemyHealth Health { get; }
    public Transform Target { get; private set; }
    public PlayerHealth TargetHealth { get; private set; }

    public bool IsEnraged { get; private set; }
    public bool HasEnteredEnrage { get; private set; }
    public bool IsPeacefulMode { get; private set; }
    public bool WasProvokedByPlayer { get; private set; }
    public bool HasHealed { get; private set; }
    public float AttackSpeedMultiplier { get; private set; } = 1f;
}
```

`BossContext` предоставляет состояниям доступ к зависимостям и данным: цели, HP, NavMeshAgent, Animator, флагам мирного режима, ярости и лечения.

### 5.6. Idle босса и мирный режим

Путь: `Assets/Scripts/Gameplay/Boss/States/BossIdleState.cs`

```csharp
public override void Tick()
{
    if (TryEnterDeath())
        return;

    if (TryEnterHeal())
        return;

    if (Boss.IsPeacefulMode && !Context.WasProvokedByPlayer)
        return;

    if (Context.HasDetectedTarget)
        StateMachine.ChangeState(Boss.AggroState, "Player entered boss detection radius");
}
```

В обычном режиме босс агрится по радиусу. В мирном режиме он остаётся в `Idle`, пока игрок его не ударит.

### 5.7. Aggro и Chase

Путь: `Assets/Scripts/Gameplay/Boss/States/BossAggroState.cs`

```csharp
if (Context.IsTargetInAttackRange)
{
    Boss.StopMovement();
    FaceTarget(Time.deltaTime);

    if (Context.CanUseHeavyAttack)
    {
        StateMachine.ChangeState(Boss.HeavyAttackState, "Heavy attack cooldown is ready");
        return;
    }

    if (Context.CanUseAttack)
        StateMachine.ChangeState(Boss.AttackState, "Player is in attack range");

    return;
}

StateMachine.ChangeState(Boss.ChaseState, "Player is outside attack range");
```

`BossAggroState` принимает решение: атаковать, использовать сильную атаку или перейти в погоню.

Путь: `Assets/Scripts/Gameplay/Boss/States/BossChaseState.cs`

```csharp
float speed = Context.IsTargetHealthLow
    ? Boss.FinisherChaseSpeed
    : Boss.ChaseSpeed;

Boss.MoveToTarget(speed);
```

`BossChaseState` отвечает именно за преследование. Если HP игрока ниже порога, босс ускоряется.

### 5.8. Обычная и сильная атака босса

Путь: `Assets/Scripts/Gameplay/Boss/States/BossAttackState.cs`

```csharp
public override void Tick()
{
    if (TryEnterDeath())
        return;

    if (TryEnterEnrage())
        return;

    _elapsed += Time.deltaTime;
    TickDamageWindow(Boss.AttackDuration, Boss.AttackDamageWindowStart, Boss.AttackDamageWindowEnd);

    if (Boss.ConsumeAttackAnimationFinished() || _elapsed >= Boss.AttackDuration)
        StateMachine.ChangeState(Boss.SelectMovementOrIdleState(), "Boss finished normal attack");
}
```

Атака использует таймер как fallback, если Animation Events ещё не подключены.

Путь: `Assets/Scripts/Gameplay/Boss/States/BossHeavyAttackState.cs`

```csharp
public override void Enter()
{
    Boss.StopMovement();
    Boss.FaceTargetImmediately();

    if (!Boss.TryStartHeavyAttack())
    {
        StateMachine.ChangeState(Boss.AggroState, "Heavy attack was on cooldown");
        return;
    }

    _elapsed = 0f;
    _hitboxesEnabledByFallback = false;
    _damageWindowOpened = false;
}
```

Сильная атака вынесена в отдельное состояние и использует отдельный trigger, урон, cooldown и длительность.

### 5.9. Enrage: ускорение атак при HP ниже 50%

Путь: `Assets/Scripts/Gameplay/Boss/BossContext.cs`

```csharp
public bool ShouldEnterEnrage =>
    !HasEnteredEnrage &&
    (EnrageRequested || IsBossHealthAtOrBelow(Controller.EnrageHealthThreshold));

public void EnterEnrage(float attackSpeedMultiplier)
{
    if (HasEnteredEnrage)
        return;

    HasEnteredEnrage = true;
    EnrageRequested = false;
    IsEnraged = true;
    AttackSpeedMultiplier = Mathf.Max(0.1f, attackSpeedMultiplier);
}
```

Флаг `HasEnteredEnrage` гарантирует, что фаза ярости включится только один раз.

Путь: `Assets/Scripts/Gameplay/Boss/BossContext.cs`

```csharp
public void TriggerAttackCooldown()
{
    AttackCooldownRemaining = Controller.AttackCooldown / AttackSpeedMultiplier;
}

public void TriggerHeavyAttackCooldown()
{
    HeavyAttackCooldownRemaining = Controller.HeavyAttackCooldown / AttackSpeedMultiplier;
}
```

После входа в ярость cooldown атак делится на `AttackSpeedMultiplier`, поэтому атаки становятся чаще.

Путь: `Assets/Scripts/Gameplay/Boss/States/BossEnrageState.cs`

```csharp
public override void Enter()
{
    Boss.DisableDamageHitboxes();
    Boss.StopMovement();
    Boss.EnterPhaseTwo();
    Boss.BeginEnrageAnimation();
    _timer = Boss.EnrageDuration;
}

public override void Tick()
{
    if (TryEnterDeath())
        return;

    _timer -= Time.deltaTime;
    if (_timer > 0f)
        return;

    StateMachine.ChangeState(Boss.SelectMovementOrIdleState(), "Boss enrage transition finished");
}
```

`BossEnrageState` является отдельным состоянием, поэтому ярость считается полноценной частью State Machine.

### 5.10. HealState

Путь: `Assets/Scripts/Gameplay/Boss/States/BossHealState.cs`

```csharp
private void FinishHeal()
{
    if (Boss.HealToFull)
        Boss.RestoreHealthToFull();
    else if (Boss.HealDuration <= 0f)
        Boss.Heal(Boss.HealAmount);

    Context.MarkHealed();
    Context.ClearEnrageRequest();
    StateMachine.ChangeState(Boss.IdleState, "Boss finished healing");
}
```

Лечение не бесконечное, потому что после лечения выставляется `HasHealed`.

---

## 6. Реализация мирного режима

### 6.1. GameMode и GameModeService

Путь: `Assets/Scripts/App/Services/GameMode.cs`

```csharp
public enum GameMode
{
    Normal,
    Peaceful
}
```

Путь: `Assets/Scripts/App/Services/IGameModeService.cs`

```csharp
public interface IGameModeService
{
    GameMode CurrentMode { get; }
    bool IsPeacefulMode { get; }
    void SetMode(GameMode mode);
}
```

Путь: `Assets/Scripts/App/Services/GameModeService.cs`

```csharp
public sealed class GameModeService : IGameModeService
{
    public GameMode CurrentMode { get; private set; } = GameMode.Normal;
    public bool IsPeacefulMode => CurrentMode == GameMode.Peaceful;

    public void SetMode(GameMode mode)
    {
        CurrentMode = mode;
    }
}
```

Режим игры хранится централизованно в сервисе, а не в случайных static-переменных.

### 6.2. Создание сервисов в ProjectEntryPoint

Путь: `Assets/Scripts/App/EntryPoints/ProjectEntryPoint.cs`

```csharp
_appServices = new AppServices(
    new AudioService(),
    new SaveService(),
    new SceneLoader(),
    new PendingLoadDataService(),
    new GameModeService());

_appServices.AudioService.LoadVolume();

SceneManager.sceneLoaded += OnSceneLoaded;

_appServices.SceneLoader.Load(_startupSceneName);
```

`ProjectEntryPoint` создаёт общий набор сервисов и передаёт их в entry point загруженной сцены.

### 6.3. Главное меню

Путь: `Assets/Scripts/UI/MainMenu/MainMenuController.cs`

```csharp
private void OnPlayClicked()
{
    _gameModeService.SetMode(GameMode.Normal);
    _sceneLoader.Load(_gameSceneName);
}

private void OnPlayPeaceModeClicked()
{
    _gameModeService.SetMode(GameMode.Peaceful);
    _sceneLoader.Load(_gameSceneName);
}
```

UI не управляет врагами напрямую. Он только выбирает режим и загружает игровую сцену.

Путь: `Assets/Scripts/UI/Views/MainMenuView.cs`

```csharp
public event Action PlayClicked;
public event Action PlayPeaceModeClicked;

private void RaisePlayClicked() => PlayClicked?.Invoke();
private void RaisePlayPeaceModeClicked() => PlayPeaceModeClicked?.Invoke();
```

`MainMenuView` отвечает за кнопки и события, а `MainMenuController` — за обработку этих событий.

### 6.4. Передача режима в игровую сцену

Путь: `Assets/Scripts/App/EntryPoints/GameSceneEntryPoint.cs`

```csharp
private void InjectBoss()
{
    GameObject bossObject = GameObject.Find(_bossObjectName);
    BossController bossController = null;

    if (bossObject != null)
        bossController = bossObject.GetComponent<BossController>();
    else
        bossController = Object.FindFirstObjectByType<BossController>();

    bossController.SetPeacefulMode(_gameModeService.IsPeacefulMode);
    bossController.Construct(_playerController.transform, roomReference != null ? roomReference.RoomBounds : null);
}
```

Босс получает режим через `SetPeacefulMode`, после чего режим сохраняется в `BossContext`.

Путь: `Assets/Scripts/App/EntryPoints/GameSceneEntryPoint.cs`

```csharp
EnemyAI[] meleeEnemies = Object.FindObjectsOfType<EnemyAI>();
foreach (EnemyAI enemyAI in meleeEnemies)
{
    enemyAI.SetPeacefulMode(_gameModeService.IsPeacefulMode);
    enemyAI.Construct(_playerController.transform, roomReference.RoomBounds);
}
```

Обычные мобы также получают режим из `GameModeService`, но через свой `EnemyContext`.

---

## 7. Система урона, HP и hitbox-атаки босса

### 7.1. Общая модель урона

Путь: `Assets/Scripts/Gameplay/Common/DamageInfo.cs`

```csharp
public enum DamageType { Physical, Magical }

public struct DamageInfo
{
    public float Amount;
    public DamageType Type;
    public GameObject Source;

    public DamageInfo(float amount, DamageType type, GameObject source = null)
    {
        Amount = amount;
        Type = type;
        Source = source;
    }
}
```

`DamageInfo` хранит количество урона, тип урона и источник. Поле `Source` используется, чтобы понять, кто нанёс урон.

Путь: `Assets/Scripts/Gameplay/Common/IDamageable.cs`

```csharp
public interface IDamageable
{
    void TakeDamage(DamageInfo damage);
}
```

Через этот интерфейс урон могут получать игрок, обычные мобы и босс.

### 7.2. Определение удара игрока по боссу

Путь: `Assets/Scripts/Gameplay/Boss/BossController.cs`

```csharp
private void HandleDamaged(DamageInfo damage)
{
    if (IsDamageFromPlayer(damage))
        NotifyHitByPlayer();
}

private bool IsDamageFromPlayer(DamageInfo damage)
{
    if (damage.Source == null)
        return false;

    if (HasTag(damage.Source, "player") || HasTag(damage.Source, "Player"))
        return true;

    return damage.Source.GetComponentInParent<PlayerController>() != null;
}
```

Когда босс получает урон от игрока, вызывается `NotifyHitByPlayer()`.

Путь: `Assets/Scripts/Gameplay/Boss/BossController.cs`

```csharp
public void NotifyHitByPlayer()
{
    if (Context == null || Context.IsDead)
        return;

    Context.MarkProvokedByPlayer();
    StateMachine.ChangeState(AggroState, "Boss was hit by player");
}
```

Это реализует требование: в мирном режиме босс начинает бой только после удара игроком.

### 7.3. Hitbox-атаки босса

Путь: `Assets/Scripts/Gameplay/Boss/BossDamageHitbox.cs`

```csharp
public void SetActive(bool isActive)
{
    _isActive = isActive;

    if (isActive)
        _damagedTargets.Clear();
}

private void TryDamage(Collider other)
{
    if (!_isActive || other == null)
        return;

    IDamageable victim = FindDamageable(other);
    if (victim == null || _damagedTargets.Contains(victim))
        return;

    _damagedTargets.Add(victim);
    victim.TakeDamage(new DamageInfo(_damageAmount, _damageType, transform.root.gameObject));
}
```

Hitbox наносит урон только когда активен. `HashSet<IDamageable>` защищает от повторного урона одной цели за один swing.

### 7.4. Animation Events и fallback

Путь: `Assets/Scripts/Gameplay/Boss/BossController.cs`

```csharp
public void EnableDamageHitboxes()
{
    _damageWindowOpenedThisAttack = true;
    foreach (BossDamageHitbox hitbox in _damageHitboxes)
    {
        if (hitbox == null)
            continue;

        hitbox.Configure(_activeAttackDamage, _damageType);
        hitbox.SetActive(true);
    }
}

public void DisableDamageHitboxes()
{
    foreach (BossDamageHitbox hitbox in _damageHitboxes)
    {
        if (hitbox != null)
            hitbox.SetActive(false);
    }
}
```

Эти методы можно подключить в Unity Animation Events. Если события не подключены, состояния атаки используют fallback по таймеру.

---

## 8. Animator и Unity-интеграция

Код босса подготовлен к работе с Animator, но Animator Controller должен быть настроен вручную.

Путь: `Assets/Scripts/Gameplay/Boss/BossController.cs`

```csharp
[Header("Animator Parameters")]
[SerializeField] private string _isMovingParameter = "IsMoving";
[SerializeField] private string _attackTriggerParameter = "Attack";
[SerializeField] private string _heavyAttackTriggerParameter = "HeavyAttack";
[SerializeField] private string _enrageTriggerParameter = "Enrage";
[SerializeField] private string _isEnragedParameter = "IsEnraged";
[SerializeField] private string _attackSpeedMultiplierParameter = "AttackSpeedMultiplier";
[SerializeField] private string _healTriggerParameter = "Heal";
```

Имена параметров вынесены в сериализуемые поля, поэтому их можно изменить в Inspector.

Путь: `Assets/Scripts/Gameplay/Boss/BossController.cs`

```csharp
private bool HasAnimatorParameter(string parameterName, AnimatorControllerParameterType parameterType)
{
    if (_animator == null || string.IsNullOrWhiteSpace(parameterName))
        return false;

    foreach (AnimatorControllerParameter parameter in _animator.parameters)
    {
        if (parameter.name == parameterName && parameter.type == parameterType)
            return true;
    }

    Debug.LogWarning($"[{name}] Animator parameter '{parameterName}' ({parameterType}) was not found.");
    return false;
}
```

Если Animator отсутствует или параметр не создан, код не падает с `NullReferenceException`, а выводит предупреждение.

---

## 9. Соответствие требованиям задания

| Требование | Реализация | Статус |
|---|---|---|
| Обычные мобы имеют State Machine | `EnemyStateMachine`, `EnemyContext`, отдельные состояния | Выполнено |
| Минимум 4 состояния обычных мобов | `Idle`, `Aggression`, `Attack`, `Flee` | Выполнено |
| Босс добавлен | `BossController`, prefab `Boss 1`, `BossContext`, `BossStateMachine` | Выполнено |
| Минимум 4 состояния босса | `Idle`, `Aggro`, `Attack`, `HeavyAttack` | Выполнено |
| 7+ состояний босса | Добавлены `Chase`, `Enrage`, `Heal`, `Death` | Выполнено |
| Ускорение атак при HP <= 50% | `BossEnrageState`, `AttackSpeedMultiplier`, уменьшение cooldown | Выполнено |
| Мирный режим обычных мобов | `EnemyContext.IsPeacefulMode`, suppression в состояниях | Выполнено |
| Мирный режим босса | `WasProvokedByPlayer`, `NotifyHitByPlayer()` | Выполнено |
| UI не управляет врагами напрямую | `MainMenuView` -> `MainMenuController` -> `GameModeService` | Выполнено |
| Урон через общую систему | `DamageInfo`, `IDamageable`, `EnemyHealth`, `PlayerHealth` | Выполнено |
| Hitbox-окна атак босса | `BossDamageHitbox`, `EnableDamageHitboxes`, `DisableDamageHitboxes` | Выполнено |
| Animator Controller | Код подготовлен, параметры нужно создать вручную | Требует настройки в Unity |

---

## 10. Важные замечания перед защитой

### 10.1. Layer босса

В prefab `Assets/OurPrefabs/Boss 1.prefab` объект `Boss 1` находится на layer `Default`.

При этом у игрока в `PlayerCombat` используется `enemyLayers`, который в prefab игрока указывает на layer `Enemy`.

Перед демонстрацией нужно убедиться, что:

- root-объект босса или collider, который должен получать урон, стоит на layer `Enemy`;
- `PlayerCombat.enemyLayers` включает layer `Enemy`;
- у босса есть `EnemyHealth`;
- у босса есть collider для получения урона.

Иначе melee-атака игрока может не попадать по боссу из-за layer mask.

### 10.2. Animator Controller

Код подготовлен, но параметры Animator нужно создать вручную:

- `IsMoving` — Bool;
- `Attack` — Trigger;
- `HeavyAttack` — Trigger;
- `Enrage` — Trigger;
- `IsEnraged` — Bool;
- `AttackSpeedMultiplier` — Float;
- `Heal` — Trigger.

### 10.3. Animation Events

Для атак босса желательно добавить события в анимации:

- `EnableDamageHitboxes()` — в начале окна урона;
- `DisableDamageHitboxes()` — в конце окна урона;
- `OnAttackAnimationFinished()` — в конце анимации атаки.

Даже если эти события не подключены, fallback по таймеру уже реализован.

---

## 11. Что нужно показать преподавателю в Unity

### 11.1. В Project окне

Показать папки:

- `Assets/Scripts/Gameplay/Enemies/States`;
- `Assets/Scripts/Gameplay/Boss`;
- `Assets/Scripts/Gameplay/Boss/States`;
- `Assets/Scripts/App/Services`;
- `Assets/Scripts/App/EntryPoints`;
- `Assets/Scripts/UI/MainMenu`.

Объяснить, что состояния находятся в отдельных классах, а не реализованы через `enum + switch`.

### 11.2. На обычном мобе

В Inspector показать:

- `EnemyAI` или `EnemyRangedAI`;
- `EnemyHealth`;
- параметры detection/attack/flee;
- что при запуске враг использует `EnemyStateMachine`.

В Play Mode показать:

- в обычном режиме моб агрится по радиусу;
- моб атакует игрока;
- в мирном режиме моб не агрится;
- при HP ниже 50% в мирном режиме моб убегает.

### 11.3. На боссе

В Inspector показать:

- `BossController`;
- `EnemyHealth`;
- `NavMeshAgent`;
- `Animator`;
- `BossDamageHitbox` на hitbox-объектах;
- массив `damageHitboxes`;
- настройки:
  - `detectionRadius`;
  - `loseTargetRadius`;
  - `attackRange`;
  - `attackCooldown`;
  - `heavyAttackCooldown`;
  - `enragedAttackSpeedMultiplier`;
  - `healThreshold`;
  - `healDuration`;
  - `chaseSpeed`;
  - `finisherChaseSpeed`.

Показать, что у босса есть минимум 7 state-классов:

```text
BossIdleState
BossAggroState
BossChaseState
BossAttackState
BossHeavyAttackState
BossEnrageState
BossHealState
```

В Play Mode показать:

- в обычном режиме босс агрится по радиусу;
- босс переходит в Chase;
- босс выполняет обычную и сильную атаку;
- при HP <= 50% входит в Enrage;
- после Enrage атаки становятся быстрее;
- при условиях лечения входит в HealState один раз;
- в мирном режиме не агрится по радиусу;
- после удара игроком переходит в Aggro/Chase.

### 11.4. В главном меню

Показать:

- обычную кнопку `Play`;
- кнопку `Play peace mode`;
- объяснить, что кнопки не меняют врагов напрямую.

Схема:

```text
Play
    -> GameMode.Normal
    -> загрузка GameScene

Play peace mode
    -> GameMode.Peaceful
    -> загрузка GameScene
```

### 11.5. Технические проверки

Перед защитой нужно проверить:

- в Console нет `NullReferenceException`;
- `git diff --check` проходит без ошибок;
- boss layer установлен корректно;
- у игрока `enemyLayers` включает layer босса;
- у босса Animator содержит нужные параметры;
- hitbox-collider'ы рук босса имеют `Is Trigger = true`;
- в сцене есть NavMesh, и босс стоит на NavMesh.

---

## 12. Вывод

В результате практической работы логика обычных врагов и босса была переведена на архитектуру с явным использованием паттерна **State** и подсистемы **State Machine**.

Обычные мобы получили состояния покоя, агрессии, атаки и бегства. Босс получил расширенную систему из 7+ состояний, включая погоню, сильную атаку, ярость и лечение. Мирный режим реализован централизованно через `GameModeService`, а не через разрозненные static-флаги.

Решение соответствует требованиям по архитектуре: UI отделён от игровой логики, MonoBehaviour-компоненты используются как Unity-адаптеры, состояния вынесены в отдельные классы, а переходы выполняются через State Machine.
