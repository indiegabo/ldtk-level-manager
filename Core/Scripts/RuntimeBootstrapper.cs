using System.Collections.Generic;
using System.Linq;
using LDtkLevelManager.Cartography;
using LDtkUnity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Profiling;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LDtkLevelManager
{
    public static class RuntimeBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void MainBootstrap()
        {
            Profiler.BeginSample("[LDtkLevelManager] Loading projects");

            // Load all projects with the label "LM_Project"
            var handle = Addressables.LoadAssetsAsync<Project>(
                ProjectsService.ProjectAddressabelsLabel,
                null
            );

            handle.WaitForCompletion();

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                return;
            }

            List<Project> projects = handle.Result.ToList();

            if (projects.Count == 0)
            {
                Logger.Error(
                    $"No projects labeled {ProjectsService.ProjectAddressabelsLabel} were found. "
                    + $"The {nameof(LDtkLevelManager)} services will not be initialized."
                );
                return;
            }

            Profiler.EndSample();

            Profiler.BeginSample("[LDtkLevelManager] Bootstrapping project service");

            // Create a new game object that will hold the service
            GameObject serviceGO = new();
            // Add the service to the game object
            ProjectsService service = serviceGO.AddComponent<ProjectsService>();
            // Initialize the service with the loaded projects
            service.Initialize(projects);
            // Make sure the game object is not destroyed when the scenes are unloaded
            Object.DontDestroyOnLoad(serviceGO);

            Profiler.EndSample();

            BootstrapLevelLoaders();
            BootstrapCartographers();
        }

        private static void BootstrapLevelLoaders()
        {
            Profiler.BeginSample("[LDtkLevelManager] Bootstrapping LevelLoaders");

            LevelLoader.ClearLoaders();

            // Create a new game object that will hold the cartographer
            GameObject loadersContainerGO = new()
            {
                name = $"[LDtkLevelManager] Level Loaders"
            };

            foreach (Project project in ProjectsService.Instance.GetAllProjects())
            {
                var loader = LevelLoader.InstantiateLoader(project);
                loader.transform.SetParent(loadersContainerGO.transform);
            }

            // Register for static access during runtime
            Object.DontDestroyOnLoad(loadersContainerGO);

            Profiler.EndSample();
        }

        private static void BootstrapCartographers()
        {
            Profiler.BeginSample("[LDtkLevelManager] Bootstrapping cartographers");

            // Create a new game object that will hold the cartographer
            GameObject cartographersContainerGO = new()
            {
                name = $"[LDtkLevelManager] Cartographers"
            };

            List<Cartographer.CartographerEntry> entries = new();

            foreach (Project project in ProjectsService.Instance.GetAllProjects())
            {
                if (!ProjectsService.Instance.TryGetLdtkJson(project, out LdtkJson ldtkJson))
                {
                    Logger.Error(
                        $"Failed to get LDtkJson for project {project}."
                        + "The cartographer will not be initialized."
                    );
                    continue;
                }

                // Create a new game object that will hold the cartographer
                GameObject cartographerGO = new()
                {
                    name = $"[Cartographer] {project.name}"
                };

                cartographerGO.transform.SetParent(cartographersContainerGO.transform);

                // Add the cartographer to the game object
                Cartographer cartographer = cartographerGO.AddComponent<Cartographer>();

                // Initialize the cartographer with the project
                cartographer.Initialize(project, ldtkJson);
                entries.Add(new Cartographer.CartographerEntry
                {
                    project = project,
                    cartographer = cartographer
                });
            }

            cartographersContainerGO.AddComponent<CartographersContainer>();

            // Register for static access during runtime
            Object.DontDestroyOnLoad(cartographersContainerGO);
            Cartographer.RegisterCartographers(entries);
            Profiler.EndSample();
        }
    }
}