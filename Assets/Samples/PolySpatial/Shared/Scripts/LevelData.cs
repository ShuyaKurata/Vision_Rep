using UnityEngine;

namespace PolySpatial.Samples
{
    public class LevelData : MonoBehaviour
    {
        public enum LevelTypes
        {
            MixedReality,
            Meshing,
            Portal
        }

        const string MixedRealityTitle = "Mixed Reality";
        const string MeshingTitle = "Meshing";
        const string PortalTitle = "Portal";

        const string MixedRealityDescription = "Scene demonstrating an unbounded app that displays AR Plane data and uses a custom ARKit hand gesture.";
        const string MeshingDescription = "Scene showing ARKit mesh capabilities with unique ways to render the mesh.";
        const string PortalDescription = "Scene demonstrating a portal effect using Occlusion Material";

        public string GetLevelTitle(LevelTypes levelType)
        {
            switch (levelType)
            {
                case LevelTypes.MixedReality:
                    return MixedRealityTitle;
                case LevelTypes.Meshing:
                    return MeshingTitle;
                case LevelTypes.Portal:
                    return PortalTitle;
                default:
                    return "";
            }
        }

        public string GetLevelDescription(LevelTypes levelType)
        {
            switch (levelType)
            {
                case LevelTypes.MixedReality:
                    return MixedRealityDescription;
                case LevelTypes.Meshing:
                    return MeshingDescription;
                case LevelTypes.Portal:
                    return PortalDescription;
                default:
                    return "";
            }
        }
    }
}
