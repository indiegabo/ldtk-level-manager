using System.Collections.Generic;
using LDtkUnity;
using UnityEngine;

namespace LDtkLevelManager.Cartography
{
    public class CartographersContainer : MonoBehaviour
    {
        public static CartographersContainer Instance { get; private set; }
        private Dictionary<Project, Cartographer> _cartographers = new();

        public Cartographer GetOrCreateFor(Project project)
        {
            if (_cartographers.TryGetValue(project, out Cartographer cartographer)) return cartographer;

            if (!ProjectsService.Instance.TryGetLdtkJson(project, out LdtkJson ldtkJson))
            {
                Debug.LogError(
                    $"Failed to get LDtkJson for project {project}."
                    + "The cartographer will not be initialized."
                );
                return null;
            }

            // Create a new game object that will hold the cartographer
            GameObject cartographerGO = new()
            {
                name = $"Cartographer: {project.name}"
            };

            var test = new GameObject();
            cartographerGO.transform.SetParent(test.transform);

            // Add the cartographer to the game object
            cartographer = cartographerGO.AddComponent<Cartographer>();
            // Initialize the cartographer with the project
            cartographer.Initialize(project, ldtkJson);
            _cartographers.Add(project, cartographer);
            return cartographer;
        }

        private void Awake()
        {
            Instance = this;
        }
    }
}