using System.Collections.Generic;
using System.Linq;
using LDtkUnity;
using UnityEngine;

namespace LDtkLevelManager
{
    public class ProjectsService : MonoBehaviour
    {
        #region Static

        public static readonly string ProjectAddressabelsLabel = "LM_Project";
        static ProjectsService _instance;
        public static ProjectsService Instance => _instance;

        #endregion

        #region Fields

        Dictionary<string, LdtkJson> _ldtkJsons = new();
        Dictionary<string, Project> _projects = new();

        #endregion

        #region Behaviour

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the <see cref="ProjectsService"/>.
        /// </summary>
        /// <param name="projects">The list of projects to initialize the service with.</param>
        /// <remarks>
        /// This method is called automatically when the game object is created using 
        /// Unity's <see cref="UnityEngine.RuntimeInitializeOnLoadMethodAttribute "/>.
        /// </remarks>
        public void Initialize(List<Project> projects)
        {
            // Set the instance of the service to this instance.
            _instance = this;

            // Set the name of the service to the name of the class.
            name = $"[LDtkLevelManager] {nameof(ProjectsService)}";

            // Iterate over the projects and add them to the dictionary.
            foreach (Project project in projects)
            {
                _projects.Add(project.Iid, project);

                // Get the LDtkJson from the project.
                LdtkJson ldtkJson = project.LDtkProject;

                // Add the project and its LDtkJson to the dictionary.
                _ldtkJsons.Add(project.Iid, ldtkJson);
            }
        }

        #endregion

        #region Serving

        public List<Project> GetAllProjects()
        {
            return _projects.Values.ToList();
        }

        /// <summary>
        /// Attempts to retrieve the <see cref="LdtkJson"/> associated with the given <see cref="Project"/>.
        /// </summary>
        /// <param name="project">The project to retrieve the LDtkJson for.</param>
        /// <param name="ldtkJson">The retrieved LDtkJson if successful, otherwise the default value of <see cref="LdtkJson"/>.</param>
        /// <returns><c>true</c> if the LDtkJson was found, otherwise <c>false</c>.</returns>
        public bool TryGetLdtkJson(Project project, out LdtkJson ldtkJson)
        {
            if (project == null) throw new System.ArgumentNullException(nameof(project));
            return _ldtkJsons.TryGetValue(project.Iid, out ldtkJson);
        }

        #endregion
    }
}