using System.Collections.Generic;
using _Scripts.Common;
using _Scripts.Game;
using _Scripts.Ships.Modules;
using _Scripts.Utils.AudioTool;
using _Scripts.Utils.AudioTool.Sounds;
using AudioTools.Sound;
using DITools;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Prefabs;
using Utilities.Random;
using Utilities.RandomService;
using Zenject;

namespace _Scripts.DI
{
    public class DiInstaller : MonoInstaller
    {
        [SerializeField] private PrefabPool prefabPool;
        [SerializeField] private ScoreService scoreService;
        [SerializeField] private ModuleRegistry moduleRegistry;
        [SerializeField] private CommonSettingsProvider settingsProvider;

        [ShowInInspector, ReadOnly] private Camera uiCamera;

        protected virtual void ConfigureServices()
        {
            Container.Configure(new List<ConfigureType>
            {
                new(typeof(IContainerConstructable), ScopeTypes.Singleton, false),
            });
        }

        public override void InstallBindings()
        {
            ConfigureServices();

            Container.Bind<IPrefabPool>().FromInstance(prefabPool).AsSingle().NonLazy();
            Container.Bind<IRandomService>().To<RandomService>().AsSingle().NonLazy();

            uiCamera = GameObject.Find("UICamera").GetComponent<Camera>();
            Container.Bind<Camera>().WithId("uiCamera").FromInstance(uiCamera).AsSingle();

            Container.Bind<ISoundManager<SoundType>>().To<SoundManager>().AsSingle().NonLazy();
            
            Container.Bind<IScoreService>()
                .FromInstance(scoreService)
                .AsSingle();
            Container.Bind<IModuleRegistry>()
                .FromInstance(moduleRegistry)
                .AsSingle();
            Container.BindInterfacesTo<CommonSettingsProvider>()
                .FromInstance(settingsProvider)
                .AsSingle();
            
        }

        private void OnDisable()
        {
            Destroy(prefabPool);
        }
    }
}