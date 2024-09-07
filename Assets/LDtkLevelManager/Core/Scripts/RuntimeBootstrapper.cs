using System.Collections.Generic;
using System.Linq;
using LDtkLevelManager.Cartography;
using LDtkUnity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Profiling;

namespace LDtkLevelManager
{
    public static class RuntimeBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void MainBootstrap()
        {
            Profiler.BeginSample("[LDtkLevelManager] Loading projects");
            // Load all projects with the label "LM_Project"
            var handle = Addressables.LoadAssetsAsync<Project>(ProjectService.ProjectAddressabelsLabel, null);
            handle.WaitForCompletion();

            if (handle.Status != UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                Logger.Error(
                    $"Failed to load projects labeled {ProjectService.ProjectAddressabelsLabel}. "
                    + "The project sevice will not be initialized."
                );

                Logger.Exception(handle.OperationException);
                return;
            }

            List<Project> projects = handle.Result.ToList();

            if (projects.Count == 0)
            {
                Logger.Warning(
                    $"No projects labeled {ProjectService.ProjectAddressabelsLabel} were found. "
                    + $"The {nameof(LDtkLevelManager)} services will not be initialized."
                );
                return;
            }
            Profiler.EndSample();

            ProjectService service = BootstrapProjectService(projects);
            BootstrapCartographers(projects, service);
        }

        private static ProjectService BootstrapProjectService(List<Project> projects)
        {
            Profiler.BeginSample("[LDtkLevelManager] Bootstrapping project service");

            // Create a new game object that will hold the service
            GameObject serviceGO = new();
            // Add the service to the game object
            ProjectService service = serviceGO.AddComponent<ProjectService>();
            // Initialize the service with the loaded projects
            service.Initialize(projects);
            // Make sure the game object is not destroyed when the scenes are unloaded
            Object.DontDestroyOnLoad(serviceGO);

            Profiler.EndSample();

            return service;
        }

        private static void BootstrapCartographers(List<Project> projects, ProjectService service)
        {
            Profiler.BeginSample("[LDtkLevelManager] Bootstrapping cartographers");

            // Create a new game object that will hold the cartographer
            GameObject cartographersContainerGO = new()
            {
                name = $"[LDtkLevelManager] Cartographers"
            };

            List<Cartographer.CartographerEntry> entries = new();

            foreach (Project project in projects)
            {
                if (!service.TryGetLdtkJson(project, out LdtkJson ldtkJson))
                {
                    Logger.Error(
                        $"Failed to get LDtkJson for project {project}."
                        + "The cartographer will not be initialized."
                    );
                    continue;
                }

                // Create a new game object that will hold the cartographer
                GameObject cartographerGO = new();
                cartographerGO.transform.SetParent(cartographersContainerGO.transform);
                cartographerGO.name = $"Cartographer: {project.name}";

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

            // Register for static access during runtime
            Cartographer.RegisterCartograpers(entries);
            Object.DontDestroyOnLoad(cartographersContainerGO);

            Profiler.EndSample();
        }
    }
}