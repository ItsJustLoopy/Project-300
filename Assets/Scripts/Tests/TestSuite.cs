// using System.Collections;
// using NUnit.Framework;
// using UnityEngine;
// using UnityEngine.TestTools;

// public class TestSuite
// {
//     private GameObject _go;
//     private LevelManager _manager;

//     private GameObject CreateCamera()
//     {
//         var camGo = new GameObject("MainCamera");
//         camGo.AddComponent<Camera>();
//         return camGo;
//     }

//     [UnitySetUp]
//     public IEnumerator Setup()
//     {
//         _go = new GameObject("LevelManager");
//         _manager = _go.AddComponent<LevelManager>();


//         _manager.mainCamera = CreateCamera();


//         _manager.levelDatas = new LevelData[0];
//         _manager.groundTilePrefab = null;

//         yield return null; // let Awake run
//     }

//     [UnityTearDown]
//     public IEnumerator TearDown()
//     {
//         Object.Destroy(_go);
//         yield return null;
//     }



//     [Test]
//     public void Awake_SetsSingletonInstance()
//     {
//         Assert.AreEqual(_manager, LevelManager.Instance);
//     }

//     [Test]
//     public void Awake_InitializesSubsystems()
//     {
//         Assert.IsNotNull(_manager.loader);
//         Assert.IsNotNull(_manager.visuals);
//         Assert.IsNotNull(_manager.undo);
//         Assert.IsNotNull(_manager.elevators);
//     }

//     [Test]
//     public void Awake_CreatesDebugCollector()
//     {
//         Assert.IsNotNull(_manager.DebugCollector);
//     }


//     [Test]
//     public void GenerateLevel_DoesNotThrow()
//     {
//         Assert.DoesNotThrow(() =>
//         {
//             _manager.GenerateLevel(0);
//         });
//     }

//     [Test]
//     public void ManageLoadedLevels_DoesNotThrow()
//     {
//         Assert.DoesNotThrow(() =>
//         {
//             _manager.ManageLoadedLevels();
//         });
//     }

//     [Test]
//     public void UpdateLevelOpacities_DoesNotThrow()
//     {
//         Assert.DoesNotThrow(() =>
//         {
//             _manager.UpdateLevelOpacities();
//         });
//     }

//     [Test]
//     public void SpawnPlayer_DoesNotThrow()
//     {
//         Assert.DoesNotThrow(() =>
//         {
//             _manager.SpawnPlayer();
//         });
//     }
// }
