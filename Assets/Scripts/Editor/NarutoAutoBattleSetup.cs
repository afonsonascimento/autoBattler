#if UNITY_EDITOR
using NarutoAutoBattle.AI;
using NarutoAutoBattle.Board;
using NarutoAutoBattle.Combat;
using NarutoAutoBattle.Core;
using NarutoAutoBattle.Economy;
using NarutoAutoBattle.Input;
using NarutoAutoBattle.Synergies;
using NarutoAutoBattle.Units;
using NarutoAutoBattle.View;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NarutoAutoBattle.Editor
{
    public static class NarutoAutoBattleSetup
    {
        const string UnitDataPath = "Assets/ScriptableObjects/Units";
        const string ClanDataPath = "Assets/ScriptableObjects/Clans";
        const string EconomyRulesPath = "Assets/ScriptableObjects/Economy/EconomyRules.asset";
        const string PrefabPath = "Assets/Prefabs/Units";
        const string ScenePath = "Assets/Scenes/Main.unity";

        [MenuItem("Naruto Auto Battle/Setup MVP Scene")]
        public static void SetupMvpScene()
        {
            EnsureFolders();
            var clans = CreateClanAssets();
            var unitData = CreateUnitDataAssets(clans);
            var economyRules = CreateEconomyRulesAsset();
            var unitPrefab = CreateUnitPrefab();
            var combatPrefab = CreateCombatUnitPrefab();
            SetupScene(unitPrefab, combatPrefab, unitData, economyRules);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(ScenePath);
            Debug.Log("Naruto Auto Battle MVP scene setup complete.");
        }

        static void EnsureFolders()
        {
            CreateFolder("Assets/ScriptableObjects");
            CreateFolder(UnitDataPath);
            CreateFolder(ClanDataPath);
            CreateFolder("Assets/ScriptableObjects/Economy");
            CreateFolder("Assets/Prefabs");
            CreateFolder(PrefabPath);
        }

        static void CreateFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            var parts = path.Split('/');
            var current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        static ClanData[] CreateClanAssets()
        {
            return new[]
            {
                CreateClan("Konoha", "konoha", "Konoha", new Color(0.2f, 0.65f, 0.3f),
                    new ClanData.Threshold { unitCount = 2, attackBonus = 0f, healthBonus = 0.15f },
                    new ClanData.Threshold { unitCount = 4, attackBonus = 0f, healthBonus = 0.30f }),
                CreateClan("Team7", "team7", "Team 7", new Color(1f, 0.6f, 0.1f),
                    new ClanData.Threshold { unitCount = 2, attackBonus = 0.20f, healthBonus = 0f },
                    new ClanData.Threshold { unitCount = 3, attackBonus = 0.35f, healthBonus = 0f }),
                CreateClan("Uchiha", "uchiha", "Uchiha", new Color(0.85f, 0.15f, 0.15f),
                    new ClanData.Threshold { unitCount = 1, attackBonus = 0.25f, healthBonus = 0f },
                    new ClanData.Threshold { unitCount = 2, attackBonus = 0.50f, healthBonus = 0f }),
                CreateClan("Hyuga", "hyuga", "Hyuga", new Color(0.85f, 0.85f, 1f),
                    new ClanData.Threshold { unitCount = 2, attackBonus = 0.15f, healthBonus = 0f },
                    new ClanData.Threshold { unitCount = 3, attackBonus = 0.25f, healthBonus = 0f }),
                CreateClan("Taijutsu", "taijutsu", "Taijutsu", new Color(0.2f, 0.75f, 0.35f),
                    new ClanData.Threshold { unitCount = 2, attackBonus = 0.20f, healthBonus = 0f },
                    new ClanData.Threshold { unitCount = 3, attackBonus = 0.35f, healthBonus = 0f }),
                CreateClan("Nara", "nara", "Nara", new Color(0.55f, 0.35f, 0.15f),
                    new ClanData.Threshold { unitCount = 2, attackBonus = 0.10f, healthBonus = 0.10f },
                    new ClanData.Threshold { unitCount = 3, attackBonus = 0.20f, healthBonus = 0.20f }),
                CreateClan("Akatsuki", "akatsuki", "Akatsuki", new Color(0.95f, 0.1f, 0.2f),
                    new ClanData.Threshold { unitCount = 2, attackBonus = 0.25f, healthBonus = 0f },
                    new ClanData.Threshold { unitCount = 3, attackBonus = 0.45f, healthBonus = 0f }),
                CreateClan("Sannin", "sannin", "Sannin", new Color(0.85f, 0.45f, 0.1f),
                    new ClanData.Threshold { unitCount = 2, attackBonus = 0.10f, healthBonus = 0.15f },
                    new ClanData.Threshold { unitCount = 3, attackBonus = 0.20f, healthBonus = 0.25f }),
                CreateClan("Jinchuriki", "jinchuriki", "Jinchuriki", new Color(0.75f, 0.2f, 0.85f),
                    new ClanData.Threshold { unitCount = 2, attackBonus = 0f, healthBonus = 0.20f },
                    new ClanData.Threshold { unitCount = 3, attackBonus = 0.10f, healthBonus = 0.35f }),
                CreateClan("InoShikaCho", "inoshikacho", "Ino-Shika-Cho", new Color(0.9f, 0.75f, 0.2f),
                    new ClanData.Threshold { unitCount = 2, attackBonus = 0.15f, healthBonus = 0f },
                    new ClanData.Threshold { unitCount = 3, attackBonus = 0.25f, healthBonus = 0.10f })
            };
        }

        static ClanData CreateClan(string fileName, string id, string displayName, Color color, params ClanData.Threshold[] thresholds)
        {
            var path = $"{ClanDataPath}/{fileName}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<ClanData>(path);
            if (existing != null)
            {
                existing.clanId = id;
                existing.displayName = displayName;
                existing.clanColor = color;
                existing.thresholds = thresholds;
                EditorUtility.SetDirty(existing);
                return existing;
            }

            var data = ScriptableObject.CreateInstance<ClanData>();
            data.clanId = id;
            data.displayName = displayName;
            data.clanColor = color;
            data.thresholds = thresholds;
            AssetDatabase.CreateAsset(data, path);
            return data;
        }

        static UnitData[] CreateUnitDataAssets(ClanData[] clans)
        {
            var konoha = clans[0];
            var team7 = clans[1];
            var uchiha = clans[2];
            var hyuga = clans[3];
            var taijutsu = clans[4];
            var nara = clans[5];
            var akatsuki = clans[6];
            var sannin = clans[7];
            var jinchuriki = clans[8];
            var inoShikaCho = clans[9];

            return new[]
            {
                // 1-cost
                CreateUnitWithJutsu("Naruto", "naruto", 1, 80, 12, 1.5f, 1f, 3f, new Color(1f, 0.55f, 0.1f),
                    JutsuType.Rasengan, "Rasengan", 4f, 1.4f, 2.5f, konoha, team7, jinchuriki),
                CreateUnitWithJutsu("Rock Lee", "rocklee", 1, 70, 14, 1.2f, 1.8f, 4.5f, new Color(0.15f, 0.55f, 0.2f),
                    JutsuType.Chidori, "Primary Lotus", 5f, 2.8f, 1.2f, taijutsu, konoha),
                CreateUnitWithJutsu("Tenten", "tenten", 1, 55, 13, 3f, 1.1f, 3f, new Color(0.85f, 0.35f, 0.35f),
                    JutsuType.LightningBlade, "Weapon Barrage", 4f, 1.6f, 3f, konoha),

                // 2-cost
                CreateUnitWithJutsu("Sakura", "sakura", 2, 50, 16, 1.5f, 1.5f, 4f, new Color(1f, 0.4f, 0.6f),
                    JutsuType.MysticalPalm, "Mystical Palm", 5f, 1.2f, 0f, konoha, team7),
                CreateUnitWithJutsu("Neji", "neji", 2, 58, 17, 2f, 1.1f, 3f, new Color(0.9f, 0.9f, 1f),
                    JutsuType.Rasengan, "Eight Trigrams", 4f, 1.3f, 2f, hyuga, konoha),
                CreateUnitWithJutsu("Hinata", "hinata", 2, 52, 15, 2f, 1f, 3f, new Color(0.75f, 0.65f, 1f),
                    JutsuType.Chidori, "Gentle Step", 4.5f, 2f, 1.5f, hyuga, konoha),
                CreateUnitWithJutsu("Kiba", "kiba", 2, 55, 16, 1.5f, 1.4f, 4.2f, new Color(0.6f, 0.45f, 0.25f),
                    JutsuType.Chidori, "Fang Over Fang", 4f, 2.2f, 1.2f, konoha),
                CreateUnitWithJutsu("Ino", "ino", 2, 48, 14, 3.5f, 0.9f, 2.8f, new Color(0.95f, 0.75f, 0.85f),
                    JutsuType.LightningBlade, "Mind Disturbance", 5f, 1.4f, 3f, inoShikaCho, konoha),
                CreateUnitWithJutsu("Shikamaru", "shikamaru", 2, 50, 13, 4f, 0.7f, 2.5f, new Color(0.45f, 0.35f, 0.2f),
                    JutsuType.LightningBlade, "Shadow Strangle", 5f, 1.5f, 3.5f, nara, inoShikaCho, konoha),

                // 3-cost
                CreateUnitWithJutsu("Sasuke", "sasuke", 3, 55, 20, 4f, 1.2f, 3f, new Color(0.2f, 0.25f, 0.7f),
                    JutsuType.Chidori, "Chidori", 3.5f, 2.5f, 1.5f, konoha, uchiha, team7),
                CreateUnitWithJutsu("Kakashi", "kakashi", 3, 60, 19, 3f, 0.9f, 2.8f, new Color(0.75f, 0.75f, 0.75f),
                    JutsuType.LightningBlade, "Lightning Blade", 4.5f, 1.8f, 4f, konoha, team7),
                CreateUnitWithJutsu("Choji", "choji", 3, 75, 14, 1.2f, 0.8f, 2.5f, new Color(0.95f, 0.7f, 0.3f),
                    JutsuType.Rasengan, "Human Boulder", 5f, 1.5f, 2.5f, inoShikaCho, konoha),

                // 4-cost
                CreateUnitWithJutsu("Jiraiya", "jiraiya", 4, 85, 22, 2f, 0.9f, 3f, new Color(0.95f, 0.55f, 0.15f),
                    JutsuType.Rasengan, "Sage Art Rasengan", 4f, 1.8f, 3f, sannin, konoha),
                CreateUnitWithJutsu("Tsunade", "tsunade", 4, 95, 18, 1.5f, 0.85f, 2.8f, new Color(0.55f, 0.35f, 0.55f),
                    JutsuType.MysticalPalm, "Creation Rebirth", 5f, 1.5f, 0f, sannin, konoha),
                CreateUnitWithJutsu("Itachi", "itachi", 4, 65, 26, 4f, 1f, 3f, new Color(0.55f, 0.1f, 0.15f),
                    JutsuType.LightningBlade, "Amaterasu", 4.5f, 2.2f, 3.5f, akatsuki, uchiha),
                CreateUnitWithJutsu("Gaara", "gaara", 4, 90, 20, 3.5f, 0.75f, 2.5f, new Color(0.85f, 0.75f, 0.25f),
                    JutsuType.Rasengan, "Sand Burial", 5f, 1.7f, 3f, jinchuriki)
            };
        }

        static UnitData CreateUnitWithJutsu(string name, string id, int cost, int hp, int atk,
            float range, float atkSpeed, float moveSpeed, Color color,
            JutsuType jutsuType, string jutsuName, float jutsuCooldown, float jutsuPower, float jutsuRadius,
            params ClanData[] clans)
        {
            var data = CreateUnitData(name, id, cost, hp, atk, range, atkSpeed, moveSpeed, color, clans);
            data.jutsuType = jutsuType;
            data.jutsuName = jutsuName;
            data.jutsuCooldown = jutsuCooldown;
            data.jutsuPower = jutsuPower;
            data.jutsuRadius = jutsuRadius;
            EditorUtility.SetDirty(data);
            return data;
        }

        static UnitData CreateUnitData(string name, string id, int cost, int hp, int atk,
            float range, float atkSpeed, float moveSpeed, Color color, params ClanData[] clans)
        {
            var path = $"{UnitDataPath}/{name}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<UnitData>(path);
            if (existing != null)
            {
                existing.unitId = id;
                existing.displayName = name;
                existing.cost = cost;
                existing.baseHealth = hp;
                existing.baseAttack = atk;
                existing.attackRange = range;
                existing.attackSpeed = atkSpeed;
                existing.moveSpeed = moveSpeed;
                existing.unitColor = color;
                existing.clans = clans;
                EditorUtility.SetDirty(existing);
                return existing;
            }

            var data = ScriptableObject.CreateInstance<UnitData>();
            data.unitId = id;
            data.displayName = name;
            data.cost = cost;
            data.baseHealth = hp;
            data.baseAttack = atk;
            data.attackRange = range;
            data.attackSpeed = atkSpeed;
            data.moveSpeed = moveSpeed;
            data.unitColor = color;
            data.clans = clans;
            AssetDatabase.CreateAsset(data, path);
            return data;
        }

        static GameObject CreateUnitPrefab()
        {
            var path = $"{PrefabPath}/Unit.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
                return existing;

            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "Unit";
            Object.DestroyImmediate(go.GetComponent<CapsuleCollider>());

            var col = go.AddComponent<CapsuleCollider>();
            col.height = 2f;
            col.radius = 0.4f;

            go.AddComponent<Unit>();

            var star = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            star.name = "StarIndicator";
            star.transform.SetParent(go.transform);
            star.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            star.transform.localScale = Vector3.one * 0.3f;
            Object.DestroyImmediate(star.GetComponent<SphereCollider>());

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        static GameObject CreateCombatUnitPrefab()
        {
            var path = $"{PrefabPath}/CombatUnit.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
                return existing;

            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "CombatUnit";
            Object.DestroyImmediate(go.GetComponent<CapsuleCollider>());
            go.AddComponent<Health>();
            go.AddComponent<CombatUnit>();
            go.AddComponent<CombatFeedback>();
            go.AddComponent<WorldHealthBar>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        static EconomyRules CreateEconomyRulesAsset()
        {
            var existing = AssetDatabase.LoadAssetAtPath<EconomyRules>(EconomyRulesPath);
            if (existing != null)
            {
                ApplyDefaultEconomyRules(existing);
                EditorUtility.SetDirty(existing);
                return existing;
            }

            var rules = ScriptableObject.CreateInstance<EconomyRules>();
            AssetDatabase.CreateAsset(rules, EconomyRulesPath);
            return rules;
        }

        static void ApplyDefaultEconomyRules(EconomyRules rules)
        {
            var defaults = ScriptableObject.CreateInstance<EconomyRules>();
            rules.totalXpForLevel = (int[])defaults.totalXpForLevel.Clone();
            rules.shopOddsByLevel = new EconomyRules.ShopOdds[defaults.shopOddsByLevel.Length];
            for (int i = 0; i < defaults.shopOddsByLevel.Length; i++)
                rules.shopOddsByLevel[i] = defaults.shopOddsByLevel[i];
            Object.DestroyImmediate(defaults);
        }

        static void SetupScene(GameObject unitPrefab, GameObject combatPrefab, UnitData[] unitData, EconomyRules economyRules)
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            var cam = Camera.main;
            if (cam != null)
            {
                cam.transform.position = new Vector3(0f, 12f, -4f);
                cam.transform.rotation = Quaternion.Euler(55f, 0f, 0f);
            }

            var boardGo = GetOrCreate("Board");
            var boardGrid = GetOrAdd<BoardGrid>(boardGo);

            var benchGo = GetOrCreate("Bench");
            var benchGrid = GetOrAdd<BenchGrid>(benchGo);

            var systemsGo = GetOrCreate("GameSystems");
            var gameManager = GetOrAdd<GameManager>(systemsGo);
            var roundManager = GetOrAdd<RoundManager>(systemsGo);
            var playerGold = GetOrAdd<PlayerGold>(systemsGo);
            var playerEconomy = GetOrAdd<PlayerEconomy>(systemsGo);
            var shopService = GetOrAdd<ShopService>(systemsGo);
            var sellService = GetOrAdd<UnitSellService>(systemsGo);
            var combatController = GetOrAdd<CombatController>(systemsGo);
            var mergeService = GetOrAdd<UnitMergeService>(systemsGo);
            var unitSpawner = GetOrAdd<UnitSpawner>(systemsGo);
            var enemyGen = GetOrAdd<EnemyBoardGenerator>(systemsGo);
            var dragController = GetOrAdd<UnitDragController>(systemsGo);
            var synergyService = GetOrAdd<SynergyService>(systemsGo);
            var ui = GetOrAdd<GameUI>(systemsGo);

            SetRef(unitSpawner, "unitPrefab", unitPrefab);
            SetRef(mergeService, "boardGrid", boardGrid);
            SetRef(mergeService, "benchGrid", benchGrid);
            SetRef(shopService, "unitPool", unitData);
            SetRef(shopService, "unitSpawner", unitSpawner);
            SetRef(shopService, "benchGrid", benchGrid);
            SetRef(shopService, "playerGold", playerGold);
            SetRef(shopService, "playerEconomy", playerEconomy);
            SetRef(playerEconomy, "playerGold", playerGold);
            SetRef(playerEconomy, "rules", economyRules);
            SetRef(shopService, "playerEconomy", playerEconomy);
            SetRef(sellService, "playerGold", playerGold);
            SetRef(sellService, "gameManager", gameManager);
            SetRef(combatController, "boardGrid", boardGrid);
            SetRef(combatController, "combatUnitPrefab", combatPrefab);
            SetRef(combatController, "synergyService", synergyService);
            SetRef(roundManager, "boardGrid", boardGrid);
            SetRef(roundManager, "playerGold", playerGold);
            SetRef(roundManager, "playerEconomy", playerEconomy);
            SetRef(roundManager, "shopService", shopService);
            SetRef(roundManager, "enemyBoardGenerator", enemyGen);
            SetRef(enemyGen, "boardGrid", boardGrid);
            SetRef(enemyGen, "unitSpawner", unitSpawner);
            SetRef(enemyGen, "unitPool", unitData);
            SetRef(gameManager, "boardGrid", boardGrid);
            SetRef(gameManager, "benchGrid", benchGrid);
            SetRef(gameManager, "playerGold", playerGold);
            SetRef(gameManager, "shopService", shopService);
            SetRef(gameManager, "combatController", combatController);
            SetRef(gameManager, "roundManager", roundManager);
            SetRef(gameManager, "dragController", dragController);
            SetRef(gameManager, "enemyBoardGenerator", enemyGen);
            SetRef(dragController, "mainCamera", cam);
            SetRef(dragController, "boardGrid", boardGrid);
            SetRef(dragController, "benchGrid", benchGrid);
            SetRef(dragController, "sellService", sellService);
            SetRef(ui, "gameManager", gameManager);
            SetRef(ui, "shopService", shopService);
            SetRef(ui, "playerGold", playerGold);
            SetRef(ui, "playerEconomy", playerEconomy);
            SetRef(ui, "synergyService", synergyService);
            SetRef(ui, "dragController", dragController);
            SetRef(synergyService, "boardGrid", boardGrid);
            SetRef(synergyService, "benchGrid", benchGrid);

            CreateFloor();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        static void CreateFloor()
        {
            var floor = GetOrCreate("Floor");
            var meshFilter = GetOrAdd<MeshFilter>(floor);
            var meshRenderer = GetOrAdd<MeshRenderer>(floor);

            if (meshFilter.sharedMesh == null)
            {
                var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                meshFilter.sharedMesh = plane.GetComponent<MeshFilter>().sharedMesh;
                Object.DestroyImmediate(plane);
            }

            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(2f, 1f, 2f);

            var mat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat");
            if (mat != null)
                meshRenderer.sharedMaterial = mat;
        }

        static GameObject GetOrCreate(string name, Transform parent = null)
        {
            Transform search = parent != null ? parent.Find(name) : GameObject.Find(name)?.transform;
            if (search != null)
                return search.gameObject;

            var go = new GameObject(name);
            if (parent != null)
                go.transform.SetParent(parent, false);
            return go;
        }

        static T GetOrAdd<T>(GameObject go) where T : Component
        {
            return go.GetComponent<T>() ?? go.AddComponent<T>();
        }

        static void SetRef(Object target, string fieldName, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop != null)
            {
                prop.objectReferenceValue = value;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        static void SetRef(Object target, string fieldName, Object[] values)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogWarning($"Could not find array property '{fieldName}' on {target.name}");
                return;
            }

            prop.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }
    }
}
#endif
