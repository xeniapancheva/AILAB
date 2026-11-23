# Лабораторная работа №1: Проектирование поведения NPC с использованием конечного автомата состояний (FSM)

## 1. Описание проекта
Небольшой 2D-прототип в **Unity**, демонстрирующий построение ИИ вражеских NPC на базе паттерна **Finite State Machine (FSM)**. В сцене реализованы управление персонажем игрока и два типа противников с разными механиками обнаружения и перемещения.

### Используемые скрипты
* `IState2D.cs` — интерфейс состояний (`Enter()`, `Update()`, `FixedUpdate()`, `Exit()`).
* `StateMachine2D.cs` — класс управления конечным автоматом, переключения состояний и логирования переходов.
* `PlayerController2D.cs` — перемещение игрока (`WASD` / стрелки) и ближняя атака (`Space`) по слою врагов.
* `StandardEnemy2D.cs` — логика обычного противника: плавное 8-направленное движение, поле зрения 180°, проверка линии видимости через `Raycast2D`, получение урона и смерть.
* `StandardEnemyStates2D.cs` — состояния обычного противника: `PatrolState2D`, `IdleState2D`, `ChaseState2D`, `SearchState2D`, `AttackState2D`.
* `TeleportEnemy2D.cs` — логика и состояния телепортирующегося противника: периодическое сканирование на 360°, способность телепортации с проверкой препятствий и перезарядкой, состояния `TeleportPatrolState2D`, `TeleportChaseState2D`, `TeleportState2D`, `TeleportAttackState2D`.

---

## 2. Архитектура FSM и состояния NPC

### Общая архитектура
* `StateMachine2D` хранит текущее активное состояние (`CurrentState`), корректно вызывает `Exit()` у предыдущего состояния и `Enter()` у нового при смене.
* `Update()` используется для проверки условий переходов и таймеров, а `FixedUpdate()` — для физических перемещений и поворотов через `Rigidbody2D`.

### Тип №1 — Обычный противник (`StandardEnemy2D`)
* **Восприятие:** сектор обзора `180°` (`viewAngle`) в радиусе `viewRadius` с проверкой препятствий через `Physics2D.Raycast` (`obstacleMask | playerMask`).
* **Перемещение:** плавное движение в 8 направлениях к цели (`MoveTowards`) с разворотом через `Rigidbody2D`.
* **Состояния:**
  * `PatrolState2D` — движение по точкам патрулирования (`patrolWaypoints`); при достижении точки переходит в `IdleState2D`, при обнаружении игрока — в `ChaseState2D`.
  * `IdleState2D` — ожидание на точке в течение заданного времени (`2f`), затем возврат в `PatrolState2D` (или переход в `ChaseState2D`, если игрок замечен).
  * `ChaseState2D` — преследование игрока; при входе в `attackRange` переходит в `AttackState2D`, при потере видимости — в `SearchState2D`.
  * `SearchState2D` — движение к `lastKnownPlayerPosition` и осмотр области (`3f` сек); при повторном обнаружении — в `ChaseState2D`, иначе — в `PatrolState2D`.
  * `AttackState2D` — поворот к игроку и выполнение атак с интервалом `attackCooldown`; при выходе игрока за пределы дистанции или потере видимости возвращает в `ChaseState2D`.

#### Таблица переходов (Обычный противник)
| Текущее состояние | Условие перехода | Следующее состояние |
| :--- | :--- | :--- |
| `PatrolState2D` | Достигнута точка маршрута | `IdleState2D` |
| `PatrolState2D` | Игрок в зоне видимости (`CanSeePlayer()`) или получен урон | `ChaseState2D` |
| `IdleState2D` | Прошло время ожидания (`>= 2f`) | `PatrolState2D` |
| `IdleState2D` | Игрок в зоне видимости (`CanSeePlayer()`) или получен урон | `ChaseState2D` |
| `ChaseState2D` | Дистанция до игрока `<= attackRange` и игрок виден | `AttackState2D` |
| `ChaseState2D` | Игрок вышел из поля зрения / скрыт препятствием | `SearchState2D` |
| `AttackState2D` | Игрок вышел за `attackRange` или пропал из видимости | `ChaseState2D` |
| `SearchState2D` | Игрок снова обнаружен (`CanSeePlayer()`) | `ChaseState2D` |
| `SearchState2D` | Время поиска истекло (`>= 3f`), игрок не найден | `PatrolState2D` |

---

### Тип №2 — Телепортирующийся противник (`TeleportEnemy2D`)
* **Восприятие:** периодическое круговое сканирование `360°` в радиусе `scanRadius` (по умолчанию `1.0f`) с интервалом `scanInterval` (`0.5f`) и проверкой препятствий лучом `Physics2D.Raycast`.
* **Телепортация:** доступна по завершении `teleportCooldown` (`4.0f`); выбирает точку на расстоянии `1.0f` за игроком и проверяет отсутствие препятствий через `Physics2D.OverlapCircle(..., 0.4f, obstacleMask)`.
* **Состояния:** `TeleportPatrolState2D`, `TeleportChaseState2D`, `TeleportState2D`, `TeleportAttackState2D`.

#### Таблица переходов (Телепортирующийся противник)
| Текущее состояние | Условие перехода | Следующее состояние |
| :--- | :--- | :--- |
| `TeleportPatrolState2D` | `Scan360()` обнаружил игрока или получен урон | `TeleportChaseState2D` |
| `TeleportChaseState2D` | Дистанция `> 3.0f` и готов кулдаун телепортации | `TeleportState2D` |
| `TeleportChaseState2D` | Дистанция до игрока `<= attackRange` | `TeleportAttackState2D` |
| `TeleportChaseState2D` | Дистанция до игрока `> 8.0f` | `TeleportPatrolState2D` |
| `TeleportState2D` | Выполнена попытка телепортации (`Enter`) | `TeleportChaseState2D` |
| `TeleportAttackState2D` | Дистанция до игрока `> attackRange + 0.6f` (гистерезис) | `TeleportChaseState2D` |

---

## 3. Управление и запуск
1. Откройте проект в **Unity** и загрузите сцену с настроенными объектами `Player`, `StandardEnemy2D` и `TeleportEnemy2D` (назначьте `LayerMask` для `Player`, `Enemy` и препятствий).
2. Нажмите **Play**.
3. Управление игроком:
   * `WASD` / стрелки — движение.
   * `Space` — атака в радиусе `attackRange`.