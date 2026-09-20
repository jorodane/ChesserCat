# Chesser Cat · 체서캣

**체스의 행마를 가진 고양이 영웅들을 이끌고 모험하는 변형 체스 로그라이트 SRPG.**

체스의 이동 규칙을 읽는 즐거움에 파티 구성, 체력, 사거리, 밀치기, 지형을 더하는 프로젝트다. 이동과 공격의 규칙을 각각 설정하고, 행동의 결과를 확인하면서 고양이들의 위치와 역할을 조합한다.

현재 저장소의 중심은 **격자 전투의 기반과 「출정식」 튜토리얼**이다. 완성된 로그라이트 한 판의 전체 진행은 아직 구현 범위에 포함되지 않는다. 이 문서는 처음 프로젝트를 실행하거나 코드를 읽는 사람을 위한 입구이며, 내부 연결과 확장 지점은 [개발자 탐색 가이드](docs/EXPLORER_GUIDE.md)에 정리했다.

## 구현과 기여 범위

[jorodane](https://github.com/jorodane)이 게임 기획과 핵심 게임플레이 시스템의 설계·C# 구현을 진행하는 개인 프로젝트다. 아래 핵심 설계는 구현 의도와 해당 코드를 함께 읽을 수 있도록 정리했다.

| 구분 | 범위와 확인 위치 |
| --- | --- |
| 기획·프로그래밍 | 변형 체스 규칙과 출정식 튜토리얼 기획, 전투 행동·기보·행마·매니저 구조 등 게임 로직. 주요 코드는 [Assets/0.Scripts](Assets/0.Scripts) |
| 현재 확인할 구현 | 격자 전투, 행동 미리보기·재생·되돌리기·분기, 프리셋 기반 이동·공격 설정, 출정식의 대화·목표 연결. 세부 상태는 [현재 구현 범위](#현재-구현-범위) |
| 외부 의존성과 리소스 | Unity 패키지, Firebase SDK, 외부 UI·셰이더·샘플 에셋을 포함한다. [패키지 목록](Packages/manifest.json)과 [저장소 지도](#저장소-지도)에서 프로젝트 코드의 위치를 확인할 수 있다. |
| 문서 작성 | 이 README와 [개발자 탐색 가이드](docs/EXPLORER_GUIDE.md)는 개발자의 기획 설명과 저장소 분석을 바탕으로 AI 보조를 사용해 작성·정리했다. |

소스 분석 기준: [`36c3d716`](https://github.com/jorodane/ChesserCat/commit/36c3d7162dc3824362735bc395ed58ebfeac56cd), 2026-09-20. 설명은 소스와 직렬화된 에셋을 확인한 결과이며, 이 문서 작성 환경에서 Unity 실행이나 플레이어 빌드를 검증하지는 않았다.

## 원하는 곳으로 바로 가기

| 하고 싶은 일 | 먼저 볼 곳 |
| --- | --- |
| 개발 범위와 외부 의존성 확인하기 | [구현과 기여 범위](#구현과-기여-범위) |
| 핵심 설계와 근거 코드 살펴보기 | [핵심 설계](#핵심-설계) |
| 직접 실행하고 출정식 체험하기 | [빠른 시작](#빠른-시작) |
| 게임의 의도와 현재 구현 구분하기 | [게임의 방향](#게임의-방향), [현재 구현 범위](#현재-구현-범위) |
| 어떤 파일부터 읽을지 정하기 | [코드를 읽는 순서](#코드를-읽는-순서) |
| 이동·공격·되돌리기 구조 이해하기 | [행동과 기보](docs/EXPLORER_GUIDE.md#행동과-기보) |
| 캐릭터·맵·대화·UI 수정하기 | [콘텐츠를 수정하는 곳](docs/EXPLORER_GUIDE.md#콘텐츠를-수정하는-곳) |
| 실행 중 막힌 지점 찾기 | [탐색 중 확인할 것](#탐색-중-확인할-것) |

## 핵심 설계

### 행동의 결과를 따라가는 전투 구성

근접 공격에는 피해, 퇴장, 밀치기, 자리 이동처럼 앞선 결과에 따라 달라지는 단계가 있다. **행동 하나의 결과가 반영된 상태에서 다음 행동을 판단하도록** 구성해, 전투가 일어나는 순서대로 행동 생성 코드를 읽고 확장할 수 있게 했다.

[CharacterBaseAction](Assets/0.Scripts/Objects/Characters/CharacterModules/CharacterBaseAction.cs)은 `IEnumerable<TurnActionInfo>`로 행동을 하나씩 내보낸다. [TurnActionBuilder.BuildActionArray()](Assets/0.Scripts/Turns/TurnActionBuilder.cs)는 받은 행동에 `GoNext(false)`를 호출한 뒤 열거를 이어 간다. 따라서 `yield return` 이후의 생존·점유 검사는 앞선 피해나 이동이 반영된 상태를 읽는다. 목록 생성이 끝나면 `finally`에서 적용한 행동을 역순으로 `GoPrev(false)`하여 원래 상태로 복원한다. 이 임시 적용은 `BuildActionArray()`가 열거를 진행할 때 이루어진다.

예를 들어 `MakeDamageAction()`은 피해 행동을 내보낸 뒤 대상의 생존 여부에 따라 퇴장 행동을 추가한다. 이후 `MakeBaseAttackAction()`은 대상이 살아 있다면 밀치기를 시도하고, 목적지 진입 가능 여부에 따라 이동 또는 복귀 행동을 만든다.

| 확인할 내용 | 근거 코드 |
| --- | --- |
| 앞 행동의 결과를 다음 판단에 반영 | [CharacterBaseAction](Assets/0.Scripts/Objects/Characters/CharacterModules/CharacterBaseAction.cs)의 `MakeDamageAction()`·`MakeBaseAttackAction()`과 [TurnActionBuilder](Assets/0.Scripts/Turns/TurnActionBuilder.cs)의 `BuildActionArray()`를 함께 확인 |
| 같은 행동 목록을 재생하고 앞뒤로 탐색 | [TurnBaseInfo](Assets/0.Scripts/Turns/TurnBaseInfo.cs)의 `Play()`·`GoNext()`·`GoPrev()`·`GetHealthDelta()` |
| 미리보기 확정과 과거 기록에서의 다른 수 시험 | [BattleManager](Assets/0.Scripts/Managers/BattleManager.cs)의 `TurnSimulationConfirm()`·`AddTurn()`·`AnalysisModeEnd()` |

이 행동 목록을 예상 HP 표시, 실제 재생, 기보 탐색과 분석용 분기에 공통으로 사용한다. 확장 시에는 각 행동의 정방향 적용과 역방향 복원이 대응되어야 하며, 임시 적용으로 발생하는 상태 변경과 이벤트의 영향도 함께 고려해야 한다. 생성·재생·복원의 세부 연결은 [행동과 기보](docs/EXPLORER_GUIDE.md#행동과-기보)에 이어진다.

### 행마 요소를 조합하는 캐릭터 설정

같은 체스 행마를 공유하면서도 통과 방식과 사거리가 다른 캐릭터를 구성하기 위해, `MoveTypeInfo`에 **형태(`style`)·판정 방식(`checker`)·거리(`maxDistance`)**를 두고 이동과 공격에 각각 설정한다.

[CharacterPreset](Assets/0.Scripts/ScriptableObjects/CharacterPreset.cs)의 `move`·`attack`을 [ChessMovementModule](Assets/0.Scripts/Objects/Characters/CharacterModules/ChessModule/ChessMovementModule.cs)이 받아, [TileManager](Assets/0.Scripts/Managers/TileManager.cs)의 후보 칸 생성과 [ChessMovementTileCheckers](Assets/0.Scripts/Objects/Characters/CharacterModules/ChessModule/ChessMovementTileCheckers.cs)의 검사를 연결한다. 실제 예로 [Veni](Assets/1.Datas/Origin/ScriptableObjects/Globals/Characters/Veni.asset)와 [Teni](Assets/1.Datas/Origin/ScriptableObjects/Globals/Characters/Teni.asset)는 대각선·3칸 설정을 공유하고 각각 `Charge`와 `Jump`를 사용한다.

기존에 지원하는 요소의 조합은 프리셋에서 조정할 수 있다. 새로운 행마나 특수 규칙을 추가하려면 후보 생성, 판정, 필요한 행동 구현까지 확장해야 한다. 캐릭터별 실제 설정은 [출정식 프리셋 표](#출정식이-첫-튜토리얼인-이유)에서 비교할 수 있다.

### 실행 순서와 확장 지점을 명시한 매니저 구조

입력 상태를 읽는 시점, 컨트롤러의 판단, 캐릭터의 처리, UI 갱신이 어떤 순서로 이어지는지 명시하기 위해 [GameManager.Update()](Assets/0.Scripts/Managers/GameManager.cs)에서 **매니저 → 컨트롤러 → 캐릭터 → 오브젝트 → UI** 순으로 업데이트 이벤트를 호출한다. 초기화와 제거에도 별도의 단계가 있다.

[ManagerBase](Assets/0.Scripts/Managers/ManagerBase.cs)는 `Connect()`·`Disconnect()`로 연결 절차를 제공하고, 각 매니저의 작업은 `OnConnected()`·`OnDisconnected()`에서 구현한다. 입력 구독과 해제의 예는 [BattleManager](Assets/0.Scripts/Managers/BattleManager.cs)에서 확인할 수 있다.

이 구조로 프로젝트의 공통 실행 절차와 기능별 구현 위치를 정했다. 새 기능을 연결할 때에는 이벤트 구독·해제와 초기화 의존성을 함께 관리해야 한다. 업데이트 이벤트는 그룹 사이의 순서를 정하며, 같은 그룹 내부의 호출 순서는 구독 순서에 따른다.

## 게임의 방향

네 명의 영웅이 고양이 왕국의 출정식에 모여 대마왕 **코코**를 물리치러 떠난다. 체스 기물의 행마가 캐릭터의 개성과 전술적 역할이 되고, 서로 다른 이동 방식으로 공간을 풀어 나가는 것이 기획의 중심이다.

- **이동과 공격을 독립적으로 설정한다.** 어떤 방향으로 움직이는지, 어디를 공격하는지, 길을 따라 돌진하는지 또는 뛰어넘는지를 조합한다.
- **체력과 위치가 함께 중요하다.** 근접 공격은 피해뿐 아니라 밀치기와 자리 차지로 이어질 수 있다. 원거리 공격은 제자리에서 처리하는 경로를 가진다.
- **결과를 읽을 수 있게 보여준다.** 이동·공격 가능 칸, 예상 체력 변화, 행동 기록, 분석용 분기를 통해 선택의 결과를 확인한다.
- **기획의 장기 방향은 로그라이트 파티 모험이다.** 종자의 성장과 프로모션, 체크메이트를 활용한 보스전, 런 진행과 성장 요소는 현재 기반 위에 확장하려는 영역이다.

### 출정식이 첫 튜토리얼인 이유

첫 목표는 왕 앞에 네 영웅을 세우는 것이다. 플레이어는 각자의 행마를 써서 지정된 네 칸에 고양이를 배치하며 자연스럽게 이동 규칙을 익힌다. 전투 전에 파티의 결성과 조작 학습이 같은 장면에서 일어난다.

킹은 이 장면의 **플레이어 스타팅 파티에 포함되지 않는다.** 왕과 여왕은 NPC로 등장한다. 기획상 킹의 상징성과 이후 역할을 살리고, 처음부터 한 칸 이동만으로 인식되는 것을 피하려는 구성이다. 특정 행마로 갈 수 있는 외딴 비밀칸과 도전과제는 자발적인 응용 학습을 유도하려는 기획이며, 도전과제 시스템의 구현을 뜻하지 않는다.

현재 출정식의 플레이어 프리셋은 다음과 같다. 아래 이동·공격은 프리셋에 기록된 설정이다.

| 프리셋 | 역할 | 이동 설정 | 공격 설정 | 기본 HP |
| --- | --- | --- | --- | ---: |
| `Gunner` | 전차·룩 계열 포대 | 직선, `Charge`, 최대 2칸 | 직선, `Range`, 최대 5칸 | 5 |
| `Hero` | 나이트 계열 용사 | 나이트 행마, `Jump`, 범위 값 2 | 나이트 행마, `Jump`, 범위 값 2 | 10 |
| `Veni` | 붉은 비숍·쌍둥이 형 | 대각선, `Charge`, 최대 3칸 | 대각선, `Charge`, 최대 3칸 | 10 |
| `Teni` | 초록 비숍·쌍둥이 동생 | 대각선, `Jump`, 최대 3칸 | 대각선, `Jump`, 최대 3칸 | 8 |

나이트의 범위 값 2는 현재 후보 칸 생성에서 사용하는 값이며, 직선으로 두 칸 걷는다는 뜻이 아니다. 설정 원본은 [캐릭터 프리셋 폴더](Assets/1.Datas/Origin/ScriptableObjects/Globals/Characters), 행마 해석은 [ChessMovementModule](Assets/0.Scripts/Objects/Characters/CharacterModules/ChessModule/ChessMovementModule.cs)과 [TileManager](Assets/0.Scripts/Managers/TileManager.cs)를 함께 확인하면 된다.

## 빠른 시작

### 준비

| 항목 | 저장소 기준 |
| --- | --- |
| Unity Editor | **6000.3.5f2** — [ProjectVersion.txt](ProjectSettings/ProjectVersion.txt) |
| 프로젝트 버전 | `0.01` — [ProjectSettings.asset](ProjectSettings/ProjectSettings.asset) |
| 렌더링 | URP `17.3.0`, 2D 에셋 기반 |
| 주요 패키지 | Addressables `2.8.1`, Input System `1.17.0`, 2D Animation `13.0.2`, uGUI `2.0.0` |
| 패키지 원본 | [manifest.json](Packages/manifest.json), [packages-lock.json](Packages/packages-lock.json) |
| Git LFS | Firebase의 `.bundle` 파일에 사용 — [.gitattributes](.gitattributes) |

```bash
git lfs install
git clone https://github.com/jorodane/ChesserCat.git
cd ChesserCat
git lfs pull
```

1. Unity Hub에서 저장소 루트를 프로젝트로 추가하고 **6000.3.5f2**로 연다.
2. 패키지 복원과 에셋 임포트가 끝날 때까지 기다린다.
3. [Assets/6.Scenes/SampleScene.unity](Assets/6.Scenes/SampleScene.unity)를 연다. [빌드 씬 목록](ProjectSettings/EditorBuildSettings.asset)에 등록된 게임 씬도 이것이다.
4. Play를 누르고 로딩 완료 안내가 나오면 **아무 키나 누르거나 클릭**한다. 로딩 화면은 완료 시 일시정지하고 입력을 기다리도록 작성되어 있다.
5. Game 뷰에 포커스를 둔 채 **F8**을 눌러 `TheBeginning.Json`을 불러온다. 현재 씬의 `GameManager.startScreen`은 `Battle`이며, 배틀 화면에 들어가는 것과 맵을 불러오는 것은 별도 과정이다.
6. 대화는 **Space / Enter**로 넘긴다. 목표 칸을 보여주는 카메라 연출이 끝나면 고양이를 선택해 배치한다. 타일 편집 창이 열려 있다면 **F9**로 닫고 조작한다.

현재 목표는 플레이어 소유 캐릭터로 `(4, 2)`, `(5, 2)`, `(7, 2)`, `(8, 2)`를 모두 채우는 것이다. 화면 표기로는 **e3, f3, h3, i3**이며, 목표 구현은 각 칸에 특정 이름의 영웅을 강제하지 않는다.

Firebase SDK는 포함되어 있지만 [DBManager.OnConnected](Assets/0.Scripts/Managers/DBManager.cs)의 초기화 호출은 주석 처리되어 있다. 현재 진입 흐름에서 Firebase 로그인은 실행 준비 단계가 아니다.

### 조작과 개발 단축키

실제 게임 입력은 씬의 `PlayerInput`에 연결된 [GeneralKeyInput.inputactions](Assets/5.Inputs/GeneralKeyInput.inputactions)와 [InputManager](Assets/0.Scripts/Managers/InputManager.cs)가 기준이다. 루트의 `Assets/InputSystem_Actions.inputactions`와 혼동하지 않도록 한다.

| 입력 | 현재 연결된 기능 |
| --- | --- |
| 고양이 좌클릭 / 드래그 | 선택, 명령 메뉴, 이동·공격 후보와 결과 미리보기·확정 |
| `1`–`8`, `Q` / `E` | 소유 기물 번호 선택, 이전 / 다음 기물 선택 |
| `D` / `A` | 선택 명령 메뉴에서 이동 / 공격 입력으로 전환 |
| `S`, `Esc` | 선택·명령 취소. `Esc`는 열린 UI나 분석 상태에 따라 닫기·메뉴도 처리 |
| 우클릭 드래그, `R` | 보드에 가이드 표시 / 가이드 지우기 |
| 방향키, 마우스 휠, `Home` | 카메라 이동 / 확대·축소 / 카메라 초기화 |
| `Tab` 누르고 있기 | 전체 캐릭터 상태 표시 |
| `Space` / `Enter` | 대화 다음 내용 |
| `Z` / `X` / `C` / `V` | 기보 처음 / 이전 / 다음 / 마지막. 대화 중에는 처음 / 이전 / 다음 / 건너뛰기로 사용 |
| `V` | 목표 소개 연출 중에는 연출 건너뛰기 |
| `F5` / `F6` | `Assets/Saves/Test.Json`에 빠른 저장 / 해당 파일 불러오기 |
| `F7` | `ClassicChess.Json` 불러오기 |
| `F8` | `TheBeginning.Json` 불러오기. 입력 이름은 `CliffLoad`지만 현재 대상은 출정식 |
| `Shift` + `F6` / `F7` / `F8` | 해당 파일의 보드만 로드하고 전투 상태 초기화 |
| `F9` | 실행 중 타일 편집 UI 토글 |
| 편집 UI에서 좌클릭 / `Shift` + 좌클릭 / `Ctrl` + 좌클릭 | 타일 생성·칠하기 / 삭제 / 타일 설정 복사 |

`F5`는 저장소에 들어 있는 `Test.Json`을 실제로 덮어쓴다. 실행 실험 이후 이 파일의 변경 여부를 확인하면 된다. `I/P/O/N/M` 등의 입력 선언도 있지만, 선언만으로 해당 메뉴의 게임 기능이 완성되어 있다고 판단하지 않는다.

## 저장소 지도

| 위치 | 내용 |
| --- | --- |
| [Assets/0.Scripts](Assets/0.Scripts) | 프로젝트 자체 C# 코드. 매니저, 캐릭터·컨트롤러, 행동 기록, UI, 데이터 형식 |
| [Assets/1.Datas/Origin/Prefabs/Globals](Assets/1.Datas/Origin/Prefabs/Globals) | 실제 게임용 캐릭터·컨트롤러·타일·UI 프리팹 |
| [Assets/1.Datas/Origin/ScriptableObjects/Globals](Assets/1.Datas/Origin/ScriptableObjects/Globals) | 캐릭터 프리셋, 대화, 목표, 아이템, 풀 설정, 지형 데이터 |
| [Assets/2.Textures](Assets/2.Textures) | 캐릭터·타일·아이콘·UI 이미지와 Sprite Library 등 시각 리소스 |
| [Assets/3.Animations](Assets/3.Animations) | 캐릭터·타일·UI의 Animation Clip과 Animator Controller |
| [Assets/4.Fonts](Assets/4.Fonts) | 폰트 리소스 |
| [Assets/5.Inputs](Assets/5.Inputs) | 게임용 Input Actions |
| [Assets/6.Scenes](Assets/6.Scenes) | 게임 진입 씬과 관련 데이터 |
| [Assets/7.Shader](Assets/7.Shader) | 프로젝트 셰이더·머티리얼 리소스 |
| [Assets/Saves](Assets/Saves) | 보드, 캐릭터 배치, 목표 이름, 행동 기록을 담는 개발용 JSON |
| [Assets/AddressableAssetsData](Assets/AddressableAssetsData) | Addressables 그룹·라벨·빌드 설정 |
| [Assets/Settings](Assets/Settings) | 렌더 파이프라인 설정 및 템플릿 씬 |
| [Packages](Packages), [ProjectSettings](ProjectSettings) | 패키지 버전과 프로젝트 설정 |

`Assets/Firebase`, `Assets/ExternalDependencyManager`, `Assets/Layer Lab`, `Assets/Plugins`, `Assets/TextMesh Pro`, `Assets/SampleTilesets`에는 외부 SDK·UI·셰이더·샘플 리소스가 있다. 게임 로직의 탐색은 `0.Scripts`와 `1.Datas`부터 시작하면 된다. 외부 패키지의 데모 씬은 게임의 진입 씬과 별개다.

## 코드를 읽는 순서

| 순서 | 파일 | 확인할 질문 |
| ---: | --- | --- |
| 1 | [GameManager](Assets/0.Scripts/Managers/GameManager.cs), [ManagerBase](Assets/0.Scripts/Managers/ManagerBase.cs) | 누가 매니저를 만들고 어떤 순서로 초기화·업데이트하는가? |
| 2 | [DataManager](Assets/0.Scripts/Managers/DataManager.cs), [ObjectManager](Assets/0.Scripts/Managers/ObjectManager.cs) | 데이터와 프리팹을 이름으로 찾고 생성하는 경로는 무엇인가? |
| 3 | [SaveManager](Assets/0.Scripts/Managers/SaveManager.cs), [SaveStructures](Assets/0.Scripts/Managers/ManagerModules/SaveStructures.cs), [TheBeginning.Json](Assets/Saves/TheBeginning.Json) | 씬 밖에 저장된 맵과 파티가 어떻게 전투로 복원되는가? |
| 4 | [BattleManager](Assets/0.Scripts/Managers/BattleManager.cs), [PlayerController](Assets/0.Scripts/Objects/Controllers/PlayerController.cs) | 선택·미리보기·확정과 턴 요청이 어떻게 연결되는가? |
| 5 | [CharacterPreset](Assets/0.Scripts/ScriptableObjects/CharacterPreset.cs), [ChessMovementModule](Assets/0.Scripts/Objects/Characters/CharacterModules/ChessModule/ChessMovementModule.cs), [TileManager](Assets/0.Scripts/Managers/TileManager.cs) | 캐릭터 설정이 어떤 이동·공격 후보를 만드는가? |
| 6 | [TurnActionBuilder](Assets/0.Scripts/Turns/TurnActionBuilder.cs), [CharacterBaseAction](Assets/0.Scripts/Objects/Characters/CharacterModules/CharacterBaseAction.cs), [TurnActionInfo](Assets/0.Scripts/Turns/TurnActionInfo.cs), [TurnBaseInfo](Assets/0.Scripts/Turns/TurnBaseInfo.cs) | 한 행동을 어떤 작은 변화로 만들고 재생·복원하는가? |
| 7 | [목표 코드](Assets/0.Scripts/ScriptableObjects/Objectives), [ChatEvents](Assets/0.Scripts/Managers/ManagerModules/ChatEvents.cs), [UI_ChatArea](Assets/0.Scripts/UIs/Functions/Informations/UI_ChatArea.cs) | 출정식의 대화·목표·완료 연출을 누가 이어 주는가? |
| 8 | [UIManager](Assets/0.Scripts/Managers/UIManager.cs), [UI 프리팹](Assets/1.Datas/Origin/Prefabs/Globals/UIs) | 코드와 Inspector 설정이 합쳐져 화면을 어떻게 구성하는가? |

특히 `TurnActionBuilder.BuildActionArray()`는 읽어 둘 만하다. 행동들을 순서대로 임시 적용해 후속 결과를 계산한 다음 역순으로 되돌려 행동 배열을 만든다. 이 배열을 미리보기와 실제 재생, 과거 기록 탐색에 사용한다. 자세한 내용은 [행동과 기보](docs/EXPLORER_GUIDE.md#행동과-기보)에서 이어진다.

## 현재 구현 범위

여기서 **구현**은 소스·에셋 연결이 있다는 뜻이며, 모든 경우의 플레이 검증을 의미하지 않는다.

| 영역 | 현재 저장소에서 확인되는 범위 |
| --- | --- |
| 격자와 행마 | 가변 크기 보드, 비어 있는 칸, 지형 데이터, 기물별 이동·공격 후보 계산 |
| 전투 행동 | 이동, 피해·회복 행동, 근접 밀치기·자리 이동, 원거리 피해, 퇴장·복원 |
| 결과 표시와 분석 | 예상 HP 변화, 행동 기록, 앞뒤 이동, 과거 위치에서 별도 분기 실행 |
| 출정식 | 네 영웅 배치, 시작·완료 대화, 목표 타일 표시·카메라 연출, 결과 화면 활성화 |
| AI | 공격 후보가 있으면 그중 무작위 선택, 없으면 이동 후보 중 무작위 선택 |
| 종자 | 주인과 종자의 관계, `masterInfo` / `pawnInfo`, 생성·저장 구조. 프로모션 진행은 별도 구현 필요 |
| 인벤토리 | 슬롯·스택·교환·분할·병합·정렬 및 UI 기반. 사용·장착 효과와 일부 API는 비어 있음 |
| 콘텐츠 도구 | 실행 중 타일 편집, JSON 저장·로드, 프리셋·대화·목표의 ScriptableObject 작성 |
| 서비스와 메뉴 | Firebase 연결 코드가 있으나 초기화 비활성. 오디오·언어 매니저, 여러 메뉴, 퍽 클래스에 뼈대가 남아 있음 |
| 장기 기획 | 로그라이트 런·보상·성장, 체크메이트 보스 페이즈, 종자 성장·프로모션, 비밀칸 도전과제는 완성된 플레이 흐름으로 연결되어 있지 않음 |

현재 턴 진행은 행동 재생 완료 후 `BattleManager.TurnEnd()`에서 컨트롤러 순서를 넘기는 형태다. 기획에서 다룬 **파티 전체 행동 단위의 턴과 보스 전용 추가 턴**이 모두 구현된 상태로 해석하지 않는다. `ClassicChess.Json`도 기본 배치를 시험하는 데이터이며, 정통 체스의 모든 특수 규칙이나 체크메이트 판정이 구현되었다는 뜻은 아니다.

## 탐색 중 확인할 것

| 상황 | 확인할 위치와 이유 |
| --- | --- |
| 로딩이 끝났는데 화면이 그대로다 | `UI_LoadingScreen.SetComplete()`는 입력 대기 상태로 전환한다. Game 뷰에 포커스를 두고 입력한다. |
| 배틀 UI만 있고 맵이 없다 | `F8`로 출정식 데이터를 불러온다. 화면 전환과 스테이지 로드는 분리되어 있다. |
| 고양이가 조작되지 않는다 | 대화·목표 소개의 카메라 잠금, 일시정지, 타일 편집 창, UI 위의 커서 상태를 확인한다. |
| 데이터·프리팹 이름을 찾지 못한다 | Addressables의 `Global` 라벨과 로드된 타입, 에셋 이름, `PoolRequest`의 `poolName`을 확인한다. [이름으로 연결되는 데이터](docs/EXPLORER_GUIDE.md#이름으로-연결되는-데이터) 참고. |
| `F8`인데 Cliff 맵이 나오지 않는다 | 현재 `SaveManager.LoadCliffChess()`는 `TheBeginning.Json`을 읽는다. |
| 프리셋 공격력을 바꿨는데 피해가 기대와 다르다 | `CharacterBase.GetAttackDamage()`는 현재 프리팹의 `baseDamage`를 반환한다. 프리셋 `damage`와 실제 적용 경로를 함께 확인한다. |
| 독립 실행 파일을 만들려 한다 | 이 안내는 에디터 탐색 기준이다. `CameraManager`와 `TileManager`의 런타임 소스에 `UnityEditor` 참조가 있고, 샘플 JSON도 `Application.dataPath/Saves`에서 직접 읽는다. 빌드용 참조 분리·데이터 배포·저장 경로는 별도 점검 대상이다. |

프로젝트 자체의 자동화 테스트나 CI 워크플로는 현재 저장소에 없다. 동작을 수정할 때의 확인 경로와 저장·복원상의 주의점은 [개발자 탐색 가이드](docs/EXPLORER_GUIDE.md)에 이어서 설명한다.
