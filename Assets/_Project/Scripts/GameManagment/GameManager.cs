using System;
using System.Collections.Generic;
using Entities;
using Entities.CameraControl;
using Entities.Controller;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils.Checkpoint;
using Utils.SceneLoader;

namespace Managers
{
    public enum GameState : byte
    {
        Running,
        Paused,
        GameOver
    }

    [System.Serializable]
    public struct DebugStats
    {
        [SerializeField] private bool isActive;
        [SerializeField] private float timeScale;

        public bool IsActive => isActive;
        public float TimeScale => timeScale;
    }

    public class GameManager : MonoBehaviour
    {
        private static GameManager gameManager;
        public static GameManager Instance => gameManager;

        private GameState currentGameState = GameState.Running;
        public GameState CurrentGameState => currentGameState;
        public Action<GameState> OnGameStateChanged;

        private SceneLoader sceneLoader;
        [SerializeField] private PauseMenuUi pauseMenuUi;
        [SerializeField] private GameOverMenuUi gameOverMenuUi;
        [SerializeField] private PlayerUiHandler playerUiHandler;
        [SerializeField] private LayerMask playerSpawnLayerMask;

        private List<BaseEntity> activeEntities = new();
        private List<BaseEntity> disabledEntities = new();
        private HashSet<BaseEntity> entitiesChangedQueue = new();
        private PlayerController playerController;
        private CameraController cameraController;
        private PlayerSpawner playerSpawner;

        private int timeDirection = 0;
        private float objectsGameSpeed = 1;

        [SerializeField] private DebugStats debugStats;
        [SerializeField] private float objectsGameSpeedChangeRate = 2f;
        [SerializeField] private float maxObjectsGameSlowDown = 0.3f;
        [SerializeField] private float maxObjectsGameSpeedUp = 1.7f;


        public float ObjectsGameSpeed => objectsGameSpeed;
        public float MaxObjectsGameSlowDown => maxObjectsGameSlowDown;
        public float MaxObjectsGameSpeedUp => maxObjectsGameSpeedUp;


        public PlayerController PlayerController => playerController;
        public PlayerUiHandler PlayerUiHandler
        {
            get
            {
                if (playerUiHandler == null)
                {
                    playerUiHandler = FindAnyObjectByType<PlayerUiHandler>(FindObjectsInactive.Include);
                }
                return playerUiHandler;
            }
        }

        public void InitPlayerUi(PlayerController inPlayerController)
        {
            if (PlayerUiHandler == null)
            {
                Debug.LogError($"PlayerUiHandler is null can run initialize");
                return;
            }
            PlayerUiHandler.OnInitialize(inPlayerController);
        }

        public void ChangeScene(ref SceneData sceneData)
        {
            sceneLoader.LoadScene(sceneData);
        }

        public void LoadThisScene()
        {
            sceneLoader.LoadScene(sceneLoader.CurrentDataScene);
        }

        private void Awake()
        {
            if (GameManager.Instance == null)
            {
                gameManager = this;
                if (transform.parent != null)
                {
                    transform.SetParent(null, true);
                }
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(this);
            }

            sceneLoader = new(new SceneData(SceneManager.GetActiveScene().buildIndex, SceneManager.GetActiveScene().name));

            sceneLoader.OnFinishedLoading += OnSceneFinnishLoading;
        }

        private void OnValidate()
        {
            if (debugStats.IsActive)
            {
                Time.timeScale = debugStats.TimeScale;
            }
            else
            {
                Time.timeScale = 1f;
            }
        }

        private void Start()
        {
            SceneSetUp();
        }

        private void Update()
        {
            // Change objects game speed depending on player input 
            if (timeDirection != 0)
            {
                if (timeDirection == 1 && objectsGameSpeed >= maxObjectsGameSpeedUp)
                {
                    objectsGameSpeed = maxObjectsGameSpeedUp;
                }
                else if (timeDirection == -1 && objectsGameSpeed <= maxObjectsGameSlowDown)
                {
                    objectsGameSpeed = maxObjectsGameSlowDown;
                }
                else
                {
                    objectsGameSpeed += Time.deltaTime * objectsGameSpeedChangeRate * timeDirection;
                }
            }
            else if (objectsGameSpeed != 1f)
            {
                objectsGameSpeed += Time.deltaTime * objectsGameSpeedChangeRate * (objectsGameSpeed < 0.99f ? 1 : -1);
                if (objectsGameSpeed >= 0.95f && objectsGameSpeed <= 1.05f)
                {
                    objectsGameSpeed = 1f;

                }
            }


            foreach (var entity in activeEntities)
            {
                entity.OnUpdate();
            }

            CleanUp();
        }

        private void FixedUpdate()
        {
            foreach (var entity in activeEntities)
            {
                entity.OnFixedUpdate();
            }
        }

        private void OnSceneFinnishLoading(bool couldLoad)
        {
            if (!couldLoad)
            {
                Debug.LogWarning("What do you mean could not load scene 💀💀💀");
                return;
            }

            SceneSetUp();
        }

        private void SceneSetUp()
        {
            activeEntities = new();
            disabledEntities = new();
            entitiesChangedQueue = new();

            BaseEntity[] allEntities = FindObjectsByType<BaseEntity>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            playerSpawner = FindAnyObjectByType<PlayerSpawner>(FindObjectsInactive.Include);

            // this can maybe break stuff in the future, scene can have object controlling this 
            ChangeGameState(GameState.Running);

            // find specific components
            for (int i = 0; i < allEntities.Length; i++)
            {
                BaseEntity baseEntity = allEntities[i];

                if (baseEntity is PlayerController)
                {
                    playerController = baseEntity as PlayerController;
                    playerController.OnDeath += OnPlayerDeath;


                }
                else if (baseEntity is CameraController)
                {
                    cameraController = baseEntity as CameraController;
                }

            }

            if (playerSpawner == null)
            {
                GameObject tmpGameObject = new GameObject("PlayerSpawner");
                tmpGameObject.transform.position = playerController.GetTransform.position;

                if (Physics.Raycast(tmpGameObject.transform.position, Vector3.down, out RaycastHit hit, 1000, playerSpawnLayerMask))
                {
                    tmpGameObject.transform.position = hit.point;
                }

                playerSpawner = tmpGameObject.AddComponent<PlayerSpawner>();
            }

            // add and init
            foreach (var baseEntity in allEntities)
            {
                if (baseEntity == null)
                {
                    Debug.Log(baseEntity);
                }
                AddEntity(baseEntity);
            }

            if (playerController != null && cameraController != null)
            {
                playerController.SetCameraController(cameraController);
            }
        }

        public void ChangeGameState(GameState newGameState)
        {
            if (newGameState == currentGameState)
            {
                return;
            }

            currentGameState = newGameState;

            switch (currentGameState)
            {
                case GameState.Running:
                    enabled = true;

                    if (playerController != null)
                    {
                        Cursor.lockState = CursorLockMode.Locked;
                    }

                    ChangePauseMenuState(false);

                    break;

                case GameState.Paused:
                    enabled = false;
                    Cursor.lockState = CursorLockMode.Confined;
                    Cursor.visible = true;
                    ChangePauseMenuState(true);

                    break;

                case GameState.GameOver:
                    enabled = false;
                    Cursor.lockState = CursorLockMode.Confined;
                    Cursor.visible = true;
                    ChangeGameOverScreen(true);

                    break;
            }

            OnGameStateChanged?.Invoke(currentGameState);
        }


        private void ActOnOldEntityState(BaseEntity inEntity)
        {
            // Act on old state
            switch (inEntity.OldEntityState)
            {
                case EntityState.Active:
                    activeEntities.Remove(inEntity);
                    break;
                case EntityState.Disabled:
                    disabledEntities.Remove(inEntity);
                    break;
            }
        }

        private void ActOnEntityState(BaseEntity inEntity)
        {
            // Act on current state
            switch (inEntity.EntityState)
            {
                case EntityState.Active:
                    activeEntities.Add(inEntity);
                    break;
                case EntityState.Disabled:
                    inEntity.OnBeingDisable();
                    disabledEntities.Add(inEntity);
                    break;
                case EntityState.ToDestroy:
                    inEntity.OnEntityDestroy += OnEntityDestroy;
                    inEntity.OnBeforeDestroy();
                    break;
            }
        }

        public void AddEntity(BaseEntity newEntity)
        {
            newEntity.OnEntityStateChanged += OnEntityChangedState;
            newEntity.OnInitialize();
            ActOnEntityState(newEntity);
        }

        private void OnEntityChangedState(BaseEntity inBaseEntity)
        {
            entitiesChangedQueue.Add(inBaseEntity);
        }

        private void OnEntityDestroy(BaseEntity inBaseEntity)
        {
            inBaseEntity.OnEntityDestroy = null;
            inBaseEntity.OnEntityStateChanged = null;
            Destroy(inBaseEntity.gameObject);
        }

        private void CleanUp()
        {
            if (entitiesChangedQueue.Count <= 0)
            {
                return;
            }

            foreach (var entity in entitiesChangedQueue)
            {
                ActOnOldEntityState(entity);
                ActOnEntityState(entity);
            }

            entitiesChangedQueue.Clear();
        }

        private void ChangePauseMenuState(bool state)
        {
            if (pauseMenuUi == null)
            {
                pauseMenuUi = FindAnyObjectByType<PauseMenuUi>(FindObjectsInactive.Include);
                if (pauseMenuUi == null)
                {
                    return;
                }
            }

            pauseMenuUi.ChangeActiveState(state);
        }

        private void ChangeGameOverScreen(bool state)
        {
            if (gameOverMenuUi == null)
            {
                gameOverMenuUi = FindAnyObjectByType<GameOverMenuUi>(FindObjectsInactive.Include);
                if (gameOverMenuUi == null)
                {
                    return;
                }
            }

            gameOverMenuUi.ChangeActiveState(state);

        }

        public void RespawnPlayer(RespawnType inRespawnType)
        {
            if (playerSpawner == null) { Debug.LogError("playerSpawner is null"); return; }

            switch (inRespawnType)
            {
                case RespawnType.FullRespawn:
                    if (playerSpawner != null)
                    {
                        playerSpawner.FullSpawnPlayer(playerController);
                    }
                    break;
                case RespawnType.CheckpointRespawn:
                    playerController.CheckPoint.SpawnPlayer(ref playerController);
                    break;
            }

        }

        public void ChangeTimeSpeed(int inTimeDirection)
        {
            if (timeDirection == inTimeDirection)
            {
                timeDirection = 0;
                return;
            }


            timeDirection = inTimeDirection;
        }

        private void OnPlayerDeath()
        {
            ChangeGameState(GameState.GameOver);
        }
    }

}