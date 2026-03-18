using System.Collections.Generic;
using Entities;
using Entities.CameraControl;
using Entities.Controller;
using SceneLoaderUtil;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public enum GameState : byte
    {
        Running,
        Paused,
        GameOver
    }

    public class GameManager : MonoBehaviour
    {
        private static GameManager gameManager;
        public static GameManager Instance => gameManager;

        private GameState currentGameState = GameState.Running;
        public GameState CurrentGameState => currentGameState;

        private SceneLoader sceneLoader;
        [SerializeField] private PauseMenuUi pauseMenuUi;

        private List<BaseEntity> activeEntities = new();
        private List<BaseEntity> disabledEntities = new();
        private HashSet<BaseEntity> entitiesChangedQueue = new();
        private PlayerController playerController;
        private CameraController cameraController;
        private PlayerSpawner playerSpawner;


        public void ChangeScene(ref SceneData sceneData)
        {
            sceneLoader.LoadScene(sceneData);
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

        private void Start()
        {
            SceneSetUp();
        }

        private void Update()
        {
            foreach (var entity in activeEntities)
            {
                entity.OnUpdate();
            }

            CleanUp();
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

            for (int i = 0; i < allEntities.Length; i++)
            {
                BaseEntity baseEntity = allEntities[i];

                if (baseEntity is PlayerController)
                {
                    playerController = baseEntity as PlayerController;
                }
                else if (baseEntity is CameraController)
                {
                    cameraController = baseEntity as CameraController;
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

                    break;
            }
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

        public void RespawnPlayer()
        {
            if (playerSpawner != null)
            {
                playerSpawner.SpawnPlayer(playerController);
            }
        }
    }

}