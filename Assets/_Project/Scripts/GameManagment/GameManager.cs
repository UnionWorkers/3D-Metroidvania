using System.Collections.Generic;
using Entities;
using SceneLoaderUtil;
using UnityEditor.Overlays;
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
        [SerializeField] private SceneReference nextScene;


        private List<BaseEntity> activeEntities = new();
        private List<BaseEntity> disabledEntities = new();
        private HashSet<BaseEntity> entitiesChangedQueue;

        public void ChangeScene(ref SceneData sceneData)
        {
            sceneLoader.LoadScene(sceneData);
        }

        private void Awake()
        {
            if (GameManager.Instance == null)
            {
                gameManager = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(this);
            }

            sceneLoader = new(new SceneData(SceneManager.GetActiveScene().buildIndex, SceneManager.GetActiveScene().name));
        }

        private void Start()
        {
            BaseEntity[] allEntities = FindObjectsByType<BaseEntity>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (int i = 0; i < allEntities.Length; i++)
            {
                AddEntity(allEntities[i]);
            }
        }

        private void Update()
        {
            foreach (var entity in activeEntities)
            {
                entity.OnUpdate();
            }

            CleanUp();
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
                    break;
                case GameState.Paused:
                    enabled = false;
                    break;
                case GameState.GameOver:
                    SceneData sceneData = nextScene.SceneData;
                    ChangeScene(ref sceneData);
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
    }

}