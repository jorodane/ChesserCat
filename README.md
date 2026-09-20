# Chesser Cat · 체서캣

**체스의 행마를 가진 고양이 영웅들을 이끌고 모험하는 변형 체스 로그라이트 SRPG입니다.**

체서캣은 체스의 이동 규칙을 읽는 즐거움에 파티 구성과 캐릭터의 개성을 더하고 있는 프로젝트입니다. 이동과 공격의 규칙을 각각 설정하고, 체력·사거리·밀치기·지형이 어우러지는 전투를 구현하고 있습니다. 플레이어가 행동의 결과를 살펴보며 고양이들의 위치와 역할을 조합하는 경험을 만들고자 합니다.

현재는 **격자 전투의 기반과 「출정식」 튜토리얼**을 중심으로 개발하고 있으며, 이 기반 위에 로그라이트 모험과 성장 요소를 확장할 계획입니다. 아래에서는 게임의 기획 의도와 주요 설계, 직접 실행하고 코드를 살펴보는 방법을 소개합니다. 시스템 간 연결과 콘텐츠 확장 방법은 [개발자 탐색 가이드](docs/EXPLORER_GUIDE.md)에 자세히 정리했습니다.

## 구현과 기여 범위

Unity와 C#으로 개발하는 개인 프로젝트로, 게임 기획과 핵심 게임플레이 시스템의 설계·구현을 담당하고 있습니다. 변형 체스의 규칙을 실제 플레이로 연결하는 과정에서 행동의 계산과 재생, 기보 탐색, 캐릭터 설정 구조에 중점을 두었습니다.

| 구분 | 작업 내용 |
| --- | --- |
| 게임 기획 | 변형 체스 규칙, 캐릭터별 전술적 역할, 출정식 튜토리얼과 모험의 진행 방향을 기획했습니다. |
| 게임플레이 프로그래밍 | 전투 행동·기보·행마·매니저 구조 등 핵심 게임 로직을 설계하고 C#으로 구현했습니다. 주요 코드는 [Assets/0.Scripts](Assets/0.Scripts)에 있습니다. |
| 콘텐츠 연결 | 프리셋으로 이동·공격을 설정하고, 출정식의 대화·목표·카메라 연출을 연결했습니다. 세부 내용은 [현재 구현 범위](#현재-구현-범위)에서 소개합니다. |
| 외부 패키지와 리소스 | Unity 패키지와 외부 UI·셰이더·샘플 에셋을 활용하고 있으며, Firebase SDK도 포함되어 있습니다. [패키지 목록](Packages/manifest.json)과 [저장소 지도](#저장소-지도)에서 구성을 살펴보실 수 있습니다. |

## 원하는 곳으로 바로 가기

| 관심 있는 내용 | 안내 |
| --- | --- |
| 개발을 담당한 부분과 사용 리소스 | [구현과 기여 범위](#구현과-기여-범위) |
| 게임의 기획 의도와 개발 현황 | [게임의 방향](#게임의-방향), [현재 구현 범위](#현재-구현-범위) |
| 핵심 설계와 관련 코드 | [핵심 설계](#핵심-설계) |
| 직접 실행하고 출정식 체험하기 | [빠른 시작](#빠른-시작) |
| 코드의 전체 흐름 살펴보기 | [코드를 읽는 순서](#코드를-읽는-순서) |
| 이동·공격·되돌리기 구조 이해하기 | [행동과 기보](docs/EXPLORER_GUIDE.md#행동과-기보) |
| 캐릭터·맵·대화·UI 수정하기 | [콘텐츠를 수정하는 곳](docs/EXPLORER_GUIDE.md#콘텐츠를-수정하는-곳) |
| 실행과 수정에 필요한 안내 | [탐색 중 확인할 것](#탐색-중-확인할-것) |

## 핵심 설계

### 행동의 결과를 따라가는 전투 구성

근접 공격 한 번에도 피해, 퇴장, 밀치기, 자리 이동처럼 앞선 결과에 따라 달라지는 여러 단계가 있습니다. 이 흐름을 코드에서도 자연스럽게 표현하기 위해 **행동 하나의 결과가 반영된 상태에서 다음 행동을 판단하도록** 구성했습니다. 전투가 일어나는 순서대로 행동을 작성하고 확장할 수 있도록 한 설계입니다.

[CharacterBaseAction](Assets/0.Scripts/Objects/Characters/CharacterModules/CharacterBaseAction.cs)은 `IEnumerable<TurnActionInfo>`로 행동을 하나씩 내보냅니다. [TurnActionBuilder.BuildActionArray()](Assets/0.Scripts/Turns/TurnActionBuilder.cs)는 받은 행동에 `GoNext(false)`를 호출해 임시 적용한 뒤 다음 행동을 요청합니다. 따라서 `yield return` 이후의 생존·점유 검사는 앞선 피해나 이동이 반영된 상태를 읽게 됩니다. 목록 생성이 끝나면 `finally`에서 적용한 행동을 역순으로 `GoPrev(false)`하여 원래 상태로 복원합니다.

예를 들어 `MakeDamageAction()`은 피해 행동을 내보낸 뒤 대상의 생존 여부에 따라 퇴장 행동을 추가합니다. 이후 `MakeBaseAttackAction()`은 대상이 살아 있다면 밀치기를 시도하고, 목적지 진입 가능 여부에 따라 이동 또는 복귀 행동을 만듭니다.

| 설계와 연결된 기능 | 관련 코드 |
| --- | --- |
| 앞 행동의 결과를 다음 판단에 반영 | [CharacterBaseAction](Assets/0.Scripts/Objects/Characters/CharacterModules/CharacterBaseAction.cs)의 `MakeDamageAction()`·`MakeBaseAttackAction()`, [TurnActionBuilder](Assets/0.Scripts/Turns/TurnActionBuilder.cs)의 `BuildActionArray()` |
| 같은 행동 목록을 재생하고 앞뒤로 탐색 | [TurnBaseInfo](Assets/0.Scripts/Turns/TurnBaseInfo.cs)의 `Play()`·`GoNext()`·`GoPrev()`·`GetHealthDelta()` |
| 미리보기 확정과 과거 기록에서의 다른 수 시험 | [BattleManager](Assets/0.Scripts/Managers/BattleManager.cs)의 `TurnSimulationConfirm()`·`AddTurn()`·`AnalysisModeEnd()` |

이렇게 만든 행동 목록은 예상 HP 표시, 실제 재생, 기보 탐색과 분석용 분기에 공통으로 사용합니다. 플레이어는 행동을 확정하기 전에 결과를 살펴보고, 과거 기록으로 돌아가 다른 수를 시험해 볼 수 있습니다. 이 구조를 확장할 때에는 각 행동의 정방향 적용과 역방향 복원을 한 쌍으로 구현하고, 임시 적용 중 발생하는 이벤트도 함께 고려해야 합니다. 생성·재생·복원의 세부 연결은 [행동과 기보](docs/EXPLORER_GUIDE.md#행동과-기보)에 정리했습니다.

### 행마 요소를 조합하는 캐릭터 설정

같은 체스 행마를 공유하더라도 통과 방식과 사거리에 따라 서로 다른 역할을 만들고자 했습니다. 이를 위해 `MoveTypeInfo`에 **형태(`style`)·판정 방식(`checker`)·거리(`maxDistance`)**를 두고, 이동과 공격에 각각 설정할 수 있도록 구성했습니다.

[CharacterPreset](Assets/0.Scripts/ScriptableObjects/CharacterPreset.cs)의 `move`·`attack`을 [ChessMovementModule](Assets/0.Scripts/Objects/Characters/CharacterModules/ChessModule/ChessMovementModule.cs)이 받아, [TileManager](Assets/0.Scripts/Managers/TileManager.cs)의 후보 칸 생성과 [ChessMovementTileCheckers](Assets/0.Scripts/Objects/Characters/CharacterModules/ChessModule/ChessMovementTileCheckers.cs)의 검사를 연결합니다. 예를 들어 비숍 쌍둥이인 [Veni](Assets/1.Datas/Origin/ScriptableObjects/Globals/Characters/Veni.asset)와 [Teni](Assets/1.Datas/Origin/ScriptableObjects/Globals/Characters/Teni.asset)는 대각선·3칸 설정을 공유하면서 각각 `Charge`와 `Jump`를 사용합니다. 같은 방향으로 움직여도 경로를 통과하는 방식이 달라, 서로 다른 위치에서 활약하도록 구성했습니다.

지원하는 요소의 조합은 프리셋에서 조정할 수 있으며, 새로운 행마나 특수 규칙은 후보 생성·판정·행동 구현을 확장해 추가하는 구조입니다. 캐릭터별 설정은 [출정식 프리셋 표](#출정식이-첫-튜토리얼인-이유)에서 비교하실 수 있습니다.

### 실행 순서와 확장 지점을 명시한 매니저 구조

입력을 읽고, 컨트롤러가 판단하고, 캐릭터가 행동한 뒤 UI가 갱신되는 흐름을 명시적으로 관리하고자 했습니다. [GameManager.Update()](Assets/0.Scripts/Managers/GameManager.cs)에서 **매니저 → 컨트롤러 → 캐릭터 → 오브젝트 → UI** 순으로 업데이트 이벤트를 호출하며, 초기화와 제거에도 별도의 단계를 두었습니다.

[ManagerBase](Assets/0.Scripts/Managers/ManagerBase.cs)는 `Connect()`·`Disconnect()`로 공통 연결 절차를 제공하고, 각 매니저의 작업은 `OnConnected()`·`OnDisconnected()`에서 구현합니다. 입력 구독과 해제의 예는 [BattleManager](Assets/0.Scripts/Managers/BattleManager.cs)에서 살펴보실 수 있습니다.

이를 통해 공통 실행 절차와 기능별 구현 위치를 정하고, 새 기능을 연결할 지점을 마련했습니다. 기능을 추가할 때에는 이벤트 구독·해제와 초기화 의존성을 함께 관리합니다. 업데이트 이벤트는 그룹 간 순서를 정하며, 같은 그룹 내부에서는 구독 순서에 따라 호출됩니다.

## 게임의 방향

네 명의 영웅이 고양이 왕국의 출정식에 모여 대마왕 **코코**를 물리치러 떠납니다. 체스 기물의 행마가 캐릭터의 개성과 전술적 역할로 이어지고, 서로 다른 이동 방식으로 공간을 풀어 나가는 것이 기획의 중심입니다.

- **이동과 공격의 조합으로 역할을 만듭니다.** 이동 방향과 공격 범위, 돌진과 도약을 각각 설정해 캐릭터마다 다른 전술적 선택을 제공합니다.
- **체력과 위치를 함께 활용합니다.** 근접 공격은 피해뿐 아니라 밀치기와 자리 차지로 이어질 수 있으며, 원거리 공격은 제자리에서 피해를 주도록 구성했습니다.
- **선택의 결과를 살펴볼 수 있게 합니다.** 이동·공격 가능 칸과 예상 체력 변화를 보여 주고, 행동 기록과 분석용 분기로 다른 수를 탐색할 수 있도록 했습니다.
- **로그라이트 파티 모험으로 확장할 계획입니다.** 종자의 성장과 프로모션, 체크메이트를 활용한 보스전, 런 진행과 성장 요소를 장기 방향으로 구상하고 있습니다.

### 출정식이 첫 튜토리얼인 이유

첫 목표는 왕 앞에 네 영웅을 세우는 것입니다. 플레이어는 각자의 행마를 사용해 지정된 네 칸에 고양이를 배치하며 자연스럽게 이동 규칙을 익힙니다. **모험을 시작하는 파티 결성의 순간이 곧 첫 조작 경험이 되도록** 출정식을 튜토리얼로 구성했습니다.

왕과 여왕은 이 장면에서 NPC로 등장하고, 플레이어는 전차·나이트·비숍 쌍둥이로 파티를 시작합니다. 킹은 상징성과 이후의 역할을 충분히 전달할 수 있는 시점에 소개하고자 했습니다. 또한 특정 행마로만 갈 수 있는 외딴 비밀칸과 도전과제를 향후 요소로 구상하고 있습니다. 이동 규칙을 익힌 플레이어가 호기심을 가지고 응용해 볼 수 있는 공간을 마련하려는 의도입니다.

현재 출정식에 사용하는 네 영웅의 프리셋 설정은 다음과 같습니다.

| 프리셋 | 역할 | 이동 설정 | 공격 설정 | 기본 HP |
| --- | --- | --- | --- | ---: |
| `Gunner` | 전차·룩 계열 포대 | 직선, `Charge`, 최대 2칸 | 직선, `Range`, 최대 5칸 | 5 |
| `Hero` | 나이트 계열 용사 | 나이트 행마, `Jump`, 범위 값 2 | 나이트 행마, `Jump`, 범위 값 2 | 10 |
| `Veni` | 붉은 비숍·쌍둥이 형 | 대각선, `Charge`, 최대 3칸 | 대각선, `Charge`, 최대 3칸 | 10 |
| `Teni` | 초록 비숍·쌍둥이 동생 | 대각선, `Jump`, 최대 3칸 | 대각선, `Jump`, 최대 3칸 | 8 |

나이트의 범위 값 2는 나이트 행마의 후보 칸을 생성하는 데 사용하는 설정값입니다. 설정 원본은 [캐릭터 프리셋 폴더](Assets/1.Datas/Origin/ScriptableObjects/Globals/Characters)에 있으며, 각 설정의 해석은 [ChessMovementModule](Assets/0.Scripts/Objects/Characters/CharacterModules/ChessModule/ChessMovementModule.cs)과 [TileManager](Assets/0.Scripts/Managers/TileManager.cs)에서 살펴보실 수 있습니다.

## 빠른 시작

### 준비

Unity 에디터에서 프로젝트를 열고 출정식 튜토리얼을 살펴보는 방법입니다.

| 항목 | 개발 환경 |
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

1. Unity Hub에서 저장소 루트를 프로젝트로 추가하고 **6000.3.5f2**로 열어 주세요.
2. 패키지 복원과 에셋 임포트가 끝날 때까지 기다려 주세요.
3. [Assets/6.Scenes/SampleScene.unity](Assets/6.Scenes/SampleScene.unity)를 열어 주세요. [빌드 씬 목록](ProjectSettings/EditorBuildSettings.asset)에 등록된 게임 진입 씬입니다.
4. Play를 누르고 로딩 완료 안내가 나오면 **아무 키나 누르거나 클릭**해 주세요. 입력을 받으면 배틀 화면으로 넘어갑니다.
5. Game 뷰에 포커스를 둔 채 **F8**을 눌러 출정식 데이터인 `TheBeginning.Json`을 불러와 주세요. 현재는 배틀 화면에 진입한 뒤 단축키로 맵을 불러오는 흐름입니다.
6. 대화는 **Space / Enter**로 넘길 수 있습니다. 목표 칸을 보여 주는 카메라 연출이 끝나면 고양이를 선택해 배치해 주세요. 타일 편집 창이 열려 있다면 **F9**로 닫을 수 있습니다.

출정식의 목표는 플레이어 소유 캐릭터로 `(4, 2)`, `(5, 2)`, `(7, 2)`, `(8, 2)`를 모두 채우는 것입니다. 화면 표기로는 **e3, f3, h3, i3**이며, 각 칸에 놓을 영웅은 자유롭게 선택할 수 있습니다.

현재 튜토리얼은 Firebase 로그인 없이 시작할 수 있습니다. Firebase SDK와 연결 코드는 포함되어 있으며, [DBManager.OnConnected](Assets/0.Scripts/Managers/DBManager.cs)의 초기화 호출은 비활성화해 두었습니다.

### 조작과 개발 단축키

게임 입력은 씬의 `PlayerInput`에 연결된 [GeneralKeyInput.inputactions](Assets/5.Inputs/GeneralKeyInput.inputactions)와 [InputManager](Assets/0.Scripts/Managers/InputManager.cs)에서 관리합니다. 조작을 수정하실 때에도 이 두 파일부터 살펴보시면 됩니다.

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

`F5`로 빠른 저장을 하면 개발용 저장 파일인 `Assets/Saves/Test.Json`을 덮어씁니다. 기존 데이터를 보관하고 싶다면 실행 전에 별도로 복사해 주세요. `I/P/O/N/M` 등 일부 메뉴 입력은 향후 연결을 위한 선언 단계입니다.

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

`Assets/Firebase`, `Assets/ExternalDependencyManager`, `Assets/Layer Lab`, `Assets/Plugins`, `Assets/TextMesh Pro`, `Assets/SampleTilesets`에는 외부 SDK·UI·셰이더·샘플 리소스가 있습니다. 직접 구현한 게임 로직과 콘텐츠 연결은 `0.Scripts`와 `1.Datas`부터 살펴보시면 됩니다. 외부 패키지에는 별도의 데모 씬도 포함되어 있습니다.

## 코드를 읽는 순서

게임의 시작부터 전투와 튜토리얼까지 흐름을 따라가실 수 있도록 주요 파일을 순서대로 정리했습니다.

| 순서 | 파일 | 살펴볼 내용 |
| ---: | --- | --- |
| 1 | [GameManager](Assets/0.Scripts/Managers/GameManager.cs), [ManagerBase](Assets/0.Scripts/Managers/ManagerBase.cs) | 매니저 생성과 초기화·업데이트 순서 |
| 2 | [DataManager](Assets/0.Scripts/Managers/DataManager.cs), [ObjectManager](Assets/0.Scripts/Managers/ObjectManager.cs) | 이름으로 데이터와 프리팹을 조회하고 생성하는 구조 |
| 3 | [SaveManager](Assets/0.Scripts/Managers/SaveManager.cs), [SaveStructures](Assets/0.Scripts/Managers/ManagerModules/SaveStructures.cs), [TheBeginning.Json](Assets/Saves/TheBeginning.Json) | 저장된 맵과 파티를 전투로 복원하는 과정 |
| 4 | [BattleManager](Assets/0.Scripts/Managers/BattleManager.cs), [PlayerController](Assets/0.Scripts/Objects/Controllers/PlayerController.cs) | 선택·미리보기·확정과 턴 요청의 연결 |
| 5 | [CharacterPreset](Assets/0.Scripts/ScriptableObjects/CharacterPreset.cs), [ChessMovementModule](Assets/0.Scripts/Objects/Characters/CharacterModules/ChessModule/ChessMovementModule.cs), [TileManager](Assets/0.Scripts/Managers/TileManager.cs) | 프리셋 설정을 바탕으로 이동·공격 후보를 계산하는 과정 |
| 6 | [TurnActionBuilder](Assets/0.Scripts/Turns/TurnActionBuilder.cs), [CharacterBaseAction](Assets/0.Scripts/Objects/Characters/CharacterModules/CharacterBaseAction.cs), [TurnActionInfo](Assets/0.Scripts/Turns/TurnActionInfo.cs), [TurnBaseInfo](Assets/0.Scripts/Turns/TurnBaseInfo.cs) | 행동의 생성·재생·복원과 기보 탐색 |
| 7 | [목표 코드](Assets/0.Scripts/ScriptableObjects/Objectives), [ChatEvents](Assets/0.Scripts/Managers/ManagerModules/ChatEvents.cs), [UI_ChatArea](Assets/0.Scripts/UIs/Functions/Informations/UI_ChatArea.cs) | 출정식의 대화·목표·완료 연출 연결 |
| 8 | [UIManager](Assets/0.Scripts/Managers/UIManager.cs), [UI 프리팹](Assets/1.Datas/Origin/Prefabs/Globals/UIs) | 코드와 Inspector 설정을 통한 화면 구성 |

전투 설계를 먼저 살펴보고 싶다면 `TurnActionBuilder.BuildActionArray()`부터 읽어 보시길 권합니다. 행동을 순서대로 임시 적용하고 역순으로 복원하는 과정에서, 미리보기와 실제 재생이 같은 행동 목록을 사용하는 구조를 보실 수 있습니다. 자세한 설명은 [행동과 기보](docs/EXPLORER_GUIDE.md#행동과-기보)로 이어집니다.

## 현재 구현 범위

전투 규칙과 이를 직접 실험할 수 있는 도구를 먼저 구축하고, 출정식에서 대화·목표·조작을 하나의 흐름으로 연결하고 있습니다. 현재 개발한 부분과 앞으로 이어갈 작업은 다음과 같습니다.

| 영역 | 개발 현황 |
| --- | --- |
| 격자와 행마 | 가변 크기 보드, 비어 있는 칸, 지형 데이터, 기물별 이동·공격 후보 계산 |
| 전투 행동 | 이동, 피해·회복 행동, 근접 밀치기·자리 이동, 원거리 피해, 퇴장·복원 |
| 결과 표시와 분석 | 예상 HP 변화, 행동 기록, 앞뒤 이동, 과거 위치에서 별도 분기 실행 |
| 출정식 | 네 영웅 배치, 시작·완료 대화, 목표 타일 표시·카메라 연출, 결과 화면 활성화 |
| AI | 공격 후보가 있으면 그중 무작위 선택, 없으면 이동 후보 중 무작위 선택 |
| 종자 | 주인과 종자의 관계, `masterInfo` / `pawnInfo`, 생성·저장 구조를 마련했습니다. 성장과 프로모션은 향후 구현할 부분입니다. |
| 인벤토리 | 슬롯·스택·교환·분할·병합·정렬과 UI 기반을 구현했습니다. 아이템 사용·장착 효과와 일부 API는 아직 구현 전입니다. |
| 콘텐츠 도구 | 실행 중 타일 편집, JSON 저장·로드, 프리셋·대화·목표의 ScriptableObject 작성 |
| 서비스와 메뉴 | Firebase 연결 코드는 초기화가 비활성화된 상태입니다. 오디오·언어 매니저, 일부 메뉴와 퍽 클래스는 기본 구조를 마련한 단계입니다. |
| 향후 확장 | 로그라이트 런·보상·성장, 체크메이트 보스 페이즈, 종자 성장·프로모션, 비밀칸 도전과제를 기획하고 있습니다. |

현재 턴은 행동 재생이 끝난 뒤 `BattleManager.TurnEnd()`에서 다음 컨트롤러로 넘어갑니다. 파티 전체 행동을 단위로 하는 턴과 보스 전용 추가 턴은 이후 확장할 계획입니다. `ClassicChess.Json`은 기본 배치를 시험하기 위한 데이터로 사용하고 있습니다. 정통 체스의 전체 특수 규칙과 체크메이트 판정까지 지원하려면 추가 구현이 필요합니다.

## 탐색 중 확인할 것

| 상황 | 안내 |
| --- | --- |
| 로딩 완료 후 입력 대기 | Game 뷰에 포커스를 두고 아무 키나 누르거나 클릭해 주세요. `UI_LoadingScreen.SetComplete()`에서 입력을 기다립니다. |
| 배틀 화면에서 맵 불러오기 | `F8`로 출정식 데이터를 불러올 수 있습니다. 화면 전환과 스테이지 로드는 별도로 처리합니다. |
| 고양이 조작이 잠긴 경우 | 대화·목표 소개 연출, 일시정지, 타일 편집 창과 UI 위의 커서 상태를 확인해 주세요. |
| 데이터·프리팹 이름 연결 | Addressables의 `Global` 라벨과 로드된 타입, 에셋 이름, `PoolRequest`의 `poolName`을 확인해 주세요. [이름으로 연결되는 데이터](docs/EXPLORER_GUIDE.md#이름으로-연결되는-데이터)에 연결 방식을 정리했습니다. |
| `F8`의 로드 대상 | `SaveManager.LoadCliffChess()`는 현재 출정식 데이터인 `TheBeginning.Json`을 읽습니다. |
| 공격력 조정 | 현재 `CharacterBase.GetAttackDamage()`는 프리팹의 `baseDamage`를 사용합니다. 피해를 조정하려면 이 값을 함께 확인해 주세요. |
| 독립 실행 파일 빌드 | 에디터 실행을 중심으로 개발하고 있습니다. 플레이어 빌드에는 `CameraManager`·`TileManager`의 `UnityEditor` 참조 분리와 `Application.dataPath/Saves`를 사용하는 JSON 배포·저장 경로 정리가 필요합니다. |

동작을 수정한 뒤 살펴볼 플레이 흐름과 저장·복원 시 고려할 부분은 [개발자 탐색 가이드](docs/EXPLORER_GUIDE.md#변경-후-확인할-경로)에 정리했습니다. 프로젝트 전용 자동화 테스트와 CI는 아직 구축 전입니다.

문서는 기획과 구현 내용을 바탕으로 AI의 도움을 받아 정리했습니다.
