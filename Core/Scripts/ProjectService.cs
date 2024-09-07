using System.Collections.Generic;
using System.Linq;
using LDtkUnity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LDtkLevelManager
{
    public class ProjectService : MonoBehaviour
    {
        #region Static

        private static readonly string ProjectAddressabelsLabel = "LM_Project";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Bootstrap()
        {
            var handle = Addressables.LoadAssetsAsync<Project>(ProjectAddressabelsLabel, null);
            handle.WaitForCompletion();

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Logger.Error($"Failed to load projects with label {ProjectAddressabelsLabel}.");
            }

            GameObject serviceGO = new();
            ProjectService service = serviceGO.AddComponent<ProjectService>();
            service.Initialize(handle.Result.ToList());
            DontDestroyOnLoad(serviceGO);
        }

        static ProjectService _instance;
        public static ProjectService Instance => _instance;

        #endregion

        #region Fields

        Dictionary<Project, LdtkJson> _ldtkJsons = new();

        #endregion

        #region Behaviour

        #endregion

        #region Initialization

        public void Initialize(List<Project> projects)
        {
            _instance = this;
            name = $"[LDtkLevelManager] {nameof(ProjectService)}";

            foreach (Project project in projects)
            {
                LdtkJson ldtkJson = project.LDtkProject;
                _ldtkJsons.Add(project, ldtkJson);
            }
        }

        #endregion

        #region Serving

        public bool TryGetLdtkJson(Project project, out LdtkJson ldtkJson)
        {
            return _ldtkJsons.TryGetValue(project, out ldtkJson);
        }

        #endregion
    }
}