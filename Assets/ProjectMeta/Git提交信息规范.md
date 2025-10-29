# Git 提交信息规范（Conventional Commits）

本文档定义项目的 Git 提交信息规范，采用业界广泛使用的 Conventional Commits 标准，以提高可读性、自动化版本管理与变更日志生成的一致性。

## 基本结构

提交信息由三部分组成：

```
<type>(<scope>): <subject>

<body>

<footer>
```

- `type`：提交类型（见下文取值）。
- `scope`：影响范围（模块/子系统名，建议小写英文，例如 `map`、`ui`、`core`）。可省略。
- `subject`：一句话简介，使用祈使句，不以句号结尾，尽量 ≤ 50 字符。
- `body`：可选，补充动机、设计取舍、影响面。每行 ≤ 72 字符，必要时用要点列出。
- `footer`：可选，用于关联 Issue/PR、声明不兼容变更（Breaking Change）。

## 类型（type）取值

- `feat`：新增功能。
- `fix`：缺陷修复。
- `docs`：文档变更（仅文档）。
- `style`：代码风格（不影响逻辑，如格式化、空格、分号）。
- `refactor`：重构（既非修复也非新功能）。
- `perf`：性能优化。
- `test`：测试相关变更。
- `build`：构建系统或外部依赖变更（如打包、依赖、脚本）。
- `ci`：持续集成配置变更。
- `chore`：其他杂项（不影响源代码或测试）。
- `revert`：回滚某次提交。

## 语义化版本建议

- `feat`：通常对应次版本升级（minor）。
- `fix`：通常对应补丁版本升级（patch）。
- 包含 `BREAKING CHANGE:` 的提交：主版本升级（major）。

版本实际发布策略由发布流程决定，本规范仅建议。

## 写作建议

- 使用祈使句：例如 “add map generator”，而非 “added” 或 “adds”。
- 简洁具体：聚焦变更的行为与影响，避免泛泛而谈。
- 一次提交做一件事：尽可能将文档与功能分离为独立提交。
- `scope` 明确：对齐项目目录或子系统名，例如 `map`、`core/save`、`ui/main-menu`。
- `body` 描述动机与设计：解释为什么、考虑过哪些方案、为何选择当前实现。
- `footer` 关联：使用 `Refs:` 或 `Closes:` 关联 Issue/PR；不兼容改动使用 `BREAKING CHANGE:` 说明迁移指南。

## 示例

```text
feat(map): add procedural map system, controller, views and save support

Introduce MapGenerator to produce multi-lane tiered nodes with interconnections.
Add MapController/NodeView for UI placement and interactions.
Persist progress with SaveManager (PlayerPrefs) and auto-restore on load.

Refs: #42
```

```text
docs(contrib): add git commit convention for the repository

Document the Conventional Commits standard, types, scopes and examples.
Explain writing guidelines and semantic versioning suggestions.
```

```text
fix(battle): prevent null ref when loading enemy data

Guard against missing EnemyConfig in BattleController.Init().

Closes: #57
```

```text
refactor(core/save): split serialization from persistence

Extract SaveSerializer to decouple JSON conversion from PlayerPrefs IO.
```

```text
perf(map): pool node views to reduce GC and instantiate cost

Replace Instantiate/Destroy with SimpleObjectPool in MapController.
```

```text
revert: revert feat(map) add procedural map system

The change caused crashes on older devices. Will reintroduce after fixes.
```

## Footer 书写

- 关联问题：`Refs: #123` / `Closes: #123` / `Fixes: #123`。
- 不兼容改动：

```
BREAKING CHANGE: rename MapController.Build() to BuildMap()

Update all callers accordingly.
```

## 分支与 PR（建议）

- 分支命名：`feature/<scope>-<short-desc>`，`fix/<scope>-<short-desc>`。
- 提交到 `main` 的变更应通过 PR 并通过 CI 检查与代码评审。
- PR 描述需列出：动机、实现、影响范围、测试覆盖、迁移指南（如有）。

## 提交规范的落地

- 在 IDE 或提交工具中配置模板（可选）。
- 开发中遵循“一提交一意图”，避免将多个无关变更混合提交。
- 如需自动生成变更日志，可结合 `conventional-changelog` 或 `semantic-release`（后续可选）。